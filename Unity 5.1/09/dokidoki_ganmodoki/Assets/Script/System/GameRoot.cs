using UnityEngine;
using System.Collections;


// f레벨별 시퀀스 모듈 베이스 클래스.
public class SequenceBase : MonoBehaviour {

	public virtual void		createDebugWindow(dbwin.Window window) {}	// 디버그 창 생성시 호출.

	public virtual void		beforeInitializeMap() {}	// 맵 생성 직전에 호출된다.
	public virtual void		start() {}					// 레벨 시작 시에 호출된다.
	public virtual void		execute() {}				// 매 프레임 호출된다.

	public virtual bool	isFinished() { return(false); }

	public SequenceBase		parent = null;
	public SequenceBase		child = null;

};

// 게임 시퀀스.
public class GameRoot : MonoBehaviour {

	// ================================================================ //

	public string	owner = "";					// 이 마을(씬)의 오너 어카운트 이름.

	public bool		is_host = true;				// 로컬 플레이어가 현재 표시 중인 마을(씬)의 소유자인가?.

	// ================================================================ //
	// 말풍선용(어딘가로 이동예정).

	public Texture texture_main    = null;
	public Texture texture_belo    = null;
	public Texture texture_kado_lu = null;
	public Texture texture_kado_ru = null;
	public Texture texture_kado_ld = null;
	public Texture texture_kado_rd = null;

	public Texture	title_image = null;					// 타이틀 화면.

	// ================================================================ //

	public enum STEP {

		NONE = -1,

		IDLE = 0,
		GAME,				// 플레이 중.
		RELOAD,				// 리로드(디버그용).

		ERROR,				// 통신 오류.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	private float		scene_timer = 0.0f;

	public	bool[]	controlable = null;

	public	struct DebugFlag {

		public	bool	play_bgm;
	};
	public	DebugFlag	debug_flag;

	protected string	next_scene = "";				// 다음에 로드할 씬.

	// ================================================================ //
	// 맵 생선 관련 파라미터.
	
	public MapInitializer 		map_initializer;		//< Reference to map creator object.
	public SequenceBase			sequence;				// 레벨별 시퀀스 모듈.

	protected Network			network;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		// 시퀀스 모듈이 설정되어 있지 않으면 기본 모듈을 추가한다.
		if(this.sequence == null) {

			this.sequence = this.gameObject.AddComponent<SequenceBase>();
		}

		// Network 클래스 컴포넌트 획득.
		GameObject	obj = GameObject.Find("Network");

		if(obj != null) {

			this.network = obj.GetComponent<Network>();
		}
	}

	void	Start()
	{
		this.step.set_next(STEP.GAME);

		this.controlable = new bool[4];

		for(int i = 0;i < 4;i++) {

			this.controlable[i] = true;
		}

		//

		this.debug_flag.play_bgm = false;

		dbwin.root();

		if(dbwin.root().getWindow("game") == null) {

			this.create_debug_window();
		}
		if(dbwin.root().getWindow("player") == null) {

			this.create_debug_window_player();
		}
	#if false
		PseudoRandom.Plant	plant = PseudoRandom.get().createPlant("test0");
		plant = PseudoRandom.get().createPlant("test1");

		for(int i = 0;i < 16;i++) {

			Debug.Log(plant.getRandom().ToString());
		}
	#endif
	}
	
	void	Update()
	{
		this.scene_timer += Time.deltaTime;

		// 오류 핸들링.
		NetEventHandling();

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.

		switch(this.step.do_transition()) {

			case STEP.GAME:
			{
				if(this.next_scene != "") {

					if(this.network != null) {

						this.network.ClearReceiveNotification();
					}
					Application.LoadLevel(this.next_scene);
					this.next_scene = "";
					this.step.set_next(STEP.IDLE);
				}
			}
			break;
		}
				
		// ---------------------------------------------------------------- //
		// 상태가 전환된 때 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.GAME:
				{
					SoundManager.get().playBGM(Sound.ID.TKJ_BGM01);

					this.sequence.beforeInitializeMap();

					this.map_initializer.initializeMap(this);

					LevelControl.get().onFloorCreated();

					this.sequence.start();
				}
				break;

				case STEP.RELOAD:
				{
					Application.LoadLevel("GameScene");
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.GAME:
			{
				// 레벨별 시퀀스 모듈.
				this.sequence.execute();
			}
			break;

		}

		// ---------------------------------------------------------------- //

	}

