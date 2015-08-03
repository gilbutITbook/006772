using System.Collections;
using System.IO;

//
// 매칭 패킷 정의.
//

// 매칭 요청 패킷 정의.
public class MatchingRequestPacket : IPacket<MatchingRequest>
{
	class MatchingRequestSerializer : Serializer
	{
		//
		public bool Serialize(MatchingRequest packet)
		{
			bool ret = true;

			ret &= Serialize(packet.version);
			int request = (int)packet.request;
			ret &= Serialize(request);
			ret &= Serialize(packet.roomId);
			ret &= Serialize(packet.name, MatchingRequest.roomNameLength);
			ret &= Serialize(packet.level);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref MatchingRequest element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.version);

			int request = 0;
			ret &= Deserialize(ref request);
			element.request = (MatchingRequestId) request;
			
			ret &= Deserialize(ref element.roomId);
			ret &= Deserialize(ref element.name, MatchingRequest.roomNameLength);
			ret &= Deserialize(ref element.level);
			
			return ret;
		}
	}
	
	// 패킷 데이터 실체.
	MatchingRequest	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public MatchingRequestPacket(MatchingRequest data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public MatchingRequestPacket(byte[] data)
	{
		MatchingRequestSerializer serializer = new MatchingRequestSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.MatchingRequest;
	}
	
	public MatchingRequest	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		MatchingRequestSerializer serializer = new MatchingRequestSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 매칭 요청 패킷 정의.
public class MatchingResponsePacket : IPacket<MatchingResponse>
{
	class MatchingResponseSerializer : Serializer
	{
		//
		public bool Serialize(MatchingResponse packet)
		{
			bool ret = true;

			int result = (int)packet.result;
			ret &= Serialize(result);
			
			int request = (int)packet.request;
			ret &= Serialize(request);

			ret &= Serialize(packet.roomId);
			ret &= Serialize(packet.name, MatchingResponse.roomNameLength);
			ret &= Serialize(packet.members);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref MatchingResponse element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}
		
			bool ret = true;

			int result = 0;
			ret &= Deserialize(ref result);
			element.result = (MatchingResult) result;
			
			int request = 0;
			ret &= Deserialize(ref request);
			element.request = (MatchingRequestId) request;
			
			ret &= Deserialize(ref element.roomId);
			ret &= Deserialize(ref element.name, MatchingResponse.roomNameLength);
			ret &= Deserialize(ref element.members);
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	MatchingResponse	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public MatchingResponsePacket(MatchingResponse data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public MatchingResponsePacket(byte[] data)
	{
		MatchingResponseSerializer serializer = new MatchingResponseSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.MatchingResponse;
	}
	
	public MatchingResponse	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		MatchingResponseSerializer serializer = new MatchingResponseSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 방 검색 결과 패킷 정의.
public class SearchRoomPacket : IPacket<SearchRoomResponse>
{
	class SearchRoomSerializer : Serializer
	{
		//
		public bool Serialize(SearchRoomResponse packet)
		{
			bool ret = true;

			ret &= Serialize(packet.roomNum);
			
			for (int i = 0; i < packet.roomNum; ++i) {
				
				ret &= Serialize(packet.rooms[i].roomId);
				
				ret &= Serialize(packet.rooms[i].name, MatchingResponse.roomNameLength);
				
				ret &= Serialize(packet.rooms[i].members);
			}
			
			return ret;
		}
		
		//
		public bool Deserialize(ref SearchRoomResponse element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.roomNum);
			
			element.rooms = new RoomInfo[element.roomNum];
			for (int i = 0; i < element.roomNum; ++i) {
				
				ret &= Deserialize(ref element.rooms[i].roomId);
				
				ret &= Deserialize(ref element.rooms[i].name, MatchingResponse.roomNameLength);
				
				ret &= Deserialize(ref element.rooms[i].members);
			}

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	SearchRoomResponse	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public SearchRoomPacket(SearchRoomResponse data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public SearchRoomPacket(byte[] data)
	{
		SearchRoomSerializer serializer = new SearchRoomSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.SearchRoomResponse;
	}
	
	public SearchRoomResponse	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		SearchRoomSerializer serializer = new SearchRoomSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}


//
// 세션 통지 패킷 정의.
//
public class SessionPacket : IPacket<SessionData>
{
	class SessionSerializer : Serializer
	{
		//
		public bool Serialize(SessionData packet)
		{
			bool ret = true;

			int result = (int)packet.result;
			ret &= Serialize(result);
			ret &= Serialize(packet.playerId);
			ret &= Serialize(packet.members);

			for (int i = 0; i < packet.members; ++i) {
				
				ret &= Serialize(packet.endPoints[i].ipAddress, EndPointData.ipAddressLength);
				ret &= Serialize(packet.endPoints[i].port);
			}
			
			return ret;
		}
		
		//
		public bool Deserialize(ref SessionData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;
			
			int result = 0;
			ret &= Deserialize(ref result);
			element.result = (MatchingResult) result;
			
			ret &= Deserialize(ref element.playerId);
			ret &= Deserialize(ref element.members);

			element.endPoints = new EndPointData[element.members];
			for (int i = 0; i < element.members; ++i) {
				
				ret &= Deserialize(ref element.endPoints[i].ipAddress, EndPointData.ipAddressLength);
				ret &= Deserialize(ref element.endPoints[i].port);
			}

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	SessionData	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public SessionPacket(SessionData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public SessionPacket(byte[] data)
	{
		SessionSerializer serializer = new SessionSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.StartSessionNotify;
	}
	
	public SessionData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		SessionSerializer serializer = new SessionSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}






//
//
// 게임용 패킷 데이터 정의.
//
//

// 게임 전 동기화 정보 패킷 정의.
public class EquipmentPacket : IPacket<CharEquipment>
{
	class EquipmentSerializer : Serializer
	{
		//
		public bool Serialize(CharEquipment packet)
		{
			bool ret = true;
			
			ret &= Serialize(packet.globalId);
			ret &= Serialize(packet.shotType);

			return ret;
		}
		
		//
		public bool Deserialize(ref CharEquipment element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.globalId);
			ret &= Deserialize(ref element.shotType);

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	CharEquipment	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public EquipmentPacket(CharEquipment data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public EquipmentPacket(byte[] data)
	{
		EquipmentSerializer serializer = new EquipmentSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.Equip;
	}
	
	public CharEquipment	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		EquipmentSerializer serializer = new EquipmentSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}



// 게임 전 동기화 정보 패킷 정의.
public class GameSyncPacket : IPacket<GameSyncInfo>
{
	class GameSyncerializer : Serializer
	{
		//
		public bool Serialize(GameSyncInfo packet)
		{
			bool ret = true;

			// 동기화할 난수의 시드.
			ret &= Serialize(packet.seed);

			// 동기화할 장비 정보.
			for (int i = 0; i < NetConfig.PLAYER_MAX; ++i) {
				// 캐릭터의 글로벌 ID.
				ret &= Serialize(packet.items[i].globalId);	
				// 캐릭터의 글로벌 ID.
				ret &= Serialize(packet.items[i].shotType);
			}

			return ret;
		}
		
		//
		public bool Deserialize(ref GameSyncInfo element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}
	
			bool ret = true;

			// 동기화할 난수의 시드.
			ret &= Deserialize(ref element.seed);
			
			// 동기화할 장비 정보.
			element.items = new CharEquipment[NetConfig.PLAYER_MAX];
			for (int i = 0; i < NetConfig.PLAYER_MAX; ++i) {
				// 캐릭터의 글로벌 ID.
				ret &= Deserialize(ref element.items[i].globalId);	
				// 캐릭터의 글로벌 ID.
				ret &= Deserialize(ref element.items[i].shotType);
			}

			return ret;
		}
	}
	
	
	// 패킷 데이터의 실체.
	GameSyncInfo	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public GameSyncPacket(GameSyncInfo data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public GameSyncPacket(byte[] data)
	{
		GameSyncerializer serializer = new GameSyncerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.GameSyncInfo;
	}
	
	public GameSyncInfo	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		GameSyncerializer serializer = new GameSyncerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
	
}

// 캐릭터 좌표 패킷 정의.
public class CharacterDataPacket : IPacket<CharacterData>
{
	class CharactorDataSerializer : Serializer
	{
		//
		public bool Serialize(CharacterData packet)
		{
		
			bool ret = true;

			ret &= Serialize(packet.characterId, CharacterData.characterNameLength);
			ret &= Serialize(packet.index);
			ret &= Serialize(packet.dataNum);

			for (int i = 0; i < packet.dataNum; ++i) {
				// CharactorCoord
				ret &= Serialize(packet.coordinates[i].x);
				ret &= Serialize(packet.coordinates[i].z);
			}	
			
			return ret;
		}
		
		//
		public bool Deserialize(ref CharacterData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.characterId, CharacterData.characterNameLength);
			ret &= Deserialize(ref element.index);
			ret &= Deserialize(ref element.dataNum);

			element.coordinates = new CharacterCoord[element.dataNum];
			for (int i = 0; i < element.dataNum; ++i) {
				// CharactorCoord
				ret &= Deserialize(ref element.coordinates[i].x);
				ret &= Deserialize(ref element.coordinates[i].z);
			}
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	CharacterData		m_packet;
	
	public CharacterDataPacket(CharacterData data)
	{
		m_packet = data;
	}
	
	public CharacterDataPacket(byte[] data)
	{
		CharactorDataSerializer serializer = new CharactorDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.CharacterData;
	}
	
	public CharacterData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		CharactorDataSerializer serializer = new CharactorDataSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 캐릭터 공격 패킷 정의.
public class AttackPacket : IPacket<AttackData>
{
	protected class AttackDataSerializer : Serializer
	{
		//
		public bool Serialize(AttackData packet)
		{
			bool ret = true;
			
			ret &= Serialize(packet.characterId, AttackData.characterNameLength);
			ret &= Serialize(packet.attackKind);

			return ret;
		}
		
		//
		public bool Deserialize(ref AttackData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}
			
			bool ret = true;
			
			ret &= Deserialize(ref element.characterId, AttackData.characterNameLength);
			ret &= Deserialize(ref element.attackKind);

			return true;
		}
	}
	
	// 패킷 데이터의 실체.
	AttackData m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public AttackPacket(AttackData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public AttackPacket(byte[] data)
	{
		AttackDataSerializer serializer = new AttackDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.AttackData;
	}
	
	public AttackData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		AttackDataSerializer serializer = new AttackDataSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}



// 아이템 패킷 정의.
public class ItemPacket : IPacket<ItemData>
{
	class ItemSerializer : Serializer
	{
		//
		public bool Serialize(ItemData packet)
		{
			bool ret = true;

			ret &= Serialize(packet.itemId, ItemData.itemNameLength);
			ret &= Serialize(packet.state);
			ret &= Serialize(packet.ownerId, ItemData.charactorNameLength);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref ItemData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;
			
			ret &= Deserialize(ref element.itemId, ItemData.itemNameLength);
			ret &= Deserialize(ref element.state);
			ret &= Deserialize(ref element.ownerId, ItemData.charactorNameLength);
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	ItemData	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public ItemPacket(ItemData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public ItemPacket(byte[] data)
	{
		ItemSerializer serializer = new ItemSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.ItemData;
	}
	
	// 게임에서 사용할 패킷 데이터 획득.
	public ItemData	GetPacket()
	{
		return m_packet;
	}
	
	// 송신용 byte[]형 데이터 획득.
	public byte[] GetData()
	{
		ItemSerializer serializer = new ItemSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 도넛에 들어간 상태의 패킷.
public class DoorPacket : IPacket<CharDoorState>
{
	class DoorSerializer : Serializer
	{
		//
		public bool Serialize(CharDoorState packet)
		{
			bool ret = true;

			ret &= Serialize(packet.globalId);
			ret &= Serialize(packet.keyId, CharDoorState.keyNameLength);
			ret &= Serialize(packet.isInTrigger);
			ret &= Serialize(packet.hasKey);

			return ret;
		}
		
		//
		public bool Deserialize(ref CharDoorState element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.globalId);
			ret &= Deserialize(ref element.keyId, CharDoorState.keyNameLength);
			ret &= Deserialize(ref element.isInTrigger);
			ret &= Deserialize(ref element.hasKey);

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	CharDoorState	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public DoorPacket(CharDoorState data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public DoorPacket(byte[] data)
	{
		DoorSerializer serializer = new DoorSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.DoorState;
	}
	
	public CharDoorState	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		DoorSerializer serializer = new DoorSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}


// 방 이동 패킷 정의..
public class RoomPacket : IPacket<MovingRoom>
{
	class RoomSerializer : Serializer
	{
		//
		public bool Serialize(MovingRoom packet)
		{
			bool ret = true;

			ret &= Serialize(packet.keyId, MovingRoom.keyNameLength);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref MovingRoom element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.keyId, MovingRoom.keyNameLength);
			
			return ret;
		}
	}
	
	
	// 패킷 데이터의 실체.
	MovingRoom	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public RoomPacket(MovingRoom data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public RoomPacket(byte[] data)
	{
		RoomSerializer serializer = new RoomSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.MovingRoom;
	}
	
	public MovingRoom	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		RoomSerializer serializer = new RoomSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
	
}


// 아이템 사용 패킷 정의.
public class ItemUsePacket : IPacket<ItemUseData>
{
	class ItemUseSerializer : Serializer
	{
		//
		public bool Serialize(ItemUseData packet)
		{
			bool ret = true;
			ret &= Serialize(packet.itemFavor);
			ret &= Serialize(packet.targetId, ItemUseData.characterNameLength);
			ret &= Serialize(packet.userId, ItemUseData.characterNameLength);
			ret &= Serialize(packet.itemCategory);

			return true;
		}
		
		//
		public bool Deserialize(ref ItemUseData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;
			ret &= Deserialize(ref element.itemFavor);
			ret &= Deserialize(ref element.targetId, ItemUseData.characterNameLength);
			ret &= Deserialize(ref element.userId, ItemUseData.characterNameLength);
			ret &= Deserialize(ref element.itemCategory);

			return true;
		}
	}
	
	// 패킷 데이터의 실체.
	ItemUseData	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public ItemUsePacket(ItemUseData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public ItemUsePacket(byte[] data)
	{
		ItemUseSerializer serializer = new ItemUseSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.UseItem;
	}
	
	public ItemUseData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		ItemUseSerializer serializer = new ItemUseSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 몬스터 리스폰 패킷 정의.
public class MonsterPacket : IPacket<MonsterData>
{
	protected class MonsterDataSerializer : Serializer
	{
		//
		public bool Serialize(MonsterData packet)
		{
			bool ret = true;

			ret &= Serialize(packet.lairId, MonsterData.monsterNameLength);
			ret &= Serialize(packet.monsterId, MonsterData.monsterNameLength);

			return ret;
		}
		
		//
		public bool Deserialize(ref MonsterData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.lairId, MonsterData.monsterNameLength);
			ret &= Deserialize(ref element.monsterId, MonsterData.monsterNameLength);

			return true;
		}
	}
	
	// 패킷 데이터의 실체.
	MonsterData m_packet;

	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public MonsterPacket(MonsterData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public MonsterPacket(byte[] data)
	{
		MonsterDataSerializer serializer = new MonsterDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	

	public PacketId	GetPacketId()
	{
		return PacketId.MonsterData;
	}
	
	public MonsterData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		MonsterDataSerializer serializer = new MonsterDataSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// HP 통지 패킷 정의.
public class HitPointPacket : IPacket<HpData>
{
	protected class HpDataSerializer : Serializer
	{
		//
		public bool Serialize(HpData packet)
		{
			bool ret = true;

			ret &= Serialize(packet.characterId, HpData.characterNameLength);
			ret &= Serialize (packet.hp);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref HpData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.characterId, HpData.characterNameLength);
			ret &= Deserialize (ref element.hp);
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	HpData m_packet;

	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public HitPointPacket(HpData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public HitPointPacket(byte[] data)
	{
		HpDataSerializer serializer = new HpDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.HpData;
	}
	
	public HpData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		HpDataSerializer serializer = new HpDataSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}


// 대미지양 패킷 정의.
public class DamageDataPacket : IPacket<DamageData>
{
	protected class DamageDataSerializer : Serializer
	{
		//
		public bool Serialize(DamageData packet)
		{
			bool ret = true;

			ret &= Serialize(packet.target, DamageData.characterNameLength);
			ret &= Serialize(packet.attacker);
			ret &= Serialize (packet.damage);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref DamageData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.target, DamageData.characterNameLength);
			ret &= Deserialize(ref element.attacker);
			ret &= Deserialize (ref element.damage);
			
			return ret;
		}
	}

	// 패킷 데이터의 실체.
	protected DamageData m_packet;
	
	// 상속용 생성자.
	public DamageDataPacket()
	{
	}

	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public DamageDataPacket(DamageData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public DamageDataPacket(byte[] data)
	{
		DamageDataSerializer serializer = new DamageDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	virtual public PacketId	GetPacketId()
	{
		return PacketId.DamageData;
	}
	
	public DamageData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		DamageDataSerializer serializer = new DamageDataSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

public class DamageNotifyPacket : DamageDataPacket
{

	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public DamageNotifyPacket(DamageData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public DamageNotifyPacket(byte[] data)
	{
		DamageDataSerializer serializer = new DamageDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}

	public override PacketId	GetPacketId()
	{
		return PacketId.DamageNotify;
	}
}

// 소환수 출현 패킷 정의.
public class SummonPacket : IPacket<SummonData>
{
	class SummonSerializer : Serializer
	{
		//
		public bool Serialize(SummonData packet)
		{
			bool ret = true;

			ret &= Serialize(packet.summon, SummonData.summonNameLength);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref SummonData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.summon, SummonData.summonNameLength);
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	SummonData	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public SummonPacket(SummonData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public SummonPacket(byte[] data)
	{
		SummonSerializer serializer = new SummonSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.Summon;
	}
	
	public SummonData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		SummonSerializer serializer = new SummonSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 보스 직접 공격 패킷 정의.
public class BossDirectPacket : IPacket<BossDirectAttack>
{
	class DirectAttackSerializer : Serializer
	{
		//
		public bool Serialize(BossDirectAttack packet)
		{
			bool ret = true;

			ret &= Serialize(packet.target, BossDirectAttack.characterNameLength);
			ret &= Serialize(packet.power);

			return ret;
		}
		
		//
		public bool Deserialize(ref BossDirectAttack element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.target, BossDirectAttack.characterNameLength);
			ret &= Deserialize(ref element.power);

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	BossDirectAttack	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public BossDirectPacket(BossDirectAttack data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public BossDirectPacket(byte[] data)
	{
		DirectAttackSerializer serializer = new DirectAttackSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.BossDirectAttack;
	}
	
	public BossDirectAttack	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		DirectAttackSerializer serializer = new DirectAttackSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 보스 범위 공격 패킷 정의.
public class BossRangePacket : IPacket<BossRangeAttack>
{
	class RangeAttackSerializer : Serializer
	{
		//
		public bool Serialize(BossRangeAttack packet)
		{
			bool ret = true;

			ret &= Serialize(packet.power);
			ret &= Serialize(packet.range);

			return ret;
		}
		
		//
		public bool Deserialize(ref BossRangeAttack element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.power);
			ret &= Deserialize(ref element.range);

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	BossRangeAttack	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public BossRangePacket(BossRangeAttack data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public BossRangePacket(byte[] data)
	{
		RangeAttackSerializer serializer = new RangeAttackSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.BossRangeAttack;
	}
	
	public BossRangeAttack	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		RangeAttackSerializer serializer = new RangeAttackSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 보스 속공 패킷 정의.
public class BossQuickPacket : IPacket<BossQuickAttack>
{
	class DirectQuickSerializer : Serializer
	{
		//
		public bool Serialize(BossQuickAttack packet)
		{
			bool ret = true;
			
			ret &= Serialize(packet.target, BossQuickAttack.characterNameLength);
			ret &= Serialize(packet.power);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref BossQuickAttack element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}
			
			bool ret = true;
			
			ret &= Deserialize(ref element.target, BossQuickAttack.characterNameLength);
			ret &= Deserialize(ref element.power);
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	BossQuickAttack	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public BossQuickPacket(BossQuickAttack data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public BossQuickPacket(byte[] data)
	{
		DirectQuickSerializer serializer = new DirectQuickSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.BossQuickAttack;
	}
	
	public BossQuickAttack	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		DirectQuickSerializer serializer = new DirectQuickSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 보스 사망 통지 패킷 정의.
public class BossDeadPacket : IPacket<BossDead>
{
	class BossDeadSerializer : Serializer
	{
		//
		public bool Serialize(BossDead packet)
		{
			bool ret = true;
			
			ret &= Serialize(packet.bossId, BossDead.bossNameLength);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref BossDead element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}
			
			bool ret = true;
			
			ret &= Deserialize(ref element.bossId, BossDead.bossNameLength);
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	BossDead	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public BossDeadPacket(BossDead data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public BossDeadPacket(byte[] data)
	{
		BossDeadSerializer serializer = new BossDeadSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.BossDead;
	}
	
	public BossDead	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		BossDeadSerializer serializer = new BossDeadSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}


// 보너스 케이크 정보 패킷 정의.
public class PrizePacket : IPacket<PrizeData>
{
	class PrizeSerializer : Serializer
	{
		//
		public bool Serialize(PrizeData packet)
		{
			bool ret = true;

			// 캐릭터 ID.	
			ret &= Serialize(packet.characterId, PrizeData.characterNameLength);
			// 케이크 수.
			ret &= Serialize(packet.cakeNum);

			return ret;
		}
		
		//
		public bool Deserialize(ref PrizeData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;

			// 캐릭터 ID.	
			ret &= Deserialize(ref element.characterId, PrizeData.characterNameLength);
			// 케이크 수.
			ret &= Deserialize(ref element.cakeNum);

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	PrizeData	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public PrizePacket(PrizeData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public PrizePacket(byte[] data)
	{
		PrizeSerializer serializer = new PrizeSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.Prize;
	}
	
	public PrizeData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		PrizeSerializer serializer = new PrizeSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 보너스 케이크 결과 정보 패킷 정의.
public class PrizeResultPacket : IPacket<PrizeResultData>
{
	class PrizeResultSerializer : Serializer
	{
		//
		public bool Serialize(PrizeResultData packet)
		{
			bool ret = true;

			// 케이크 데이터 수.	
			ret &= Serialize(packet.cakeDataNum);
			// 케이크 수.
			for (int i = 0; i < 4; ++i) {
				ret &= Serialize(packet.cakeNum[i]);
			}

			return ret;
		}
		
		//
		public bool Deserialize(ref PrizeResultData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;
			
			// 케이크 데이터 수.	
			ret &= Deserialize(ref element.cakeDataNum);
			// 케이크 수.
			element.cakeNum = new int[element.cakeDataNum];
			for (int i = 0; i < element.cakeDataNum; ++i) {
				ret &= Deserialize(ref element.cakeNum[i]);
			}

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	PrizeResultData	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public PrizeResultPacket(PrizeResultData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public PrizeResultPacket(byte[] data)
	{
		PrizeResultSerializer serializer = new PrizeResultSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.PrizeResult;
	}
	
	public PrizeResultData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		PrizeResultSerializer serializer = new PrizeResultSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

// 채팅 패킷 정의.
public class ChatPacket : IPacket<ChatMessage>
{
	class ChatSerializer : Serializer
	{
		//
		public bool Serialize(ChatMessage packet)
		{
			bool ret = true;
			
			ret &= Serialize(packet.characterId, ChatMessage.characterNameLength);
			ret &= Serialize(packet.message, ChatMessage.messageLength);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref ChatMessage element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되지 않았다.
				return false;
			}

			bool ret = true;
			
			ret &= Deserialize(ref element.characterId, ChatMessage.characterNameLength);
			ret &= Deserialize(ref element.message, ChatMessage.messageLength);

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	ChatMessage	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈하기 위한 생성자.
	public ChatPacket(ChatMessage data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하기 위한 생성자.
	public ChatPacket(byte[] data)
	{
		ChatSerializer serializer = new ChatSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	
	public PacketId	GetPacketId()
	{
		return PacketId.ChatMessage;
	}
	
	public ChatMessage	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		ChatSerializer serializer = new ChatSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}
