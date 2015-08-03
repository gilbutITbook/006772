using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;


// 캐릭터 생성 등.
public class CharacterRoot : MonoBehaviour {

	public GameObject[]			chr_model_prefabs = null;		// 모델 프리팹.
#if false
	private AcountManager		account_man;						// 어카운트 매니저.
#endif

	private chrController	player = null;

	public Material		damage_material = null;					// 대미지 연출용 머티리얼.
	public Material		vanish_material = null;					// 퇴장 연출용 머티리얼.

	public GameObject	player_bullet_negi_prefab = null;		// 플레이어 샷 프리팹（파）.
	public GameObject	player_bullet_yuzu_prefab = null;		// 플레이어 샷 프리팹(유자).

	public GameObject	enemy_bullet_prefab = null;				// 적 탄환용 프리팹.

	private Network 	m_network = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
#if false
		this.account_man = this.gameObject.GetComponent<AcountManager>();
#endif
		
		// Network 클래스의 컴포넌트 획득.
		GameObject obj = GameObject.Find("Network");
		
		if(obj != null) {
			m_network = obj.GetComponent<Network>();
			if (m_network != null) {
				m_network.RegisterReceiveNotification(PacketId.CharacterData, OnReceiveCharacterPacket);
				m_network.RegisterReceiveNotification(PacketId.AttackData, OnReceiveAttackPacket);
				m_network.RegisterReceiveNotification(PacketId.ChatMessage, OnReceiveChatMessage);
				m_network.RegisterReceiveNotification(PacketId.HpData, OnReceiveHitPointPacket);
				m_network.RegisterReceiveNotification(PacketId.DamageData, OnReceiveDamageDataPacket);
				m_network.RegisterReceiveNotification(PacketId.DamageNotify, OnReceiveDamageNotifyPacket);
				// 소환수 관리는 PartyControl로 변경했습니다.
				// 따라서 소환수 출현 패킷 수신 함수도 PartyControl로 이동했습니다.
				//m_network.RegisterReceiveNotification(PacketId.Summon, OnReceiveSummonPacket);
			}
		}
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	public chrController		getPlayer()
	{
		return(this.player);
	}

	// 로컬 플레이어의 캐릭터를 만든다.
	public chrController		createPlayerAsLocal(string chr_name)
	{
		chrController	chr_control = this.create_chr_common(chr_name, "chrBehaviorLocal");

		CameraControl.getInstance().setPlayer(chr_control);

		this.player = chr_control;

		return(chr_control);
	}

	// 적 캐릭터를 만든다.
	public chrController		createEnemy(string chr_name)
	{
		string		behavior_class_name = "chrBehavior" + chr_name;

		chrController	chr_control = this.create_chr_common(chr_name, "", behavior_class_name);

		return(chr_control);
	}

	// 적 캐릭터를 만든다.
	public chrController		createEnemy(string chr_name, string controller_class_name, string behavior_class_name)
	{
		return create_chr_common(chr_name, controller_class_name, behavior_class_name);
	}

	// 네트워크 플레이어 캐릭터를 만든다.
	public chrController		createPlayerAsNet(string chr_name)
	{
		chrController	chr_control = this.create_chr_common(chr_name, "chrBehaviorNet");

		return(chr_control);
	}

	// 페이크 네트워크 플레어 캐릭터를 만든다.
	public chrController		createPlayerAsFakeNet(string chr_name)
	{
		chrController	chr_control = this.create_chr_common(chr_name, "chrBehaviorFakeNet");

		return(chr_control);
	}

	// 소환수를 소환한다.
	public chrController	summonBeast(string chr_name, string behavior_name)
	{
		chrController	chr_control = this.create_chr_common(chr_name, behavior_name);

		return(chr_control);
	}

	// NPC 캐릭터를 만든다.
	public chrController		createNPC(string chr_name)
	{
		chrController	chr_control = this.create_chr_common(chr_name, "chrBehaviorNPC");

		return(chr_control);
	}

