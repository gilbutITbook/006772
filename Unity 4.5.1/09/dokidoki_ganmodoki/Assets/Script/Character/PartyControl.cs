using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Character {

// 소환수의 종류.
public enum BEAST_TYPE {

	NONE = -1,

	DOG = 0,	// 개.
	NEKO,		// 고양이.

	NUM,
}

}

public class PartyControl : MonoBehaviour {

	protected List<chrBehaviorPlayer>	players = new List<chrBehaviorPlayer>();

	protected chrBehaviorBase			beast;					// 소환수(동료).

	protected float 					summon_time = 0.0f;		// 소환시간.

	protected const float				SUMMON_INTERVAL = 30.0f;

	protected const float				SUMMON_TIME_CONDITION = 5.0f;	

	protected RoomController			current_room;
	protected RoomController			next_room;

	// 도어 개폐를 관리.
	protected Dictionary<string, bool>	door_state = new Dictionary<string, bool>();

	// 지금 있는 방 열쇠를 가지고 있는가?.

	protected Dictionary<string, int>	has_key  = new Dictionary<string, int>();


	protected Network 					m_network = null;

	protected enum SUMMON_STATE
	{
		INTERVAL = 0,			// 출현까지의 대기 시간.
		CHECK_APPEAR,			// 출현 체크.
		APPEAR,					// 출현 중.
	}

	protected	SUMMON_STATE			state = SUMMON_STATE.INTERVAL;


	protected Character.BEAST_TYPE		request_summon_beast = Character.BEAST_TYPE.NONE;	// 소환수(디버그용).
	

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{

		if(dbwin.root().getWindow("party") == null) {

			this.create_debug_window();
		}

		// Network클래스의 컴포넌트 취득.
		GameObject obj = GameObject.Find("Network");
		
		if(obj != null) {
			m_network = obj.GetComponent<Network>();
			if (m_network != null) {
				m_network.RegisterReceiveNotification(PacketId.MovingRoom, OnReceiveMovingRoomPacket);
				// 소환수 관리는 PartyControl로 변경됨.
				// 그 때문에 소환수 출현 패킷 수신 함수는 CharacterRoot에서 PartyControl로 이동함.
				m_network.RegisterReceiveNotification(PacketId.Summon, OnReceiveSummonPacket);
			}
		}

		this.next_room = null;
	}
	
	void	Update()
	{
		// 쿼리 갱신.
		this.update_queries();


		// 소환수 처리.
		switch (this.state) {

			case SUMMON_STATE.INTERVAL:
			{
				this.summon_time += Time.deltaTime;
				
				if (this.summon_time > SUMMON_INTERVAL) {
					this.state = SUMMON_STATE.CHECK_APPEAR;
				}
			}
			break;

			case SUMMON_STATE.CHECK_APPEAR:
			{
				// 소환수 출현 체크.
				this.checkSummonBeast();
			}
			break;

			case SUMMON_STATE.APPEAR:
			{
				// 소환수 해제 체크.
				this.summon_time += Time.deltaTime;
				
				if (this.summon_time > SUMMON_INTERVAL) {
					
					this.unsummonBeast();
				}	
			}
			break;
		}

		if (this.beast == null) {

		}
		else {

		}
	}

	// ---------------------------------------------------------------- //
	// 쿼리 갱신.

	protected void	update_queries()
	{
	}

	// ================================================================ //

	// 로컬 플레이어 만들기.
	public void		createLocalPlayer(int account_global_index)
	{
		if(this.players.Count == 0) {

			AccountData	account_data = AccountManager.get().getAccountData(account_global_index);

			string		avator_name = "Player_" + account_data.avator_id;

			chrBehaviorLocal	local_player = CharacterRoot.getInstance().createPlayerAsLocal(avator_name).GetComponent<chrBehaviorLocal>();

			local_player.control.local_index = 0;
			local_player.control.global_index = account_global_index;

			local_player.position_in_formation = this.getInFormationOffset(account_global_index);

			SHOT_TYPE shot_type = GlobalParam.get().shot_type[account_global_index];
			local_player.changeBulletShooter(shot_type);

			this.players.Add(local_player);
		}
	}

