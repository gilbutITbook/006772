using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 캐릭터 생성 등.
public class CharacterRoot : MonoBehaviour {


	public GameObject[]			chr_model_prefabs = null;		// 모델의 프리팹.

	protected AcountManager		account_man;						// 어카운트 매니저.
	protected GameInput			game_input;						// 마우스 등의 입력.

	protected List<QueryBase>		queries = new List<QueryBase>();		// 쿼리.

	private Network				m_network = null;

	// ================================================================ //
	
	private bool				guestConnected = false;
	
	private bool				guestDisconnected = false;

	// ================================================================ //
	// MonoBehaviour로부터 상속 .

	void	Start()
	{
		this.account_man = this.gameObject.GetComponent<AcountManager>();
		this.game_input = this.gameObject.GetComponent<GameInput>();


		m_network = GameObject.Find("Network").GetComponent<Network>();
		if (m_network != null) {
			m_network.RegisterReceiveNotification(PacketId.CharacterData, OnReceiveCharacterPacket);
			m_network.RegisterReceiveNotification(PacketId.Moving, OnReceiveMovingPacket);
			m_network.RegisterReceiveNotification(PacketId.ChatMessage, OnReceiveChatMessage);
			m_network.RegisterReceiveNotification(PacketId.GameSyncInfoHouse, OnReceiveSyncGamePacket);
			m_network.RegisterEventHandler(OnEventHandling);
		}
	}
	
	void	Update()
	{
		this.process_query();

		// 원격 접속 시의 처리
		if (guestConnected) {
			SendGameSyncInfo();
			guestConnected = false;
		}
	}

	// 쿼리 갱신.
	protected void	process_query()
	{
		// 페일 세이프 ＆ 개발용.

		foreach(var query in this.queries) {

			//Debug.Log("[Query]" + query.timer);

			query.timer += Time.deltaTime;

			if(m_network == null) {

				// GameScene부터 시작했을 때（Title거치지 않음）.
				// 네트워크 오브젝트가 만들어지지 않았다.
				query.set_done(true);
				query.set_success(true);

			} else {

				// 타임아웃.
				if(query.timer > 5.0f) {

					query.set_done(true);
					query.set_success(false);
				}
			}
		}

		this.queries.RemoveAll(x => x.isDone());
	}

	// ================================================================ //

	public void		deletaCharacter(chrController character)
	{
		GameObject.Destroy(character.gameObject);
	}

	// 로컬 플레이어의 캐릭터를 만듭니다.
	public chrController		createPlayerAsLocal(string chr_name)
	{
		chrController	chr_control = this.create_chr_common(chr_name, "chrBehaviorLocal");

		return(chr_control);
	}

	// 네트 플레이어 캐릭터를 만듭니다.
	public chrController		createPlayerAsNet(string chr_name)
	{
		chrController	chr_control = this.create_chr_common(chr_name, "chrBehaviorNet");

		return(chr_control);
	}

	// NPC 캐릭터를 만듭니다.
	public chrController		createNPC(string chr_name)
	{
		chrController	chr_control = this.create_chr_common(chr_name, "chrBehaviorNPC");

		return(chr_control);
	}

	// 플레이어를 만듭니다.
	private chrController		create_chr_common(string name, string behavior_class_name)
	{
		chrController	chr_control = null;

		do {

			// 모델의 프리팹을 찾습니다.
	
			GameObject	prefab = null;
	
			string	prefab_name = "ChrModel_" + name;
	
			prefab = System.Array.Find(this.chr_model_prefabs, x => x.name == prefab_name);
	
			if(prefab == null) {
	
				Debug.LogError("Can't find prefab \"" + prefab_name + "\".");
				break;
			}
	
			//
	

			GameObject	go = GameObject.Instantiate(prefab) as GameObject;

			go.name = name;

			// 리기드바디.

			Rigidbody	rigidbody = go.AddComponent<Rigidbody>();

			rigidbody.constraints  = RigidbodyConstraints.None;
        	rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
	       	rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;

			// 컨트롤러.

			chr_control = go.AddComponent<chrController>();

			chr_control.model       = go;
			chr_control.account_name = name;
			chr_control.account_man  = this.account_man;

			chr_control.game_input  = this.game_input;

			// 비헤이비어.

			chr_control.behavior = go.GetComponent<chrBehaviorBase>();

			if(chr_control.behavior == null) {

				chr_control.behavior = go.AddComponent(behavior_class_name) as chrBehaviorBase;

			} else {

				// 비헤이비어가 처음부터 붙어있을 때는 만들지 않습니다.
			}

			chr_control.behavior.controll = chr_control;
			chr_control.behavior.initialize();

		} while(false);

		return(chr_control);
	}

