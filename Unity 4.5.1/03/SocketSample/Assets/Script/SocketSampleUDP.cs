using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;

public class SocketSampleUDP : MonoBehaviour
{
	
	// 접속할 곳의 IP 어드레스.
	private string			m_address = "";
	
	// 접속할 곳의 포트 번호.
	private const int 		m_port = 50765;

	// 통신용 변수.
	private Socket			m_socket = null;

	// 상태. 
	private State			m_state;

	// 상태 정의. 
	private enum State
	{
		SelectHost = 0,
		CreateListener,
		ReceiveMessage,
		CloseListener,
		SendMessage,
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
		case State.CreateListener:
			CreateListener();
			break;

		case State.ReceiveMessage:
			ReceiveMessage();
			break;

		case State.CloseListener:
			CloseListener();
			break;

		case State.SendMessage:
			SendMessage();
			break;

		default:
			break;
		}
	}

	// 소켓 생성.
	void CreateListener()
	{
		Debug.Log("[UDP]Start communication.");
		
		// 소켓을 생성합니다.
		m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		// 사용할 포트 번호를 할당합니다.
		m_socket.Bind(new IPEndPoint(IPAddress.Any, m_port));

		m_state = State.ReceiveMessage;
	}

	// 다른 단말에서 보낸 메시지 수신.
	void ReceiveMessage()
	{
		byte[] buffer = new byte[1400];
		IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
		EndPoint senderRemote = (EndPoint)sender;

		if (m_socket.Poll(0, SelectMode.SelectRead)) {
			int recvSize = m_socket.ReceiveFrom(buffer, SocketFlags.None, ref senderRemote);
			if (recvSize > 0) {
				string message = System.Text.Encoding.UTF8.GetString(buffer);
				Debug.Log(message);
				m_state = State.CloseListener;
			}
		}
	}
	
	// 대기 종료.
	void CloseListener()
	{	
		// 대기를 종료합니다.
		if (m_socket != null) {
			m_socket.Close();
			m_socket = null;
		}

		m_state = State.Endcommunication;

		Debug.Log("[UDP]End communication.");
	}

	// 클라이언트와의 접속, 송신, 접속 종료.
	void SendMessage()
	{
		Debug.Log("[UDP]Start communication.");

		// 서버에 접속.
		m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

		// 메시지 송신.
		byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Hello, this is client.");
		IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(m_address), m_port);
		m_socket.SendTo(buffer, buffer.Length, SocketFlags.None, endpoint);

		// 접속 종료.
		m_socket.Shutdown(SocketShutdown.Both);
		m_socket.Close();

		m_state = State.Endcommunication;

		Debug.Log("[UDP]End communication.");
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
			m_state = State.CreateListener;
		}
		
		// 클라이언트를 선택했을 때의 접속할 서버 주소를 입력합니다.
		m_address = GUI.TextField(new Rect(20, 100, 200, 20), m_address);
		if (GUI.Button (new Rect (20,70,150,20), "Connect to server")) {
			m_state = State.SendMessage;
		}	
	}
}