	// 디폴트 컨트롤러 클래스로(플레이어를) 만든다.
	private chrController		create_chr_common(string name, string behavior_class_name)
	{
		return create_chr_common(name, "chrController", behavior_class_name);
	}
	
	
	// 플레이어를 만든다.
	private chrController		create_chr_common(string name, string controller_class_name, string behavior_class_name)
	{
		chrController	chr_control = null;

		do {

			// 모델 프리팹을 찾는다.

			GameObject	prefab = null;
	
			string	prefab_name = "ChrModel_" + name;

			if (this.chr_model_prefabs == null)
			{
				Debug.LogError("chr model prefabs is null");
				break;
			}

			prefab = System.Array.Find(this.chr_model_prefabs, x => x.name == prefab_name);
	
			if(prefab == null) {
	
				Debug.LogError("Can't find prefab \"" + prefab_name + "\".");
				break;
			}

			//
	

			GameObject	go = GameObject.Instantiate(prefab) as GameObject;

			go.name = name;

			// 리지드바디.

			Rigidbody	rigidbody = go.AddComponent<Rigidbody>();

			rigidbody.constraints = RigidbodyConstraints.None;
			rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
       		rigidbody.constraints |= RigidbodyConstraints.FreezeRotationX;
			rigidbody.constraints |= RigidbodyConstraints.FreezeRotationY;
			rigidbody.constraints |= RigidbodyConstraints.FreezeRotationZ;

			// 컨트롤러.
			// (*)컨트롤러는 로컬 휴먼 플레이어 / 네트 휴먼 플레이어 / AI에 의존하지 않고 불변이 되는 요소가 기술된 클래스.
			chr_control = go.GetComponent<chrController>();

			if(chr_control == null) {

				if(controller_class_name != "") {

					//chr_control = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(go, "Assets/Script/Character/CharacterRoot.cs (184,20)", controller_class_name) as chrController;
					Assembly asm1 = typeof(chrController).Assembly;
					Type type1 = asm1.GetType(controller_class_name);
					chr_control = (chrController)go.AddComponent(type1);

				}

				// 컨트롤러를 만들 수 없다 = 전용 컨트롤러가 없을 때는.
				// 기본 컨트롤러를 추가한다.
				if(chr_control == null) {
	
					chr_control = go.AddComponent<chrController>();
				}
			}

			// 비헤이비어.

			chr_control.behavior = go.GetComponent<chrBehaviorBase>();

			if(chr_control.behavior == null) {

				//chr_control.behavior = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(go, "Assets/Script/Character/CharacterRoot.cs (201,28)", behavior_class_name) as chrBehaviorBase;
				Assembly asm = typeof(chrBehaviorBase).Assembly;
				Type type = asm.GetType(behavior_class_name);
				chr_control.behavior = (chrBehaviorBase)go.AddComponent(type);

			} else {

				// 비헤이비어가 처음부터 추가되어 있을 때는 만들지 않는다.
			}

			chr_control.behavior.control  = chr_control;
			chr_control.behavior.initialize();


		} while(false);

		return(chr_control);
	}

	// 아바타 이름으로 플레이어 캐릭터를 찾는다.
	public chrController	findPlayer(string avator_id)
	{
		GameObject[]	charactors = GameObject.FindGameObjectsWithTag("Player");

		chrController	charactor = null;

		foreach(GameObject go in charactors) {

			chrController	chr          = go.GetComponent<chrController>();

			if (chr.global_index < 0) {
			
				break;
			}

			AccountData		account_data = AccountManager.get().getAccountData(chr.global_index);

			if(account_data.avator_id == avator_id) {

				charactor = chr;
				break;
			}
		}

		return(charactor);
	}

	// 캐릭터를 오브젝트 이름으로 찾는다.
	public T findCharacter<T>(string name) where T : chrBehaviorBase
	{
		T	behavior = null;

		do {

			GameObject	go = GameObject.Find(name);

			if(go == null) {

				break;
			}

			var	chr = go.GetComponent<chrController>();

			if(chr == null) {

				break;
			}

			behavior = chr.behavior as T;

		} while(false);

		return(behavior);
	}


	// ================================================================ //
	// 비헤이비어용 커맨드.
	// 쿼리 계.

