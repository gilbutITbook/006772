using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;




public class Network : MonoBehaviour {

	private SessionTCP		m_sessionTcp = null;
	
	private SessionUDP		m_sessionUdp = null;

	private int				m_serverNode = -1;

	private int[]			m_clientNode = new int[NetConfig.PLAYER_MAX];

	private NodeInfo[]		m_reliableNode = new NodeInfo[NetConfig.PLAYER_MAX];

	private NodeInfo[]		m_unreliableNode = new NodeInfo[NetConfig.PLAYER_MAX];

	// 송수신용 패킷의 최대 크기.
	private const int		m_packetSize = 1400;
	
	// 수신 패킷 처리 함수 델리게이트.
	public delegate			void RecvNotifier(int node, PacketId id, byte[] data);

	// 수신 패킷 분류 해시 테이블.
	private Dictionary<int, RecvNotifier> m_notifier = new Dictionary<int, RecvNotifier>();
	
	// 이벤트 핸들러.
	private List<NetEventState>	m_eventQueue = new List<NetEventState>();

	// 이벤트 발생 플래그.
	//private bool			m_eventOccured = false;

	private class NodeInfo
	{
		public int	node = 0;
	}

	public enum ConnectionType
	{
		Reliable = 0,
		Unreliable,
	}

	void Awake()
	{
		m_sessionTcp = new SessionTCP();
		m_sessionTcp.RegisterEventHandler(OnEventHandlingReliable);

		m_sessionUdp = new SessionUDP();
		m_sessionUdp.RegisterEventHandler(OnEventHandlingUnreliable);

		for (int i = 0; i < m_clientNode.Length; ++i) {
			m_clientNode[i] = -1;
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		byte[] packet = new byte[m_packetSize];
		for (int id = 0; id < m_reliableNode.Length; ++id) {
			if (m_reliableNode[id] != null) {
				int node = m_reliableNode[id].node;
				if (IsConnected(node) == true) {
					// 도달 보장 패킷 수신.
					while (m_sessionTcp.Receive(node, ref packet) > 0) {
						// 수신 패킷 분류.
						Receive(node, packet);
					}
				}
			}
		}

		// 비도달 보장 패킷 수신.
		for (int id = 0; id < m_unreliableNode.Length; ++id) {
			if (m_unreliableNode[id] != null) {
				int node = m_unreliableNode[id].node;
				if (IsConnected(node) == true) {
					// 도달 보장이 없는 패킷의 수신.
					while (m_sessionUdp.Receive(node, ref packet) > 0) {
						// 수신 패킷 분류.
						Receive(node, packet);
					}
				}
			}		
		}
	}

	void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit called.!");

		StopServer();
	}

	public bool StartServer(int port, int connectionMax, ConnectionType type)
	{
		Debug.Log("Start server called.!");

		// 리스닝 소켓 생성.
		try {
			if (type == ConnectionType.Reliable) {
				// 도달 보장을 위한 TCP 통신 시작.
				m_sessionTcp.StartServer(port, connectionMax);
			}
			else {
				// 도달 보장이 필요치 않는 UDP 통신은 언제든지 받을 수 있게 리스닝 시작.
				m_sessionUdp.StartServer(port, connectionMax);
			}
		}
		catch {
			Debug.Log("Server fail start.!");
			return false;
		}

		Debug.Log("Server started.!");


		return true;
	}

	public void StopServer()
	{
		Debug.Log("StopServer called.");

		// 서버를 정지.
		if (m_sessionUdp != null) {
			m_sessionUdp.StopServer();
		}

		if (m_sessionTcp != null) {
			m_sessionTcp.StopServer();
		}


		Debug.Log("Server stopped.");
	}

	// 
	public int Connect(string address, int port, ConnectionType type)
	{
		int node = -1;

		if (type == ConnectionType.Reliable && m_sessionTcp != null) {
			// 도달 보장을 위한 TCP 통신을 시작.
			node = m_sessionTcp.Connect(address, port);
		}

		if (type == ConnectionType.Unreliable && m_sessionUdp != null) {
			// 도달 보장을 하지 않는 UDP 통신을 시작.
			node = m_sessionUdp.Connect(address, port);
		}

		return node;
	}

