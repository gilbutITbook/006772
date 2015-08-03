using System.Collections;
using System.IO;

//
// 게임 전 동기 패킷 정의(아이템용).
//
public class SyncGamePacket : IPacket<SyncGameData>
{
	public class GameSyncSerializer : Serializer
	{
		//
		public bool Serialize(SyncGameData packet)
		{
			
			bool ret = true;
			ret &= Serialize(packet.version);

			ret &= Serialize(packet.moving.characterId, MovingData.characterNameLength);
			ret &= Serialize(packet.moving.houseId, MovingData.houseIdLength);
			ret &= Serialize(packet.moving.moving);
		
			ret &= Serialize(packet.itemNum);		
			for (int i = 0; i < packet.itemNum; ++i) {
				// CharacterCoord
				ret &= Serialize(packet.items[i].itemId, ItemData.itemNameLength);
				ret &= Serialize(packet.items[i].state);
				ret &= Serialize(packet.items[i].ownerId, ItemData.characterNameLength);
			}	
			
			return ret;
		}
		
		//
		public bool Deserialize(ref SyncGameData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 정의되어 있지 않습니다.
				return false;
			}

			bool ret = true;
			ret &= Deserialize(ref element.version);

			// MovingData 구조체.
			ret &= Deserialize(ref element.moving.characterId, MovingData.characterNameLength);
			ret &= Deserialize(ref element.moving.houseId, MovingData.houseIdLength);
			ret &= Deserialize(ref element.moving.moving);

			ret &= Deserialize(ref element.itemNum);
			element.items = new ItemData[element.itemNum];
			for (int i = 0; i < element.itemNum; ++i) {
				// ItemData
				ret &= Deserialize(ref element.items[i].itemId, ItemData.itemNameLength);
				ret &= Deserialize(ref element.items[i].state);
				ret &= Deserialize(ref element.items[i].ownerId, ItemData.characterNameLength);
			}
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	SyncGameData		m_packet;
	
	public SyncGamePacket(SyncGameData data)
	{
		m_packet = data;
	}
	
	public SyncGamePacket(byte[] data)
	{
		GameSyncSerializer serializer = new GameSyncSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.GameSyncInfo;
	}
	
	public SyncGameData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		GameSyncSerializer serializer = new GameSyncSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

//
// 게임 전 동기 패킷 정의(이사용).
//
public class SyncGamePacketHouse : IPacket<SyncGameData>
{
	// 패킷 데이터의 실체.
	SyncGameData		m_packet;
	
	public SyncGamePacketHouse(SyncGameData data)
	{
		m_packet = data;
	}
	
	public SyncGamePacketHouse(byte[] data)
	{
		SyncGamePacket.GameSyncSerializer serializer = new SyncGamePacket.GameSyncSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}

	// 같은 패킷에서 ID만 변경합니다.
	public PacketId	GetPacketId()
	{
		return PacketId.GameSyncInfoHouse;
	}

	public SyncGameData	GetPacket()
	{
		return m_packet;
	}

	public byte[] GetData()
	{
		SyncGamePacket.GameSyncSerializer serializer = new SyncGamePacket.GameSyncSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

//
// 아이템 패킷 정의.
//
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
			ret &= Serialize(packet.ownerId, ItemData.characterNameLength);
			
			return ret;
		}
		
