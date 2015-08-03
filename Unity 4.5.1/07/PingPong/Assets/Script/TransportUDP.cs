using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class TransportUDP : MonoBehaviour {

	private Socket			m_socket = null;

	private Thread			m_thread = null;

	private	bool			m_isStarted = false;

	// 서버. 
	private bool 			m_isServer = false;
	
	// 접속. 
	private	bool			m_isConnected = false;

	// 접속할 주소 정보.
	private IPEndPoint		m_remoteEndPoint;

	// 송신 버퍼.
	private PacketQueue		m_sendQueue;
	
	// 수신 버퍼.
	private PacketQueue		m_recvQueue;
	

	// 송수신용 패킷의 최대 크기.
	private const int		m_packetSize = 1400;


	// 타임아웃 시간.
	private const int 		m_timeOutSec = 5;

	private DateTime		m_ticker;

	
	// 이벤트 통지 델리게이트.
	public delegate void 	EventHandler(NetEventState state);
	// 이벤트 핸들러.
	private EventHandler	m_handler;


	// Use this for initialization
	void Start()
	{
		// 송수신 버퍼를 작성합니다.
		m_sendQueue = new PacketQueue();
		m_recvQueue = new PacketQueue();
	}
	
	// Update is called once per frame
	void FixedUpdate()
	{
	
	}

	void OnApplicationQuit()
	{
		if (m_isStarted == true) {
			StopServer();
		}
	}

	public bool StartServer(int port)
	{
		Debug.Log("Start server called[Port:" + port + "]");
		
		// 리스닝 소켓을 생성합니다.
		try {
			if (m_socket == null) {
				m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			}
			m_socket.Bind(new IPEndPoint(IPAddress.Any, port));
		}
		catch {
			return false;
		}

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

		Disconnect ();

		if (m_socket != null) {
			m_socket.Close();
			m_socket = null;
		}		

		m_isServer = false;
		m_isStarted = false;

		Debug.Log("Server stopped.");
	}

	// 
	public bool Connect(string address, int port)
	{
		Debug.Log("TransportUdp::Connect called.[Port:" + port + "]");

		try {
			if (m_socket == null) {
				m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			}
			m_socket.Connect(address, port);
		}
		catch {
			Debug.Log("TransportUdp::Connect failed.");
			return false;
		}

		bool ret = true;
		if (m_isStarted == false) {
			ret = LaunchThread();
			if (ret == true) {
				m_isConnected = true;
			}
		}

		// 접속을 알립니다.
		if (m_handler != null) {
			NetEventState state = new NetEventState();
			state.type = NetEventType.Connect;
			state.result = NetEventResult.Success;
			m_handler(state);
		}

		Debug.Log("TransportUdp::Connect success.");

		return ret;
	}

	public bool Disconnect()
	{
		if (m_socket != null) {
			// 소켓 닫기.
			m_socket.Close();
			m_socket = null;
				
			// 접속 끊김을 알립니다.
			if (m_handler != null) {
				NetEventState state = new NetEventState();
				state.type = NetEventType.Disconnect;
				state.result = NetEventResult.Success;
				m_handler(state);
			}
		}

		m_isStarted = false;
		m_isConnected = false;
		Debug.Log("TransportTcp::Disconnect called.");

		return true;
	}

	//
	public int Send(byte[] data, int size)
	{
		return m_sendQueue.Enqueue(data, size);
	}
	
	//
	public int Receive(ref byte[] buffer, int size) 
	{
		return m_recvQueue.Dequeue(ref buffer, size);
	}

	//
	public bool IsConnected()
	{
		return	m_isConnected;
	}

	//
	public bool IsServer()
	{
		return	m_isServer;
	}

	public IPEndPoint GetRemoteEndPoint()
	{
		return m_remoteEndPoint;
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

	//
	bool LaunchThread()
	{
		try {
			// Dispatch용 스레드 시작.
			m_thread = new Thread(new ThreadStart(Dispatch));
			m_thread.Start();
		}
		catch {
			Debug.Log("Cannot launch thread.");
			return false;
		}
		
		m_isStarted = true;	
	
		return true;
	}

	// 
	void Dispatch()
	{
		while (m_isStarted == true) {
			// 클라이언트의 접속을 기다립니다.
			AcceptClient();

			// 클라이언트와의 송수신을 처리합니다..
			if (m_socket != null) {			
				// 송신 처리.
				DispatchSend();
				
				// 수신 처리.
				DispatchReceive();

				// 타임아웃 처리.
				CheckTimeout();
			}

			Thread.Sleep(3);
		}
	}

	void AcceptClient()
	{
		if (m_isConnected == false &&
			m_socket != null && 
		    m_socket.Poll(0, SelectMode.SelectRead)) {
			// 클라이언트로부터 접속되었습니다.
			m_isConnected = true;
			// 통신 시작 시각을 기록.
			m_ticker = DateTime.Now;

			// 접속을 알립니다.
			if (m_handler != null) {
				NetEventState state = new NetEventState();
				state.type = NetEventType.Connect;
				state.result = NetEventResult.Success;
				m_handler(state);
			}
		}
	}

	void DispatchSend()
	{
		if (m_socket == null) {
			return;
		}

		try {
			// 송신 처리.
			if (m_socket.Poll(0, SelectMode.SelectWrite)) {
				byte[] buffer = new byte[m_packetSize];
				
				int sendSize = m_sendQueue.Dequeue(ref buffer, buffer.Length);
				while (sendSize > 0) {
					int res = m_socket.Send(buffer, sendSize, SocketFlags.None);	
					sendSize = m_sendQueue.Dequeue(ref buffer, buffer.Length);
					//Debug.Log("Send udp data." + res);
				}
			}
		}
		catch {
			return;
		}
	}

	void DispatchReceive()
	{
		if (m_socket == null) {
			return;
		}

		// 수신 처리.
		try {
			while (m_socket.Poll(0, SelectMode.SelectRead)) {
				byte[] buffer = new byte[m_packetSize];

				int recvSize = m_socket.Receive(buffer, buffer.Length, SocketFlags.None);

				//Debug.Log("Recv udp data." + recvSize);

				if (recvSize == 0) {
					// 끊기.
					Disconnect();
				}
				else if (recvSize > 0) {
					m_recvQueue.Enqueue(buffer, recvSize);
					// 수신 시각을 갱신.
					m_ticker = DateTime.Now;
				}
			}
		}
		catch {
			return;
		}
	}

	void CheckTimeout()
	{
		TimeSpan ts = DateTime.Now - m_ticker;

		if (m_isConnected && ts.Seconds > m_timeOutSec) {
			Debug.Log("Disconnect because of timeout.");
			// 타임아웃될 때까지 데이터가 도달하지 않았습니다.
			// 이해를 간단히 하기 위해, 굳이 통신 스레드에서 메인 스레드를 호출했습니다.
			// 원래라면 접속 종료 요청을 발행하고 메인 스레드 쪽에서 요청을 감시하다가. 
			// 메인 스레드 쪽 처리에서 접속을 끊게 합니다.
			Disconnect();
		}
	}
}