	public void Disconnect(int node)
	{	
		if (m_sessionTcp != null) {
			m_sessionTcp.Disconnect(node);
		}
		
		if (m_sessionUdp != null) {
			m_sessionUdp.Disconnect(node);
		}
	}

	public void Disconnect()
	{
		if (m_sessionTcp != null) {
			m_sessionTcp.Disconnect();
		}
		
		if ( m_sessionUdp != null) {
			m_sessionUdp.Disconnect();
		}

		m_notifier.Clear();
	}

	public void RegisterReceiveNotification(PacketId id, RecvNotifier notifier)
	{
		int index = (int)id;

		m_notifier.Add(index, notifier);
	}

	public void ClearReceiveNotification()
	{
		m_notifier.Clear();
	}
	
	public void UnregisterReceiveNotification(PacketId id)
	{
		int index = (int)id;
		
		if (m_notifier.ContainsKey(index)) {
			m_notifier.Remove(index);
		}
	}
	
	// 이벤트 통지 함수 등록.
	public NetEventState GetEventState()
	{

		if (m_eventQueue.Count == 0) {
			return null;
		}

		NetEventState state = m_eventQueue[0];

		m_eventQueue.RemoveAt(0);

		return 	state;
	}

	// 접속 확인.
	public bool IsConnected(int node)
	{
		if (m_sessionTcp != null) {
			if (m_sessionTcp.IsConnected(node)) {
				return true;
			}
		}
		
		if (m_sessionUdp != null) {
			if (m_sessionUdp.IsConnected(node)) {
				return true;
			}
		}

		return	false;
	}

	//
	public bool IsServer()
	{
		if (m_sessionTcp == null) {
			return false;
		}

		return	m_sessionTcp.IsServer();
	}


	public IPEndPoint GetLocalEndPoint(int node)
	{
		if (m_sessionTcp == null) {
			return default(IPEndPoint);
		}

		return m_sessionTcp.GetLocalEndPoint(node);
	}
	
	public int Send<T>(int node, PacketId id, IPacket<T> packet)
	{
		int sendSize = 0;
		
		if (m_sessionTcp != null) {
			// 모듈에서 사용하는 헤더 정보 생성.
			PacketHeader header = new PacketHeader();
			HeaderSerializer serializer = new HeaderSerializer();
					
			header.packetId = id;

			byte[] headerData = null;
			if (serializer.Serialize(header) == true) {
				headerData = serializer.GetSerializedData();
			}
			byte[] packetData = packet.GetData();
			
			byte[] data = new byte[headerData.Length + packetData.Length];
			
			int headerSize = Marshal.SizeOf(typeof(PacketHeader));
			Buffer.BlockCopy(headerData, 0, data, 0, headerSize);
			Buffer.BlockCopy(packetData, 0, data, headerSize, packetData.Length);

			//string str = "Send Packet[" +  id  + "]";

			sendSize = m_sessionTcp.Send(node, data, data.Length);
		}
		
		return sendSize;
	}

	public int SendReliable<T>(int node, IPacket<T> packet)
	{
		int sendSize = 0;
		
		if (m_sessionTcp != null) {
			// 모듈에서 사용하는 헤더 정보를 생성.
			PacketHeader header = new PacketHeader();
			HeaderSerializer serializer = new HeaderSerializer();
			
			header.packetId = packet.GetPacketId();

			byte[] headerData = null;
			if (serializer.Serialize(header) == true) {
				headerData = serializer.GetSerializedData();
			}

			byte[] packetData = packet.GetData();
			byte[] data = new byte[headerData.Length + packetData.Length];
			
			int headerSize = Marshal.SizeOf(typeof(PacketHeader));
			Buffer.BlockCopy(headerData, 0, data, 0, headerSize);
			Buffer.BlockCopy(packetData, 0, data, headerSize, packetData.Length);
			
			//string str = "Send reliable packet[" +  header.packetId  + "]";
			
			sendSize = m_sessionTcp.Send(node, data, data.Length);
			if (sendSize < 0 && m_eventQueue != null) {
				// 송신 오류.
				NetEventState state = new NetEventState();
				state.node = node;
				state.type = NetEventType.SendError;
				state.result = NetEventResult.Failure;
				m_eventQueue.Add(state);
			}
		}
		
		return sendSize;
	}

