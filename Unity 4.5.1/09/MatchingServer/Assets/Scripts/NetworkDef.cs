using UnityEngine;
using System.Collections;

// 최대 플레이어 수.
public class NetConfig
{
	public const int SERVER_VERSION = 1;

	public const int PLAYER_MAX = 4;

	public const int MATCHING_SERVER_PORT = 50763;
	public const int GAME_SERVER_PORT = 50764;
	public const int GAME_PORT = 50765;
}

// 이벤트 종류.
public enum NetEventType 
{
	Connect = 0,	// 접속 이벤트.
	Disconnect,		// 접속 단절 이벤트.
	SendError,		// 송신 오류.
	ReceiveError,	// 수신 오류.
}

// 이벤트 결과.
public enum NetEventResult
{
	Failure = -1,	// 실패.
	Success = 0,	// 성공.
}

// 이벤트의 상태를 통지.
public class NetEventState
{
	public int 				node;	// 이벤트가 발생한 노드.
    public NetEventType     type;	// 이벤트 타입.
    public NetEventResult   result;	// 이벤트 결과.
}