	void	OnGUI()
	{
		// 배경 이미지.

		if(GlobalParam.getInstance().fadein_start) {

			float	title_alpha = Mathf.InverseLerp(1.0f, 0.0f, this.scene_timer);

			if(title_alpha > 0.0f) {

				GUI.color = new Color(1.0f, 1.0f, 1.0f, title_alpha);
				GUI.DrawTexture(new Rect(0.0f, 0.0f, Screen.width, Screen.height), this.title_image, ScaleMode.ScaleToFit, true);
			}
		}

		switch (this.step.get_current()) {
			case STEP.ERROR:
			{
				NotifyError();
			}
			break;
		}
	}

	// ================================================================ //

	// 씬을 로드한다.
	public void	setNextScene(string next_scene)
	{
		this.next_scene = next_scene;
	}

	// 케이크 무한 제공(보스 전 후의 보너스)중 ?.
	public bool	isNowCakeBiking()
	{
		bool	ret = false;

		do {

			if(this.sequence == null) {
	
				break;
			}

			BossLevelSequence	boss_seq = this.sequence as BossLevelSequence;

			if(boss_seq == null) {

				break;
			}

			ret = boss_seq.isNowCakeBiking();

		} while(false);

		return(ret);
	}

	// 사용하는 단말이 호스트?.
	public bool	isHost()
	{
		bool	ret = true;

		if(this.network != null) {

			ret = GlobalParam.get().is_host;

		} else {

			ret = true;
		}

		return(ret);
	}

	// 네트워크 플레이어와 통신이 연결되었는가?.
	public bool		isConnected(int global_account_index)
	{
		bool	ret = false;

		if(this.network != null) {

			int node = this.network.GetClientNode(global_account_index);

			ret = this.network.IsConnected(node);

		} else {

			// 디버그용.
			if(global_account_index == GlobalParam.get().global_account_id) {

				ret = true;

			} else {

				ret = GlobalParam.get().db_is_connected[global_account_index];
			}
		}

		return(ret);
	}

	// 로컬 플레이어를 만든다.
	public void		createLocalPlayer()
	{
		PartyControl.get().createLocalPlayer(GlobalParam.get().global_account_id);
	}

	// 리모트 플레이어를 만든다..
	public void		createNetPlayers()
	{
		Debug.Log("Create Remote players");

		for(int i = 0;i < NetConfig.PLAYER_MAX;i++) {

			// 로컬 플레이어는 스킵.
			if(i == GlobalParam.get().global_account_id) {

				continue;
			}

			Debug.Log("Create Remote players[" + i + "] " + this.isConnected(i));

			if(!this.isConnected(i)) {

				continue;
			}
			
			if(this.network != null) {

				PartyControl.get().createNetPlayer(i);

			} else {

				PartyControl.get().createFakeNetPlayer(i);
			}
		}

		if(this.network == null) {

			for(int i = 0;i < PartyControl.get().getFriendCount();i++) {
	
				chrBehaviorFakeNet	friend = PartyControl.get().getFriend(i) as chrBehaviorFakeNet;
	
				if(friend == null) {
	
					continue;
				}
	
				friend.in_formation = true;
			}
		}
	}

	public void		restartGameScane()
	{
		this.step.set_next(STEP.RELOAD);
	}

	//적들의 게임 오브젝트의 사고(비헤이비어)와 제어(컨트롤러)에 일시정지를 설정, 해제.
	public void		setEnemyPause(bool newPause)
	{
		foreach (GameObject gobj in GameObject.FindGameObjectsWithTag("Enemy")) {
			gobj.GetComponent<chrControllerEnemyBase>().SetPause(newPause);
			gobj.GetComponent<chrBehaviorEnemy>().SetPause(newPause);
		}
		foreach (GameObject gobj in GameObject.FindGameObjectsWithTag("EnemyLair")) {
			gobj.GetComponent<chrControllerEnemyBase>().SetPause(newPause);
			gobj.GetComponent<chrBehaviorEnemy>().SetPause(newPause);
		}
		foreach (GameObject gobj in GameObject.FindGameObjectsWithTag("Boss")) {
			gobj.GetComponent<chrControllerEnemyBase>().SetPause(newPause);
			gobj.GetComponent<chrBehaviorEnemy>().SetPause(newPause);
		}
		foreach (GameObject gobj in GameObject.FindGameObjectsWithTag("Enemy Bullet")) {
			gobj.GetComponent<EnemyBulletControl>().SetPause(newPause);
		}
	}
	
	
	// ================================================================ //
	
	
	public void NetEventHandling()
	{
		if (network == null) {
			return;
		}

		NetEventState state = network.GetEventState();
		
		if (state == null) {
			return;
		}
		
		switch (state.type) {
		case NetEventType.Connect:
			Debug.Log("[CLIENT]connect event handling:" + state.node);
			break;
			
		case NetEventType.Disconnect:
			Debug.Log("[CLIENT]disconnect event handling:" + state.node);
			DisconnectEventProc(state.node);
			break;
		}
	}