	// 캐릭터를 찾습니다.
	public chrController	findCharacter(string name)
	{
		chrController	character = null;

		do {

			GameObject[]	characters = GameObject.FindGameObjectsWithTag("Charactor");

			if(characters.Length == 0) {

				break;
			}

			GameObject		go;

			go = System.Array.Find(characters, x => x.name == name);

			if(go == null) {

				break;
			}

			character = go.GetComponent<chrController>();

			if(character == null) {

				Debug.Log("Cannot find character:" + name);

				go = GameObject.Find(name);

				if(go != null) {

					break;
				}
				character = go.GetComponent<chrController>();
			}

		} while(false);

		return(character);
	}

	// 캐릭터를 찾습니다.
	public T	findCharacter<T>(string name) where T : chrBehaviorBase
	{
		T		behavior = null;

		do {

			chrController	character = this.findCharacter(name);

			if(character == null) {

				break;
			}

			behavior = character.behavior as T;

		} while(false);

		return(behavior);
	}

	// 게임 입력(마우스 등)을 얻습니다.
	public GameInput	getGameInput()
	{
		return(this.game_input);
	}

	// ================================================================ //
	// 비헤이비어용 커맨드.
	// 쿼리 계통.

	// 쿼리-말하기(말풍선）.
	public QueryTalk	queryTalk(string words, bool local = true)
	{
		QueryTalk		query = null;

		do {

			query = new QueryTalk(words);

			this.queries.Add(query);

		} while(false);

		GameObject netObj = GameObject.Find("Network");
		if (netObj && local) {		
			// Network 클래스의 컴포넌트 획득합니다.
			Network network  = netObj.GetComponent<Network>();
			// 말풍선 요청을 보냅니다.
			ChatMessage chat = new ChatMessage();
			chat.characterId = GameRoot.getInstance().account_name_local;
			chat.message = words;
			ChatPacket packet = new ChatPacket(chat);
			network.SendReliable<ChatMessage>(packet);
		}

		return(query);
	}

	// 쿼리-이사 시작해도 되는가?.
	public QueryHouseMoveStart	queryHouseMoveStart(string house_name, bool local = true)
	{
		QueryHouseMoveStart		query = null;

		do {
			
			chrBehaviorNPC_House	house = CharacterRoot.getInstance().findCharacter<chrBehaviorNPC_House>(house_name);

			if(house == null) {

				break;
			}			

			query = new QueryHouseMoveStart(house_name);

			this.queries.Add(query);

		} while(false);

		// 이사 시작 요청을 보냅니다.
		GameObject netObj = GameObject.Find("Network");
		if (netObj && local) {		
			// Network 클래스의 컴포넌트 획득합니다.
			Network network  = netObj.GetComponent<Network>();
			// 이사 시작 요청을 보냅니다.
			MovingData moving = new MovingData();
			moving.characterId = GameRoot.getInstance().account_name_local;
			moving.houseId = house_name;
			moving.moving = true;
			MovingPacket packet = new MovingPacket(moving);
			network.SendReliable<MovingData>(packet);

			// 이사 정보 보존.
			GlobalParam.get().local_moving = moving;
		}

		return(query);
	}

	// 쿼리 - 이사를 마쳐도 되는가？.
	public QueryHouseMoveEnd	queryHouseMoveEnd(bool local = true)
	{
		QueryHouseMoveEnd		query = null;

		do {

			query = new QueryHouseMoveEnd();

			this.queries.Add(query);

		} while(false);

		// 이사 종료 요청을 보냅니다.
		GameObject netObj = GameObject.Find("Network");
		if (netObj && local) {		
			// Network 클래스의 컴포넌트를 획득합니다.
			Network network  = netObj.GetComponent<Network>();
			// 이사 종료 요청을 보냅니다.
			MovingData moving = new MovingData();
			moving.characterId = GameRoot.getInstance().account_name_local;
			moving.houseId = "";
			moving.moving = false;
			MovingPacket packet = new MovingPacket(moving);
			network.SendReliable<MovingData>(packet);
		}

		return(query);
	}

	// 아바타 이름으로 플레이어 캐릭터를 찾습니다.
	public chrController	findPlayer(string avator_id)
	{
		GameObject[]	characters = GameObject.FindGameObjectsWithTag("Player");

		chrController	character = null;
		
		foreach(GameObject go in characters) {
			
			chrController	chr = go.GetComponent<chrController>();
			AcountData		account_data = AcountManager.get().getAccountData(chr.global_index);
			
			if(account_data.avator_id == avator_id) {
				
				character = chr;
				break;
			}
		}

		if (character == null) {
			GameObject go = GameObject.Find(avator_id);
			if (go != null) {
				character = go.GetComponent<chrController>();
			}
		}

		return(character);
	}


	// ================================================================ //

	private	static CharacterRoot	instance = null;

	public static CharacterRoot	getInstance()
	{
		if(CharacterRoot.instance == null) {

			CharacterRoot.instance = GameObject.Find("GameRoot").GetComponent<CharacterRoot>();
		}

		return(CharacterRoot.instance);
	}

	public static CharacterRoot	get()
	{
		return(CharacterRoot.getInstance());
	}