	// 네트 플레이어 만들기.
	public void		createNetPlayer(int account_global_index)
	{
		chrBehaviorLocal	local_player = this.players[0].GetComponent<chrBehaviorLocal>();

		chrBehaviorNet		net_player = null;

		int		local_index = this.players.Count;

		AccountData	account_data = AccountManager.get().getAccountData(account_global_index);

		string		avator_name = "Player_" + account_data.avator_id;

		net_player = CharacterRoot.getInstance().createPlayerAsNet(avator_name).GetComponent<chrBehaviorNet>();

		net_player.control.local_index  = local_index;
		net_player.control.global_index = account_global_index;
		net_player.local_player          = local_player;

		net_player.position_in_formation = this.getInFormationOffset(account_global_index);

		SHOT_TYPE shot_type = GlobalParam.get().shot_type[account_global_index];
		net_player.changeBulletShooter(shot_type);

		net_player.transform.Translate(this.getLocalPlayer().control.getPosition() + net_player.position_in_formation);

		this.players.Add(net_player);
	}

	// 네트 플레이어 삭제.
	public void		deleteNetPlayer(int account_global_index)
	{
		do {

			chrBehaviorPlayer	friend = this.getFriendByGlobalIndex(account_global_index);

			if(friend == null) {

				break;
			}

			this.players.Remove(friend);

			GameObject.Destroy(friend.gameObject);

		} while(false);
	}

	// 페이크 네트 플레이어 만들기.
	public void		createFakeNetPlayer(int account_global_index)
	{
		chrBehaviorLocal	local_player = this.players[0].GetComponent<chrBehaviorLocal>();

		chrBehaviorFakeNet	net_player = null;

		int		local_index = this.players.Count;

		AccountData	account_data = AccountManager.get().getAccountData(account_global_index);

		string		avator_name = "Player_" + account_data.avator_id;

		net_player = CharacterRoot.getInstance().createPlayerAsFakeNet(avator_name).GetComponent<chrBehaviorFakeNet>();

		net_player.control.local_index  = local_index;
		net_player.control.global_index = account_global_index;
		net_player.local_player          = local_player;

		net_player.position_in_formation = this.getInFormationOffset(account_global_index);

		net_player.transform.Translate(this.getLocalPlayer().control.getPosition() + net_player.position_in_formation);

		this.players.Add(net_player);
	}

	// 소환수 출현 체크.
	private void 	checkSummonBeast()
	{
		if(m_network == null) {
			return;
		}

		bool isInRange = true;

		if (this.players.Count <= 1) {
			// 혼자일 때는 나타나지 않음.
			return;
		}

		chrBehaviorLocal local_behavior = this.getLocalPlayer();

		Vector3 local_pos = local_behavior.transform.position;

		for (int gid = 0; gid < NetConfig.PLAYER_MAX; ++gid) {

			int node = m_network.GetClientNode(gid);

			if (m_network.IsConnected(node) == false) {
				continue;
			}

			chrBehaviorPlayer remote_behavior = this.getFriendByGlobalIndex(gid);

			if (remote_behavior == null) {
				isInRange = false;
				break;
			}

			// 5m 범위 내에 모여 있으면 나타난다.
			Vector3 remote_pos = remote_behavior.transform.position;
			if ((local_pos - remote_pos).magnitude > 5.0f) {
				isInRange = false;
				break;
			}

		}

		if (isInRange) {
			this.summon_time += Time.deltaTime;
		}
		else {
			// 출현 조건을 리셋.
			this.summon_time = 0.0f;
		}

		if (this.summon_time > SUMMON_TIME_CONDITION) {
			Character.BEAST_TYPE type = (Random.Range(0, 10) < 7)? Character.BEAST_TYPE.DOG : Character.BEAST_TYPE.NEKO;
			notifySummonBeast(type);
			this.summon_time = 0.0f;
		}
	}

