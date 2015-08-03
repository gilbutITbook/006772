using UnityEngine;
using System.Collections;
using System.Net;

// 최대 플레이 인원수.
public class NetConfig
{
	public static int PLAYER_MAX = 4;

	public static int SERVER_PORT = 50764;
	public static int GAME_PORT = 50765;
}

// 이벤트 종류.
public enum NetEventType 
{
	Connect = 0,	// 접속 이벤트.
	Disconnect,		// 접속 종료 이벤트.
	SendError,		// 송신 오류.
	ReceiveError,	// 수신오류.
}

// 이벤트 결과.
public enum NetEventResult
{
	Failure = -1,	// 실패.
	Success = 0,	// 성공.
}

// 이벤트 상태 알림.
public class NetEventState
{
    public NetEventType     type;		// 이벤트 타입.
    public NetEventResult   result;		// 이벤트 결과.
	public IPEndPoint		endPoint;	// 접속할 엔드포인트.
}
