using UnityEngine;
using System.Collections;
using System.Net;
using System.Text;

// 게임의 시퀀스.
public class GameRoot : MonoBehaviour {

	// ================================================================ //

	public string	owner = "";					// 이 마을(씬)의 주인 어카운트 이름.

	public bool		is_host = true;				// 로컬 플레이어가 현재 표시 중인 씬(마을)의 주인?.

	// ================================================================ //
	// 

	public Texture	title_image = null;					// 타이틀 화면.

	// ================================================================ //

	public string	account_name_local = "";
	public string	account_name_net   = "";

	public chrController		local_player = null;
	public chrController		net_player   = null;

	// ================================================================ //

	public enum STEP {

		NONE = -1,

		GAME = 0,			// 플레이 중.
		TO_TITLE,			// 타이틀 화면으로 돌아갑니다.
		VISIT,				// 다른 플레이어의 마을에 놀러 갑니다.
		WELCOME,			// 다른 플레이어가 놀러왔습니다.
		BYEBYE,				// 다른 플레이어가 돌아갑니다.
		GO_HOME,			// 자기 마을로 돌아옵니다.

		CHARACTER_CHANGE,	// 캐릭터를 바꿉니다(디버그용).

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	private float		scene_timer = 0.0f;

	private float		disp_timer = 0.0f;

	// 접속 시 이벤트 처리 플래그.
	private bool		request_connet_event = false;
	// 접속 종료 시 이벤트 처리 플래그.
	private bool		request_disconnet_event = false;

	// ================================================================ //
	// MonoBehaviour에서 상속. 

	void	Start()
	{
		this.step.set_next(STEP.GAME);

		//

		dbwin.root();

		if(dbwin.root().getWindow("game") == null) {

			this.create_debug_window();
		}

		// 이벤트 감시용.
		// Network 클래스의 컴포넌트를 취득.
		GameObject obj = GameObject.Find("Network");
		
		if(obj != null) {
			Network network = obj.GetComponent<Network>();
			network.RegisterReceiveNotification(PacketId.GoingOut, OnReceiveGoingOutPacket);
			network.RegisterEventHandler(OnEventHandling);
		}
	}
	
