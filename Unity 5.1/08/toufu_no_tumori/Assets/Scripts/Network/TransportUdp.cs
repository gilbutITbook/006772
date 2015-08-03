using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;

public class TransportUdp {

	// 리스닝 소켓.
	// UDP에서는 m_socket을 사용할 수 있지만 편의상 
	// 리스닝 소켓을 다른 소켓을 사용합니다. 
	// 다음 장에서 TransportUDP 클래스 확장을 위해 나우었습니다.
	private Socket			m_listener = null;

	// 통신용 소켓.
	private Socket			m_socket = null;
	private int				m_port = -1;

	private IPEndPoint		m_endPoint;// = null;

	private IPEndPoint		m_remoteEndPoint;

	// 송신 버퍼.
	private PacketQueue		m_sendQueue = new PacketQueue();
	
	// 수신 버퍼.
	private PacketQueue		m_recvQueue = new PacketQueue();

	// 접속 플래그.
	private	bool			m_icConnected = false;

	// 수신 플래그.
	private	bool			m_isCommunicating = false;
	
	// 송수신용 패킷의 최대 크기.
	private const int		m_packetSize = 1400;
	
	
	// 타임아웃 시간.
	private const int 		m_timeOutSec = 60 * 3;
	
	// 타임아웃 티커.
	private DateTime		m_timeOutTicker;

	// 킵얼라이브 간격.
	private const int		m_keepAliveInter = 10; 

	// 킵 얼라이브 티커.
	private DateTime		m_keepAliveTicker;

	// 이벤트 알림 델리게이트.
	public delegate void 	EventHandler(NetEventState state);
	// 이벤트 핸들러.
	private EventHandler	m_handler;

	// 접속 확인용 더미 패킷 데이터.
	private const string 	m_requestData = "KeepAlive.";

	// 대기 시작.
	public bool StartServer(int port)
	{		
		Debug.Log("TransportUdp::StartServer called. port:" + port);
		
		// 리스닝 소켓을 생성합니다.
		try {
			m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			m_listener.Bind(new IPEndPoint(IPAddress.Any, port));
			m_port = port;
			m_isCommunicating = false;
		}
		catch {
			return false;
		}

		return true;
	}
	
	// 대기 종료.
	public void StopServer()
	{
		Disconnect ();

		if (m_listener != null) {
			// 리스너로서 사용한 소켓을 닫습니다.
			m_listener.Close();
			m_listener = null;
		}		

		m_isCommunicating = false;
		Debug.Log("TransportUdp::StopServer called.");
	}	
	
	// 접속 요청 처리.
	public bool Connect(string ipAddress, int port)
	{
		try {
			IPAddress addr = IPAddress.Parse(ipAddress);
			
			m_endPoint = new IPEndPoint(addr, port);
			m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}
		catch {
			return false;
		}

		// 접속을 알립니다.
		if (m_handler != null) {
			NetEventState state = new NetEventState();
			state.type = NetEventType.Connect;
			state.result = NetEventResult.Success;
			m_handler(state);
		}

		m_icConnected = true;
		Debug.Log("TransportUdp::Connect called.");

		return true;
	}
	
	// 접속 종료 요청 처리.
	public bool Disconnect() 
	{
		if (m_socket != null) {
			// 소켓 닫기.			
			m_socket.Close();
			m_socket = null;

			// 접속 종료를 알립니다.
			if (m_handler != null) {
				NetEventState state = new NetEventState();
				state.type = NetEventType.Disconnect;
				state.result = NetEventResult.Success;
				m_handler(state);
			}
		}

		m_icConnected = false;
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
	
	// 접속 요청을 했다..
	public bool IsConnected()
	{
		return	m_icConnected;
	}

	// 접속했다.
	public bool IsCommunicating()
	{
		return	m_isCommunicating;
	}

	// 접속할 엔드포인트 취득.
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

	// 송수신 처리.
	public void Dispatch()
	{
		// 클라이언트로의 접속을 기다립니다.
		AcceptClient();

		// 클라이언트와의 송수신을 처리합니다.クライアントとの送受信を処理します.
		if (m_socket != null || m_listener != null) {				
			// 송신 처리.
			DispatchSend();
			// 수신 처리.
			DispatchReceive();
			// 타임 아웃 처리.
			CheckTimeout();
		}

		// 킵 얼라이브.
		if (m_socket != null) {
			// 통신 상대에게 접속을 시작했음을 정기적으로 알립니다.
			TimeSpan ts = DateTime.Now - m_keepAliveTicker;
			
			if (ts.Seconds > m_keepAliveInter) {
				byte[] request = System.Text.Encoding.UTF8.GetBytes(m_requestData);
				m_socket.SendTo(request, request.Length, SocketFlags.None, m_endPoint);	
				m_keepAliveTicker = DateTime.Now;
			}
		}
	}

	void AcceptClient()
	{
		if (m_isCommunicating == false &&
		    m_listener != null && m_listener.Poll(0, SelectMode.SelectRead)) {
			// 클라이언트가 접속했습니다.
			m_isCommunicating = true;
			// 통신 시작 시각을 기록.
			m_timeOutTicker = DateTime.Now;
			
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
					m_socket.SendTo(buffer, sendSize, SocketFlags.None, m_endPoint);	
					sendSize = m_sendQueue.Dequeue(ref buffer, buffer.Length);
				}
			}
		}
		catch {
			return;
		}
	}

	void DispatchReceive()
	{
		if (m_listener == null) {
			return;
		}

		// 수신 처리..
		try {
			while (m_listener.Poll(0, SelectMode.SelectRead)) {
				byte[] buffer = new byte[m_packetSize];
				IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
				EndPoint remoteEp = (EndPoint)sender;
				
				int recvSize = m_listener.ReceiveFrom(buffer, SocketFlags.None, ref remoteEp);

				m_remoteEndPoint = (IPEndPoint)remoteEp;
				if (m_endPoint == null) {
					m_endPoint = m_remoteEndPoint;
				}

				//Debug.Log("remote:" + m_remoteEndPoint.Address.ToString());

				string str = System.Text.Encoding.UTF8.GetString(buffer);
				if (m_requestData.CompareTo(str.Trim('\0')) == 0) {
					// 접속 요청 패킷.
					;
				}
				else if (recvSize == 0) {
					// 접속 종료.
					Disconnect();
				}
				else if (recvSize > 0) {
					// 데이터를 수신.
					m_recvQueue.Enqueue(buffer, recvSize);
					// 수신 시각 갱신.
					m_timeOutTicker = DateTime.Now;
				}
			}
		}
		catch {
			return;
		}
	}
	
	void CheckTimeout()
	{
		TimeSpan ts = DateTime.Now - m_timeOutTicker;
		
		if (m_icConnected && m_isCommunicating && ts.Seconds > m_timeOutSec) {
			Debug.Log("Disconnect because of timeout.");
			// 타임아웃 시간까지 데이터가 도달하지 않았습니다.
			// 이해를 간단히 하기 위해 굳이 통신 스레드에서 메인 스레드를 호출하고 있습니다.
			// 원래는 접속 종료 요청을 발행하고 메인 스레드 쪽에서 요청을 감시하다가.
			// 메인 스레드 쪽 처리에서 접속을 끊게 합니다.
			Disconnect();
		}
	}
}

