using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameObjectExtension;

// 문.
// 방 사이의 문 또는 플로어 사이의 문.
public class DoorControl : MonoBehaviour {

	public Map.RoomIndex	room_index;		// 이 문이 설치된 방.

	public bool is_unlocked = false;		// 문 열쇠가 열렸는가?.
	
	public int KeyType;						// 문을 열 수 있는 열쇠의 색.

	public DoorControl		connect_to;		// 연결되어 있는 문(옆 방의 문).

	public Map.EWSN			door_dir;

	public enum TYPE {

		NONE = -1,

		ROOM = 0,			// 방 사이의 문.
		FLOOR,				// 플로어 사이의 문.

		NUM,
	};

	public TYPE		type = TYPE.NONE;

	protected 	RoomController		room;				// 이 문이 있는 방.
	protected	string				keyItemName;		// link to the key that can unlock this.

	protected	List<int>		entered_players = new List<int>();		// 이벤트 박스 안에 있는 플레어어의 local_index.

	// ================================================================ //

	protected enum STEP {

		NONE = -1,

		SLEEP = 0,				// 슬립(이벤트 박스가 반응하지 않게).
		WAIT_ENTER,				// 플레이어 전원이 이벤트 박스에 들어오길 기다린다.

		EVENT_IN_ACTION,		// 방 이동 이벤트 실행 중.

		WAIT_LEAVE,				// 플레이어 전원이 이벤트 박스에서 나오길 기다린다.

		NUM,
	};
	protected Step<STEP>	step = new Step<STEP>(STEP.NONE);

	public Network			m_network = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.step.set_next(STEP.WAIT_ENTER);

		this.setCreamVisible(true);