	public void SendReliableToAll<T>(IPacket<T> packet)
	{
		foreach (NodeInfo info in m_reliableNode) {
			if (info != null) {

				int sendSize = SendReliable<T>(info.node, packet);
				if (sendSize < 0 && m_eventQueue != null) {
					// 송신 오류.
					NetEventState state = new NetEventState();
					state.node = info.node;
					state.type = NetEventType.SendError;
					state.result = NetEventResult.Failure;
					m_eventQueue.Add(state);
				}

			}
		}
	}

	public void SendReliableToAll(PacketId id, byte[] data)
	{
		foreach (NodeInfo info in m_reliableNode) {

			if (info != null && info.node >= 0) {
				// 모듈에서 사용하는 헤더 정보를 생성.
				PacketHeader header = new PacketHeader();
				HeaderSerializer serializer = new HeaderSerializer();
				
				header.packetId = id;
				
				byte[] headerData = null;
				if (serializer.Serialize(header) == true) {
					headerData = serializer.GetSerializedData();
				}
				
				byte[] packetData = data;
				byte[] pdata = new byte[headerData.Length + packetData.Length];
				
				int headerSize = Marshal.SizeOf(typeof(PacketHeader));
				Buffer.BlockCopy(headerData, 0, pdata, 0, headerSize);
				Buffer.BlockCopy(packetData, 0, pdata, headerSize, packetData.Length);

				int sendSize = m_sessionTcp.Send(info.node, pdata, pdata.Length);
				if (sendSize < 0 && m_eventQueue != null) {
					// 송신 오류.
					NetEventState state = new NetEventState();
					state.node = info.node;
					state.type = NetEventType.SendError;
					state.result = NetEventResult.Failure;
					m_eventQueue.Add(state);
				}

			}
		}
	}

	public int SendUnreliable<T>(int node, IPacket<T> packet)
	{
		int sendSize = 0;
		
		if (m_sessionUdp != null) {
			// 모듈에서 사용하는 헤더 정보를 생성.
			PacketHeader header = new PacketHeader();
			HeaderSerializer serializer = new HeaderSerializer();
			
			header.packetId = packet.GetPacketId();

			byte[] headerData = null;
			if (serializer.Serialize(header) == true) {
				headerData = serializer.GetSerializedData();
			}
			byte[] packetData = packet.GetData();
			
			byte[] data = new byte[headerData.Length + packetData.Length];
			
			int headerSize = Marshal.SizeOf(typeof(PacketHeader));
			Buffer.BlockCopy(headerData, 0, data, 0, headerSize);
			Buffer.BlockCopy(packetData, 0, data, headerSize, packetData.Length);
			
			sendSize = m_sessionUdp.Send(node, data, data.Length);

 			if (sendSize < 0 && m_eventQueue != null) {
				// 송신 오류.
				NetEventState state = new NetEventState();
				state.node = node;
				state.type = NetEventType.SendError;
				state.result = NetEventResult.Failure;
				m_eventQueue.Add(state);
			}
		}
		
		return sendSize;
	}

	public void SendUnreliableToAll<T>(IPacket<T> packet)
	{
		foreach (NodeInfo info in m_unreliableNode) {
			if (info != null) {
				 SendUnreliable<T>(info.node, packet);
			}
		}
	}

	private void Receive(int node, byte[] data)
	{
		PacketHeader header = new PacketHeader();
		HeaderSerializer serializer = new HeaderSerializer();

		serializer.SetDeserializedData(data);
		bool ret = serializer.Deserialize(ref header);
		if (ret == false) {
			Debug.Log("Invalid header data.");
			// 패킷으로서 인식할 수 없으므로 폐기한다.
			return;			
		}

		int packetId = (int)header.packetId;
		if (m_notifier.ContainsKey(packetId) &&
		    m_notifier[packetId] != null) {
			int headerSize = Marshal.SizeOf(typeof(PacketHeader));
			byte[] packetData = new byte[data.Length - headerSize];
			Buffer.BlockCopy(data, headerSize, packetData, 0, packetData.Length);
	
			m_notifier[packetId](node, header.packetId, packetData);
		}
	}

