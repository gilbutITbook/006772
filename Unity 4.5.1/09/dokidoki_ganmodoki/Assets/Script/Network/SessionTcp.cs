using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class SessionTCP : Session<TransportTCP>
{

	// 
	public override bool CreateListener(int port, int connectionMax)
	{
		try {
			m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			m_listener.Bind(new IPEndPoint(IPAddress.Any, port));
			m_listener.Listen(connectionMax);
		}
		catch {
			return false;
		}

		return true;
	}
	
	//
	public override bool DestroyListener()
	{
		if (m_listener == null) {
			return false;
		}
		
		m_listener.Close();
		m_listener = null;

		return true;
	}	
	
	public override void AcceptClient() 
	{
		if ((m_listener != null) && m_listener.Poll(0, SelectMode.SelectRead)) {
			Debug.Log("[TCP]AcceptClient");

			// 접속 요청이 왔다.
			Socket socket = m_listener.Accept();

			int node = -1;
			try {
				Debug.Log("[TCP]Create transport");
				TransportTCP transport = new TransportTCP();
				transport.Initialize(socket);
				transport.transportName = "serverSocket";
				Debug.Log("[TCP]JoinSession");
				node = JoinSession(transport);
			}
			catch {
				Debug.Log("[TCP]Connect fail.");
				return;
			}

			if (node >= 0 && m_handler != null) {
				NetEventState state = new NetEventState();
				state.type = NetEventType.Connect;
				state.result = NetEventResult.Success;
				m_handler(node, state);
			}
			Debug.Log("[TCP]Connected from client. [port:" + m_port + "]");
		}
	}
}

