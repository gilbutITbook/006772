using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;


public class TransportTCP : ITransport
{

	private	int				m_nodeId = -1;

	// 통신용 소켓.
	private Socket			m_socket = null;

	// 접속할 포트 번호.
	private int				m_port = -1;

	// 접속 플래그.
	private	bool			m_isConnected = false;

	// 송신 버퍼.
	private PacketQueue		m_sendQueue = new PacketQueue();
	
	// 수신 버퍼.
	private PacketQueue		m_recvQueue = new PacketQueue();
	
	// 통지 델리게이트.
	private EventHandler	m_handler;
	
	// 송수신용 패킷의 최대 크기.
	private const int		m_packetSize = 1400;


	// 같은 단말 실행 시 판별용으로 리스닝 소켓의 포트 번호를 보존.
	private int				m_serverPort = -1;


	public string	transportName = "";

	public TransportTCP()
	{
	}

	public TransportTCP(Socket socket, string name)
	{
		m_socket = socket;
		transportName = name;
	}

	public bool Initialize(Socket socket)
	{
		m_socket = socket;
		m_isConnected = true;

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
		if (m_socket == null) {
			return default(IPEndPoint);
		}
		
		return m_socket.LocalEndPoint as IPEndPoint;
	}

	public IPEndPoint GetRemoteEndPoint()
	{
		if (m_socket == null) {
			return default(IPEndPoint);
		}
		
		return m_socket.RemoteEndPoint as IPEndPoint;
	}

	public void SetServerPort(int port)
	{
		m_serverPort = port;
	}

	// 
	public bool Connect(string address, int port)
	{
		Debug.Log("Transport connect called");

		if (m_socket != null) {
			return false;
		}

		try {
			m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			m_socket.Connect(address, port);
			m_socket.NoDelay = true;
			m_port = port;
			m_isConnected = true;
			Debug.Log("Connection success");
		}
		catch (SocketException e) {
			m_socket = null;
			m_isConnected = false;
			Debug.Log("Connect fail");
			Debug.Log(e.ToString());
		}

		string str = "TransportTCP connect:" + m_isConnected.ToString(); 
		Debug.Log(str);
		if (m_handler != null) {
			// 접속 결과를 통지합니다.
			NetEventState state = new NetEventState();
			state.type = NetEventType.Connect;
			state.result = (m_isConnected == true)? NetEventResult.Success : NetEventResult.Failure;
			m_handler(this, state);
			Debug.Log("event handler called");
		}

		return m_isConnected;
	}

	public void Disconnect()
	{
		m_isConnected = false;

		if (m_socket != null) {
			// 소켓 닫기.
			m_socket.Shutdown(SocketShutdown.Both);
			m_socket.Close();
			m_socket = null;
		}

		// 절단을 통지합니다. 
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
		return m_sendQueue.Enqueue(data, size);
	}
	
	//
	public int Receive(ref byte[] buffer, int size) 
	{
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

	// 
	public void Dispatch()
	{
		// 클라이언트와의 송수신을 처리합니다.
		if (m_isConnected == true && m_socket != null) {

			// 송신 처리.
			DispatchSend();
			
			// 수신 처리.
			DispatchReceive();
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
					m_socket.Send(buffer, sendSize, SocketFlags.None);	
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
		if (m_socket == null) {
			return;
		}

		// 수신 처리.
		try {
			while (m_socket.Poll(0, SelectMode.SelectRead)) {
				byte[] buffer = new byte[m_packetSize];

				int recvSize = m_socket.Receive(buffer, buffer.Length, SocketFlags.None);
				
				if (recvSize == 0) {
					// 끊기.
					Debug.Log("[TCP]Disconnect recv from other.");
					Disconnect();
				}
				else if (recvSize > 0) {
					//Debug.Log("[TCP]DispatchReceive received [Port:" + m_port + "]");				
					m_recvQueue.Enqueue(buffer, recvSize);
				}
			}
		}
		catch {
			return;
		}
	}


	public void SetReceiveData(byte[] data, int size)
	{	
		// 수신 데이터를 버퍼에 추가.
		if (size > 0) {
			//			Debug.Log("DispatchReceive received");				
			m_recvQueue.Enqueue(data, size);
		}
	}

	//
	public bool IsConnected()
	{
		return	m_isConnected;
	}

}
