using System.Collections;
using System.Net;
using System.Net.Sockets;


// 이벤트 통지 델리게이트.
public delegate void 	EventHandler(ITransport transport, NetEventState state);


public interface ITransport
{
	// Use this for initialization
	bool		Initialize(Socket socket);

	//
	bool		Terminate();

	int			GetNodeId();

	void		SetNodeId(int node);

	IPEndPoint	GetLocalEndPoint();

	IPEndPoint	GetRemoteEndPoint();

	//
	int			Send(byte[] data, int size);
	
	//
	int			Receive(ref byte[] buffer, int size);

	//
	bool		Connect(string ipAddress, int port);
	
	// 
	void		Disconnect();
	
	// 
	void		Dispatch();

	//
	//void		SetReceiveData(byte[] data, int size);

	//
	void		RegisterEventHandler(EventHandler handler);

	//
	void		UnregisterEventHandler(EventHandler handler);


	// 같은 단말에서 실행할 때 포트 번호로 송신원을 판멸하기 위해 킵 얼라이브용.
	// 포트 번호를 설정합니다.
	void 		SetServerPort(int port);
}

