using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class MatchingServer : MonoBehaviour
{

	private const int 	maxRoomNum = 4;

	private const int 	maxMemberNum = NetConfig.PLAYER_MAX;


	private int			counter = 0;

	private class RoomContent
	{
		public int 		node = -1;

		public int 		roomId = -1;

		public string	name = "";

		public int		level = 0;

		public bool		isClosed = false;

		public float	elapsedTime = 0.0f;

		public int[]	members = Enumerable.Repeat(-1, maxMemberNum).ToArray();

	}

	private class MemberList
	{
		public int			node = -1;

		public int			roomId = -1;

		public IPEndPoint	endPoint = null;
	}

	private string[] 	levelString = new string[] {
		"지정하지 않음", "간단", "보통", "어려움"
	};

	private Dictionary<int, RoomContent> 	rooms_ = new Dictionary<int, RoomContent>();


	private Dictionary<int, MemberList> 	m_members = new Dictionary<int, MemberList>();


	private Network 	network_ = null;
	
	private int			roomIndex = 0;


	private	RoomContent		joinedRoom = new RoomContent();

	private	MemberList[]	sessionMembers = new MemberList[maxMemberNum];

	private State			matchingState = State.Idle;

	private float			timer = 0.0f;

	private enum State
	{
		Idle = 0,
		MatchingServer,
		RoomCreateRequested,
		RoomSearchRequested,
		RoomJoinRequested,
		StartSessionRequested,
		StartSessionNotified,
		MatchingEnded,
	}


	// Use this for initialization
	void Start()
	{
		GameObject obj = new GameObject("Network");
		network_ = obj.AddComponent<Network>();

		matchingState = State.Idle;

		if (network_ != null) {
			network_.RegisterReceiveNotification(PacketId.MatchingRequest, OnReceiveMatchingRequest);
			network_.StartServer(NetConfig.MATCHING_SERVER_PORT, maxRoomNum);

			network_.RegisterEventHandler(OnEventHandling);

			if (network_.IsServer() == true) {
				matchingState = State.MatchingServer;
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		switch (matchingState) {

		case State.RoomCreateRequested:
			WaitMembers(joinedRoom);
			break;

		case State.RoomJoinRequested:
			WaitMembers(joinedRoom);
			break;

		case State.StartSessionNotified:
			ConnectSessionMember();
			break;
		}

		// 멤버가 없어진 방 삭제.
		Dictionary<int, RoomContent> rooms = new Dictionary<int, RoomContent>(rooms_);

		foreach (RoomContent room in rooms.Values) {

			if (room.isClosed) {
				rooms_[room.roomId].elapsedTime += Time.deltaTime;
				if (rooms_[room.roomId].elapsedTime > 3.0f) {
					// 게임을 시작한 방 삭제
					DisconnectRoomMembers(room);
					rooms_.Remove(room.roomId);
				}
			}
			else {
				int count = 0;
				for (int i = 0; i < room.members.Length; ++i) {
					if (room.members[i] != -1) {
						++count;
					}
				}

				if (count == 0) {
					// 연결이 끊어져 방이 사라짐.
					rooms_.Remove(room.roomId);
				}
			}
		}

		timer += Time.deltaTime;

		++counter;
	}


	void WaitMembers(RoomContent room)
	{
		if (timer > 5.0f) {
			int roomId = room.roomId;
			SearchRoom(-1, roomId, room.level);
			timer = 0.0f;
		}
	}

	void ConnectSessionMember()
	{
		matchingState = State.MatchingEnded;
	}


	void OnGUI()
	{
		int px = 10;
		int py = 40;
		int sx = 400;
		int sy = 100;

		Rect rect = new Rect(px, 5, 400, 100);

		string label = "매칭 서버 시작 중 ";
		for (int i = 0; i < (counter % 600) / 60; ++i) {
			label += ".";
		}

		// 호스트 이름을 가져옵니다.
		string hostname = Dns.GetHostName();
		// 호스트 이름으로 IP 주소를 가져옵니다.
		IPAddress[]	adrList = Dns.GetHostAddresses(hostname);
		string serverAddress = adrList[0].ToString();

		label += "\n[IP주소:" + serverAddress + "][포트:" + NetConfig.MATCHING_SERVER_PORT + "][접속수" + m_members.Count + "] ";

		GUI.Label(rect, label);

		foreach (RoomContent room in rooms_.Values) {
			DrawRoomInformation(new Rect(px, py, sx, sy),  room);
			py += sy + 10;
		}
	}

	void DrawRoomInformation(Rect rect, RoomContent room)
	{
		string infoText = "";
		
		infoText += "방이름[" + room.roomId + "]: " + room.name + "\t\t";
		
		int count = 0;
		string epStr = "";
		for (int i = 0; i < maxMemberNum; ++i) {
			if (room.members[i] != -1) {
				IPEndPoint ep = network_.GetEndPoint(room.members[i]);

				if (ep != null) {
					epStr += "멤버" + (i+1).ToString() + "의 주소: " + ep.Address + ":" + ep.Port + "\n"; 
					++count;
				}
			}
		}
		
		infoText += "인수: " + count + "\t\t레벨: " + levelString[room.level] + "\n\n";
		infoText += epStr;
		
		GUI.TextField(new Rect(rect.x, rect.y, rect.width, rect.height), infoText);

	}


	//
	// 매칭 서버 측 처리.
	//



	void DisconnectRoomMembers(RoomContent room)
	{
		foreach (int node in room.members) {
			if (node != -1) {
				network_.Disconnect(node);
				m_members.Remove(node);
			}
		}
	}


	//
	// 패킷 수신 처리.
	//
	
	void OnReceiveMatchingRequest(int node, byte[] data)
	{
		MatchingRequestPacket packet = new MatchingRequestPacket(data);
		MatchingRequest request = packet.GetPacket();

		string str = "ReceiveMatchingRequest:" + request.request;
		Debug.Log(str);

		if (request.version != NetConfig.SERVER_VERSION) {
			Debug.Log("Invalid request version.");
			// 다른 버전의 헤더는 폐기합니다.
			Debug.Log("Current ver:" + NetConfig.SERVER_VERSION + " Req ver:" + request.version);
			return;
		}

		switch (request.request) {
			case MatchingRequestId.CreateRoom: {
				CreateRoom(node, request.name, request.level);
			}	break;

			case MatchingRequestId.SearchRoom: {
				SearchRoom(node, request.roomId, request.level);
			}	break;

			case MatchingRequestId.JoinRoom: {
				JoinRoom(node, request.roomId);
			}	break;

			case MatchingRequestId.StartSession: {
				StartSession(node, request.roomId);
			}	break;
		}
	}
	
	void CreateRoom(int node, string name, int level)
	{
		Debug.Log("ReceiveCreateRoomRequest");

		MatchingResponse response = new MatchingResponse();


		response.request = MatchingRequestId.CreateRoom;

		if (rooms_.Count < maxRoomNum) {

			RoomContent room = new RoomContent();


			room.roomId = roomIndex;

			room.name = name;

			room.level = level;

			// 자기 자신을 맨앞으로 설정.
			room.members[0] = node;
			
			m_members[node].roomId = roomIndex;

			rooms_.Add(roomIndex, room);
			++roomIndex;


			response.result = MatchingResult.Success;
			response.roomId = room.roomId;
			response.name = "";

			string str = "Request node:" + node + " Created room id:" + response.roomId;
			Debug.Log(str);
		}
		else {
			response.result = MatchingResult.RoomIsFull;
			response.roomId = -1;
			Debug.Log("Create room failed.");
		}

		MatchingResponsePacket packet = new MatchingResponsePacket(response);
		
		network_.SendReliable<MatchingResponse>(node, packet);
	}


	void SearchRoom(int node, int roomId, int level)
	{
		Debug.Log("ReceiveSearchRoomPacket");

		SearchRoomResponse response = new SearchRoomResponse();

		response.rooms = new RoomInfo[rooms_.Count];


		int index = 0;
		foreach (RoomContent r in rooms_.Values) {
			if (roomId == r.roomId ||
			    roomId != r.roomId && level >= 0 && (level == 0 || level == r.level)) {
				response.rooms[index].roomId = r.roomId;
				response.rooms[index].name = r.name;

				int count  = 0;
				for (int i = 0; i < r.members.Length; ++i) {
					if (r.members[i] != -1) {
						++count;
					}
				}
				response.rooms[index].members = count;
				
				++index;
			}
		}

		response.roomNum = index;


		SearchRoomPacket packet = new SearchRoomPacket(response);

		network_.SendReliable<SearchRoomResponse>(node, packet);

		
		string str = "Request node:" + node + " Created room num:" + response.roomNum;
		Debug.Log(str);
		for (int i = 0; i < response.roomNum; ++i) {
			str = "Room name[" + i + "]:" +  response.rooms[i].name + 
				  " [id:" + response.rooms[i].roomId + ":" + response.rooms[i].members +"]";
			Debug.Log(str);
		}
	}

	void JoinRoom(int node, int roomId)
	{
		Debug.Log("ReceiveJoinRoomPacket");

		MatchingResponse response = new MatchingResponse();

		response.roomId = -1;
		response.request = MatchingRequestId.JoinRoom;

		int memberNum = 0;
		if (rooms_.ContainsKey(roomId) == true) {
			RoomContent room = rooms_[roomId];

			m_members[node].roomId = roomId;
			
			response.result = MatchingResult.MemberIsFull;
			for (int i = 0; i < maxMemberNum; ++i) {
				if (room.members[i] == -1) {
					// 자리가 있음.
					room.members[i] = node;
					rooms_[roomId] = room;
					response.result = MatchingResult.Success;
					response.roomId = roomId;
					response.name = room.name;
					break;
				}
			}

			// 정원 체크.
			for (int i = 0; i < room.members.Length; ++i) {
				if (room.members[i] != -1) {
					++memberNum;
				}
			}
		}
		else {
			Debug.Log("JoinRoom failed.");
			response.result = MatchingResult.RoomIsGone;
			response.name = "";
		}
		
		MatchingResponsePacket packet = new MatchingResponsePacket(response);	
		
		network_.SendReliable<MatchingResponse>(node, packet);
	}

	void StartSession(int node, int roomId)
	{
		string str = "ReceiveStartSessionRequest[roomId:" + roomId + "]";
		Debug.Log(str);
		
		SessionData response = new SessionData();

		RoomContent room = null;
		if (rooms_.ContainsKey(roomId) == true) {
			
			room = rooms_[roomId];

			response.endPoints = new EndPointData[maxMemberNum];
			
			int index = 0;
			for (int i = 0; i < maxMemberNum; ++i) {
				if (room.members[i] != -1) {
					
					IPEndPoint ep = network_.GetEndPoint(room.members[i]) as IPEndPoint;
					response.endPoints[index].ipAddress = ep.Address.ToString();
					response.endPoints[index].port = NetConfig.GAME_PORT;
					++index;
				}	
			}
			
			response.members = index;
			response.result = MatchingResult.Success;
		}
		else {
			response.result = MatchingResult.RoomIsGone;
		}

		if (room != null) {

			rooms_[roomId].isClosed = true;

			str = "Request room id: " + roomId + " MemberNum:" + response.members + " result:" + response.result;
			Debug.Log(str);

			for (int i = 0; i < response.members; ++i) {
				str = "member[" + i + "]" + ":" + response.endPoints[i].ipAddress + ":" + response.endPoints[i].port;
				Debug.Log(str);
			}

			int index = 0;
			for (int i = 0; i < room.members.Length; ++i) {

				int target = room.members[i];

				if (target != -1) {
						
					response.playerId = index;

					SessionPacket packet = new SessionPacket(response);
					
					network_.SendReliable<SessionData>(target, packet);

					++index;
				}
			}


		}
	}

	public void OnEventHandling(int node, NetEventState state)
	{
		string str = "Node:" + node + " type:" + state.type.ToString() + " State:" + state.type + "[" + state.result + "]";
		Debug.Log("OnEventHandling called");
		Debug.Log(str);
		
		switch (state.type) {
		case NetEventType.Connect: {
			MemberList member = new MemberList();
			
			member.node = node;
			member.endPoint = network_.GetEndPoint(node);

			m_members.Add(node, member);
		} 	break;
			
		case NetEventType.Disconnect: {

            if (m_members.ContainsKey(node)) {

				int roomId = m_members[node].roomId;
				if (rooms_.ContainsKey(roomId)) {
					for (int i = 0; i < rooms_[roomId].members.Length; ++i) {
						if (rooms_[roomId].members[i] == node) {
							rooms_[roomId].members[i] = -1;
							break;
						}
					}
				}

                m_members.Remove(node);
            }			
		}	break;
			
		}
	}
}

