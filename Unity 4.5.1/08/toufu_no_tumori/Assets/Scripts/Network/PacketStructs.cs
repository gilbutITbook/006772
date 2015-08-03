using UnityEngine;
using System;
using System.Collections;
using System.Net;


public enum PacketId
{
	// 게임용 패킷.
	GameSyncInfo,
	GameSyncInfoHouse,
	CharacterData,
	ItemData,
	Moving,
	GoingOut,
	ChatMessage,
	
	Max,
}


public struct PacketHeader
{
	// 패킷 종류.
	public PacketId 	packetId;
}


//
//
// 게임용 패킷 데이터 정의.
//
//


//
// 게임 전 동기화 정보.
//
public struct SyncGameData
{
	public int			version;	// 패킷 ID.
	public MovingData	moving;		// 이사 정보.
	public int 			itemNum;	// 아이템 정보수.
	public ItemData[]	items;		// 아이템 정보.
}

//
// 아이템 획득 정보.
//
public struct ItemData
{
	public string 		itemId;		// 아이템 식별자.
	public int			state;		// 아이템 취득 상태.
	public string 		ownerId;	// 소유자 ID.

	public const int 	itemNameLength = 32;		// 아이템 이름 길이.
	public const int 	characterNameLength = 64;	// 캐릭터 ID 길이.
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
		CharacterCoord	c;
		
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
	public string			characterId;	// 캐릭터 ID.
	public int 				index;			// 위치 좌표 인덱스.
	public int				dataNum;		// 좌표 데이터 수.
	public CharacterCoord[]	coordinates;	// 좌표 데이터.
	
	public const int 	characterNameLength = 64;	// 캐릭터 ID 길이.
}

//
// 이사 정보.
//
public struct MovingData
{
	public string		characterId;	// 캐릭터 ID.
	public string		houseId;		// 집 ID.
	public bool 		moving;			// 이사 정보.

	public const int 	characterNameLength = 64;	// 캐릭터 ID의 길이.
	public const int 	houseIdLength = 32;			// 집 ID 길이.
}

//
// 정원 이동 정보.
//
public struct GoingOutData
{
	public string		characterId;	// 캐릭터ID.
	public bool 		goingOut;		// 놀러갈 정보.

	public const int 	characterNameLength = 64;	// 캐릭터 ID 길이.
	public const int 	houseIdLength = 32;			// 집ID 길이.
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

