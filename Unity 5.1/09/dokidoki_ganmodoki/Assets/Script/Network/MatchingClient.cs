// 한 대의 단말에서 동작시킬 경우에 정의한다.
//#define UNUSE_MATCHING_SERVER

using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class MatchingClient : MonoBehaviour
{
	// 매칭할 수 있는 최대 방 수.
	private const int 	maxRoomNum = 4;

	// 참가할 수 있는 최대 플레이어 수.
	private const int 	maxMemberNum = NetConfig.PLAYER_MAX;

	private class RoomContent
	{
		public int 		node = -1;

		public int 		roomId = -1;

		public string	roomName = "";

		public int[]	members = Enumerable.Repeat(-1, maxMemberNum).ToArray();
	}

	public class MemberList
	{
		public int			node = -1;

		public IPEndPoint	endPoint;
	}


	private Network 	network_;

	private int 		level_ = 0;

	private string[] 	captions_ = new string[] {
		"지정하지 않음", "간단", "보통", "어려움"
	};

	// for client.
	private string		roomName = "";

	private	bool		isRoomOwner = false;


	private Dictionary<int, RoomContent> 	m_rooms = new Dictionary<int, RoomContent>();
	
	//private Dictionary<int, MemberList> 	m_members = new Dictionary<int, MemberList>();

	private int				playerId = -1;

	private	RoomContent		joinedRoom = new RoomContent();

	private	MemberList[]	sessionMembers = new MemberList[maxMemberNum];

	private int 			m_memberNum = 0;

	private State			matchingState = State.Idle;

	private float			timer = 0.0f;

	private	int				serverNode = 0;

	private enum State
	{
		Idle = 0,
		MatchingServer,
		RoomCreateRequested,
		RoomSearchRequested,
		RoomJoinRequested,
		WaitMembers,
		StartSessionRequested,
		StartSessionNotified,
		MatchingEnded,

		RoomCreateFailed,
		RoomJoinFailed,
	}


	// Use this for initialization
	void Start()
	{
		GameObject obj = GameObject.Find("Network");
		network_ = obj.GetComponent<Network>();

		if (network_ != null) {
			network_.RegisterReceiveNotification(PacketId.MatchingResponse, this.OnReceiveMatchingResponse);
			network_.RegisterReceiveNotification(PacketId.SearchRoomResponse, this.OnReceiveSearchRoom);
			network_.RegisterReceiveNotification(PacketId.StartSessionNotify, this.OnReceiveStartSession);
		}

		// 처음은 모든 방 검색.
		RequestSearchRoom(-1, 0);

		matchingState = State.Idle;

#if UNUSE_MATCHING_SERVER
		matchingState = State.StartSessionNotified;
#endif
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

#if UNUSE_MATCHING_SERVER
		case State.StartSessionNotified:
			OnReceiveStartSession(0, 0, null);
			break;
#endif
		}

		timer += Time.deltaTime;
	}


	void WaitMembers(RoomContent room)
	{
		if (timer > 5.0f) {
			RequestSearchRoom(room.roomId, -1);
			timer = 0.0f;
		}
	}
	
	public void OnGUIMatching()
	{


		switch (matchingState) {
		case State.Idle:
			OnGUISelectRoomType();
			break;

		case State.RoomCreateRequested:
			OnGUIRoomCreated();
			break;

		case State.RoomJoinRequested:
			DrawRoomInfo(false);
			break;

		case State.MatchingEnded:
			DrawRoomInfo(false);
			break;

		case State.RoomCreateFailed:
			NotifyError("방을 생성할 수 없습니다.");
			break;

		case State.RoomJoinFailed:
			NotifyError("방에 참가할 수 없습니다.");
			break;
		}
	}


	void OnGUISelectRoomType()
	{
		int px = Screen.width/2;
		int py = Screen.height/2 - 50;
		int sx = 180;
		int sy = 30;

		{
			GUIStyle style = new GUIStyle();
			style.fontSize = 16;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.black;
			
			GUI.Label(new Rect(px-100, py-20, 160, 20), "방 이름", style);
			roomName = GUI.TextField(new Rect(px-100, py, sx, sy), roomName);
		}

		if (GUI.Button(new Rect(px+90, py, sx-60, sy), "방을 만든다")) {
			RequestCreateRoom(roomName, level_);
			matchingState = State.RoomCreateRequested;
		}
		
		if (GUI.Button(new Rect(px+90, py+40, sx-60, sy), "방을 찾는다")) {
			RequestSearchRoom(-1, level_);
		}


		{
			GUIStyle style = new GUIStyle();
			style.fontSize = 14;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.black;
			
			// 레벨 선택.
			level_ = GUI.SelectionGrid(new Rect(px-100, py+40, sx, 40), level_, captions_, 2);  
		}


		DrawRoomInfo(true);
	}

	void OnGUIRoomCreated()
	{
		int px = Screen.width/2;
		int py = Screen.height/2 - 50;

		if (GUI.Button(new Rect(px-100, py, 200, 30), "게임을 시작한다")) {
			RequestStartSession(joinedRoom);
		}

		DrawRoomInfo(false);
	}

	private void DrawRoomInfo(bool enable)
	{
		int px = Screen.width/2 - 150;
		int py = Screen.height/2 + 70;
		int sy = 30;

		{
			GUIStyle style = new GUIStyle();
			style.fontSize = 16;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.black;

			if (matchingState == State.Idle) {
				GUI.Label(new Rect(px+30, py-20, 160, 20), "발견한 방", style);
			}
			else if (matchingState == State.RoomCreateRequested || 
			         matchingState == State.RoomJoinRequested ||
			         matchingState == State.MatchingEnded) {
				GUI.Label(new Rect(px+110, py-20, 160, 20), "방의 상황", style);
			}
		}

		int index = 1;
		foreach (RoomContent room in m_rooms.Values) {
			string label = "방" + index + " : " + room.roomName + 
				" (앞으로: " + (maxMemberNum - room.members[0]).ToString() + "명)";
			if (GUI.Button(new Rect(px, py, 300, sy), label)) {
				if (enable) {
					RequestJoinRoom(room);
				}
			}
			py += sy + 10;
			++index;
		}
	}

	public bool IsFinishedMatching()
	{
		return (matchingState == State.MatchingEnded);
	}

	public bool	IsRoomOwner()
	{
		return isRoomOwner;
	}

	public void SetServerNode(int node)
	{
		serverNode = node;
	}

	public int GetPlayerId()
	{
		return playerId;
	}

	public MemberList[] GetMemberList()
	{
		return sessionMembers;
	}

	public int GetMemberNum()
	{
		return m_memberNum;
	}

	//
	// 클라이언트 측 처리.
	//

	void RequestCreateRoom(string name, int level)
	{
		MatchingRequest request = new MatchingRequest();

		request.request = MatchingRequestId.CreateRoom;
		request.version = NetConfig.SERVER_VERSION;
		request.name = name;
		request.roomId = -1;
		request.level = level;

		MatchingRequestPacket packet = new MatchingRequestPacket(request);

		if (network_ != null) {
			network_.SendReliable<MatchingRequest>(serverNode, packet);
		}
	}

	void RequestSearchRoom(int roomId, int level) 
	{
		MatchingRequest request = new MatchingRequest();

		request.request = MatchingRequestId.SearchRoom;
        request.version = NetConfig.SERVER_VERSION;
		request.name = "";
		request.roomId = roomId;
		request.level = level;

		MatchingRequestPacket packet = new MatchingRequestPacket(request);
		
		network_.SendReliable<MatchingRequest>(serverNode, packet);
	}

	void RequestJoinRoom(RoomContent room)
	{
		MatchingRequest request = new MatchingRequest();
		
		request.request = MatchingRequestId.JoinRoom;
        request.version = NetConfig.SERVER_VERSION;
        request.name = "";
		request.roomId = room.roomId;
		request.level = -1;

		if (network_ != null) {
			MatchingRequestPacket packet = new MatchingRequestPacket(request);
		
			network_.SendReliable<MatchingRequest>(serverNode, packet);
		}

		string str = "RequestJoinRoom:" + room.roomId;
		Debug.Log(str);
	}
	
	void RequestStartSession(RoomContent room)
	{
		MatchingRequest request = new MatchingRequest();
		
		request.request = MatchingRequestId.StartSession;
        request.version = NetConfig.SERVER_VERSION;
        request.name = room.roomName;
		request.roomId = room.roomId;
		request.level = -1;

		Debug.Log("Request start session[room:" + room.roomName + " " + room.roomId + "]");

		if (network_ != null) {
			MatchingRequestPacket packet = new MatchingRequestPacket(request);
		
			network_.SendReliable<MatchingRequest>(serverNode, packet);
		}
	}

	//
	// 패킷 수신 처리.
	//
	
	void OnReceiveMatchingResponse(int node, PacketId id, byte[] data)
	{
		MatchingResponsePacket packet = new MatchingResponsePacket(data);
		MatchingResponse response = packet.GetPacket();

		string str = "ReceiveMatchingResponse:" + response.request;
		Debug.Log(str);

		switch (response.request) {
		case MatchingRequestId.CreateRoom:
			CreateRoomResponse(response.result, response.roomId);
			break;

		case MatchingRequestId.JoinRoom:
			JoinRoomResponse(response.result, response.roomId) ;
			break;

		default:
			Debug.Log("Unknown request.");
			break;
		}
	}

	void OnReceiveSearchRoom(int node, PacketId id, byte[] data)
	{
		Debug.Log("ReceiveSearchRoom");

		SearchRoomPacket packet = new SearchRoomPacket(data);
		SearchRoomResponse response = packet.GetPacket();

		string str = "Created room num:" + response.roomNum;
		Debug.Log(str);

		SearchRoomResponse(response.roomNum, response.rooms);
	}

	void OnReceiveStartSession(int node, PacketId id, byte[] data)
	{
		Debug.Log("ReceiveStartSession");

#if UNUSE_MATCHING_SERVER
		SessionData response = new SessionData();

		{
			int memberNum = NetConfig.PLAYER_MAX;
			string hostname = Dns.GetHostName();
			IPAddress[] adrList = Dns.GetHostAddresses(hostname);
			response.endPoints = new EndPointData[memberNum];

			response.result = MatchingResult.Success;
			response.playerId = GlobalParam.get().global_account_id;
			response.members = memberNum;

			for (int i = 0; i < memberNum; ++i) {
				response.endPoints[i] = new EndPointData();
				response.endPoints[i].ipAddress = adrList[0].ToString();
				response.endPoints[i].port = NetConfig.GAME_PORT;
			}

		}
#else
		SessionPacket packet = new SessionPacket(data);
		SessionData response = packet.GetPacket();
#endif
		playerId = response.playerId;

		SetSessionMembers(response.result, response.members, response.endPoints);

		matchingState = State.MatchingEnded;
	}


	void CreateRoomResponse(MatchingResult result, int roomId)
	{
		Debug.Log("CreateRoomResponse");

		if (result == MatchingResult.Success) {
			isRoomOwner = true;
			joinedRoom.roomId = roomId;
			matchingState = State.RoomCreateRequested;

			RequestSearchRoom(roomId, -1);

			string str = "Create room is success [id:" + roomId + "]";
			Debug.Log(str);
		}
		else {
			matchingState = State.RoomCreateFailed;

			Debug.Log("Create room is failed.");
		}
	}

	void SearchRoomResponse(int roomNum, RoomInfo[] rooms)
	{
		m_rooms.Clear();

		for (int i = 0; i < roomNum; ++i) {
			RoomContent r = new RoomContent();

			r.roomId = rooms[i].roomId;
			r.roomName = rooms[i].name;
			r.members[0] = rooms[i].members;

			m_rooms.Add(rooms[i].roomId, r);

			string str = "Room name[" + i + "]:" + rooms[i].name + 
				" [id:" + rooms[i].roomId + ":" + rooms[i].members +"]";
			Debug.Log(str);
		}
	}

	void JoinRoomResponse(MatchingResult result, int roomId) 
	{
		Debug.Log("JoinRoomResponse");

		if (result == MatchingResult.Success) {
			joinedRoom.roomId = roomId;
			matchingState = State.RoomJoinRequested;

			string str = "Join room was success [id:" + roomId + "]";
			Debug.Log(str);
		}
		else {
			matchingState = State.RoomJoinFailed;
			RequestSearchRoom(-1, -1);
			Debug.Log("Join room was failed.");
		}
	}

	void SetSessionMembers(MatchingResult result, int memberNum, EndPointData[] endPoints)
	{
		Debug.Log("StartSessionNotify");

		string str = "MemberNum:" + memberNum + " result:" + result;
		Debug.Log(str);

		m_memberNum = memberNum;

		for (int i = 0; i < memberNum; ++i) {

			MemberList member = new MemberList();

			member.node = i;
			member.endPoint = new IPEndPoint(IPAddress.Parse(endPoints[i].ipAddress), endPoints[i].port);

			sessionMembers[i] = member;

			str = "member[" + i + "]:" + member.endPoint.Address.ToString() + " : " + endPoints[i].port;
			Debug.Log(str);
		}
	}

	// 오류 알림.
	private void NotifyError(string message)
	{
		GUIStyle style = new GUIStyle(GUI.skin.GetStyle("button"));
		style.normal.textColor = Color.white;
		style.fontSize = 25;
		
		float sx = 450;
		float sy = 200;
		float px = Screen.width / 2 - sx * 0.5f;
		float py = Screen.height / 2 - sy * 0.5f;
		
		if (GUI.Button (new Rect (px, py, sx, sy), message, style)) {
			matchingState = State.Idle;
		}
	}
}

