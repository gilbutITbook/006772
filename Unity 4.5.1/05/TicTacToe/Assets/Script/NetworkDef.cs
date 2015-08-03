using UnityEngine;
using System.Collections;

// 이벤트 종류.
public enum NetEventType 
{
	Connect = 0,	// 접속 이벤트.
	Disconnect,		// 끊김 이벤트.
	SendError,		// 송신 오류.
	ReceiveError,	// 수신 오류.
}

// 이벤트 결과.
public enum NetEventResult
{
	Failure = -1,	// 실패.
	Success = 0,	// 성공.
}

// 이벤트 상태 통지.
public class NetEventState
{
	public NetEventType		type;	// 이벤트 타입.
	public NetEventResult	result;	// 이벤트 결과.
}