	// 접속 끊김에 관한 이벤트 처리.
	private void DisconnectEventProc(int node)
	{
		int global_index = network.GetPlayerIdFromNode(node);
		
		if (node == network.GetServerNode () ||
			global_index == 0) {
			// 서버와 호스트와 접속이 끊기면 종료.
			this.step.set_next (STEP.ERROR);
		} else {
			Debug.Log("[CLIENT]disconnect character:" + global_index);

			PartyControl.get ().deleteNetPlayer (global_index);
		}
	}

	// 오류 알림.
	private void NotifyError()
	{
		GUIStyle style = new GUIStyle(GUI.skin.GetStyle("button"));
		style.normal.textColor = Color.white;
		style.fontSize = 25;
		
		float sx = 450;
		float sy = 200;
		float px = Screen.width / 2 - sx * 0.5f;
		float py = Screen.height / 2 - sy * 0.5f;

		string message = "통신 오류가 발생했습니다.\n게임을 종료합니다.\n\n버튼을 누르세요.";
		if (GUI.Button (new Rect (px, py, sx, sy), message, style)) {
			this.step.set_next(STEP.NONE);
			if (GlobalParam.get().is_host) {
				network.StopGameServer();
			}
			network.StopServer();
			network.Disconnect();

			GameObject.Destroy(network);

			Application.LoadLevel("TitleScene");
		}
	}

	// ================================================================ //

	// 디버그 창을 만든다(플레이어 관련).
	protected void		create_debug_window_player()
	{
		var		window = dbwin.root().createWindow("player");

		window.createButton("당했다")
			.setOnPress(() =>
			{
				var		player = PartyControl.getInstance().getLocalPlayer();

				player.control.causeDamage(100.0f, -1);
			});

		window.createButton("아이스 과식")
			.setOnPress(() =>
			{
				var		player = PartyControl.getInstance().getLocalPlayer();

				if(!player.isNowJinJin()) {

					player.startJinJin();

				} else {

					player.stopJinJin();
				}	
			});

		window.createButton("크림 범벅 테스트")
			.setOnPress(() =>
			{
				var		player = PartyControl.getInstance().getLocalPlayer();

				if(!player.isNowCreamy()) {

					player.startCreamy();

				} else {

					player.stopCreamy();
				}	
			});

		window.createButton("체력 회복 테스트")
			.setOnPress(() =>
			{
				var		player = PartyControl.getInstance().getLocalPlayer();

				if(!player.isNowHealing()) {

					player.startHealing();

				} else {

					player.stopHealing();
				}	
			});

		window.createButton("아이스 당첨 테스트")
			.setOnPress(() =>
			{
				window.close();
				EventRoot.get().startEvent<EventIceAtari>();
			});

		window.createButton("무기 교체")
			.setOnPress(() =>
			{
				var		player = PartyControl.getInstance().getLocalPlayer();
	
				SHOT_TYPE	shot_type = player.getShotType();

				shot_type = (SHOT_TYPE)(((int)shot_type + 1)%((int)SHOT_TYPE.NUM));

				player.changeBulletShooter(shot_type);
			});
	}

