// 한 대의 단말로 동작시킬 경우에 정의한다.
//#define UNUSE_MATCHING_SERVER

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class GameServer : MonoBehaviour {

	// 게임 서버 버전.
	public const int			SERVER_VERSION = 1; 	

	// 통신 모듈 컴포넌트.
	Network						network_ = null;

	// 세션 관리 정보(노드번호)와 플레이어 ID 연결.
	Dictionary<int, int>		m_nodes = new Dictionary<int, int>();

	// 방 이동 관리.
	Dictionary<string, int>		m_doors = new Dictionary<string, int>();

	// 키를 가진 캐릭터의 비트 마스크.
	//(로 하려고 했지만 본문 중에 시프트하는 바람에 마스크가 되지 않았네요 ^^;).
	private static int 			KEY_MASK = NetConfig.PLAYER_MAX;

	private int					m_playerNum = 0;

	private int 				m_currentPartyMask = 0;

	// 초기 장비 정보 보존.
	Dictionary<int, int> 		m_equips = new Dictionary<int, int>();

	private bool				m_syncFlag = false;
	
	// 미획득 시 소유자 ID.
	const string 				ITEM_OWNER_NONE = "";


	enum PickupState
	{
		None = 0, 				// 미획득.
		PickingUp,				// 획득 중.
		Picked,					// 획득.
		Dropping,				// 폐기 중.
		Dropped,				// 폐기.
	}

	private class ItemState
	{
		public string			itemId = "";					// 유니크한 id.
		public PickupState		state = PickupState.None;		// 아이템 획득 상황.
		public string 			ownerId = ITEM_OWNER_NONE;		// 소유자.
	}

	// 아이템 관리 테이블.
	Dictionary<string, ItemState>		itemTable_;

	private int[]				prizeNum = new int[NetConfig.PLAYER_MAX];	

	// 아이템 정보 수신 플래그.
	private bool				isRecvPrize = false;
	
	// 매칭 서버를 사용하지 않을 때 접속 확인용 킵 얼라이브 Ticker.
	private float[]				m_keepAlive = new float[NetConfig.PLAYER_MAX];

	// 킵 얼라이브 타임아웃.
	private const float 		KEEPALIVE_TIMEOUT = 10.0f;


	void Awake () {
	
		itemTable_ = new Dictionary<string, ItemState>();

		GameObject obj = new GameObject("Network-GameServer");
		network_ = obj.AddComponent<Network>();

		if (network_ != null) {
			DontDestroyOnLoad(network_);

			// 초기 장비 정보 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.Equip, this.OnReceiveEquipmentPacket);
			// 아이템 데이터 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.ItemData, this.OnReceiveItemPacket);
			// 아이템 사용 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.UseItem, this.OnReceiveReflectionPacket);
			// 이동 정보 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.DoorState, this.OnReceiveDoorPacket);
			// 몬스터 발생 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.MonsterData, this.OnReceiveReflectionPacket);
			// HP 정보 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.HpData, this.OnReceiveReflectionPacket);
			// 대미지양 정보 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.DamageData, this.OnReceiveReflectionPacket);
			// 대미지 알림 정보 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.DamageNotify, this.OnReceiveReflectionPacket);
			// 소환수 소환 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.Summon, this.OnReceiveReflectionPacket);
			// 보스 직접 공격 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.BossDirectAttack, this.OnReceiveReflectionPacket);
			// 보스 범위 공격 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.BossRangeAttack, this.OnReceiveReflectionPacket);
			// 보스 속공 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.BossQuickAttack, this.OnReceiveReflectionPacket);
			// 보스 사망 알림 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.BossDead, this.OnReceiveReflectionPacket);
			// 보너스 획득 정보 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.Prize, this.OnReceivePrizePacket);
			// 채팅 메시지 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.ChatMessage, this.OnReceiveReflectionPacket);
		}

	}
	
	// Update is called once per frame
	void Update () {

		// 이벤트 핸들링.
		EventHandling();

		// 초기 장비 동기화 감시.
		checkInitidalEquipment();

		// 방 이동 상태 감시.
		checkDoorOpen();

		// 보너스 케이크 정보 수신 체크.
		checkReceivePrizePacket();

#if UNUSE_MATCHING_SERVER
		// 접속 중인 단말의 동적 체크.
		// 매칭 서버를 사용하지 않을 경우의 접속 단말을 확정하기 위해 체크.
		CheckConnection ();
#endif
	}


	public bool StartServer(int playerNum)
	{
		Debug.Log("Gameserver launched.");

		if (network_ == null) {
			Debug.Log("GameServer start fail.");
			return false; 
		}

		// 참가 인원.
		m_playerNum = playerNum;

		// 참가 플레이어 마스크.
		for (int i = 0; i < m_playerNum; ++i) {
			m_currentPartyMask |= 1 << i;

			prizeNum[i] = 0;
		}

		itemTable_.Clear();
		
		for (int i = 0; i < prizeNum.Length; ++i) {
			prizeNum[i] = 0;
		}
		
		for (int i = 0; i < m_keepAlive.Length; ++i) {
			m_keepAlive[i] = 0.0f;
		}

		return network_.StartServer(NetConfig.GAME_SERVER_PORT, NetConfig.PLAYER_MAX, Network.ConnectionType.Reliable);
	}


	public void StopServer()
	{
		if (network_ == null) {
			Debug.Log("GameServer is not started.");

			return;
		}

		network_.StopServer();

		Debug.Log("Gameserver shutdown.");
	}

	// ================================================================ //


	public void OnReceiveEquipmentPacket(int node, PacketId id, byte[] data)
	{
		EquipmentPacket packet = new EquipmentPacket(data);
		CharEquipment equip = packet.GetPacket();

		Debug.Log("[SERVER] Receive equipment packet [Account:" + equip.globalId + "][Shot:" + equip.shotType + "]");

		// 캐릭터의 장비를 보존.
		if (m_equips.ContainsKey(equip.globalId)) {
			m_equips[equip.globalId] = equip.shotType;
		}
		else {
			m_equips.Add(equip.globalId, equip.shotType);
		}

		// 세션 관리 정보와 플레이어 ID를 연결.
		if (m_nodes.ContainsKey(node) == false) {
			m_nodes.Add(node, equip.globalId);
		}

		m_syncFlag = true;

		// 실제 체크는 checkInitidalEquipment로 매 프레임 한다.
	}

	private void checkInitidalEquipment() 
	{
		if (m_syncFlag == false) {
			return;
		}

		int equipFlag = 0;
		foreach (int index in m_equips.Keys) {
			equipFlag |= 1 << index;
		}

		// 수신한 패킷 데이터로부터 캐릭터 ID와 장비의 수신을 체크.
		equipFlag &= m_currentPartyMask;
		if (equipFlag == m_currentPartyMask) {

			// 전원의 무기 선택 정보가 모였으므로 던전 돌입.
			GameSyncInfo sync = new GameSyncInfo();

			// 게임 서버의 난수로 시드를 결정한다.
			TimeSpan ts = new TimeSpan(DateTime.Now.Ticks);
			double seconds = ts.TotalSeconds;
			sync.seed = (int) ((long)seconds - (long)(seconds/1000.0)*1000);

			Debug.Log("Seed: " + sync.seed);

			// 장비 정보를 저장.
			sync.items = new CharEquipment[NetConfig.PLAYER_MAX];
			for (int i = 0; i < NetConfig.PLAYER_MAX; ++i) {
				sync.items[i].globalId = i;
				if (m_equips.ContainsKey(i)) {
					sync.items[i].shotType = m_equips[i];
				}
				else {
					sync.items[i].shotType = 0;
				}
			}
			
			if (network_ != null) {
				// 각 단말에 알림.
				GameSyncPacket syncPacket = new GameSyncPacket(sync);
				network_.SendReliableToAll<GameSyncInfo>(syncPacket);
			}

			// 매칭 서버를 사용하지 않을 때의 테스트용으로 초기 장비 정보를 클리어해 둔다.
			m_equips.Clear();
			Debug.Log("[SERVER] Clear equipment info.");

			m_syncFlag = false;
		}
	}
	
	public void OnReceiveItemPacket(int node, PacketId id, byte[] data)
	{
		ItemPacket packet = new ItemPacket(data);
		ItemData item = packet.GetPacket();
		
		string log = "[SERVER] ReceiveItemData " +
			"itemId:" + item.itemId +
				" state:" + item.state.ToString() +
				" ownerId:" + item.ownerId;
		Debug.Log(log);
		dbwin.console().print(log);

		PickupState state = (PickupState) item.state;
		switch (state) {
		case PickupState.PickingUp:
			MediatePickupItem(item.itemId, item.ownerId);
			break;
			
		case PickupState.Dropping:
			MediateDropItem(item.itemId, item.ownerId);
			break;
			
		default:
			break;
		}
	}
	
	
	void MediatePickupItem(string itemId, string charId)
	{
		ItemState istate;
		string log = "";
		
		if (itemTable_.ContainsKey(itemId) == false) {
			// 발견되지 않았으므로 신규 아이템으로 만든다.
			istate = new ItemState();

			istate.itemId = itemId;
			istate.state = PickupState.Picked;
			istate.ownerId = charId;
			// 신규 아이템으로 만들어 등록.
			itemTable_.Add(itemId, istate);
			
			log = "[SERVER] Unregisterd item pickedup " +
				"itemId:" + itemId +
					" ownerId:" + istate.ownerId;
			Debug.Log(log);
			dbwin.console().print(log);

			// 아이템의 획득 상황에 변경이 있으면 단말에 알림.
			SendItemState(istate);
			
			return;
		}
		
		istate = itemTable_[itemId];
		
		// 다른 캐릭터가 가지고 있지 않은지 확인.
		if (istate.state == PickupState.None) {
			// 이 아이템은 획득 가능.
			istate.state = PickupState.Picked;
			istate.ownerId = charId;
			// 아이템 정보를 갱신.
			itemTable_[itemId] = istate;
			
			log = "[SERVER] Registerd item picked " +
				"itemId:" + itemId +
					" ownerId:" + istate.ownerId;
			Debug.Log(log);
			dbwin.console().print(log);

			// 아이템 획득 상황에 변경이 있으면 단말에 알림.
			SendItemState(istate);
		}
		else {
			log = "[SERVER] Already item pickedup " +
				"itemId:" + itemId +
					" state:" + istate.state.ToString() +
					" ownerId:" + istate.ownerId;
			dbwin.console().print(log);
			Debug.Log(log);
		}
	}
	
	
	void MediateDropItem(string itemId, string charId)
	{
		string log = "";
		
		// 아이템이 없는 경우는 무시한다.
		if (itemTable_.ContainsKey(itemId) == false) {
			return;
		}
		
		ItemState istate = itemTable_[itemId];
		if (istate.state != PickupState.Picked ||
		    istate.ownerId != charId) {
			// 이 아이템은 폐기할 수 없다.
			log = "[SERVER] Illegal item drop state " +
				"itemId:" + itemId +
					" state:" + istate.state.ToString() +
					" ownerId:" + istate.ownerId;
			Debug.Log(log);
			return;		
		}
		
		// 아이템을 폐기한다.
		istate.state = PickupState.None;
		istate.ownerId = ITEM_OWNER_NONE;
		itemTable_[itemId] = istate;
		
		log = "[SERVER] Item dropped " +
			"itemId:" + itemId +
				" state:" + istate.state.ToString() +
				" ownerId:" + istate.ownerId;
		Debug.Log(log);
		
		// 아이템이 폐기됐음을 게스트에 알림.
		SendItemState(istate);
	}

	
	bool SendItemState(ItemState state)
	{
		// 아이템 획득 응답.
		ItemData data = new ItemData();
		
		data.itemId = state.itemId;
		data.ownerId = state.ownerId;
		data.state = (state.state == PickupState.None)? (int)PickupState.Dropped : (int)state.state;
		
		string log = "[SERVER] Send Item State" +
			"itemId:" + data.itemId +
				" state:" + data.state.ToString() +
				" ownerId:" + data.ownerId;
		Debug.Log(log);
		
		ItemPacket packet = new ItemPacket(data);
		network_.SendReliableToAll<ItemData>(packet);

		return true;
	}
	
	public void OnReceiveDoorPacket(int node, PacketId id, byte[] data)
	{
		DoorPacket packet = new DoorPacket(data);
		CharDoorState door = packet.GetPacket();

		string log = "[SERVER] DoorPacket " +
				"keyId:" + door.keyId +
				" globalId:" + door.globalId +
				" is in:" + door.isInTrigger +
				" hasKey:" + door.hasKey;
		Debug.Log(log);

		int doorFlag = 0;
		if (m_doors.ContainsKey(door.keyId)) {
			// 이미 누군가가 올라탄 도어.
			doorFlag = m_doors[door.keyId];
		}
		else {
			// 신규 도어.
			m_doors.Add(door.keyId, doorFlag);
		}

		// 수신 패킷 데이터에서 캐릭터의 ID와 열쇠의 소유 상태를 업데이트.
		if (door.isInTrigger) {
			doorFlag |= 1 << door.globalId;
			if (door.hasKey) {
				doorFlag |= 1 << KEY_MASK;
			}
		}
		else {
			doorFlag &= ~(1 << door.globalId);
			if (door.hasKey) {
				doorFlag &= ~(1 << KEY_MASK);
			}
		}

		log = "[SERVER] Door flag keyId:" + door.keyId + ":" + Convert.ToString(doorFlag, 2).PadLeft(5,'0');
		Debug.Log(log);

		// 상태 갱신.
		m_doors[door.keyId] = doorFlag;
		
		// 실제 체크는 checkDoorOpen으로 매 프레임 한다.
	}

	private void checkDoorOpen()
	{
		Dictionary<string, int> doors = new Dictionary<string, int>(m_doors);

		foreach (string keyId in doors.Keys) {

			// 수신한 패킷 데이터에서 캐릭터의 ID와 열쇠의 소유 상태를 보존.
			int doorFlag = m_doors[keyId];

			int mask = ((1 << KEY_MASK) | m_currentPartyMask);

			doorFlag &= mask;
			if (doorFlag == mask) {
				// 열쇠를 가지고 전우너 도넛에 올라탔으므로 방 이동 알림.
				MovingRoom room = new MovingRoom();
				room.keyId = keyId;
	
				string log = "[SERVER] Room move Packet " + "keyId:" + room.keyId;
				Debug.Log(log);

				RoomPacket roomPacket = new RoomPacket(room);
				
				if (network_ != null) {
					network_.SendReliableToAll<MovingRoom>(roomPacket);
				}
				
				// 사용이 끝났으므로 클리어.
				m_doors[keyId] = 0;
			}
		}
	}

	public void OnReceivePrizePacket(int node, PacketId id, byte[] data)
	{
		PrizePacket packet = new PrizePacket (data);
		PrizeData prize = packet.GetPacket ();

		int gid = getGlobalIdFromName(prize.characterId);

		string log = "[SERVER] Recv prize Packet[" + prize.characterId + "]:" + prize.cakeNum;
		Debug.Log(log);

		if (gid < 0) {
			return;
		}

		prizeNum[gid] = prize.cakeNum;

		// 케이크 획득 정보 감시 시작.
		isRecvPrize = true;
	}

	private int getGlobalIdFromName(string name)
	{
		AccountData data = AccountManager.getInstance().getAccountData(name);

		return data.global_index;
	}

	private void checkReceivePrizePacket()
	{
		if (isRecvPrize == false) {
			return;
		}

		for (int i = 0; i < NetConfig.PLAYER_MAX; ++i) {
			int node = network_.GetClientNode(i);
			if (network_.IsConnected(node) && prizeNum[i] < 0) {
				// 아직 모이지 않았다.
				return;
			}
		}

		PrizeResultData data = new PrizeResultData ();

		// 각 클라이언트에 획득 결과 알림.
		data.cakeDataNum = NetConfig.PLAYER_MAX;
		data.cakeNum = new int[NetConfig.PLAYER_MAX];
		for (int i = 0; i < data.cakeDataNum; ++i) {
			data.cakeNum[i] = prizeNum[i];
		}

		PrizeResultPacket packet = new PrizeResultPacket(data);
		network_.SendReliableToAll(packet);

		isRecvPrize = false;
	}

	public void OnReceiveReflectionPacket(int node, PacketId id, byte[] data)
	{
		if (network_ != null) {
			//Debug.Log("[SERVER]OnReceiveReflectionData from node:" + node);
			network_.SendReliableToAll(id, data);
		}
	}


	// ================================================================ //

	private void DisconnectClient(int node)
	{
		Debug.Log("[SERVER]DisconnectClient");
		
		network_.Disconnect(node);

		if (m_nodes.ContainsKey(node) == false) {
			return;
		}

		// 현재 접속 중인 클라이언트의 플래그를 반전시킨다.
		int gid = m_nodes[node];
		m_currentPartyMask &= ~(1 << gid);
	}

	// ================================================================ //

	public void EventHandling()
	{
		NetEventState state = network_.GetEventState();

		if (state == null) {
			return;
		}
				
		switch (state.type) {
		case NetEventType.Connect:
			Debug.Log("[SERVER]NetEventType.Connect");
			break;
			
		case NetEventType.Disconnect:
			Debug.Log("[SERVER]NetEventType.Disconnect");
			DisconnectClient(state.node);
			break;
		}
	}

	// ================================================================ //

#if UNUSE_MATCHING_SERVER
	private void CheckConnection()
	{
		int[] nodeList = new int[NetConfig.PLAYER_MAX];

		for (int i = 0; i < nodeList.Length; ++i) {
			nodeList[i] = -1;
		}

		foreach (int node in m_nodes.Keys) {
			int gid = m_nodes[node];

			nodeList[gid] = node;
		}


		for (int i = 0; i < NetConfig.PLAYER_MAX; ++i) {

			int node = nodeList[i];
			if (node >= 0) {
				m_keepAlive[i] = 0.0f;
			}
			else {
				m_keepAlive[i] += Time.deltaTime;
			}

			int mask = m_currentPartyMask & (1 << i);
			if (mask != 0 && m_keepAlive[i] > KEEPALIVE_TIMEOUT) {
				Debug.Log("[SERVER] KeepAlive timeout [" + i + "]:" + node);
				m_currentPartyMask &= ~(1 << i);

				Debug.Log("Current player mask:" + Convert.ToString(m_currentPartyMask, 2).PadLeft(4,'0'));
			}
		}
	}
#endif
}
