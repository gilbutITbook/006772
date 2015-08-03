using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameObjectExtension;
using MathExtension;

// 무기 선택 씬의 시퀀스 제어.
public class WeaponSelectLevelSequence : SequenceBase {

	public enum STEP {

		NONE = -1,

		IDLE = 0,
		DEMO0,				// 무아저씨 등장 데모.
		SELECT_WEAPON,		// 무기 아이템을 선택할 때까지.
		PICKUP_KEY,			// 열쇠를 주을 때까지.
		ENTER_DOOR,			// 문에 들어갈 때까지.
		TRANSPORT,			// 플로어 이동 이벤트.
		WAIT_FRIEND,		// 다른 플레이어 대기 기다림.
		FINISH,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	//===================================================================
	// 플로어  키 관련 속성.

	protected string	key_item_name = "key04";
	protected string	key_instance_name = "";

	//===================================================================

	protected chrBehaviorLocal	player = null;
	protected chrBehaviorKabu	kabusan = null;

	public GameObject	spotlight_prefab = null;
	public GameObject	spotlight_kabusan_prefab = null;
	public GameObject	spotlight_key_prefab = null;

	protected GameObject		spotlight_player = null;
	protected GameObject		spotlight_kabusan = null;
	protected GameObject[]		spotlight_items = null;
	protected GameObject		spotlight_key = null;

	protected bool[]			select_done_players = null;				// 각 플레이어가 도어에 들어갔는가?.
	protected bool				select_scene_finish = false;			// 무기 선택 씬 끝?.

	protected List<SelectingIcon>	selecting_icons;

	//===================================================================

	// 통신 모듈.
	protected Network			m_network;

	//===================================================================
	
	// 디버그 윈도 생성 시에 호출.
	public override void		createDebugWindow(dbwin.Window window)
	{
		window.createButton("클리어").setOnPress(() =>{ this.step.set_next(STEP.FINISH);});
	}

	// 레벨 시작 시에 호출.
	public override void		start()
	{
		this.player = PartyControl.get().getLocalPlayer();
		this.kabusan = CharacterRoot.get().findCharacter<chrBehaviorKabu>("NPC_Kabu_San");

		// 스포트 라이트.
		this.spotlight_player  = this.spotlight_prefab.instantiate();
		this.spotlight_kabusan = this.spotlight_kabusan_prefab.instantiate();

		this.spotlight_items = new GameObject[2];
		for(int i = 0;i < 2;i++) {
			this.spotlight_items[i] = this.spotlight_prefab.instantiate();
		}
		this.spotlight_items[0].setPosition(WeaponSelectMapInitializer.getNegiItemPosition().Y(4.0f));
		this.spotlight_items[1].setPosition(WeaponSelectMapInitializer.getYuzuItemPosition().Y(4.0f));

		this.spotlight_key = this.spotlight_key_prefab.instantiate();
		this.spotlight_key.SetActive(false);

		// 플래그 - 각 플레이어의 선택이 끝났는가?.

		this.select_done_players = new bool[NetConfig.PLAYER_MAX];

		for(int i = 0;i < this.select_done_players.Length;i++) {

			if(GameRoot.get().isConnected(i)) {

				this.select_done_players[i] = false;

			} else {

				// 참가하지 않은 플레이어는 '선택완료'로 해 둔다.
				this.select_done_players[i] = true;
			}
		}
		
		// 다른 플레이어의 상황을 나타내는 아이콘 생성하다.
		this.create_selecting_icons();

		// Network 클래스의 컴포넌트를 획득.
		GameObject	obj = GameObject.Find("Network");
		
		if(obj != null) {
			
			this.m_network = obj.GetComponent<Network>();

			if (this.m_network != null) {
				m_network.RegisterReceiveNotification(PacketId.GameSyncInfo, OnReceiveSyncPacket);
			}
		}

		this.step.set_next(STEP.DEMO0);
	}

	// 매 프레임 호출.
	public override void		execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크.