	// 디버그 창을 만든다(여러 가지).
	protected void		create_debug_window()
	{
		var		window = dbwin.root().createWindow("game");

		window.createCheckBox("어카운트１", GlobalParam.get().db_is_connected[0])
			.setOnChanged((bool is_checked) =>
			{
				GlobalParam.get().db_is_connected[0] = is_checked;
			});			
		window.createCheckBox("어카운트 ２", GlobalParam.get().db_is_connected[1])
			.setOnChanged((bool is_checked) =>
			{
				GlobalParam.get().db_is_connected[1] = is_checked;
			});			
		window.createCheckBox("어카운트３", GlobalParam.get().db_is_connected[2])
			.setOnChanged((bool is_checked) =>
			{
				GlobalParam.get().db_is_connected[2] = is_checked;
			});			
		window.createCheckBox("어카운트 ４", GlobalParam.get().db_is_connected[3])
			.setOnChanged((bool is_checked) =>
			{
				GlobalParam.get().db_is_connected[3] = is_checked;
			});			

		window.createCheckBox("BGM", this.debug_flag.play_bgm)
			.setOnChanged((bool is_checked) =>
			{
				this.debug_flag.play_bgm = is_checked;

				if(this.debug_flag.play_bgm) {

					SoundManager.get().playBGM(Sound.ID.TKJ_BGM01);

				} else {

					SoundManager.get().stopBGM();
				}
			});

		/*window.createButton("물렸다")
			.setOnPress(() =>
			{
				dbwin.console().print("hoge");
				//this.next_step = STEP.TO_TITLE;
			});*/
#if false
		window.createButton("적 만들기")
			.setOnPress(() =>
			{
				dbwin.console().print("fuga");

				/*GameObject		go =  GameObject.FindGameObjectWithTag("EnemyLair");
	
				if(go != null) {
	
					chrBehaviorEnemyLair0	lair = go.GetComponent<chrBehaviorEnemyLair0>();
	
					lair.createEnemy();
				}*/
			});

		window.createButton("적을 잔뜩 만들다")
			.setOnPress(() =>
			{
				EnemyRoot.getInstance().createManyEnemies();
			});

		window.createButton("메가 크래시")
			.setOnPress(() =>
			{
				EnemyRoot.getInstance().debugCauseDamageToAllEnemy();
			});
#endif
		window.createButton("메가메가 크래시")
			.setOnPress(() =>
			{
				EnemyRoot.getInstance().debugCauseVanishToAllEnemy();
			});
		window.createButton("제네레이터를 찾는다")
			.setOnPress(() =>
			{
				var		enemies = EnemyRoot.get().getEnemies();

				var		lairs = enemies.FindAll(x => (x.behavior as chrBehaviorEnemy_Lair) != null);

				foreach(var lair in lairs) {

					Debug.Log(lair.name);
				}
			});
#if false
		window.createButton("모두에게 대미지")
			.setOnPress(() =>
			{
				var		players = PartyControl.getInstance().getPlayers();
	
				foreach(var player in players) {
	
					player.controll.vital.causeDamage(10.0f);
				}
			});


		window.createButton("때린다")
			.setOnPress(() =>
			{
				var		player = PartyControl.getInstance().getLocalPlayer();
	
				player.controll.cmdSetMotion("m003_attack", 1);
	
				var		enemies = EnemyRoot.getInstance().getEnemies();
	
				foreach(var enemy in enemies) {
	
					enemy.cmdSetMotion("m003_attack", 1);
				}
			});

		window.createButton("마법")
			.setOnPress(() =>
			{
				var		player = PartyControl.getInstance().getLocalPlayer();

				player.controll.cmdSetMotion("m004_use", 1);
			});

		window.createButton("아팟")
			.setOnPress(() =>
			{
				var		player = PartyControl.getInstance().getLocalPlayer();
	
				player.controll.causeDamage(0.0f);

				var		enemies = EnemyRoot.getInstance().getEnemies();
	
				foreach(var enemy in enemies) {
	
					enemy.cmdSetMotion("m005_damage", 1);
				}
			});
#endif
		window.createButton("일시정지 설정")
			.setOnPress(() =>
            {
				this.setEnemyPause(true);
			});

		window.createButton("일시정지 해제")
			.setOnPress(() =>
            {
				this.setEnemyPause(false);
			});

		this.sequence.createDebugWindow(window);
	}

	// ================================================================ //
	// 인스턴스.

	private	static GameRoot	instance = null;

	public static GameRoot	getInstance()
	{
		if(GameRoot.instance == null) {

			GameRoot.instance = GameObject.Find("GameRoot").GetComponent<GameRoot>();
		}

		return(GameRoot.instance);
	}
	public static GameRoot	get()
	{
		return(GameRoot.getInstance());
	}
}