	// 소환수를 소환한다.
	public void	notifySummonBeast(Character.BEAST_TYPE beast_type)
	{
		string				beast_name = "";

		do {
			
			if(this.beast != null) {
				
				break;
			}
			
			switch(beast_type) {
					
				case Character.BEAST_TYPE.DOG:
				{
					beast_name = "Dog";
				}
				break;
				
				case Character.BEAST_TYPE.NEKO:
				{
					beast_name = "Neko";
				}
				break;
			}
			
			if(beast_name == "") {
				
				break;
			}

			// 소환수 출현 알림.
			if (m_network != null) {
				
				SummonData data = new SummonData();
				
				data.summon = beast_name;
				
				SummonPacket packet = new SummonPacket(data);
				int serverNode = m_network.GetServerNode();
				m_network.SendReliable<SummonData>(serverNode, packet);
				
				Debug.Log("[CLIENT] send summon beast:" + beast_name);
			}

		} while(false);
	}


	// 소환수를 소환
	public void		summonBeast(string beast_name)
	{

		do {

			if(this.beast != null) {

				break;
			}

			if(beast_name == "") {

				break;
			}

			string		avator_name   = "Beast_" + beast_name;
			string		behavior_name = "chrBehaviorBeast_" + beast_name;

			chrController	chr = CharacterRoot.getInstance().summonBeast(avator_name, behavior_name);

			if(chr == null) {

				break;
			}
			
			this.beast = chr.behavior;

			this.beast.control.cmdSetPositionAnon(this.getLocalPlayer().control.getPosition() + Vector3.back*4.0f);

			this.summon_time = 0.0f;

			this.state = SUMMON_STATE.APPEAR;

			//Debug.Log("[CLIENT] Summon beast:" + beast_name);

		} while(false);
	}

	// 소환 해제.
	private void	unsummonBeast()
	{
		if (this.beast == null) {
			return;
		}

		this.summon_time += Time.deltaTime;

		string beast_name = this.beast.GetComponent<chrController>().name;
		GameObject go = GameObject.Find(beast_name);
		if (go != null) {

			GameObject.Destroy(go);
		}

		this.beast = null;

		this.summon_time = 0.0f;

		//Debug.Log("[CLIENT] Unsummon beast:" + beast_name);

		this.state = SUMMON_STATE.INTERVAL;
	}

	// ================================================================ //

	// 플레이어 캐릭터를 전부 가져오기 .
	public List<chrBehaviorPlayer>	getPlayers()
	{
		return(this.players);
	}

	// 파티 멤버 수 가져오기.
	public int	getPlayerCount()
	{
		return(this.players.Count);
	}

	// 자신 이외의 멤버 수 가져오기.
	public int	getFriendCount()
	{
		return(this.players.Count - 1);
	}

	// 자신 이외의 멤버 가져오기.
	public chrBehaviorPlayer	getFriend(int friend_index)
	{
		chrBehaviorPlayer	friend = null;

		if(0 <= friend_index && friend_index < this.players.Count - 1) {

			friend = this.players[friend_index + 1];
		}

		return(friend);
	}

	// 자신 이외의 파티 멤버를 글로벌 인덱스로 가져오기.
	public chrBehaviorPlayer	getFriendByGlobalIndex(int friend_global_index)
	{
		chrBehaviorPlayer	friend = null;

		foreach(var player in this.players) {

			if((player as chrBehaviorLocal) != null) {

				continue;
			}

			if(player.control.global_index != friend_global_index) {

				continue;
			}

			friend = player;
			break;
		}

		return(friend);
	}

	// 로컬 플레이어 획득.
	public chrBehaviorLocal		getLocalPlayer()
	{
		chrBehaviorLocal	player = null;

		if(this.players.Count > 0) {

			player = this.players[0].GetComponent<chrBehaviorLocal>();
		}

		return(player);
	}

	// 계정 이름을 사용하여 플레이어를 검색하고 획득.
	public chrBehaviorPlayer getPlayerWithAccountName(string account_name)
	{
		foreach (chrBehaviorPlayer player in players) {

			string player_name = player.name.Replace("Player_", "");
			if (player_name == account_name) {

				return player;
			}
		}

		return null;
	}

