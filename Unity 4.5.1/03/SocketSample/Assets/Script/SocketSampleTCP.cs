using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;

public class SocketSampleTCP : MonoBehaviour
{
	
	// 접속할 곳의 IP주소.
	private string			m_address = "";
	
	// 접속할 곳의 포트 번호.
	private const int 		m_port = 50765;

	// 리스닝 소켓.
	private Socket			m_listener = null;

	// 통신용 변수.
	private Socket			m_socket = null;

	// 상태. 
	private State			m_state;

	// 상태정의. 
	private enum State
	{
		SelectHost = 0,
		StartListener,
		AcceptClient,
		ServerCommunication,
		StopListener,
		ClientCommunication,
		Endcommunication,
	}


	// Use this for initialization
	void Start ()
	{
		m_state = State.SelectHost;

		IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
		System.Net.IPAddress hostAddress = hostEntry.AddressList[0];
		Debug.Log(hostEntry.HostName);
		m_address = hostAddress.ToString();
	}
	
	// Update is called once per frame
	void Update ()
	{
		switch (m_state) {
		case State.StartListener:
			StartListener();
			break;

		case State.AcceptClient:
			AcceptClient();
			break;

		case State.ServerCommunication:
			ServerCommunication();
			break;

		case State.StopListener:
			StopListener();
			break;

		case State.ClientCommunication:
			ClientProcess();
			break;

		default:
			break;
		}
	}

	// 대기 시작.
	void StartListener()
	{
		Debug.Log("Start server communication.");
		
		// 소켓을 생성합니다. .
		m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		// 사용할 포트 번호를 할당합니다.
		m_listener.Bind(new IPEndPoint(IPAddress.Any, m_port));
		// 대기를 시작합니다. 
		m_listener.Listen(1);

		m_state = State.AcceptClient;
	}

	// 클라이언트의 접속 대기.
	void AcceptClient()
	{
		if (m_listener != null && m_listener.Poll(0, SelectMode.SelectRead)) {
			// 클라이언트가 접속했습니다.
			m_socket = m_listener.Accept();
			Debug.Log("[TCP]Connected from client.");
			m_state = State.ServerCommunication;
		}
	}

	// 클라이언트의 메시지 수신.
	void ServerCommunication()
	{
		byte[] buffer = new byte[1400];
		int recvSize = m_socket.Receive(buffer, buffer.Length, SocketFlags.None);
		if (recvSize > 0) {
			string message = System.Text.Encoding.UTF8.GetString(buffer);
			Debug.Log(message);
			m_state = State.StopListener;
		}
	}

	// 대기 종료.
	void StopListener()
	{	
		// 대기를 종료합니다.
		if (m_listener != null) {
			m_listener.Close();
			m_listener = null;
		}

		m_state = State.Endcommunication;

		Debug.Log("[TCP]End server communication.");
	}

	// 클라이언트와의 접속, 송신, 접속해제.
	void ClientProcess()
	{
		Debug.Log("[TCP]Start client communication.");

		// 서버에 접속.
		m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		m_socket.NoDelay = true;
		m_socket.SendBufferSize = 0;
		m_socket.Connect(m_address, m_port);

		// 메시지 송신.
		byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Hello, this is client.");	
		m_socket.Send(buffer, buffer.Length, SocketFlags.None);

		// 접속 해제. 
		m_socket.Shutdown(SocketShutdown.Both);
		m_socket.Close();

		Debug.Log("[TCP]End client communication.");
	}

	void OnGUI()
	{
		if (m_state == State.SelectHost) {
			OnGUISelectHost();
		}
	}

	void OnGUISelectHost()
	{
		if (GUI.Button (new Rect (20,40, 150,20), "Launch server.")) {
			m_state = State.StartListener;
		}
		
		// 클라이언트를 선택했을 때의 접속할 서버 주소를 입력합니다. 
		m_address = GUI.TextField(new Rect(20, 100, 200, 20), m_address);
		if (GUI.Button (new Rect (20,70,150,20), "Connect to server")) {
			m_state = State.ClientCommunication;
		}	
	}
}
