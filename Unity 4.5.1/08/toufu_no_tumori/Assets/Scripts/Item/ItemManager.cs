using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour {

	public	GameObject		item_base_prefab = null;

	public	GameObject[]	item_model_prefabs;
	
	// ---------------------------------------------------------------- //

	private List<QueryBase>		queries = new List<QueryBase>();		// 쿼리.

	// ---------------------------------------------------------------- //
	
	public struct ItemState
	{
		public string					item_id;		// 유니크한 id.
		public ItemController.State		state;			// 네트워크적인 상태.
		public string 					owner;			// 만든 사람(어카운트 명).
	}

	private List<ItemState>		item_request = new List<ItemState>();

	private Network 			m_network = null;


	// MonoBehaviour로부터 상속.
	// ================================================================ //

	void	Start()
	{
		// Network 클래스의  컴포넌트 획득.
		GameObject obj = GameObject.Find("Network");

		if(obj != null) {
			m_network = obj.GetComponent<Network>();
			m_network.RegisterReceiveNotification(PacketId.ItemData, OnReceiveItemPacket);
		}
	}
	
	void	Update()
	{
		this.process_item_query();
	}

	// 쿼리 갱신.
	protected void	process_item_query()
	{
		// 페일 세이프 & 개발용.
		foreach(var query in this.queries) {

			query.timer += Time.deltaTime;

			if(m_network == null) {

				// GameScene부터 시작했을 때(Title을 거치지 않음).
				// 네트워크 오브젝트가 만들지지 않는다.
				query.set_done(true);
				query.set_success(true);

			} else {

				// 타임 아웃.
				if(query.timer > 5.0f) {

					query.set_done(true);
					query.set_success(false);
				}
			}
		}

		this.queries.RemoveAll(x => x.isDone());
	}

	// ================================================================ //
	
	// 아이템을 만듭니다.
	public string		createItem(string type, string owner, bool active = true)
	{
		string	ret = "";

		do {

			ItemController	item = (GameObject.Instantiate(this.item_base_prefab) as GameObject).GetComponent<ItemController>();

			// 모델의 프리팹을 찾습니다.
	
			GameObject	item_model_prefab = null;
	
			string	prefab_name = "ItemModel_" + type;
	
			item_model_prefab = System.Array.Find(this.item_model_prefabs, x => x.name == prefab_name);
	
			if(item_model_prefab == null) {
	
				Debug.LogError("Can't find prefab \"" + prefab_name + "\".");
				break;
			}
	
			//
	
			GameObject	item_model_go = GameObject.Instantiate(item_model_prefab) as GameObject;
	
			item_model_go.transform.parent = item.transform;

			item.id            = type;
			item.owner_account = owner;
			item.model         = item_model_go;
	
			item.transform.parent = this.gameObject.transform;
			item.name             = item.id;
	
			item.rigidbody.isKinematic = true;

			// 비헤이비어.

			// 커스텀 비헤이비어는 컨트롤러의 자식(모델의 프리팹)에 붙어있습니다.
			item.behavior = item.gameObject.GetComponentInChildren<ItemBehaviorBase>();

			if(item.behavior == null) {

				item.behavior = item.gameObject.AddComponent<ItemBehaviorBase>();

			} else {

				// 비헤이비어가 처음부터 붙어 있을 때는 만들지 않습니다.
			}


			item.behavior.controll = item;
			item.behavior.is_active = active;
			item.behavior.initialize();

			ret = item.id;

		} while(false);

		return(ret);
	}

	// 아이템의 위치를 획득합니다.
	public bool		getItemPosition(out Vector3 position, string item_id)
	{
		bool	ret = false;

		position = Vector3.zero;

		do {

			ItemController	item = this.find_item(item_id);

			if(item == null) {
	
				break;
			}

			position = item.transform.position;

			ret = true;

		} while(false);

		return(ret);
	}

	// 아이템의 콜리전 크기를 가져옵니다.
	public bool		getItemSize(out Vector3 size, string item_id)
	{
		bool	ret = false;

		size = Vector3.zero;

		do {

			ItemController	item = this.find_item(item_id);

			if(item == null) {
	
				break;
			}

			size = item.gameObject.collider.bounds.size;

			ret = true;

		} while(false);

		return(ret);
	}

	// 아이템의 위치를 설정합니다.
	public void		setPositionToItem(string item_id, Vector3 position)
	{
		do {

			ItemController	item = this.find_item(item_id);

			if(item == null) {
	
				break;
			}

			// 땅에 떨어진 장소의 위치를 구합니다.

			float		radius = item.GetComponent<SphereCollider>().radius;
			Ray			ray    = new Ray(position + Vector3.up*radius*2.0f, Vector3.down);
			RaycastHit	hit;

			// 자기 자신에게 RAY가 이 닿지 않게.
			item.collider.enabled = false;

			if(Physics.SphereCast(ray, radius, out hit)) {

				position = hit.point;
			}

			item.collider.enabled = true;

			item.transform.position = position;

		} while(false);
	}

	// 아이템 표시/비표시 설정
	public void 	setVisible(string name, bool visible)
	{
		ItemController item = ItemManager.get().findItem(name);
		
		if (item == null) {
			return;
		}
		
		item.setVisible(visible);
	}
	
	// 떨어져 있는 아이템을 줍습니다.
	public ItemController	pickItem(QueryItemPick query, string owner_id, string item_id)
	{
		// 프로그램의 버그를 막고자 쿼리가 없으면
		// 주울 수 없게 했습니다.

		ItemController	item = null;
		
		do {
			
			// 일단 쿼리 결과도 체크합니다.
			if(!query.isSuccess()) {

				break;
			}
	
			item = this.find_item(item_id);
			
			if(item == null) {
				
				break;
			}

			item.picker = owner_id;
			item.startPicked();

		} while(false);

		return(item);
	}

	// 가지고 있는 아이템을 버립니다.
	public void 	dropItem(string owner_id, ItemController item)
	{
		item.picker = "";
	}


	// 아이템을 찾습니다.
	private	ItemController	find_item(string id)
	{
		ItemController	item = null;

		do {

			Transform	it = this.gameObject.transform.FindChild(id);

			if(it == null) {

				break;
			}

			item = it.gameObject.GetComponent<ItemController>();

		} while(false);

		return(item);
	}

	// 아이템을 찾습니다.
	public ItemController	findItem(string name)
	{
		ItemController	item = null;

		GameObject[]	items = GameObject.FindGameObjectsWithTag("Item");

		GameObject		go = System.Array.Find(items, x => x.name == name);

		if(go != null) {

			item = go.GetComponent<ItemController>();
		}

		return(item);
	}

	// 아이템을 성장 상태로 합니다(주울 수 있게 한다).
	public void		finishGrowingItem(string item_id)
	{
		ItemController	item = null;
		
		do {
	
			item = this.find_item(item_id);
			
			if(item == null) {
				
				break;
			}
	
			item.finishGrowing();

		} while(false);
	}

	// 아이템의 활성화/비활성화 설정.
	public void 	activeItme(string item_id, bool active)
	{
		ItemController	item = null;
		
		do {
			
			item = this.find_item(item_id);
			
			if(item == null) {
				
				break;
			}

			item.behavior.activeItem(active);
			
		} while(false);
	}

	// ================================================================ //
	// 비헤이비어용 커맨드.
	// 쿼리.

	// 떨어져 있는 아이템을 주워도 되는가?.
	public QueryItemPick	queryPickItem(string owner_id, string item_id, bool local, bool force)
	{
		ItemController		item = null;
		QueryItemPick		query = null;
		bool				notify = false;

		do {
			
			item = this.find_item(item_id);
			
			if(item == null) {
				break;
			}
		
			// 성장 중인 것은 주울 수 없습니다.
			if(!item.isPickable() && !force) {
				
				break;
			}
			
			// 이미 누군가가 소지 중인 것은 주울 수 없습니다.
			if(item.picker != "") {
				
				break;
			}
			
			query = new QueryItemPick(item_id);
			
			this.queries.Add(query);
			notify = true;
		} while(false);

		// 로컬 캐릭터가 아이템의 쿼리를 발행했을 때만 서버에 송신합니다.]
		if(notify && local) {
			// 아이템의 스테이트를 네트워크로 보냅니다.
			SendItemStateChanged(item_id, ItemController.State.PickingUp, owner_id);
		}

		return(query);
	}

	// 소유 중인 아이템을 버려도 되는가?.
	public QueryItemDrop	queryDropItem(string owner_id, ItemController item, bool local)
	{
		QueryItemDrop		query = null;

		do {
			
			// 자신의 것이 아닌 것은 주울 수 없습니다.
			if(item.picker != owner_id) {

				break;
			}

			query = new QueryItemDrop(item.id);

			this.queries.Add(query);

		} while(false);

		if(item != null && local) {
			// 아이템의 스테이트를 네트워크에 보냅니다.
			SendItemStateChanged(item.id, ItemController.State.Dropping, owner_id);
		}

		return(query);
	}


	// ================================================================ //

	public ItemState FindItemState(string item_name) 
	{
		foreach (ItemState state in GlobalParam.get().item_table.Values) {
			if (item_name.Contains(state.item_id)) {
				return state;
			}
		}

		ItemState dummy;
		dummy.item_id = "";
		dummy.state = ItemController.State.None;
		dummy.owner = "";

		return default(ItemState);
	}


	// ================================================================ //
	// 통신에 관한 함수.


	// 아이템 변경 통지 함수.
	private void SendItemStateChanged(string item_id, ItemController.State state, string owner_id)
	{
		if(m_network == null) {
			return;
		}

		Debug.Log("SendItemStateChanged.");

		// 아이템 획득을 문의합니다.
		ItemData data = new ItemData();
		
		data.itemId = item_id;
		data.ownerId = owner_id;
		data.state = (int)state;
		
		ItemPacket packet = new ItemPacket(data);
		
		m_network.SendReliable<ItemData>(packet);

		string log = "[CLIENT] SendItemStateChanged " +
			"itemId:" + item_id +
				" state:" + state.ToString() + 
				" ownerId:" + owner_id;
		Debug.Log(log);
	}

	// 아이템 정보 패킷 획득 함수.
	public void OnReceiveItemPacket(PacketId id, byte[] data)
	{
		ItemPacket packet = new ItemPacket(data);
		ItemData item = packet.GetPacket();

		// 서버의 상태와 동기화합니다.
		ItemState istate = new ItemState();
		istate.item_id = item.itemId;
		ItemController.State state = (ItemController.State)item.state;
		istate.state = (state == ItemController.State.Dropped)? ItemController.State.None : state;
		istate.owner = item.ownerId;
		if (GlobalParam.get().item_table.ContainsKey(item.itemId)) {
			GlobalParam.get().item_table.Remove(istate.item_id); 
		}
		GlobalParam.get().item_table.Add(istate.item_id, istate);

		string log = "[CLIENT] Receive itempacket. " +
			"itemId:" + item.itemId +
				" state:" + state.ToString() +
				" ownerId:" + item.ownerId;
		Debug.Log(log);
		
		if (state == ItemController.State.Picked) {
			Debug.Log("Receive item pick.");

			// 응답이 있는 쿼리를 검색.
			QueryItemPick	query_pick = null;
			foreach(var query in this.queries) {
				QueryItemPick pick = query as QueryItemPick;
				if (pick != null && pick.target == item.itemId) {
					query_pick = pick;
					break;
				}
			}

			bool remote_pick = true;

			if (query_pick != null) {
				if (item.ownerId == GlobalParam.get().account_name) {                                                 
					Debug.Log("Receive item pick local:" + item.ownerId);
					item_query_done(query_pick, true);
					remote_pick = false;
				}
				else {
					Debug.Log("Receive item pick remote:" + item.ownerId);
					item_query_done(query_pick, false);
				}
			}

			if (remote_pick == true) {
				// 리모트 캐릭터가 취득하게 합니다.
				chrController remote = CharacterRoot.getInstance().findPlayer(item.ownerId);
				if (remote) {
					// 아이템 획득 쿼리 발행.
					QueryItemPick query = remote.cmdItemQueryPick(item.itemId, false, true);
					if (query != null) {
						item_query_done(query, true);
					}
				}
			}
		}
		else if (state == ItemController.State.Dropped) {
			Debug.Log("Receive item drop.");

			// 응답이 있는 쿼리를 검색.
			QueryItemDrop	query_drop = null;
			foreach(var query in this.queries) {
				QueryItemDrop drop = query as QueryItemDrop;
				if (drop != null && drop.target == item.itemId) {
					query_drop = drop;
					break;
				}
			}

			bool remote_drop = true;

			if (query_drop != null) {
				// 요청에 대한 응답이 있을 때.
				if (item.ownerId == GlobalParam.get().account_name) { 
					// 자신이 획득.
					Debug.Log("Receive item drop local:" + item.ownerId);
					item_query_done(query_drop, true);
					remote_drop = false;
				}
				else {
					// 상대가 획득.
					Debug.Log("Receive item pick remote:" + item.ownerId);
					item_query_done(query_drop, false);
				}
			}

			if (remote_drop == true) {                                                 
				// 리모트 캐릭터가 회득하게 합니다.
				chrController remote = CharacterRoot.getInstance().findPlayer(item.ownerId);
				if (remote) {
					// 아이템 획득 쿼리 발행.
					Debug.Log ("QuetyitemDrop:cmdItemQueryDrop");
					QueryItemDrop query = remote.cmdItemQueryDrop(false);
					if (query != null) {
						query.is_drop_done = true;
						item_query_done(query, true);
					}
				}
			}
		}
		else {
			Debug.Log("Receive item error.");
		}
	}


	private void item_query_done(QueryBase query, bool success)
	{
		query.set_done(true);
		query.set_success(success);

		Debug.Log("cmdItemQuery done.");
	}

	// ================================================================ //
	// 인스턴스.

	private	static ItemManager	instance = null;

	public static ItemManager	getInstance()
	{
		if(ItemManager.instance == null) {

			ItemManager.instance = GameObject.Find("Item Manager").GetComponent<ItemManager>();
		}

		return(ItemManager.instance);
	}

	public static ItemManager	get()
	{
		return(ItemManager.getInstance());
	}
}