	// 플레이어가 열쇠를 들었다.
	public void	pickKey(string key_name, string account_name) 
	{
		chrBehaviorPlayer player = getPlayerWithAccountName(account_name);

		if (player == null) {
			return;
		}

		string[] key_str = key_name.Split('_');
		string key_id = key_str[0];

		if (has_key.ContainsKey(key_id) == false) {
			has_key.Add(key_id, player.control.local_index);
		}
		else {
			has_key[key_id] = player.control.local_index;
		}
	}

	public void	dropKey(string key_id, string account_name) 
	{
		chrBehaviorPlayer player = getPlayerWithAccountName(account_name);
		
		if (player == null) {
			return;
		}
		
		if (has_key.ContainsKey(key_id)) {
			has_key.Remove(key_id);
		}
	}

	// 플레이어가 열쇠를 가지고 있는가.
	public bool	hasKey(int  account_global_index, string door_id) 
	{
		string[] key_id = door_id.Split('_');

		if (has_key.ContainsKey(key_id[0]) == false) {
			return false;
		}

		return (has_key[key_id[0]] == account_global_index);
	}

	// ================================================================ //

	// 시작 위치 오프셋을 구한다.
	// (플레이어끼리 겹치지 않게).
	public Vector3	getPositionOffset(int account_global_index)
	{
		Vector3		offset = Vector3.zero;

		switch(account_global_index) {

			case 0: offset = new Vector3( 2.0f, 0.0f,  2.0f);	break;
			case 1:	offset = new Vector3(-2.0f, 0.0f,  2.0f);	break;
			case 2:	offset = new Vector3( 2.0f, 0.0f, -2.0f);	break;
			case 3:	offset = new Vector3(-2.0f, 0.0f, -2.0f);	break;
		}

		return(offset);
	}

	// 포메이션 이동 중(주로 FakeNet) 위치 오프셋.
	public Vector3	getInFormationOffset(int account_global_index)
	{
		Vector3		offset = this.getPositionOffset(account_global_index);

		Vector3		local_player_offset = this.getPositionOffset(GlobalParam.getInstance().global_account_id);

		return(offset - local_player_offset);
	}

	// ================================================================ //

	// '지금 있는 방' 설정.
	public void		setCurrentRoom(RoomController room)
	{
		this.current_room = room;
		this.next_room    = room;

		MapCreator.get().SetCurrentRoom(this.current_room);
	}

	// '지금 있는 방' 가져오기.
	public RoomController	getCurrentRoom()
	{
		return(this.current_room);
	}

	// '다음 방' 설정.
	public void		setNextRoom(RoomController room)
	{
		this.next_room = room;
	}

	// '다음 방' 가져오기.
	public RoomController	getNextRoom()
	{
		return(this.next_room);
	}

	// ================================================================ //

	// 도어 여닫기.
	// 개폐 제어를 PartyControl에서 하기보다 DoorControl에서 감시하는 편이.
	// 편리하므로 개폐 상태를 관리해서 DoorControl에서 상태를 가져오게.
	// 사양을 변경함.
	public void 	cmdMoveRoom(string key_id)
	{
		if (door_state.ContainsKey(key_id) == false) {
			string log = "[CLIENT] Add open door:" + key_id;
			Debug.Log(log);

			door_state.Add(key_id, true);
		}
	}

	public bool	isDoorOpen(string door_id)
	{
		string log = "[CLIENT] Search open door:";

		if (door_id == null) {
			// 이름 없는 도어는 열려있는 걸로 한다.
			log += "(null)";
			Debug.Log(log);

			return true;
		}

		string[] key_id = door_id.Split('_');

		log += key_id[0] + "[" + door_state.ContainsKey(key_id[0]) + "]";
		//Debug.Log(log);

		return door_state.ContainsKey(door_id);//key_id[0]);
	}

	public void clearDoorState(string door_id)
	{
		string log = "[CLIENT] clear open state:";
	
		do {
			if (door_id == null) {
				// 이름 없는 도어는 무시한다.
				log += null;
				break;
			}
			
			door_state.Remove(door_id);
			log += door_id;

		} while (false);

		Debug.Log(log);

	}

