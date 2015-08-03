using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


public class TransportTcp {

	// 리스닝 소켓.
	private Socket			m_listener = null;
	
	// 통신용 소켓.
	private List<Socket>	m_socket = null;
	private int				m_port = -1;

	// 서버 플래그.
	private bool 			m_isServer = false;
	
	// 접속 플래그.
	private	bool			m_isConnected = false;

	// 송신 버퍼.
	private PacketQueue		m_sendQueue = new PacketQueue();
	
	// 수신 버퍼.
	private PacketQueue		m_recvQueue = new PacketQueue();
	
	// 이벤트 통지 델리게이트.
	public delegate void 	EventHandler(NetEventState state);
	// 이벤트 핸들러.
	private EventHandler	m_handler;
	

	// 송수신용 패킷의 최대 크기.
	private const int		m_packetSize = 1400;

	private System.Object 	lockObj = new System.Object();

	public TransportTcp()
	{
		// 클라이언트와의 접속용 소켓 리스트 생성.
		m_socket = new List<Socket>();
	}

	public bool StartServer(int port)
	{
		Debug.Log("TransportTcp::StartServer called. port:" + port);

		try {
			// 리스닝 소켓을 생성합니다.
			m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			m_listener.NoDelay = true;
			m_listener.Bind(new IPEndPoint(IPAddress.Any, port));
			m_listener.Listen(1);
			m_port = port;
		}
		catch {
			return false;
		}

		m_isServer = true;

		return true;
	}

	public void StopServer()
	{
		Debug.Log("TransportTcp::StopServer called.");

		Disconnect ();

		if (m_listener != null) {
			m_listener.Close();
			m_listener = null;
		}		

		m_isServer = false;
	}

	// 
	public bool Connect(string address, int port)
	{
		try {
			lock (lockObj) {
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.NoDelay = true;
				socket.Connect(address, port);
				m_socket.Add(socket);
			}
		}
		catch {
			return false;
		}

		m_isConnected = true;
		Debug.Log("TransportTcp::Connect called.");

		return true;
	}

	public bool Disconnect()
	{
		if (m_socket != null) {
			lock (lockObj) {
				// 소켓 닫기.
				foreach (Socket socket in m_socket) {
					socket.Shutdown(SocketShutdown.Both);
					socket.Close();
				}
				m_socket.Clear();
				m_socket = null;
			}
			
			// 접속 종료를 알립니다.
			if (m_handler != null) {
				NetEventState state = new NetEventState();
				state.type = NetEventType.Disconnect;
				state.result = NetEventResult.Success;
				m_handler(state);
			}
		}

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
	public void Dispatch()
	{
		// 클라이언트의 접속을 기다립니다..
		AcceptClient();

		// 클라이언트와의 송수신을 처리합니다.
		if (m_isConnected == true && m_socket != null) {
			lock (lockObj) {
				// 송신 처리.
				DispatchSend();
				
				// 수신 처리.
				DispatchReceive();
			}
		}
	}

	void AcceptClient()
	{
		Console.WriteLine("AcceptClient.");
		if (m_listener != null && m_listener.Poll(0, SelectMode.SelectRead)) {
			// 클라이언가 접속했습니다.
			Socket socket = m_listener.Accept();
			m_socket.Add(socket);
			m_isConnected = true;
			if (m_handler != null) {
				NetEventState state = new NetEventState();
				state.type = NetEventType.Connect;
				state.result = NetEventResult.Success;
				m_handler(state);
			}
			Debug.Log("Connected from client. [port:" + m_port + "]");
		}
	}

	void DispatchSend()
	{
		if (m_socket == null) {
			return;
		}

		try {
			// 송신 처리.
			//if (m_socket.Poll(0, SelectMode.SelectWrite)) {
				byte[] buffer = new byte[m_packetSize];
				
				int sendSize = m_sendQueue.Dequeue(ref buffer, buffer.Length);
				while (sendSize > 0) {
					foreach (Socket socket in m_socket) {
						socket.Send(buffer, sendSize, SocketFlags.None);	
					}
					sendSize = m_sendQueue.Dequeue(ref buffer, buffer.Length);
				}
			//}
		}
		catch {
			if (m_handler != null) {
				NetEventState state = new NetEventState();
				state.type = NetEventType.SendError;
				state.result = NetEventResult.Failure;
				m_handler(state);
			}
		}
	}

	void DispatchReceive()
	{
		if (m_socket == null) {
			return;
		}

		// 수신 처리.
		try {
			foreach (Socket socket in m_socket) {
				if (socket.Poll(0, SelectMode.SelectRead)) {
					byte[] buffer = new byte[m_packetSize];

					int recvSize = socket.Receive(buffer, buffer.Length, SocketFlags.None);

					Debug.Log("TransportTcp Receive data [size:" + recvSize + "][port:" + m_port +"]");
					if (recvSize == 0) {
						// 끊기.
						Disconnect();
					}
					else if (recvSize > 0) {
						m_recvQueue.Enqueue(buffer, recvSize);
					}
				}
			}
		}
		catch {
			if (m_handler != null) {
				NetEventState state = new NetEventState();
				state.type = NetEventType.ReceiveError;
				state.result = NetEventResult.Failure;
				m_handler(state);
			}
		}
	}
}
