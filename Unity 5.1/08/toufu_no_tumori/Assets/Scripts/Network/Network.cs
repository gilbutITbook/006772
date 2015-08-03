using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;





public class Network : MonoBehaviour {

	private TransportTcp	m_tcp = null;
	//private Dictionary<int, TransportTcp>	m_transports = null;

	private TransportUdp	m_udp = null;

	private Thread			m_thread = null;

	private	bool			m_isStarted = false;

	// 서버.
	private bool 			m_isServer = false;
	
	
	private const int 		m_headerVersion = 1;


	// 송수신용 패킷 최대 크기.
	private const int		m_packetSize = 1400;


	private const int packetMax = (int)PacketId.Max;

	// 수신 패킷 처리함수 델리게이트. 
	public delegate	void	RecvNotifier(PacketId id, byte[] data);
	// 수신 패킷 분배 해시 테이블.
	private Dictionary<int, RecvNotifier> m_notifier = new Dictionary<int, RecvNotifier>();

	// 이벤트 통지 델리게이트.
	public delegate void 	EventHandler(NetEventState state);
	// 이벤트 핸들러.
	private EventHandler	m_handler;

	// 이벤트 발생 플래그.
	private bool			m_eventOccured = false;

	// 접속 프로토콜 지정.
	public enum ConnectionType
	{
		TCP = 0,		// TCP만 접속대상.
		UDP,			// UDP만 접속대상.
		Both,			// TCP,UDP 모두 접속 대상.
	}

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}

	// Use this for initialization
	void Start()
	{

	}
	
	// Update is called once per frame
	void Update()
	{
		if (IsConnected() == true) {
			byte[] packet = new byte[m_packetSize];
	
			// 도달 보증 패킷을 수신합니다.
			while (m_tcp != null && m_tcp.Receive(ref packet, packet.Length) > 0) {
				// 수신 패킷을 분배합니다.
				ReceivePacket(packet);
			}
	
			// 비도달 보증 패킷을 수신합니다.
			while (m_udp != null && m_udp.Receive(ref packet, packet.Length) > 0) {
				// 수신 패킷을 분배합니다.
				ReceivePacket(packet);
			}
		}		
	}

	void OnApplicationQuit()
	{
		if (m_isStarted == true) {
			StopServer();
		}
	}

	public bool StartServer(int port, ConnectionType type)
	{
		Debug.Log("Network::StartServer called. port:" + port);
		
		// 리스닝 소켓을 생성합니다.
		try {
			// 도달을 보장하는 TCP통신을 시작합니다.
			if (type == ConnectionType.TCP ||
			    type == ConnectionType.Both) {
				m_tcp = new TransportTcp();
				m_tcp.StartServer(port);
				m_tcp.RegisterEventHandler(OnEventHandling);
			}
			// 도달 보장이 필요 없는 UDP 통신을 시작합니다.
			if (type == ConnectionType.UDP ||
			    type == ConnectionType.Both) {
				m_udp = new TransportUdp();
				m_udp.StartServer(port);
				m_udp.RegisterEventHandler(OnEventHandling);
			}
		}
		catch {
			Debug.Log("Network::StartServer fail.!");
			return false;
		}

		Debug.Log("Network::Server started.!");

		m_isServer = true;

		return LaunchThread();
	}

	public void StopServer()
	{
		m_isStarted = false;
		if (m_thread != null) {
			m_thread.Join();
			m_thread = null;
		}

		// 서버 실행을 정지.
		if (m_tcp != null) {
			m_tcp.StopServer();
		}
		
		if (m_udp != null) {
			m_udp.StopServer();
		}

		m_notifier.Clear();

		m_isServer = false;
		m_eventOccured = false;

		Debug.Log("Server stopped.");
	}

	// 
	public bool Connect(string address, int port, ConnectionType type)
	{
		try {
			Debug.Log("Addrss:" + address + " port:" + port + " type:" + type.ToString());

			bool ret = true;
			if (type == ConnectionType.TCP ||
			    type == ConnectionType.Both) {
				// 도달을 보장하는 TCP 통신을 시작합니다.
				if (m_tcp == null) {
					m_tcp = new TransportTcp();
					m_tcp.RegisterEventHandler(OnEventHandling);
				}
				ret &= m_tcp.Connect(address, port);
			}

			if (type == ConnectionType.UDP ||
			    type == ConnectionType.Both) {
				// 도달을 보장하지 않는 UDP 통신을 시작합니다. 
				if (m_udp == null) {
					m_udp = new TransportUdp();
					m_udp.RegisterEventHandler(OnEventHandling);
				}
				ret &= m_udp.Connect(address, port);
			}

			if (ret == false) {
				if (m_tcp != null) { m_tcp.Disconnect(); }
				if (m_udp != null) { m_udp.Disconnect(); }
				return false;
			}
		}
		catch {
			return false;
		}


		return LaunchThread();
	}

	public bool Disconnect()
	{	
		if (m_tcp != null) {
			m_tcp.Disconnect();
		}
		
		if (m_udp != null) {
			m_udp.Disconnect();
		}

		m_isStarted = false;
		m_eventOccured = false;

		return true;
	}
	
	public void RegisterReceiveNotification(PacketId id, RecvNotifier notifier)
	{
		int index = (int)id;

		if (m_notifier.ContainsKey(index)) {
			m_notifier.Remove(index);
		}

		m_notifier.Add(index, notifier);
	}

	public void UnregisterReceiveNotification(PacketId id)
	{
		int index = (int)id;

		if (m_notifier.ContainsKey(index)) {
			m_notifier.Remove(index);
		}
	}

	// 이벤트 통지 함수 등록.
	public void RegisterEventHandler(EventHandler handler)
	{
		m_handler += handler;
	}
	
	// 이벤트 통지 함수 삭제.
	public void UnregisterEventHandler(EventHandler handler)
	{
		m_handler -= handler;
	}


	// 접속 가능 상태에 있습니다.
	public bool IsConnected()
	{
		bool isTcpConnected = false;
		bool isUdpConnected = false;

		if (m_tcp != null && m_tcp.IsConnected()) {
			isTcpConnected = true;
		}
		
		if (m_udp != null && m_udp.IsConnected()) {
			isUdpConnected = true;
		}		
		
		if (m_tcp != null && m_udp == null) {
			return isTcpConnected;
		}

		if (m_tcp == null && m_udp != null) {
			return isUdpConnected;
		}

		return	(isTcpConnected && isUdpConnected);
	}

	// 서로 통신합니다.
	public bool IsCommunicating()
	{
		bool isTcpConnected = false;
		bool isUdpConnected = false;
		
		if (m_tcp != null && m_tcp.IsConnected()) {
			isTcpConnected = true;
		}
		
		if (m_udp != null && m_udp.IsCommunicating()) {
			isUdpConnected = true;
		}		
		
		if (m_tcp != null && m_udp == null) {
			return isTcpConnected;
		}
		
		if (m_tcp == null && m_udp != null) {
			return isUdpConnected;
		}
		
		return	(isTcpConnected && isUdpConnected);
	}

	//
	public bool IsServer()
	{
		return	m_isServer;
	}


	//
	bool LaunchThread()
	{
		Debug.Log("Launching thread.");	
		
		try {
			// Dispatch용 스레드 실행.
			m_thread = new Thread(new ThreadStart(Dispatch));
			m_thread.Start();
		}
		catch {
			Debug.Log("Cannot launch thread.");
			return false;
		}
		
		m_isStarted = true;	
		Debug.Log("Thread launched.");	
	
		return true;
	}

	// 
	void Dispatch()
	{
		while (m_isStarted == true) {

			// 클라이언트와의 수신을 처리합니다.
			if (m_tcp != null) {			
				// TCP의 송수신 처리.
				m_tcp.Dispatch();
			}

			if ( m_udp != null) {			
				// UDP의 송수신 처리.
				m_udp.Dispatch();
			}

			Thread.Sleep(5);
		}
	}
	
	public int Send(PacketId id, byte[] data)
	{
		int sendSize = 0;
		
		if (m_tcp != null) {
			// 모듈에서 사용할 헤더 정보를 생성합니다. 
			PacketHeader header = new PacketHeader();
			HeaderSerializer serializer = new HeaderSerializer();
			
			header.packetId = id;

			byte[] headerData = null;
			if (serializer.Serialize(header) == true) {
				headerData = serializer.GetSerializedData();
			}
			
			byte[] packetData = new byte[headerData.Length + data.Length];
			
			int headerSize = Marshal.SizeOf(typeof(PacketHeader));
			Buffer.BlockCopy(headerData, 0, packetData, 0, headerSize);
			Buffer.BlockCopy(data, 0, packetData, headerSize, data.Length);
			
			sendSize = m_tcp.Send(data, data.Length);
		}
		
		return sendSize;
	}	


	public int SendReliable<T>(IPacket<T> packet)
	{
		int sendSize = 0;
		
		if (m_tcp != null) {
			// 모듈에서 사용할 헤더 정보를 생성합니다.
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
			
			string str = "Send reliable packet[" +  header.packetId  + "]";
			
			sendSize = m_tcp.Send(data, data.Length);
		}
		
		return sendSize;
	}

	
	public void SendReliableToAll<T>(IPacket<T> packet)
	{
		byte[] data = packet.GetData();

		int sendSize = m_tcp.Send(data, data.Length);
		if (sendSize < 0) {
			// 송신 오류.
		}
	}
	
	public void SendReliableToAll(PacketId id, byte[] data)
	{
		if (m_tcp != null) {
			// 모듈에서 사용할 헤더 정보를 생성합니다.
			PacketHeader header = new PacketHeader();
			HeaderSerializer serializer = new HeaderSerializer();
			
			header.packetId = id;
			
			byte[] headerData = null;
			if (serializer.Serialize(header) == true) {
				headerData = serializer.GetSerializedData();
			}

			byte[] pdata = new byte[headerData.Length + data.Length];
			
			int headerSize = Marshal.SizeOf(typeof(PacketHeader));
			Buffer.BlockCopy(headerData, 0, pdata, 0, headerSize);
			Buffer.BlockCopy(data, 0, pdata, headerSize, data.Length);
			
			string str = "Send reliable packet[" +  header.packetId  + "]";

			int sendSize = m_tcp.Send(pdata, pdata.Length);
			if (sendSize < 0) {
				// 송신 오류.
			}
		}
	}

	public int SendUnreliable<T>(IPacket<T> packet)
	{
		int sendSize = 0;
		
		if (m_udp != null) {
			// 모듈에서 사용할 헤더 정보를 생성합니다. 
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
			
			sendSize = m_udp.Send(data, data.Length);
		}
		
		return sendSize;
	}

	public void ReceivePacket(byte[] data)
	{
		PacketHeader header = new PacketHeader();
		HeaderSerializer serializer = new HeaderSerializer();
		
		bool ret = serializer.Deserialize(data, ref header);
		if (ret == false) {
			// 패킷으로서 인식할 수 없으므로 폐기합니다.
			return;			
		}

		if (m_udp != null && m_udp.IsCommunicating() == true) {
			// 통신상대로부터 첫 수신 시에 상호 통신할 수 있도록 접속 통지를 합니다.
			if (m_handler != null && m_eventOccured == false) {
				NetEventState state = new NetEventState();
				state.type = NetEventType.Connect;
				state.result = NetEventResult.Success;
				state.endPoint = m_udp.GetRemoteEndPoint();
				Debug.Log("Event handler call.");
				m_handler(state);
				m_eventOccured = true;
			}
		}

		int packetId = (int)header.packetId;
		if (m_notifier.ContainsKey(packetId) &&
			m_notifier[packetId] != null) {
			int headerSize = Marshal.SizeOf(typeof(PacketHeader));//sizeof(PacketId) + sizeof(int);
			byte[] packetData = new byte[data.Length - headerSize];
			Buffer.BlockCopy(data, headerSize, packetData, 0, packetData.Length);
	
			m_notifier[packetId]((PacketId)packetId, packetData);
		}
	}

	public bool StartGameServer()
	{
		Debug.Log("GameServer called.");

		GameObject obj = new GameObject("GameServer");
		GameServer server = obj.AddComponent<GameServer>();
		if (server == null) {
			Debug.Log("GameServer failed start.");
			return false;
		}

		server.StartServer();
		DontDestroyOnLoad(server);
		Debug.Log("GameServer started.");
		
		return true;
	}
	
	public void StopGameServer()
	{
		GameObject obj = GameObject.Find("GameServer");
		if (obj) {
			GameObject.Destroy(obj);
		}

		Debug.Log("GameServer stoped.");
	}


	public void OnEventHandling(NetEventState state)
	{
		if (m_handler != null) {
			m_handler(state);
		}
	}
}