	// ================================================================ //
	// 방 이동 알림 패킷 수신 함수.

	public void OnReceiveMovingRoomPacket(int node, PacketId id, byte[] data)
	{
		RoomPacket packet = new RoomPacket(data);
		MovingRoom room = packet.GetPacket();

		string log = "[CLIENT] Receive moving room packet: " + room.keyId;
		Debug.Log(log);

		// 방 이동 명령 발행.
		cmdMoveRoom(room.keyId);
	}

	// 소환수 출현 정보 수신 함수.
	// 소환수는 PartyControl 클래스에서 관리하게 되었으므로.
	// 이쪽으로 이동.
	public void OnReceiveSummonPacket(int node, PacketId id, byte[] data)
	{		
		SummonPacket packet = new SummonPacket(data);
		SummonData summon = packet.GetPacket();

		string log = "[CLIENT] Receive summon packet: " + summon.summon;
		Debug.Log(log);

		if (this.beast != null) {
			// 이미 나타났므로 수신 패킷은 무시.
			Debug.Log("[CLIENT] Beast is already summoned.");
			return;
		}

		// 소환수 출현.
		this.summonBeast(summon.summon);
	}

	// ================================================================ //


	protected void		create_debug_window()
	{
		var		window = dbwin.root().createWindow("party");

		window.createButton("다음 사람")
			.setOnPress(() =>
			{
				GlobalParam.getInstance().global_account_id = (GlobalParam.getInstance().global_account_id + 1)%4;
				GlobalParam.getInstance().fadein_start = false;
	
				GameRoot.get().restartGameScane();
			});

		window.createButton("도와줘요~")
			.setOnPress(() =>
			{
				int		friend_count = PartyControl.get().getFriendCount();

				if(friend_count < 3) {

					int		friend_global_index = (GlobalParam.getInstance().global_account_id + friend_count + 1)%4;

					this.createFakeNetPlayer(friend_global_index);
				}
			});

		window.createButton("바이바~이")
			.setOnPress(() =>
			{
				int		friend_count = PartyControl.get().getFriendCount();

				if(friend_count >= 1) {

					chrBehaviorPlayer	player = this.getFriend(0);

					int		friend_global_index = player.control.global_index;

					this.deleteNetPlayer(friend_global_index);
				}
			});

		window.createButton("집합!")
			.setOnPress(() =>
			{
				int		friend_count = this.getFriendCount();

				for(int i = 0;i < friend_count;i++) {

					chrBehaviorFakeNet	friend = this.getFriend(i) as chrBehaviorFakeNet;

					if(friend == null) {

						continue;
					}

					friend.in_formation = true;
				}
			});

		window.createButton("해산!")
			.setOnPress(() =>
			{
				int		friend_count = this.getFriendCount();

				for(int i = 0;i < friend_count;i++) {

					chrBehaviorFakeNet	friend = this.getFriend(i) as chrBehaviorFakeNet;

					if(friend == null) {

						continue;
					}

					friend.in_formation = false;
				}
			});

		window.createButton("아저씨개!")
			.setOnPress(() =>
			{
				notifySummonBeast(Character.BEAST_TYPE.DOG);
				//this.request_summon_beast = Character.BEAST_TYPE.DOG;
			});

		window.createButton("아줌마냥!")
			.setOnPress(() =>
			{
				notifySummonBeast(Character.BEAST_TYPE.NEKO);
				//this.request_summon_beast = Character.BEAST_TYPE.NEKO;
			});

		window.createButton("소환해제")
			.setOnPress(() =>
			            {
				unsummonBeast();
			});
	}

	// ================================================================ //
	// 인스턴스.

	private	static PartyControl	instance = null;

	public static PartyControl	getInstance()
	{
		if(PartyControl.instance == null) {

			PartyControl.instance = GameObject.Find("GameRoot").GetComponent<PartyControl>();
		}

		return(PartyControl.instance);
	}

	public static PartyControl	get()
	{
		return(PartyControl.getInstance());
	}
}