	public void SendCharacterCoord(string charId, int index, List<CharacterCoord> character_coord)
	{
		GameObject netObj = GameObject.Find("Network");
		if(netObj) {		
			// Network 클래스의 컴포넌트를 획득.
			Network network  = netObj.GetComponent<Network>();
			if (network.IsConnected() == true) {
				// 패킷 데이터 생성.
				CharacterData data = new CharacterData();
				
				data.characterId = charId;
				data.index = index;
				data.dataNum = character_coord.Count;
				data.coordinates = new CharacterCoord[character_coord.Count];
				for (int i = 0; i < character_coord.Count; ++i) {
					data.coordinates[i] = character_coord[i];
				}

				// 캐릭터 좌표 송신.
				CharacterDataPacket packet = new CharacterDataPacket(data);
				int sendSize = network.SendUnreliable<CharacterData>(packet);
				if (sendSize > 0) {
				//	Debug.Log("Send character coord.[index:" + index + "]");
				}
			}
		}
	}

	
	public void 	OnReceiveCharacterPacket(PacketId id, byte[] data)
	{
		CharacterDataPacket packet = new CharacterDataPacket(data);
		CharacterData charData = packet.GetPacket();

		if (GlobalParam.get().is_in_my_home != GlobalParam.get().is_remote_in_my_home) {
			return;
		}

		chrBehaviorNet remote = CharacterRoot.get().findCharacter<chrBehaviorNet>(charData.characterId);
		if (remote != null) {
			remote.CalcCoordinates(charData.index, charData.coordinates);
		}
	}

	public void 	OnReceiveMovingPacket(PacketId id, byte[] data)
	{
		Debug.Log("OnReceiveMovingPacket");

		MovingPacket packet = new MovingPacket(data);
		MovingData moving = packet.GetPacket();
		
		Debug.Log("[CharId]" + moving.characterId);
		Debug.Log("[HouseName]" + moving.houseId);
		Debug.Log("[Moving]" + moving.moving);

		chrController remote =
			CharacterRoot.get().findCharacter(moving.characterId);
		
		// 이사 쿼리 발행.
		if (remote != null) {
			if (moving.moving) {
				Debug.Log("cmdQueryHouseMoveStart");
				QueryHouseMoveStart query = remote.cmdQueryHouseMoveStart(moving.houseId, false);
				if (query != null) {
					query.set_done(true);
					query.set_success(true);
				}
			}
			else {
				Debug.Log("cmdQueryHouseMoveEnd");
				QueryHouseMoveEnd query = remote.cmdQueryHouseMoveEnd(false);
				if (query != null) {
					query.set_done(true);
					query.set_success(true);
				}
			}
		}

		// 이사 정보 보존.
		GlobalParam.get().remote_moving = moving;
	}
	
	public void 	OnReceiveChatMessage(PacketId id, byte[] data)
	{
		Debug.Log("OnReceiveChatMessage");

		ChatPacket packet = new ChatPacket(data);
		ChatMessage chat = packet.GetPacket();

		Debug.Log("{CharId]" + chat.characterId);
		Debug.Log("[CharMsg]" + chat.message);

		chrController remote =
			CharacterRoot.get().findCharacter(chat.characterId);

		// 채팅 메시지 표시 쿼리 발행.
		if (remote != null) {
			QueryTalk talk = remote.cmdQueryTalk(chat.message);
			if (talk != null) {
				talk.set_done(true);
				talk.set_success(true);
			}
		}
	}

	public void OnReceiveSyncGamePacket(PacketId id, byte[] data)
	{
		Debug.Log("Receive GameSyncPacket[CharacterRoot].");
		
		SyncGamePacket packet = new SyncGamePacket(data);
		SyncGameData sync = packet.GetPacket();

		Debug.Log("[CharId]" + sync.moving.characterId);
		Debug.Log("[HouseName]" + sync.moving.houseId);
		Debug.Log("[Moving]" + sync.moving.moving);

		if (sync.moving.characterId.Length == 0) {
			// 이사하지 않았다.
			return;
		}

		// 이사 정보 보존.
		GlobalParam.get().remote_moving = sync.moving;
	}

	// ================================================================ //
	
	private void SendGameSyncInfo()
	{
		Debug.Log("[CLIENT]SendGameSyncInfo");

		SyncGameData data = new SyncGameData();

		data.version = GameServer.SERVER_VERSION;
		data.itemNum = 0;

		// 호스트에서는 이사 정보만 보냅니다.
		data.moving = new MovingData();
		if (GlobalParam.get().local_moving.moving) {
			data.moving = GlobalParam.get().local_moving;
		}
		else {
			data.moving.characterId = "";
			data.moving.houseId = "";
			data.moving.moving = false;
		}

		SyncGamePacketHouse packet = new SyncGamePacketHouse(data);
		if (m_network != null) {
			m_network.SendReliable<SyncGameData>(packet);
		}
	}

	// ================================================================ //
	
	
	public void OnEventHandling(NetEventState state)
	{
		switch (state.type) {
		case NetEventType.Connect:
			Debug.Log("connect event");
			guestConnected = true;
			break;
			
		case NetEventType.Disconnect:
			guestDisconnected = true;
			break;
		}
	}

}

