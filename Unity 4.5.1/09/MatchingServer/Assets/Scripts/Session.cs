using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public abstract class Session<T>
	where T : ITransport, new()
{

	// 리스닝 소켓.
	protected Socket				m_listener = null;

	protected int					m_port = 0;

	// 현재 접속 ID.
	protected int					m_nodeIndex = 0;
	
	protected Dictionary<int, T>	m_transports = new Dictionary<int, T>();

	//
	// 스레드 관련 멤버 변수.
	//

	protected bool					m_threadLoop = false;
	
	protected Thread				m_thread = null;


	// 
	protected System.Object 		m_transportLock = new System.Object();

	// 
	protected System.Object 		m_nodeIndexLock = new System.Object();
	
	// 서버 플래그.	
	protected bool	 				m_isServer = false;

	//
	protected int 					m_mtu = 1400;


	// 이벤트 통지 델리게이트.
	public delegate void 			EventHandler(int node, NetEventState state);
	// 이벤트 핸들러.
	protected EventHandler			m_handler;
	
	// 
	~Session() 
	{
		Disconnect();
	}
	
	
	public bool StartServer(int port, int connectionMax)
	{
		// 리스닝 소켓을 생성합니다..
		bool ret = CreateListener(port, connectionMax);
		if (ret == false) {
			Debug.Log("Starat server failed.");
			return false;
		}

		//
		if (m_threadLoop == false) {
			CreateThread();
		}

		m_port = port;
		m_isServer = true;
		
		return true;
	}
	
	public void StopServer()
	{
		m_isServer = false;

		DestroyThread();

		DestroyListener();

		Debug.Log("Server stopped.");
	}



	// 
	protected bool CreateThread()
	{
		Debug.Log("CreateThread called.");

		// 수신처리 스레드 시작.
		try {
			m_thread = new Thread(new ThreadStart(ThreadDispatch));
			m_threadLoop = true;
			m_thread.Start();
		}
		catch {
			return false;
		}


		Debug.Log("Thread launched.");

		return true;
	}

	protected bool DestroyThread()
	{
		Debug.Log("DestroyThread called.");

		if (m_threadLoop == true) {
			// 
			m_threadLoop = false;

			if (m_thread != null) {
				// 수신처리 스레드 종료.
				m_thread.Join();
				// 수신처리 스레드 폐기.
				m_thread = null;
			}
		}

		return true;
	}

	//
	protected int JoinSession(Socket socket)
	{
		// 세션에 참가.
		T transport = new T();

		if (socket != null) {
			// 소켓을 설정합니다.
			transport.Initialize(socket);
		}

		return JoinSession(transport);
	}

	protected int JoinSession(T transport)
	{
		int node = -1;
		lock (m_nodeIndexLock) {
			node = m_nodeIndex;
			++m_nodeIndex;
		}
		
		transport.SetNodeId(node);
		
		// 이벤트 통지를 받을 함수를 등록합니다.
		transport.RegisterEventHandler(OnEventHandling);
		
		try {
			lock (m_transportLock) {
				m_transports.Add(node, transport);
			}
		}
		catch { 
			return -1;
		}
		
		return node;
	}


	// 
	protected bool LeaveSession(int node)
	{
		if (node < 0) {
			return false;	
		}

		if (m_transports.ContainsKey(node) == false) {
			return false;
		}
					
		T transport = (T) m_transports[node];
		if (transport == null) {
			return false;
		}

		lock (m_transportLock) {
			// Transport 폐기.
			transport.Terminate();

			m_transports.Remove(node);
		}

		return true;
	}

	public bool IsServer()
	{
		return m_isServer;
	}

	// 
	public int GetNodeNum()
	{
		return m_transports.Count;
	}
	
	public IPEndPoint GetLocalEndPoint(int node)
	{
		if (m_transports.ContainsKey(node) == false) {
			return default(IPEndPoint);
		}
		
		IPEndPoint ep;
		T transport = m_transports[node];
		ep = transport.GetLocalEndPoint();
		
		return ep;
	}

	public IPEndPoint GetRemoteEndPoint(int node)
	{
		if (m_transports.ContainsKey(node) == false) {
			return default(IPEndPoint);
		}

		IPEndPoint ep;
		T transport = m_transports[node];
		ep = transport.GetRemoteEndPoint();

		return ep;
	}

	// 접속 요청 감시.
	int FindTransoprt(IPEndPoint sender)
	{
		foreach (int node in m_transports.Keys) {
			T transport = m_transports[node];
			IPEndPoint ep = transport.GetLocalEndPoint();
			if (ep.Address.ToString() == sender.Address.ToString()) {
				return node;
			}
		}
		
		return -1;
	}

	//
	public virtual void ThreadDispatch()
	{	
		
		string str = "ThreadDispatch:" + m_threadLoop.ToString();
		Debug.Log(str);
		
		while (m_threadLoop) {
			// 접속 요청 감시.
			AcceptClient();
			
			// 세션 안 노드의 송수신 처리.
			Dispatch();
			
			// 다른 스레드에 처리를 위임합니다.
			Thread.Sleep(5);		
		}
		
		Debug.Log("Thread end.");
	}


	public virtual int Connect(string address, int port)
	{
		Debug.Log("Connect call");

		if (m_threadLoop == false) {
			Debug.Log("CreateThread");
			CreateThread();
		}
	
		int node = -1;
		bool ret = false;
		try {
			Debug.Log("transport Connect");
			T transport = new T();
			ret = transport.Connect(address, port);
			if (ret) {
				node = JoinSession(transport);
				Debug.Log("JoinSession node:" + node);
				// 같은 단말에서 실행할 때 포트 번호로 판별하기 위해 포트 번호를 설정.
				transport.SetServerPort(m_port);
			}
		}
		catch {
			Debug.Log("Connect fail.[exception]");
		}

		if (m_handler != null) {
			NetEventState state = new NetEventState();
			state.type = NetEventType.Connect;
			state.result = (ret)? NetEventResult.Success : NetEventResult.Failure;
			m_handler(node, state);
		}

		return node;
	}

	public virtual bool Disconnect(int node)
	{
		if (node < 0) {
			return false;
		}

		T transport = m_transports[node];
		if (transport != null) {
			transport.Disconnect();
			LeaveSession(node);
		}

		if (m_handler != null) {
			NetEventState state = new NetEventState();
			state.type = NetEventType.Disconnect;
			state.result = NetEventResult.Success;
			m_handler(node, state);
		}

		return true;
	}

	public virtual bool Disconnect()
	{
		// 스레드 정지.
		DestroyThread();
		
		// 접속 중인 Transport의 연결을 끊습니다.
		lock (m_transportLock) {
			foreach (T trans in m_transports.Values) {
				trans.Disconnect();
				trans.Terminate();
			}
		}

		return true;
	}

	//
	public virtual int Send(int node, byte[] data, int size)
	{
		if (node < 0) {
			return -1;
		}

		int sendSize = 0;
		try {
			T transport = (T)m_transports[node];
			sendSize = transport.Send(data, size);
		}
		catch {
			return -1;
		}

		return sendSize;	
	}
	
	//
	public virtual int Receive(int node, ref byte[] buffer)
	{
		if (node < 0) {
			return -1;
		}

		int recvSize = 0;
		try { 
			T transport = m_transports[node];
			recvSize = transport.Receive(ref buffer, buffer.Length);
		}
		catch {
			return -1;
		}

		return recvSize;
	}

	//
	public virtual void Dispatch()
	{
		Dictionary<int, T> transports = new Dictionary<int, T>(m_transports);
		
		// 송신 처리.
		foreach (T trans in transports.Values) {
			trans.Dispatch();
		}

		// 수신 처리.
		DispatchReceive();

	}

	//
	protected virtual void DispatchReceive()
	{
		// 리스닝 소켓에서 일괄 수신한 데이터를 각 트랜스포트에 분배합니다.
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
	public virtual void OnEventHandling(ITransport itransport, NetEventState state)
	{
		int node = itransport.GetNodeId();

		string str = "SignalNotification[" + node + "] :" + state.type.ToString() + " state:" + state.ToString();
		Debug.Log(str);

		do {
			if (m_transports.ContainsKey(node) == false) {
				// 찾지 못했을 때.
				string msg = "NodeId[" + node + "] is not founded.";
				Debug.Log(msg);
				break;
			}

			switch (state.type) {
			case NetEventType.Connect:
				break;

			case NetEventType.Disconnect:
				LeaveSession(node);
				break;
			}
		} while (false);

		// 이벤트 통지 함수가 등록되어 있으면 콜백합니다.
		if (m_handler != null) {
			m_handler(node, state);
		}
	}


	public abstract bool	CreateListener(int port, int connectionMax);
	
	
	public abstract bool 	DestroyListener();
	
	
	public abstract void	AcceptClient();
	
}

