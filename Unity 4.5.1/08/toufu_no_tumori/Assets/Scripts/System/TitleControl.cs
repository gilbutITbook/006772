using UnityEngine;
using System.Collections;
using System.Net;

// 타이틀 화면 제어.
public class TitleControl : MonoBehaviour {

	public GUISkin	gui_skin = null;
	public GUISkin	gui_skin_text = null;

	//private bool			avator1 = true;				// 캐릭터  true:"Folk1" false:"Folk2".

	public Texture	title_image = null;					// 타이틀 화면.

	// ================================================================ //

	public enum STEP {

		NONE = -1,

		WAIT = 0,			// 입력 대기.
		SERVER_START,		// 대기 시작.
		SERVER_CONNECT,		// 게임 서버에 접속.
		CLIENT_CONNECT,		// 클라이언트 간 접속.
		PREPARE,			// 각종 준비.
		GAME_START,			// 게임 시작.

		ERROR,				// 오류 발생.
		WAIT_RESTART,		// 오류에서 복귀 대기.

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;

	private float		step_timer = 0.0f;

	private const int 	usePost = 50765;

	// 통신 모듈의 컴포넌트.
	private Network		network_ = null;

	// 호스트 플래그.
	private bool		isHost = false;

	// 호스트 IP 주소.
	private string		hostAddress = "";

	// 게임 시작 동기화 정보 취득 상태 .
	private bool		isReceiveSyncGameData = false;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.step      = STEP.NONE;
		this.next_step = STEP.WAIT;

		GlobalParam.getInstance().fadein_start = true;


		// 호스트 이름을 취득합니다.
		this.hostAddress = "";
		string hostname = Dns.GetHostName();
		// 호스트 이름에서 IP 주소를 가져옵니다.
		IPAddress[] adrList = Dns.GetHostAddresses(hostname);
		hostAddress = adrList[0].ToString();

		GameObject obj = new GameObject("Network");
		if (obj != null) {
			network_ = obj.AddComponent<Network>();
			network_.RegisterReceiveNotification(PacketId.GameSyncInfo, OnReceiveSyncGamePacket);
		}
	}
	