		// Network 클래스의 컴포넌트 획득.
		GameObject obj = GameObject.Find("Network");
		if(obj != null) {
			m_network = obj.GetComponent<Network>();
		}
	}
	
	void	Update()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.

		switch(this.step.do_transition()) {

			case STEP.WAIT_ENTER:
			{
				if(this.is_unlocked) {
					if(this.keyItemName == null) {
						if(this.entered_players.Count >= PartyControl.get().getPlayerCount()) {

							this.step.set_next(STEP.EVENT_IN_ACTION);
							PartyControl.get().clearDoorState(this.keyItemName);
						}
					}
					else {
						if(this.entered_players.Count > 0 &&
							PartyControl.get().isDoorOpen(this.keyItemName)) {

							this.step.set_next(STEP.EVENT_IN_ACTION);
							PartyControl.get().clearDoorState(this.keyItemName);
						}
					}
				}
			}
			break;

			case STEP.EVENT_IN_ACTION:
			{
			}
			break;

			case STEP.WAIT_LEAVE:
			{
				if(this.entered_players.Count == 0) {

					this.step.set_next(STEP.WAIT_ENTER);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.SLEEP:
				{
					if(this.moji_effect != null) {

						this.moji_effect.gameObject.destroy();

						this.moji_effect = null;
					}
				}
				break;

				case STEP.WAIT_ENTER:
				{
					if(this.is_unlocked) {

						if(this.moji_effect == null) {
	
							this.moji_effect = EffectRoot.get().createDoorMojisEffect(this.transform.position);
						}
					}
				}
				break;

				case STEP.WAIT_LEAVE:
				{
					if(this.moji_effect != null) {
	
						this.moji_effect.gameObject.destroy();
						this.moji_effect = null;
					}
				}
				break;

				case STEP.EVENT_IN_ACTION:
				{
					TransportEvent	transport_event = EventRoot.get().startEvent<TransportEvent>();

					if(transport_event != null) {

						transport_event.setDoor(this);

					} else {

						Debug.LogWarning("Can't begin Transport Event");
					}

					// 다음 방을 설정해 둔다.
					if(this.connect_to != null) {

						PartyControl.get().setNextRoom(this.connect_to.room);
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.WAIT_ENTER:
			{
			}
			break;

		}

		// ---------------------------------------------------------------- //

	}

	// 트리거에 충돌한 순간만 호출되는 메소드.
	void	OnTriggerEnter(Collider other)
	{
		// 이벤트 박스에 들어간 플레이어를 리스트에 추가한다.
		do {

			if(other.tag != "Player") {

				break;
			}

			chrController	player = other.gameObject.GetComponent<chrController>();

			if(player == null) {

				break;
			}

			if(player.local_index < 0) {

				break;
			}

			if(this.entered_players.Contains(player.local_index)) {

				break;
			}

			this.entered_players.Add(player.local_index);

			// 게임 서버에 알림.
			if (this.step.get_current() == STEP.WAIT_ENTER &&
				player.global_index == GlobalParam.get().global_account_id) {

				CharDoorState door = new CharDoorState();
				door.globalId = player.global_index;
				door.keyId = (this.keyItemName != null)? this.keyItemName : "NONE";
				door.isInTrigger = true;
				door.hasKey = (this.keyItemName != null)? PartyControl.getInstance().hasKey(player.local_index, door.keyId) : true;

				string log = "DoorId:" + door.keyId + " trigger:" + door.isInTrigger + " hasKey:" + door.hasKey;
				Debug.Log(log);

				DoorPacket packet = new DoorPacket(door);
				if (m_network != null) {
					int server_node = m_network.GetServerNode();
					m_network.SendReliable<CharDoorState>(server_node, packet);
				} else {
					PartyControl.get().cmdMoveRoom(door.keyId);
				}
			}

		} while(false);
	}

	// 트리거로부터 뭔가가 부딪힌 순간만 호출되는 메소드.
	void	OnTriggerExit(Collider other)
	{
		// 이벤트 박스에서 나온 플레이어를 리스트에서 제거.
		do {

			if(other.tag != "Player") {

				break;
			}

			chrController	player = other.gameObject.GetComponent<chrController>();

			if(player == null) {

				break;
			}

			if(!this.entered_players.Contains(player.local_index)) {

				break;
			}

			this.entered_players.Remove(player.local_index);

			// 게임 서버에 알림.
			if (player.global_index == GlobalParam.get().global_account_id) {
				CharDoorState door = new CharDoorState();
				door.globalId = player.global_index;
				door.keyId = (this.keyItemName != null)? this.keyItemName : "NONE";
				door.isInTrigger = false;
				door.hasKey = PartyControl.getInstance().hasKey(player.local_index, door.keyId);

				string log = "DoorId:" + door.keyId + " trigger:" + door.isInTrigger + " hasKey:" + door.hasKey;
				Debug.Log(log);

				DoorPacket packet = new DoorPacket(door);
				if (m_network != null) {
					int serer_node = m_network.GetServerNode();
					m_network.SendReliable<CharDoorState>(serer_node, packet);
				} else {
					PartyControl.get().clearDoorState(door.keyId);
				}
			}

		} while(false);
	}

	// ================================================================ //

	// EVENT_IN_ACTION을 시작한다.
	public void		beginEventInAction()
	{
		this.step.set_next(STEP.EVENT_IN_ACTION);
	}

	// WAIT_LEAVE를 시작한다.
	public void		beginWaitLeave()
	{
		this.step.set_next(STEP.WAIT_LEAVE);
	}

	// WAIT_ENTER를 시작한다.
	public void		beginWaitEnter()
	{
		this.step.set_next(STEP.WAIT_ENTER);
	}

	// 슬립.
	public void		beginSleep()
	{
		this.step.set_next(STEP.SLEEP);
	}

	// 슬립 해제.
	public void		endSleep()
	{
		if(this.step.get_current() == STEP.SLEEP && this.step.get_next() == STEP.NONE) {

			this.step.set_next(STEP.WAIT_ENTER);
		}
	}

	// 크림을 표시/비표시한다.
	public void		setCreamVisible(bool is_visible)
	{
		var	cream = this.transform.FindChild("Cream");

		if(cream != null) {

			cream.gameObject.SetActive(is_visible);
		}
	}

	// ================================================================ //

	public bool IsUnlocked()
	{
	#if true
		// 그림 소재 확인용으로 임시 변경함.
		return(is_unlocked);
	#else
		return is_unlocked || connect_to.is_unlocked;
	#endif
	}

	protected	DoorMojiControl		moji_effect = null;

	public void Unlock()
	{
		is_unlocked = true;

		this.setCreamVisible(!this.is_unlocked);

		if(this.room.isCurrent()) {

		}

		if(this.room.isCurrent()) {

			this.step.set_next(STEP.WAIT_ENTER);

		} else {

			this.step.set_next(STEP.SLEEP);
		}

		// 열쇠를 삭제한다.
		// 자신이 연결된 문 = 열쇠를 주운 문이 아닐 때는.
		// 열쇠 아이템을 여기서 삭제해 두지 않으면 안 된다.

		string	key_instance_name = Item.Key.getInstanceName((Item.KEY_COLOR)this.KeyType, this.room_index);

		ItemManager.getInstance().deleteItem(key_instance_name);
	}

	public Vector3	getPosition()
	{
		return(this.gameObject.transform.position);
	}

	// 문을 명시적으로 잠든다. 디버그에서만 사용될 것 같으므로 앞에 "db"(debug)를 붙였다.
	public void	dbLock()
	{
		is_unlocked = false;

		this.setCreamVisible(!this.is_unlocked);

		if(this.moji_effect != null) {

			GameObject.Destroy(this.moji_effect.gameObject);
			this.moji_effect = null;
		}
	}

	public void SetKey(string keyItemName)
	{
		this.keyItemName = keyItemName;
	}

	public void SetRoom(RoomController room)
	{
		this.room = room;
	}

	public RoomController GetRoom()
	{
		return this.room;
	}

	//
}
