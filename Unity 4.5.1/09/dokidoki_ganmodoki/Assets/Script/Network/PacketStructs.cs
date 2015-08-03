using UnityEngine;
using System;
using System.Collections;
using System.Net;


public enum PacketId
{
	// 매칭용 패킷.
	MatchingRequest = 0,		// 매칭 요청 패킷.
	MatchingResponse, 			// 매칭 응답 패킷.
	SearchRoomResponse, 		// 방 검색 응답.
	StartSessionNotify, 		// 게임 시작 통지.

	// 게임용 패킷.
	Equip,						// 초기 장비 정보.
	GameSyncInfo,				// 게임전 동기화 정보.
	CharacterData,				// 캐릭터 좌표 정보.
	AttackData,					// 캐릭터 공격 정보.
	ItemData,					// 아이템 획득/ 폐기 정보.
	UseItem,					// 아이템 사용 정보.
	DoorState,					// 도어 상태.
	MovingRoom,					// 방 이동 정보.
	HpData,						// HP 통지.
	DamageData,					// 호스트에 대미지 통지.
	DamageNotify,				// 모든 단말에 대미지양을 통지.
	MonsterData,				// 몬스터 발생.
	Summon,						// 소환수 정보.
	BossDirectAttack,			// 보스 직접 공격.
	BossRangeAttack,			// 보스 범위 공격.
	BossQuickAttack,			// 보스 속공.
	BossDead,					// 보스 사망 통지.
	Prize,						// 보너스 획득 정보.
	PrizeResult,				// 보너스 획득 결과.
	ChatMessage,				// 채팅 메시지.

	Max,
}


public enum MatchingRequestId
{
	CreateRoom = 0,
	JoinRoom,
	StartSession,
	SearchRoom,

	Max,
}

public enum MatchingResult 
{
	Success = 0,


	RoomIsFull,
	MemberIsFull,
	RoomIsGone,

}

public struct PacketHeader
{
	// 패킷 종별.
	public PacketId 	packetId;
}

//
// 매칭 요청.
//
public struct MatchingRequest
{
	public int					version;	// 패킷ID.
	public MatchingRequestId	request;	// 요청 내용.
	public int 					roomId;		// 요청 방 ID.
	public string				name;		// 생성할 방 이름.
	public int					level;		// 레벨 지정.
	
	public const int roomNameLength = 32;	// 방 이름의 길이.
}

//
// 매칭 응답.
//
public struct MatchingResponse
{
	// 요청 결과.
	public MatchingResult		result;
	
	// 요청 내용.
	public MatchingRequestId	request;

	// 응답 방 ID.
	public int 					roomId;

	// 
	public string			 	name;

	// 참가 인원.
	public int					members;

	// 방 이름 길이.
	public const int roomNameLength = 32;
}

//
// 방 정보.
//
public struct RoomInfo
{
	// 요청 방 ID.
	public int 					roomId;
	
	// 생성할 방 이름.
	public string				name;

	// 참가 인원 
	public int					members;

	// 방 이름 길이.
	public const int roomNameLength = 32;
}

//
// 방 검색 결과.
//
public struct SearchRoomResponse
{
	// 검색한 방 수.
	public int			roomNum;

	// 방 정보..
	public RoomInfo[]	rooms;
}

//
// 접속처 정보.
//
public struct EndPointData
{
	public string		ipAddress;
	
	public int 			port;

	// IP 어드레스 길이.
	public const int ipAddressLength = 32;
}

//
// 세션 정보.
//
public struct SessionData
{
	public MatchingResult 	result;

	public int				playerId;		// 동일한 단말에서 동작시킬 때 사용.

	public int				members;

	public EndPointData[]	endPoints;
}


//
//
// 게임용 패킷 데이터 정의.
//
//


//
// 게임전 동기화 정보.
//
public struct CharEquipment
{
	public int			globalId;	// 캐릭터의 글로벌 ID.

	// 장비 정보는 아이템 ID를 송신하지 않고 타입을 송신하기만 하면 되는.
	// 사양으로 변경했다.
	// 이 때문에 문자열인 아이템 ID가 아니라 아이템 타입인 int형으로.
	// 통신을 하게 되었다..
	public int			shotType;	// 선택한 무기 정보.
}

//
// 전원의 동기화 정보.
//
public struct GameSyncInfo
{
	public int				seed;		// 동기화할 난수의 시드.
	public CharEquipment[]	items;		// 동기화할 장비 정보.
}


//
// 아이템 획득정보.
//
public struct ItemData
{
	public string 		itemId;		// 아이템 식별자.
	public int			state;		// 아이템의 획득 상태.
	public string 		ownerId;	// 소유자 ID.

	public const int 	itemNameLength = 32;		// 아이템 이름의 길이.
	public const int 	charactorNameLength = 64;	// 캐릭터 ID의 길이.
}


//
// 캐릭터 좌표 정보.
//
public struct CharacterCoord
{
	public float	x;		// 캐릭터의 x좌표.
	public float	z;		// 캐릭터의 z좌표.
	
