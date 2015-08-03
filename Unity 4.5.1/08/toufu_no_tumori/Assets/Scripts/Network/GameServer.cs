using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
enum PickupState
{
	None = 0,
	Pickingup,
	Pickedup,
	Dropping,
	Dropped,
}
*/
enum PickupState
{
	Growing = 0, 			// 발생 중.
	None, 					// 미취득.
	PickingUp,				// 취득 중.
	Picked,					// 취득.
	Dropping,				// 폐기 중.
	Dropped,				// 폐기.
}

public class GameServer : MonoBehaviour {

	// 미취득 시 소유자 ID.
	private const string 	ITEM_OWNER_NONE = "";

	// 게임 서버에서 사용하는 포트 번호.
	private const int 		serverPort = 50764;

	// 게임 서버의 버전.
	public const int		SERVER_VERSION = 1; 	
		


	// 통신 모듈 컴포넌트.
	Network					network_ = null;

	// 아이템 상태.
	private struct ItemState
	{
		public string			itemId;			// 유니크한 id.
		public PickupState		state;			// 아이템 취득 상황.
		public string 			ownerId;		// 소유자.
	}

	Dictionary<string, ItemState>		itemTable_;

	
	void Awake() {
	
		itemTable_ = new Dictionary<string, ItemState>();

		GameObject go = new GameObject("ServerNetwork");
		network_ = go.AddComponent<Network>();
		if (network_ != null) {
			// 아이템 데이터 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.ItemData, this.OnReceiveItemPacket);
			// 이사 이벤트 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.Moving, this.OnReceiveReflectionData);
			// 놀러가는 이벤트 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.GoingOut, this.OnReceiveReflectionData);
			// 채팅 메시지 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.ChatMessage, this.OnReceiveReflectionData);
			// 게임 시작 전 정보 수신 함수 등록.
			network_.RegisterReceiveNotification(PacketId.GameSyncInfoHouse, this.OnReceiveReflectionData);

			// 이벤트 핸들러.
			network_.RegisterEventHandler(OnEventHandling);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public bool StartServer()
	{
		return network_.StartServer(NetConfig.SERVER_PORT, Network.ConnectionType.TCP);
	}

	public void StopServer()
	{
		network_.StopServer();
	}

	public void OnReceiveItemPacket(PacketId id, byte[] data)
	{
		ItemPacket packet = new ItemPacket(data);
		ItemData item = packet.GetPacket();

		PickupState state = (PickupState) item.state;

		string log = "[SERVER] ReceiveItemData " +
						"itemId:" + item.itemId +
						" state:" + state.ToString() +
						" ownerId:" + item.ownerId;
		Debug.Log(log);

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

		int count = 1;
		foreach (ItemState state in itemTable_.Values) {
			log = "[SERVER] current item[" + count +"]" + 
					"itemId:" + state.itemId + 
					" state:" + state.state.ToString() +
					" ownerId:" + state.ownerId;
			Debug.Log(log);
			++count;
		}

		if (itemTable_.ContainsKey(itemId) == false) {
			// 발견되지 않았으므로 신규 아이템으로 합니다.
			istate = new ItemState();

			istate.itemId = itemId;
			istate.state = PickupState.Picked;
			istate.ownerId = charId;
			// 신규 아이템으로서 등록.
			itemTable_.Add(itemId, istate);

			log = "[SERVER] Unregisterd item pickedup " +
					"itemId:" + itemId +
					" ownerId:" + istate.ownerId;
			Debug.Log(log);

			// 아이템 취득 상황에 변경이 있을 때는 단말에 알림
			SendItemState(istate);
			
			return;
		}

		istate = itemTable_[itemId];

		// 다른 캐릭터가 취득하지 않았는지 확인.
		if (istate.state == PickupState.None) {
			// 이 아이템은 취득 가능.
			istate.state = PickupState.Picked;
			istate.ownerId = charId;
			// 아이템 정보를 갱신.
			itemTable_[itemId] = istate;

			log = "[SERVER] Registerd item pickedup " +
					"itemId:" + itemId +
					" ownerId:" + istate.ownerId;
			Debug.Log(log);

			// 아이템의 취득 상황에 변경이 있을 때는 단말에 알림.
			SendItemState(istate);
		}
		else {
			log = "[SERVER] Already item pickedup " +
					"itemId:" + istate.itemId + "(" + itemId + ")"+
					" state:" + istate.state.ToString() +
					" ownerId:" + istate.ownerId;
			Debug.Log(log);
		}
	}
	

	void MediateDropItem(string itemId, string charId)
	{
		string log = "";

		// 아이템이 없을 때는 무시합니다.
		if (itemTable_.ContainsKey(itemId) == false) {
			return;
		}

		ItemState istate = itemTable_[itemId];
		if (istate.state != PickupState.Picked ||
		    istate.ownerId != charId) {
			// 이 아이템은 폐기할 수 없습니다.
			log = "[SERVER] Illegal item drop state " +
					"itemId:" + itemId +
					" state:" + istate.state.ToString() +
					" ownerId:" + istate.ownerId;
			Debug.Log(log);
			return;		
		}

		// 아이템을 폐기합니다.
		istate.state = PickupState.None;
		istate.ownerId = charId;
		itemTable_[itemId] = istate;
		
		log = "[SERVER] Item dropped " +
				"itemId:" + itemId +
				" state:" + istate.state.ToString() +
				" ownerId:" + istate.ownerId;
		Debug.Log(log);

		// 아이템이 폐기되었음을 게스트에게 알림.
		SendItemState(istate);

		istate.ownerId = ITEM_OWNER_NONE;
		itemTable_[itemId] = istate;
	}


	private bool SendItemState(ItemState state)
	{
		// 아이템 취득 응답.
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
		network_.SendReliable<ItemData>(packet);

		return true;
	}

	public void OnReceiveReflectionData(PacketId id, byte[] data)
	{
		Debug.Log("[SERVER]OnReceiveReflectionData");
		network_.SendReliableToAll(id, data);
	}


	private void SendGameSyncInfo()
	{
		Debug.Log("[SERVER]SendGameSyncInfo start");

		SyncGameData data = new SyncGameData();

		data.version = SERVER_VERSION;
		data.itemNum = itemTable_.Count;
		data.items = new ItemData[itemTable_.Count];

		// 서버에서는 이사 정보는 보내지 않습니다.
		data.moving = new MovingData();
		data.moving.characterId = "";
		data.moving.houseId = "";
		data.moving.moving = false;

		int index = 0;
		foreach (ItemState state in itemTable_.Values) {
			data.items[index].itemId = state.itemId;
			data.items[index].ownerId = state.ownerId;
			data.items[index].state = (int)state.state;
			string log = "[SERVER] Item sync[" + index + "]" +
					"itemId:" + data.items[index].itemId +
					" state:" + data.items[index].state +
					" ownerId:" + data.items[index].ownerId;
			Debug.Log(log);
			++index;
		}

		Debug.Log("[SERVER]SendGameSyncInfo end");

		SyncGamePacket packet = new SyncGamePacket(data);
		network_.SendReliable<SyncGameData>(packet);
	}

	private void DisconnectClient()
	{
		Debug.Log("[SERVER]DisconnectClient");

		network_.Disconnect();
	}


	// test

	void OnGUI()
	{
	}

	// ================================================================ //
	
	
	public void OnEventHandling(NetEventState state)
	{
		switch (state.type) {
		case NetEventType.Connect:
			Debug.Log("[SERVER]NetEventType.Connect");
			SendGameSyncInfo();
			break;

		case NetEventType.Disconnect:
			Debug.Log("[SERVER]NetEventType.Disconnect");
			DisconnectClient();
			break;
		}
	}

}
