using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;

public class TransportUDP : ITransport
{

	private	int				m_nodeId = -1;

	private Socket			m_socket = null;
	
	private IPEndPoint		m_localEndPoint = null;

	private IPEndPoint		m_remoteEndPoint = null;
	// 송신 버퍼.
	private PacketQueue		m_sendQueue = new PacketQueue();
	
	// 수신 버퍼.
	private PacketQueue		m_recvQueue = new PacketQueue();

	// 송신 패킷 크기.
	private int				m_packetSize = 1400;
	
	// 접속 플래그.
	private	bool			m_isRequested = false;
	
	// 수신 플래그.
	private	bool			m_isConnected = false;

	// 타임아웃 시간.
	private const int 		m_timeOutSec = 10;
	
	// 타임아웃 티커.
	private DateTime		m_timeOutTicker;
	
	// 킵 얼라이브 간격.
	private const int		m_keepAliveInter = 2; 

	// 킵 얼라이브 티커.
	private DateTime		m_keepAliveTicker;
	
	// 접속 확인용 더미 패킷 데이터.
	public const string 	m_requestData = "KeepAlive.";
	
	// 이벤트 핸들러.
	private EventHandler	m_handler;

	
	// 같은 단말 실행 시 판별용으로 리스닝 소켓의 포트 번호를 보존.
	private int				m_serverPort = -1;
	

	public TransportUDP()
	{

	}

	public TransportUDP(Socket socket) 
	{
		m_socket = socket;
	}

	public bool Initialize(Socket socket)
	{
		m_socket = socket;
		m_isRequested = true;
		
		return true;
	}

	public bool Terminate()
	{
		m_socket = null;
		
		return true;
	}
	
	public int GetNodeId()
	{
		return m_nodeId;
	}
	
	public void SetNodeId(int node)
	{
		m_nodeId = node;
	}
	
	public IPEndPoint GetLocalEndPoint()
	{
		return m_localEndPoint;
	}

	public IPEndPoint GetRemoteEndPoint()
	{
		return m_remoteEndPoint;
	}

	public void SetServerPort(int port)
	{
		m_serverPort = port;
	}
	
	//
	public bool Connect(string ipAddress, int port)
	{
		if (m_socket == null) {
			m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			Debug.Log("Create new socket.");
		}

		try {			
			m_localEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
			m_isRequested = true;
			Debug.Log("Connection success");
		}
		catch {
			m_isRequested = false;
			Debug.Log("Connect fail");
		}

		string str = "TransportUDP connect:" + m_isRequested.ToString(); 
		Debug.Log(str);
		if (m_handler != null) {
			// 접속 결과를 알립니다.
			NetEventState state = new NetEventState();
			state.type = NetEventType.Connect;
			state.result = (m_isRequested == true)? NetEventResult.Success : NetEventResult.Failure;
			m_handler(this, state);
			Debug.Log("event handler called");
		}

		return m_isRequested;
	}
	
	// 
	public void Disconnect() 
	{
		m_isRequested = false;

		if (m_socket != null) {
			// 소켓 닫기.
			m_socket.Shutdown(SocketShutdown.Both);
			m_socket.Close();
			m_socket = null;
		}
		
		// 끊어졌음을 알립니다.
		if (m_handler != null) {
			NetEventState state = new NetEventState();
			state.type = NetEventType.Disconnect;
			state.result = NetEventResult.Success;
			m_handler(this, state);
		}
	}
	
	//
	public int Send(byte[] data, int size)
	{
		if (m_sendQueue == null) {
			return 0;
		}

		return m_sendQueue.Enqueue(data, size);
	}
	
	//
	public int Receive(ref byte[] buffer, int size) 
	{
		if (m_recvQueue == null) {
			return 0;
		}

		return m_recvQueue.Dequeue(ref buffer, size);
	}

	public void RegisterEventHandler(EventHandler handler)
	{
		m_handler += handler;
	}

	public void UnregisterEventHandler(EventHandler handler)
	{
		m_handler -= handler;
	}
	
	// 접속 요청을 했다.
	public bool IsRequested()
	{
		return	m_isRequested;
	}
	
	// 접속됐다.
	public bool IsConnected()
	{
		return	m_isConnected;
	}

	// 
	public void Dispatch()
	{
		// 송신 처리.
		DispatchSend();

		// 타임아웃 처리.
		CheckTimeout();

		// 킵 얼라이브.
		if (m_socket != null) {
			// 통신 상대에게 접속했다는 사실을 정기적으로 알립니다.
			TimeSpan ts = DateTime.Now - m_keepAliveTicker;
			
			if (ts.Seconds > m_keepAliveInter) {
				// UDP 접속에 관해 샘플 코드에서는 핸드쉐이크 하지 않으므로.
				// 같은 단말에서 실행할 때는 포트 번호로 송신원을 판별해야만 합니다.
				// 이 때문에 접속 트리거가 되는 킵 얼라이브 패킷에 IP 주소와.
				// 포트 번호를 실어 판별할 수 있게 합니다. 
				string message = m_localEndPoint.Address.ToString() + ":" + m_serverPort + ":" + m_requestData;
				byte[] request = System.Text.Encoding.UTF8.GetBytes(message);
				m_socket.SendTo(request, request.Length, SocketFlags.None, m_localEndPoint);	
				m_keepAliveTicker = DateTime.Now;
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
					m_socket.SendTo(buffer, sendSize, SocketFlags.None, m_localEndPoint);	
					sendSize = m_sendQueue.Dequeue(ref buffer, buffer.Length);
				}
			}
		}
		catch {
			return;
		}
	}
	
	public void SetReceiveData(byte[] data, int size, IPEndPoint endPoint)
	{	
		string str = System.Text.Encoding.UTF8.GetString(data).Trim('\0');
		if (str.Contains(m_requestData)) {
			// 접속 요청 패킷 수신.
			if (m_isConnected == false && m_handler != null) {
				NetEventState state = new NetEventState();
				state.type = NetEventType.Connect;
				state.result = NetEventResult.Success;
				m_handler(this, state);

				//IPEndPoint ep = m_localEndPoint;
				//Debug.Log("[UDP]Connected from client. [address:" + ep.Address.ToString() + " port:" + ep.Port + "]");
			}

			m_isConnected = true;
			m_remoteEndPoint = endPoint;
			m_timeOutTicker = DateTime.Now;
		}
		else if (size > 0) {
			m_recvQueue.Enqueue(data, size);
		}
	}

	void CheckTimeout()
	{
		TimeSpan ts = DateTime.Now - m_timeOutTicker;
		
		if (m_isRequested && m_isConnected && ts.Seconds > m_timeOutSec) {
			Debug.Log("Disconnect because of timeout.");
			// 타임아웃할 시간까지 데이터가 도달되지 않았습니다.
			// 이해를 간단히 하기 위해 일부러 통신 스레드로부터 메인 스레드를 호출하고 있습니다.
			// 원래라면 절단 요청을 발행하여 메인 스레드 쪽에서 요청을 감시해서.
			// 메인 스레드 쪽 처리에서 끊기로 합시다.
			Disconnect();
		}
	}
}