	public CharacterCoord(float x, float z)
	{
		this.x = x;
		this.z = z;
	}
	public Vector3	ToVector3()
	{
		return(new Vector3(this.x, 0.0f, this.z));
	}
	public static CharacterCoord	FromVector3(Vector3 v)
	{
		return(new CharacterCoord(v.x, v.z));
	}
	
	public static CharacterCoord	Lerp(CharacterCoord c0, CharacterCoord c1, float rate)
	{
		CharacterCoord	c = new CharacterCoord();
		
		c.x = Mathf.Lerp(c0.x, c1.x, rate);
		c.z = Mathf.Lerp(c0.z, c1.z, rate);
		
		return(c);
	}
}

//
// 캐릭터의 이동 정보.
//
public struct CharacterData
{
	public string 			characterId;	// 캐릭터 ID.
	public int 				index;			// 위치 좌표의 인덱스.
	public int				dataNum;		// 좌표 데이터 수.
	public CharacterCoord[]	coordinates;	// 좌표 데이터.

	public const int 		characterNameLength = 64;	// 캐릭터 ID의 길이.
}

//
// 캐릭터의 공격 정보.
//
public struct AttackData
{
	public string		characterId;		// 캐릭터 ID.
	public int			attackKind;			// 공격 종류.
	
	public const int 	characterNameLength = 64;	// 캐릭터 ID 길이.
}

//
// 몬스터 리스폰 정보.
//
public struct MonsterData
{
	public string		lairId;			//  제네레이터 이름.
	public string 		monsterId;		// 몬스터 이름.

	public const int 	monsterNameLength = 64;	// 몬스터 이름의 길이.
}


//
// 대미지양 정보.
//
public struct DamageData
{
	public string 			target;			// 공격받은 캐릭터 ID.
	public int	 			attacker;		// 공격한 어카운트 ID.
	public float			damage;			// 대미지양.

	public const int 		characterNameLength = 64;	// 캐릭터 ID 길이.
}

//
// 캐릭터 HP 정보.
//
public struct HpData
{
	public string 			characterId;	// 캐릭터 ID.
	public float			hp;				// HP.
	
	public const int 		characterNameLength = 64;	// 캐릭터 ID 길이.
}

//
// 도넛에 들어간 상태 정보.
//
public struct CharDoorState
{
	public int			globalId;		// 글로벌 ID.
	public string		keyId;			// 열쇠 ID.
	public bool			isInTrigger;	// 도넛 위에 있는 상태.
	public bool			hasKey;			// 열쇠를 가지고 있는가.

	public const int 	keyNameLength = 64;	// 열쇠 ID 길이.
}


//
// ルーム移動通知.
//
public struct MovingRoom
{
	public string		keyId;				// カギID.

	public const int 	keyNameLength = 32;	// カギ名 길이.
}

//
// 아이템 사용 정보.
//
public struct ItemUseData
{
	public int		itemFavor;	// 아이템 효과.
	public string	targetId;	// 효과를 적용할 캐릭터 ID.
	public string	userId;		// 아이템을 사용할 캐릭터 ID.

	public int		itemCategory;	// 아이템 효과 종류.

	public const int characterNameLength = 64;	// 캐릭터 ID 길이.
}

//
// 소환수 출현정보.
//
public struct SummonData
{
	public string		summon;					// 소환수 종류.

	public const int 	summonNameLength = 32;	// 소환수 이름 길이.
}


//
// 보스 공격 정보.
//

// 直接공격.
public struct BossDirectAttack
{
	public string		target;		// ターゲットの캐릭터 .
	public float		power;		// 공격力.

	public const int 	characterNameLength = 64;	// 캐릭터 ID 길이.
}

// 범위공격.
public struct BossRangeAttack
{
	public float	power;		// 공격력.
	public float	range;		// 범위.
}

// 빠른 공격.
public struct BossQuickAttack
{
	public string		target;		// 목표 캐릭터 .
	public float		power;		// 공격력.
	
	public const int 	characterNameLength = 64;	// 캐릭터 ID 길이.
}

// ボス死亡通知.
public struct BossDead
{
	public string		bossId;		// 보스 ID.
	
	public const int 	bossNameLength = 64;	// 보스 ID 길이.
}

//
// ご褒美케이크情報.
//
public struct PrizeData
{
	public string		characterId;	// 캐릭터 ID.
	public int			cakeNum;		// 케이크の数.

	public const int 	characterNameLength = 64;	// 캐릭터 ID 길이.
}

//
// 보너스 케이크 결과 정보.
//
public struct PrizeResultData
{
	public int 		cakeDataNum;	// 케이크 데이터 수.
	public int []	cakeNum;		// 먹은 케이크 수.
}


//
// 채팅 메시지.
//
public struct ChatMessage
{
	public string		characterId; // 캐릭터 ID.
	public string		message;	 // 채팅 메시지.
	
	public const int 	characterNameLength = 64;	// 캐릭터 ID 길이.
	public const int	messageLength = 64;
}