		//
		public bool Deserialize(ref ItemData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되어 있지 않습니다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.itemId, ItemData.itemNameLength);
			ret &= Deserialize(ref element.state);
			ret &= Deserialize(ref element.ownerId, ItemData.characterNameLength);
			
			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	ItemData	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈 하는 생성자.
	public ItemPacket(ItemData data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈 하는 생성자. 
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
	
	// 게임에서 사용할 패킷 데이터를 획득.
	public ItemData	GetPacket()
	{
		return m_packet;
	}
	
	// 송신용 byte[]형 데이터를 획득.
	public byte[] GetData()
	{
		ItemSerializer serializer = new ItemSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

//
// 캐릭터 좌표 패킷 정의.
//
public class CharacterDataPacket : IPacket<CharacterData>
{
	class CharacterDataSerializer : Serializer
	{
		//
		public bool Serialize(CharacterData packet)
		{
			
			Serialize(packet.characterId, CharacterData.characterNameLength);
			
			Serialize(packet.index);
			Serialize(packet.dataNum);
			
			for (int i = 0; i < packet.dataNum; ++i) {
				// CharacterCoord
				Serialize(packet.coordinates[i].x);
				Serialize(packet.coordinates[i].z);
			}	
			
			return true;
		}
		
		//
		public bool Deserialize(ref CharacterData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되어 있지 않습니다.
				return false;
			}
			
			Deserialize(ref element.characterId, CharacterData.characterNameLength);
			
			Deserialize(ref element.index);
			Deserialize(ref element.dataNum);
			
			element.coordinates = new CharacterCoord[element.dataNum];
			for (int i = 0; i < element.dataNum; ++i) {
				// CharacterCoord
				Deserialize(ref element.coordinates[i].x);
				Deserialize(ref element.coordinates[i].z);
			}
			
			return true;
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
		CharacterDataSerializer serializer = new CharacterDataSerializer();
		
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
		CharacterDataSerializer serializer = new CharacterDataSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

//
// 이사 패킷 정의.
//
public class MovingPacket : IPacket<MovingData>
{
	class MovingSerializer : Serializer
	{
		//
		public bool Serialize(MovingData packet)
		{
			
			bool ret = true;
			
			ret &= Serialize(packet.characterId, MovingData.characterNameLength);
			ret &= Serialize(packet.houseId, MovingData.houseIdLength);
			ret &= Serialize(packet.moving);

			return ret;
		}
		
		//
		public bool Deserialize(ref MovingData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되어 있지 않습니다.
				return false;
			}
			
			bool ret = true;
			ret &= Deserialize(ref element.characterId, MovingData.characterNameLength);
			ret &= Deserialize(ref element.houseId, MovingData.houseIdLength);
			ret &= Deserialize(ref element.moving);

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	MovingData		m_packet;
	
	public MovingPacket(MovingData data)
	{
		m_packet = data;
	}
	
	public MovingPacket(byte[] data)
	{
		MovingSerializer serializer = new MovingSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.Moving;
	}
	
	public MovingData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		MovingSerializer serializer = new MovingSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

//
// 이사 패킷 정의.
//
public class GoingOutPacket : IPacket<GoingOutData>
{
	class GoingOutDataSerializer : Serializer
	{
		//
		public bool Serialize(GoingOutData packet)
		{
			
			bool ret = true;
			
			ret &= Serialize(packet.characterId, MovingData.characterNameLength);
			ret &= Serialize(packet.goingOut);

			return ret;
		}
		
		//
		public bool Deserialize(ref GoingOutData element)
		{
			if (GetDataSize() == 0) {
				// 데이터가 설정되어 있지 않습니다. 
				return false;
			}
			
			bool ret = true;
			ret &= Deserialize(ref element.characterId, MovingData.characterNameLength);
			ret &= Deserialize(ref element.goingOut);

			return ret;
		}
	}
	
	// 패킷 데이터의 실체.
	GoingOutData		m_packet;
	
	public GoingOutPacket(GoingOutData data)
	{
		m_packet = data;
	}
	
	public GoingOutPacket(byte[] data)
	{
		GoingOutDataSerializer serializer = new GoingOutDataSerializer();
		
		serializer.SetDeserializedData(data);
		serializer.Deserialize(ref m_packet);
	}
	
	public PacketId	GetPacketId()
	{
		return PacketId.GoingOut;
	}
	
	public GoingOutData	GetPacket()
	{
		return m_packet;
	}
	
	
	public byte[] GetData()
	{
		GoingOutDataSerializer serializer = new GoingOutDataSerializer();
		
		serializer.Serialize(m_packet);
		
		return serializer.GetSerializedData();
	}
}

//
// 채팅 패킷 정의.
//
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
				// 데이터 설정되어 있지 않습니다.
				return false;
			}

			bool ret = true;

			ret &= Deserialize(ref element.characterId, ChatMessage.characterNameLength);
			ret &= Deserialize(ref element.message, ChatMessage.messageLength);

			return true;
		}
	}
	
	// 패킷 데이터의 실체.
	ChatMessage	m_packet;
	
	
	// 패킷 데이터를 시리얼라이즈를 위한 생성사.
	public ChatPacket(ChatMessage data)
	{
		m_packet = data;
	}
	
	// 바이너리 데이터를 패킷 데이터로 디시리얼라이즈하는 생성자.
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