	// 쿼리　대화(말풍선).
	public QueryTalk	queryTalk(string account_id, string words, bool is_local)
	{
		QueryTalk		query = null;
		
		do {
			
			query = new QueryTalk(account_id, words);

			if (query == null) {

				break;
			}

			QueryManager.get().registerQuery(query);


			if (m_network !=null && is_local) {		
				// 말풍선 요청을 보낸다.
				ChatMessage chat = new ChatMessage();

				chat.characterId = PartyControl.get().getLocalPlayer().getAcountID();
				chat.message = words;

				ChatPacket packet = new ChatPacket(chat);

				int serverNode = m_network.GetServerNode();
				m_network.SendReliable<ChatMessage>(serverNode, packet);
			}
		} while(false);
		

		return(query);
	}

	// ---------------------------------------------------------------- //

	public void SendAttackData(string charId, int attacKind)
	{
		if (m_network != null) {
			AttackData data = new AttackData();
			
			data.characterId = charId;
			data.attackKind = attacKind;

			AttackPacket packet = new AttackPacket(data);
			m_network.SendUnreliableToAll<AttackData>(packet);
		}
	}
	
	public void SendCharacterCoord(string charId, int index, List<CharacterCoord> character_coord)
	{
		if (m_network != null) {
			// 패킷 데이터 만들기.
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
			m_network.SendUnreliableToAll<CharacterData>(packet);
		}
	}

	public void NotifyDamage(string charId, int attacker, float damage)
	{
		if (m_network != null) {
			DamageData data = new DamageData();
			
			data.target = charId;
			data.attacker = attacker;
			data.damage = damage;
			
			DamageNotifyPacket packet = new DamageNotifyPacket(data);
			m_network.SendReliableToAll<DamageData>(packet);
		}
	}
	
	public void NotifyHitPoint(string charId, float hp)
	{
		if(m_network != null) {		
			// 패킷 데이터 작성.
			HpData data = new HpData();
			
			data.characterId = charId;
			data.hp = hp;

			// 캐릭터 좌표 송신.
			HitPointPacket packet = new HitPointPacket(data);
			m_network.SendReliableToAll<HpData>(packet);
		}
	}

	// ---------------------------------------------------------------- //

	public void 	OnReceiveCharacterPacket(int node, PacketId id, byte[] data)
	{
		CharacterDataPacket packet = new CharacterDataPacket(data);
		CharacterData charData = packet.GetPacket();
		
		chrController controller = findPlayer(charData.characterId);
		
		if(controller == null) {
			return;
		}

		// 캐릭터 좌표 보간.
		chrBehaviorNet behavior = controller.behavior as chrBehaviorNet;
		if (behavior != null) {
			behavior.CalcCoordinates(charData.index, charData.coordinates);
		}
	}

	public void 	OnReceiveAttackPacket(int node, PacketId id, byte[] data)
	{
		AttackPacket packet = new AttackPacket(data);
		AttackData attack = packet.GetPacket();

		//Debug.Log("[CLIENT] Receive Attack packet:" + attack.characterId);

		chrController controller = findPlayer(attack.characterId);
		
		if(controller == null) {
			return;
		}
		
		// 캐릭터 좌표 보간.
		chrBehaviorNet behavior = controller.behavior as chrBehaviorNet;
		if (behavior != null) {
			if (attack.attackKind == 0) {
				behavior.cmdShotAttack();
			}
			else {
				behavior.cmdMeleeAttack();
			}
		}
	}

	public void OnReceiveMovingRoomPacket(int node, PacketId id, byte[] data)
	{
#if false
		RoomPacket packet = new RoomPacket(data);
		MovingRoom room = packet.GetPacket();
		
		// 방 이동 커맨드 발행.
		PartyControl.getInstance().cmdMoveRoom(room.keyId);
#endif
	}


	// HP 통지 정보 수신 함수.
	public void OnReceiveHitPointPacket(int node, PacketId id, byte[] data)
	{
		HitPointPacket packet = new HitPointPacket(data);
		HpData hpData = packet.GetPacket();

		//Debug.Log("[CLIENT] Receive hitpoint packet:" + hpData.characterId);

		chrBehaviorBase behavior = findCharacter<chrBehaviorBase>(hpData.characterId);

		if (behavior == null) {
			return;
		}

		chrController controller = behavior.control;

		if (controller == null) {
			return;
		}

		if (controller.global_index < 0) {
			return;
		}

		//string log = "[CLIENT] Set HP:" + hpData.characterId + " HP:" + hpData.hp;
		//Debug.Log(log);
		
		// 캐릭터의 HP 반영.
		controller.setHitPoint(hpData.hp);
	}