	void	Update()
	{
		this.scene_timer += Time.deltaTime;
		this.disp_timer -= Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크합니다.

		switch(this.step.do_transition()) {

			case STEP.GAME:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환될 때 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.CHARACTER_CHANGE:
				{
					GlobalParam.get().account_name = this.account_name_net;
					GlobalParam.get().skip_enter_event = true;

					Application.LoadLevel("GameScene 1");
				}
				break;
	
				case STEP.GAME:
				{
					this.account_name_local = GlobalParam.get().account_name;
					this.account_name_net   = GameRoot.getPartnerAcountName(this.account_name_local);

					if(this.owner == this.account_name_local) {

						this.is_host = true;

					} else {

						// 다른 플레이어의 마을에 갔을 때.
						this.is_host = false;
					}

					// 플레이어를 만듭니다.
					this.local_player = CharacterRoot.get().createPlayerAsLocal(this.account_name_local);

					this.local_player.cmdSetPosition(Vector3.zero);

					if(GlobalParam.get().is_in_my_home == GlobalParam.get().is_remote_in_my_home) {

						this.net_player = CharacterRoot.get().createPlayerAsNet(this.account_name_net);

						this.net_player.cmdSetPosition(Vector3.left*1.0f);
					}

					// 레벨 데이터(level_data.txt)를 읽고 NPC/아이템을 배치합니다.
					MapCreator.get().loadLevel(this.account_name_local, this.account_name_net, !this.is_host);

					SoundManager.get().playBGM(Sound.ID.TFT_BGM01);

					//

					if(!GlobalParam.get().skip_enter_event) {

						EnterEvent	enter_event = EventRoot.get().startEvent<EnterEvent>();
			
						if(enter_event != null) {
					
							enter_event.setPrincipal(this.local_player.behavior as chrBehaviorPlayer);
							enter_event.setIsLocalPlayer(true);
						}
					}

					foreach (ItemManager.ItemState istate in GlobalParam.get().item_table.Values) {
						if (istate.state == ItemController.State.Picked) {
							// 이미 아이템을 획득했다면 가져갈 수 있게 합니다.
							ItemManager.get().finishGrowingItem(istate.item_id);
							chrController controll = CharacterRoot.getInstance().findPlayer(istate.owner);
							if (controll != null) {
								QueryItemPick query = controll.cmdItemQueryPick(istate.item_id, false, true);
								if (query != null) {
									query.is_anon = true;
									query.set_done(true);
									query.set_success(true);
								}
							}
						}
					}

				}
				break;

				// 다른 플레이어가 찾아왔습니다.
				case STEP.WELCOME:
				{
					if(this.net_player == null) {

						EnterEvent	enter_event = EventRoot.get().startEvent<EnterEvent>();
		
						if(enter_event != null) {

							this.net_player = CharacterRoot.getInstance().createPlayerAsNet(this.account_name_net);
			
							this.net_player.cmdSetPosition(Vector3.left);
		
							enter_event.setPrincipal(this.net_player.behavior as chrBehaviorPlayer);
							enter_event.setIsLocalPlayer(false);
						}
					}
				}
				break;

				// 다른 플레이어가 돌아갑니다.
				case STEP.BYEBYE:
				{
					if(this.net_player != null) {

						LeaveEvent	leave_event = EventRoot.get().startEvent<LeaveEvent>();
		
						if(leave_event != null) {
			
							leave_event.setPrincipal(this.net_player.behavior as chrBehaviorPlayer);
							leave_event.setIsLocalPlayer(false);
						}
					}
				}
				break;

				case STEP.VISIT:
				{
					GlobalParam.get().is_in_my_home = false;
					Application.LoadLevel("GameScene 1");
				}
				break;

				case STEP.GO_HOME:
				{
					// 자기 마을로 돌아옵니다.
					GlobalParam.get().is_in_my_home = true;
					Application.LoadLevel("GameScene 1");
				}
				break;

				case STEP.TO_TITLE:
				{
					Application.LoadLevel("TitleScene");
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.GAME:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 통신에 의한 이벤트 처리.

		// Network 클래스의 컴포넌트 획득.
		GameObject go = GameObject.Find("Network");
		Network network = go.GetComponent<Network>();
		if (network != null) {
			if (network.IsCommunicating() == true) {
				if (request_connet_event) {
					// 접속 이벤트를 발동합니다.
					GlobalParam.get().is_connected = true;
					request_connet_event = false;
				}
				else if (GlobalParam.get().is_connected == false) {
					// 접속했습니다.
					Debug.Log("Guest connected.");
					request_connet_event = true;
					disp_timer = 5.0f;
				}
			}
		}

		// 접속 종료 이벤트를 발동합니다.
		if (request_disconnet_event) {
			GlobalParam.get().is_disconnected = true;
			request_disconnet_event = false;
			// 접속 종료 시 이벤트.
			disconnect_event();
		}
	
		// ---------------------------------------------------------------- //

		if(Input.GetKeyDown(KeyCode.Z)) {

			dbwin.console().print("로그 테스트 " + this.log_test_count);
			this.log_test_count++;
		}
	}
	private int 	log_test_count = 0;

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

		if (GlobalParam.get().is_disconnected) {
			GUI.Button(new Rect(Screen.width - 220.0f, Screen.height - 50.0f, 200.0f, 30.0f),
			           "친구와 연결이 끊어졌습니다");
		}
		else if (GlobalParam.get().is_connected) {
			if (this.disp_timer > 0.0f) {
				string message = (GlobalParam.get().account_name == "Toufuya")? "친구가 놀러왔습니다" :
																				"친구와 놀 수 있습니다";
				// 대기 중인 플레이어에 게스트가 왔음을 알립니다.
				GUI.Button(new Rect(Screen.width - 280.0f, Screen.height - 50.0f, 250.0f, 30.0f), message);
			}
		}
	}

	// ================================================================ //

	// 자기 이외의 플레이어 계정을 가져옵니다.
	public static string	getPartnerAcountName(string myname)
	{
		string	partner = "";

		if(myname == "Toufuya") {

			partner = "Daizuya";

		} else {

			partner = "Toufuya";
		}

		return(partner);
	}

	// 통신 상대와의 접속 상태.
	public bool isConnected()
	{
		return (GlobalParam.get().is_connected && !GlobalParam.get().is_disconnected);
	}

	// ================================================================ //

	public void OnReceiveGoingOutPacket(PacketId id, byte[] data)
	{
		GoingOutPacket packet = new GoingOutPacket(data);
		GoingOutData go = packet.GetPacket();

		Debug.Log("OnReceiveGoingOutPacket");
		if (GlobalParam.get().account_name == go.characterId) {
			// 자신은 이미 행동이 끝났으므로 처리하지 않습니다.
			return;
		}

		if (GlobalParam.get().is_in_my_home) {
			// 자신의 정원에 있습니다..
			if (go.goingOut) {
				// 친구가 찾아왔습니다.
				this.step.set_next(STEP.WELCOME);	
				GlobalParam.get().is_remote_in_my_home = true;		
			}
			else {
				// 친구가 돌아갑니다.
				this.step.set_next(STEP.BYEBYE);
				GlobalParam.get().is_remote_in_my_home = false;		
			}
		}
		else {
			// 친구 정원에 있습니다.
			if (go.goingOut) {
				// 친구가 갑니다.
				this.step.set_next(STEP.BYEBYE);
				GlobalParam.get().is_remote_in_my_home = true;		
			}
			else {
				// 친구가 돌아옵니다.
				this.step.set_next(STEP.WELCOME);
				GlobalParam.get().is_remote_in_my_home = false;		
			}
		}
	}

	
	public void NotifyFieldMoving()
	{
		GameObject go = GameObject.Find("Network");
		if (go != null) {
			Network network = go.GetComponent<Network>();
			if (network != null) {
				GoingOutData data = new GoingOutData();
				
				data.characterId = GlobalParam.get().account_name;
				data.goingOut = GlobalParam.get().is_in_my_home;
				
				GoingOutPacket packet = new GoingOutPacket(data);
				network.SendReliable<GoingOutData>(packet);
			}
		}
	}

	// ================================================================ //
	

	public void OnEventHandling(NetEventState state)
	{
		switch (state.type) {
		case NetEventType.Connect:
			// 접속 이벤트는 이 함수가 등록되기 전에 발생하는 경우가 있으므로.
			// 잘 못 가져오는 일이 있습니다.
			// 이 때문에 접속 상태를 감시해서 접속 이벤트를 발생 시킵니다.
			break;

		case NetEventType.Disconnect:
			Debug.Log("Guest disconnected.");
			request_disconnet_event = true;
			break;
		}
	}

	// ================================================================ //

	protected void disconnect_event()
	{
		if (GlobalParam.get().is_in_my_home == false && 
		    GlobalParam.get().is_remote_in_my_home == false) {

			chrBehaviorPlayer	player = this.net_player.behavior as chrBehaviorPlayer;

			if(player != null) {

				if(!player.isNowHouseMoving()) {

					HouseMoveStartEvent	start_event = EventRoot.get().startEvent<HouseMoveStartEvent>();
			
					start_event.setPrincipal(player);
					start_event.setHouse(CharacterRoot.get().findCharacter<chrBehaviorNPC_House>("House1"));
				}
			}
		}
		else if(GlobalParam.get().is_in_my_home &&
		        GlobalParam.get().is_remote_in_my_home) {
			this.step.set_next(STEP.BYEBYE);	
		}
	}


	// ================================================================ //

	protected void		create_debug_window()
	{
		var		window = dbwin.root().createWindow("game");

		window.createButton("캐릭터를 변경합니다")
			.setOnPress(() =>
			{
				this.step.set_next(STEP.CHARACTER_CHANGE);
			});

		if(GlobalParam.get().is_in_my_home) {

			window.createButton("놀러갑니다!")
				.setOnPress(() =>
				{
					LeaveEvent	leave_event = EventRoot.get().startEvent<LeaveEvent>();

					leave_event.setPrincipal(this.local_player.behavior as chrBehaviorPlayer);
					leave_event.setIsLocalPlayer(true);
				});

		} else {

			window.createButton("집으로 돌아갑니다~")
				.setOnPress(() =>
				{
					LeaveEvent	leave_event = EventRoot.get().startEvent<LeaveEvent>();

					leave_event.setPrincipal(this.local_player.behavior as chrBehaviorPlayer);
					leave_event.setIsLocalPlayer(true);
				});
		}

		window.createButton("누군가 왔다!")
			.setOnPress(() =>
			{
				this.step.set_next(STEP.WELCOME);
			});

		window.createButton("바이바~이")
			.setOnPress(() =>
			{
				this.step.set_next(STEP.BYEBYE);
			});

		window.createButton("출발 이벤트 테스트")
			.setOnPress(() =>
			{
				LeaveEvent	leave_event = EventRoot.get().startEvent<LeaveEvent>();

				leave_event.setPrincipal(this.local_player.behavior as chrBehaviorPlayer);
				leave_event.setIsLocalPlayer(true);
				leave_event.setIsMapChange(false);

				window.close();
			});

		window.createButton("도착 이벤트 테스트")
			.setOnPress(() =>
			{
				EnterEvent	enter_event = EventRoot.get().startEvent<EnterEvent>();

				enter_event.setPrincipal(this.local_player.behavior as chrBehaviorPlayer);

				window.close();
			});

		window.createButton("이사 시작 이벤트 테스트")
			.setOnPress(() =>
			{
				HouseMoveStartEvent	start_event = EventRoot.get().startEvent<HouseMoveStartEvent>();

				start_event.setPrincipal(this.local_player.behavior as chrBehaviorPlayer);
				start_event.setHouse(CharacterRoot.get().findCharacter<chrBehaviorNPC_House>("House1"));

				window.close();
			});
		window.createButton("이사 종료 이벤트 테스트")
			.setOnPress(() =>
			{
				HouseMoveEndEvent	end_event = EventRoot.get().startEvent<HouseMoveEndEvent>();

				end_event.setPrincipal(this.local_player.behavior as chrBehaviorPlayer);
				end_event.setHouse(CharacterRoot.get().findCharacter<chrBehaviorNPC_House>("House1"));

				window.close();
			});
	}

#if false
	void	WindowFunction(int id)
	{
		int		x = 10;
		int		y = 30;

		if(GUI.Button(new Rect(x, y, 100, 20), "질렸다")) {

			this.next_step = STEP.TO_TITLE;
		}

		y += 30;

	}
#endif

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
