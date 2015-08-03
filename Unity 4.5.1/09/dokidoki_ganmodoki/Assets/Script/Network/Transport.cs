using System.Collections;
using System.Net;
using System.Net.Sockets;


// 이벤트 통지 델리게이트.
public delegate void 	EventHandler(ITransport transport, NetEventState state);


public interface ITransport
{
	// Use this for initialization
	bool		Initialize(Socket socket);

	// 종료 처리.
	bool		Terminate();

	// 노드 번호 얻기.
	int			GetNodeId();

	// 노드 번호 설정.
	void		SetNodeId(int node);

	// 접속원 엔드포인트 얻기.
	IPEndPoint	GetLocalEndPoint();

	// 접속할 엔드포인트 얻기.
	IPEndPoint	GetRemoteEndPoint();

	// 송신함수.
	int			Send(byte[] data, int size);
	
	// 수신함수.
	int			Receive(ref byte[] buffer, int size);

	// 접속처리.
	bool		Connect(string ipAddress, int port);
	
	// 절단 처리.
	void		Disconnect();
	
	// 송수신 처리.
	void		Dispatch();

	// 접속 확인 함수.
	bool		IsConnected();

	// 이벤트 함수 등록.
	void		RegisterEventHandler(EventHandler handler);

	// 이벤트 함수 삭제.
	void		UnregisterEventHandler(EventHandler handler);


	// 같은 단말에서 실행할 때 포트 번호로 송신원을 판별하기 위해 킵 얼라이브용.
	// 포트 번호를 설정한다.
	void 		SetServerPort(int port);
}