		switch(this.step.do_transition()) {

			// 무아저씨 등장 데모.
			case STEP.DEMO0:
			{
				if(EventRoot.get().getCurrentEvent() == null) {

					this.step.set_next(STEP.SELECT_WEAPON);
				}
			}
			break;

			// 무기 아아템을 선택할 때까지.
			case STEP.SELECT_WEAPON:
			{
				// 무기 아이템을 선택했으면 다음으로 .
				if(this.player.getShotType() != SHOT_TYPE.EMPTY) {

					// 선택되지 않은 무기 아이템 삭제.
					List<ItemController>	shot_items = ItemManager.get().findItems(x => x.name.StartsWith("shot"));

					foreach(var item in shot_items) {

						ItemManager.get().deleteItem(item.name);
					}

					this.select_done_players[this.player.getGlobalIndex()] = true;

					// 무기 아이템의 스포트 라이트를 지우고 열쇠 위치에 스포트 라이트.
					this.spotlight_items[0].setPosition(WeaponSelectLevelSequence.getKeyStayPosition().Y(4.0f));
					this.spotlight_items[1].SetActive(false);

					this.step.set_next(STEP.PICKUP_KEY);
				}
			}
			break;

			// 열쇠를 주을 때까지.
			case STEP.PICKUP_KEY:
			{
				if(ItemManager.get().findItem(this.key_instance_name) == null) {

					this.spotlight_items[0].SetActive(false);
					this.step.set_next(STEP.ENTER_DOOR);
				}
			}
			break;

			// 문으로 들어갈 때까지.
			case STEP.ENTER_DOOR:
			{
				TransportEvent	ev = EventRoot.get().getCurrentEvent<TransportEvent>();

				if(ev != null) {

					ev.setEndAtHoleIn(true);

					this.step.set_next(STEP.TRANSPORT);
				}
			}
			break;

			// 플로어 이동 이벤트.
			case STEP.TRANSPORT:
			{
				TransportEvent	ev = EventRoot.get().getCurrentEvent<TransportEvent>();

				if(ev == null) {

					// 초기 장비의 동기화 대기 쿼리를 발행 .
					var	query = new QuerySelectFinish(this.player.control.getAccountID());

					// 쿼리의 타임아웃을 연장.
					query.timeout = 20.0f;

					QueryManager.get().registerQuery(query);
					
					
					// 선택한 무기를 게임 서버에 알림.
					if (this.m_network != null) {
						CharEquipment equip = new CharEquipment();
						
						equip.globalId = GlobalParam.get().global_account_id;
						equip.shotType = (int) this.player.getShotType();
						
						Debug.Log("[CLIENT] Send equip AccountID:" + equip.globalId + " ShotType:" + equip.shotType);

						EquipmentPacket packet = new EquipmentPacket(equip);
						int serverNode = this.m_network.GetServerNode();
						this.m_network.SendReliable<CharEquipment>(serverNode, packet);
					}
					else {
						query.set_done(true);
						query.set_success(true);
					}

					this.step.set_next(STEP.WAIT_FRIEND);
				}
			}
			break;

			// 다른 플레이어 기다림.
			case STEP.WAIT_FRIEND:
			{
				// 던전으로 이동하는 신호 기다림.
				if(this.select_scene_finish) {

					// 만약을 위해 선택 완료가 되지 않은 아이콘을 강제로 선택 완료로 표시.
					// 해버린다.
					foreach(var icon in this.selecting_icons) {
	
						if(icon.step.get_current() == SelectingIcon.STEP.HAI) {
	
							continue;
						}
						icon.beginHai();
					}

					this.step.set_next_delay(STEP.FINISH, 2.0f);
				}
			}
			break;
		}
				
		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 무아저씨 등장 데모.
				case STEP.DEMO0:
				{
					EventRoot.get().startEvent<EventWeaponSelect>();
				}
				break;

				// 열쇠 줍기.
				case STEP.PICKUP_KEY:
				{
					// 플로어 열쇠가 외위에서 떨어진다.
					this.create_floor_key();
				}
				break;

				// 문에 들어갈 때까지.
				case STEP.ENTER_DOOR:
				{
					this.spotlight_key.SetActive(true);
				}
				break;

				// 플로어 이동 이벤트.
				case STEP.TRANSPORT:
				{
					this.kabusan.onBeginTransportEvent();
				}
				break;

				// 다른 플레이어 기다림.
				case STEP.WAIT_FRIEND:
				{	
					// 다른 플레이어의 상황을 나타내는 아이콘을 표시.
					foreach(var icon in this.selecting_icons) {
			
						icon.setVisible(true);
					}
				}
				break;