	public void OnEventHandlingReliable(int node, NetEventState state)
	{
		Debug.Log("OnEventHandling called");
		string str = "Node:" + node + " type:" + state.type.ToString() + " State:" + state.type + "[" + state.result + "]";
		Debug.Log(str);
		
		switch (state.type) {
		case NetEventType.Connect: {
			for (int i = 0; i < m_reliableNode.Length; ++i) {
				if (m_reliableNode[i] == null) {
					NodeInfo info = new NodeInfo();
					
					info.node = node;
					m_reliableNode[i] = info;
					break;
				}
				else if (m_reliableNode[i].node == -1) {
					m_reliableNode[i].node = node;
				}
			}
			
		} 	break;
			
		case NetEventType.Disconnect: {
			for (int i = 0; i < m_reliableNode.Length; ++i) {

				if (m_reliableNode[i] != null && m_reliableNode[i].node == node) {

					m_reliableNode[i].node = -1;
					break;
				}
			}
			
		}	break;
			
		}

		
		if (m_eventQueue != null) {
			// 이벤트 등록.
			NetEventState eState = new NetEventState();
			eState.node = node;
			eState.type = state.type;
			eState.result = NetEventResult.Success;
			m_eventQueue.Add(eState);
		}

	}
	
	public void OnEventHandlingUnreliable(int node, NetEventState state)
	{
		Debug.Log("OnEventHandlingUnreliable called");
		string str = "Node:" + node + " type:" + state.type.ToString() + " State:" + state.result.ToString();
		Debug.Log(str);

		switch (state.type) {
			case NetEventType.Connect: {
				for (int id = 0; id < m_unreliableNode.Length; ++id) {
					if (m_unreliableNode[id] == null) {
						NodeInfo info = new NodeInfo();

						info.node = node;
						m_unreliableNode[id] = info;
					break;
					}
					else if (m_unreliableNode[id].node == -1) {
						m_unreliableNode[id].node = node;
					}
				}

			} 	break;

			case NetEventType.Disconnect: {

				for (int i = 0; i < m_unreliableNode.Length; ++i) {
					
					if (m_unreliableNode[i] != null && m_unreliableNode[i].node == node) {
						m_unreliableNode[i].node = -1;
					break;
					}
				}

			}	break;
		}

		if (m_eventQueue != null) {
			// 이벤트 등록.
			NetEventState eState = new NetEventState();
			eState.node = node;
			eState.type = state.type;
			eState.result = NetEventResult.Success;
			m_eventQueue.Add(eState);
		} 
	}

	public bool StartGameServer(int playerNum)
	{
		GameObject obj = new GameObject("GameServer");
		GameServer server = obj.AddComponent<GameServer>();
		if (server == null) {
			Debug.Log("GameServer failed start.");
			return false;
		}
		
		server.StartServer(playerNum);
		DontDestroyOnLoad(server);
		Debug.Log("GameServer started.");
		
		return true;
	}

	public void StopGameServer()
	{
		GameObject obj = GameObject.Find("GameServer");
		if (obj) {
			GameServer server = obj.GetComponent<GameServer>();
			if (server != null) {
				server.StopServer();
			}
			GameObject.Destroy(obj);
		}
		
		Debug.Log("GameServer stoped.");
	}

	public void SetServerNode(int node)
	{
		m_serverNode = node;
	}

	public int GetServerNode()
	{
		return m_serverNode;
	}

	public void SetClientNode(int gid, int node)
	{
		m_clientNode[gid] = node;
	}

	public int GetClientNode(int gid)
	{
		return m_clientNode[gid];
	}

	public int GetPlayerIdFromNode(int node)
	{

		for (int i = 0; i < m_clientNode.Length; ++i) {
			if (m_clientNode[i] == node) {
				return i;
			}
		}

		return -1;
	}


}