	// 대미지양 통지 정보 수신 함수.
	public void OnReceiveDamageDataPacket(int node, PacketId id, byte[] data)
	{
		DamageDataPacket packet = new DamageDataPacket(data);
		DamageData damage = packet.GetPacket();

		//string log = "ReceiveDamageDataPacket:" + damage.target + "(" + damage.attacker + ") Damage:" + damage.damage;
		//Debug.Log(log);

		if (m_network == null || GameRoot.get().isHost()  == false) {
			// 호스트에 가는 통지 패킷이기에 다른 단말은 무시한다.
			return;
		}

		DamageNotifyPacket sendPacket = new DamageNotifyPacket(damage);

		m_network.SendReliableToAll<DamageData>(sendPacket);
	}

	// 대미지양 통지 정보 수신 함수.
	public void OnReceiveDamageNotifyPacket(int node, PacketId id, byte[] data)
	{
		DamageNotifyPacket packet = new DamageNotifyPacket(data);
		DamageData damage = packet.GetPacket();
#if false
		string avator_name = "";
		AccountData	account_data = AccountManager.get().getAccountData(damage.attacker);
		avator_name = account_data.avator_id;

		string log = "ReceiveDamageNotifyPacket:" + damage.target + "(" + damage.attacker + ") Damage:" + damage.damage;
		Debug.Log(log);
#endif
		chrBehaviorEnemy behavior = findCharacter<chrBehaviorEnemy>(damage.target);
		if (behavior == null) {
			return;
		}

		//log = "Cause damage:" + avator_name + " -> " + damage.target + " Damage:" + damage.damage;
		//Debug.Log(log);

		// 캐릭터의 대미지를 반영.
		behavior.control.causeDamage(damage.damage, damage.attacker, false);
	}

#if false
	// 소환수 출현 정보 수신 함수.
	public void OnReceiveSummonPacket(int node, PacketId id, byte[] data)
	{
		// 소화수 관리가 PartyControl로 변경되었으므로.
		// 소환수 출현 패킷 수신 함수도 PartyControl로 이동했다.
	}
#endif

	public void 	OnReceiveChatMessage(int node, PacketId id, byte[] data)
	{
		Debug.Log("OnReceiveChatMessage");
		
		ChatPacket packet = new ChatPacket(data);
		ChatMessage chat = packet.GetPacket();
		
		Debug.Log("[CharId]" + chat.characterId);
		Debug.Log("[CharMsg]" + chat.message);
		

		chrController controller = findPlayer(chat.characterId);
		// 채팅 메시지 표시 쿼리 발행.
		if (controller != null) {
			QueryTalk talk = queryTalk(chat.characterId, chat.message, false);
			if (talk != null) {
				talk.set_done(true);
				talk.set_success(true);
			}
		}
	}

	// ================================================================ //
	
	private void SendGameSyncInfo()
	{
		Debug.Log("[CLIENT]SendGameSyncInfo");
		
		GameSyncInfo data = new GameSyncInfo();
		
		data.seed = 0;
		data.items = new CharEquipment[NetConfig.PLAYER_MAX];
		
		GameSyncPacket packet = new GameSyncPacket(data);
		if (m_network != null) {
			int serverNode = m_network.GetServerNode();
			m_network.SendReliable<GameSyncInfo>(serverNode, packet);
		}
	}

	// ================================================================ //

	private	static CharacterRoot	instance = null;

	public static CharacterRoot	get()
	{
		if(CharacterRoot.instance == null) {

			CharacterRoot.instance = GameObject.Find("CharacterRoot").GetComponent<CharacterRoot>();
		}

		return(CharacterRoot.instance);
	}
	public static CharacterRoot	getInstance()
	{
		return(CharacterRoot.get());
	}
}