				case STEP.FINISH:
				{
					GameRoot.get().setNextScene("GameScene");
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 다른 플레이어 기다림.
			case STEP.WAIT_FRIEND:
			{
				foreach(var icon in this.selecting_icons) {

					if(icon.step.get_current() == SelectingIcon.STEP.UUN) {

						continue;
					}
					if(this.select_done_players[icon.player_index]) {

						icon.beginHai();
					}
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //

		this.spotlight_player.setPosition(this.player.control.getPosition().Y(4.0f));
		this.spotlight_kabusan.setPosition(this.kabusan.control.getPosition().Y(8.0f));

		this.update_queries();
	}

	// ---------------------------------------------------------------- //

	// 다른 플레이어의 상황을 나타내는 아이콘을 생성한다.
	protected void		create_selecting_icons()
	{
		this.selecting_icons = new List<SelectingIcon>();

		for(int i = 0;i < NetConfig.PLAYER_MAX;i++) {

			if(!GameRoot.get().isConnected(i)) {

				continue;
			}

			this.selecting_icons.Add(Navi.get().createSelectingIcon(i));
		}

		switch(this.selecting_icons.Count) {

			case 1:
			{
				this.selecting_icons[0].setPosition(new Vector3(0.0f, 0.0f, 0.0f));
			}
			break;

			case 2:
			{
				this.selecting_icons[0].setPosition(new Vector3(-75.0f, 0.0f, 0.0f));
				this.selecting_icons[0].setFlip(true);
				this.selecting_icons[1].setPosition(new Vector3(75.0f, 0.0f, 0.0f));
			}
			break;

			case 3:
			{
				this.selecting_icons[0].setPosition(new Vector3(-75.0f,   0.0f, 0.0f));
				this.selecting_icons[0].setFlip(true);
				this.selecting_icons[1].setPosition(new Vector3( 75.0f,  50.0f, 0.0f));
				this.selecting_icons[2].setPosition(new Vector3(150.0f, -50.0f, 0.0f));
			}
			break;

			case 4:
			{
				this.selecting_icons[0].setPosition(new Vector3(-75.0f,  50.0f, 0.0f));
				this.selecting_icons[0].setFlip(true);
				this.selecting_icons[1].setPosition(new Vector3(-150.0f, -50.0f, 0.0f));
				this.selecting_icons[1].setFlip(true);
				this.selecting_icons[2].setPosition(new Vector3( 75.0f,  50.0f, 0.0f));
				this.selecting_icons[3].setPosition(new Vector3(150.0f, -50.0f, 0.0f));
			}
			break;
		}

		foreach(var icon in this.selecting_icons) {

			icon.setVisible(false);
		}
	}

	// 쿼리를 갱신한다.
	private void	update_queries()
	{

		List<QueryBase>		done_queries = QueryManager.get().findDoneQuery<QuerySelectFinish>();

		foreach(var query in done_queries) {

			QuerySelectFinish	query_select = query as QuerySelectFinish;

			if(query_select == null) {

				continue;
			}

			switch(query_select.getType()) {

				case "select.finish":
				{
					// 무기 선택 씬 종료 신호를 받는다.
					this.select_scene_finish = true;
				}
				break;
			}

			// 용무를 마쳤으므로 삭제한다.
			query_select.set_expired(true);
		}
	}

	// 플로어 키가 위에서 내려온다.
	protected void create_floor_key()
	{
		this.key_instance_name = key_item_name + "." + this.player.getAcountID();

		ItemManager.get().createItem(key_item_name, this.key_instance_name, PartyControl.get().getLocalPlayer().getAcountID());
		ItemManager.get().setPositionToItem(this.key_instance_name, WeaponSelectLevelSequence.getKeyStayPosition());

		ItemController	item_key = ItemManager.get().findItem(this.key_instance_name);

		item_key.behavior.beginFall();
	}

	public static Vector3	getKeyStayPosition()
	{
		return(new Vector3(0.0f, 0.0f, 0.0f));
	}


	// ---------------------------------------------------------------- //
	// 패킷 수신 함수].

	// 동기 대기 패킷 수신.
	public void OnReceiveSyncPacket(int node, PacketId id, byte[] data)
	{
		Debug.Log("[CLIENT]OnReceiveSyncPacket");
		
		GameSyncPacket packet = new GameSyncPacket(data);
		GameSyncInfo sync = packet.GetPacket();

		GlobalParam.get().seed = sync.seed;

		// 초기 장비를 보존한다.
		for (int i = 0; i < sync.items.Length; ++i) {

			CharEquipment equip = sync.items[i];

			GlobalParam.get().shot_type[equip.globalId] = (SHOT_TYPE)equip.shotType;
			this.select_done_players[equip.globalId] = true;

			Debug.Log("[CLIENT] AccountID:" + equip.globalId + " ShotType:" + equip.shotType);
		}

		// 응답이 있는 쿼리를 검색.
		string account_id = this.player.control.getAccountID();
		QuerySelectFinish	query = QueryManager.get().findQuery<QuerySelectFinish>(x => x.account_id == account_id);

		if (query != null) {
			Debug.Log("[CLIENT]QuerySelectDone done");
			query.set_done(true);
			query.set_success(true);
		}
		
		Debug.Log("[CLIENT]Recv seed:" + sync.seed);
	}
}