	void	Update()
	{
		// ---------------------------------------------------------------- //
		// 단계 내의 경과 시간을 진행합니다.

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크합니다.


		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.WAIT:
				{

				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.next_step != STEP.NONE) {

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			switch(this.step) {
	
			case STEP.SERVER_START:
				{
					Debug.Log("Launch Listening socket.");
					// 같은 단말에서 실행할 수 있게 포트 번호를 변경합니다. 
					// 다른 단말에서 실행할 경우는 포트 번호가 같은 것을 사용합니다.
					int port = isHost? NetConfig.GAME_PORT : NetConfig.GAME_PORT + 1;
					bool ret = network_.StartServer(port, Network.ConnectionType.UDP);
					if (isHost) {
						// 게임 서버 시작.
						ret &= network_.StartGameServer();
					}
					if (ret == false) {
						// 오류로 강제 이행.
						Debug.Log("Error occured.");
						this.step = STEP.ERROR;
					}
				}
				break;

			case STEP.SERVER_CONNECT:
				{
					Debug.Log("Connect to gameserver.");
					// 호스트 이름을 취득합니다.
					string serverAddress = this.hostAddress;
					if (isHost) {
						string hostname = Dns.GetHostName();
						IPAddress[] adrList = Dns.GetHostAddresses(hostname);
						serverAddress = adrList[0].ToString();
					}
					bool ret = network_.Connect(serverAddress, NetConfig.SERVER_PORT, Network.ConnectionType.TCP);
					if (ret == false) {
						// 오류 강제 이행.
						Debug.Log("Error occured.");
						this.step = STEP.ERROR;
					}
				}
				break;

			case STEP.CLIENT_CONNECT:
				{
					Debug.Log("Connect to host.");
					// 같은 단말에서 실행할 수 있게 포트 번호를 변경합니다. 
					// 다른 단말에서 실행할 경우는 포트 번호가 같은 것을 사용합니다.
					int port = isHost? NetConfig.GAME_PORT + 1 : NetConfig.GAME_PORT;
					bool ret = network_.Connect(this.hostAddress, port, Network.ConnectionType.UDP);
					if (ret == false) {
						// 오류로 강제 이행.
						Debug.Log("Error occured.");
						this.step = STEP.ERROR;
					}
				}
				break;

			case STEP.GAME_START:
				{
					if (isHost) {
						GlobalParam.getInstance().account_name = "Toufuya";
						GlobalParam.getInstance().global_acount_id = 0;
					}
					else {
						GlobalParam.getInstance().account_name = "Daizuya";
						GlobalParam.getInstance().global_acount_id = 1;
					}
					GlobalParam.get().is_host = isHost;
					Application.LoadLevel("GameScene 1");
				}
				break;

			case STEP.WAIT_RESTART:
				{
					network_.Disconnect();
					network_.StopGameServer();
					network_.StopServer();
				}
				break;
			}
			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step) {

		case STEP.SERVER_START:
			{
				if (this.step_timer > 1.0f){
					this.next_step = STEP.SERVER_CONNECT;
				}
			}
			break;

		case STEP.SERVER_CONNECT:
			{
				this.next_step = STEP.CLIENT_CONNECT;
			}
			break;
			
		case STEP.CLIENT_CONNECT:
			{
				this.next_step = STEP.PREPARE;
			}
			break;

		case STEP.PREPARE:
			{
				// 게임 시작 전 동기화 정보를 취득할 때까지 기다립니다.
				if (isReceiveSyncGameData) {
					this.next_step = STEP.GAME_START;
				}
			}
			break;
			
		case STEP.WAIT:
			{
			}
			break;

		case STEP.ERROR:
			{
				this.next_step = STEP.WAIT_RESTART;
			}
			break;

		case STEP.WAIT_RESTART:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

	void	OnGUI()
	{

		GUI.skin = this.gui_skin;

		// 배경 이미지.

		GUI.DrawTexture(new Rect(0.0f, 0.0f, Screen.width, Screen.height), this.title_image);

		int		x = Screen.width/2 - 100;
		int		y = Screen.height/2 - 100;


		if (this.step == STEP.WAIT_RESTART) {
			NotifyError();
		}
		else {
			// 스타트 버튼.

			x = 80;
			y = 220;

			this.hostAddress = GUI.TextField(new Rect(x, y, 150, 20), this.hostAddress);
			
			GUIStyle style = new GUIStyle();
			style.fontSize = 18;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			GUI.Label(new Rect(x, y-20, 200.0f, 50.0f), "친구 주소", style);
			y += 25;

			if(GUI.Button(new Rect(x, y, 150, 20), "친구를 기다린다")) {
				this.isHost = true;
				this.next_step = STEP.SERVER_START;
			}

			if(GUI.Button(new Rect(x+160, y, 100, 20), "놀러 간다")) {
				this.isHost = false;
				this.next_step = STEP.SERVER_START;
			}
		}


		//
		GUI.skin = null;
	}

	// ---------------------------------------------------------------- //
	// 통신 처리 함수.

	// 
	public void OnReceiveSyncGamePacket(PacketId id, byte[] data)
	{
		Debug.Log("Receive GameSyncPacket.[TitleControl]");
		
		SyncGamePacket packet = new SyncGamePacket(data);
		SyncGameData sync = packet.GetPacket();
		
		for (int i = 0; i < sync.itemNum; ++i) {
			string log = "[CLIENT] Sync item pickedup " +
				"itemId:" + sync.items[i].itemId +
					" state:" + sync.items[i].state + 
					" ownerId:" + sync.items[i].ownerId;
			Debug.Log(log);
			
			ItemManager.ItemState istate = new ItemManager.ItemState();
			
			// 아이템의 상태를 매니저에 등록.
			istate.item_id = sync.items[i].itemId;
			istate.state = (ItemController.State) sync.items[i].state;
			istate.owner = sync.items[i].ownerId;
			
			if (GlobalParam.get().item_table.ContainsKey(istate.item_id)) {
				GlobalParam.get().item_table.Remove(istate.item_id);
			}
			GlobalParam.get().item_table.Add(istate.item_id, istate);
		}

		isReceiveSyncGameData = true;
	}

	// 오류 통지.
	void NotifyError()
	{
		GUISkin skin = GUI.skin;
		GUIStyle style = new GUIStyle(GUI.skin.GetStyle("button"));
		style.normal.textColor = Color.white;
		style.fontSize = 25;
		
		float sx = 450;
		float sy = 200;
		float px = Screen.width / 2 - sx * 0.5f;
		float py = Screen.height / 2 - sy * 0.5f;
		
		string message = "게임을 시작할 수 없습니다.\n\n버튼을 누르세요.";
		if (GUI.Button (new Rect (px, py, sx, sy), message, style)) {
			this.step      = STEP.WAIT;
			this.next_step = STEP.NONE;
		}
	}
}
