using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour {

	public	GameObject		item_base_prefab = null;

	public	GameObject[]	item_model_prefabs;

	public Texture			texture_icon_soda_ice = null;
	public Texture			texture_ice_bar = null;

	// ---------------------------------------------------------------- //

	// ---------------------------------------------------------------- //

	public Network			m_network = null;

	public delegate 		void ItemEventHandler(ItemController.State state, string owner_id, string item_id);

	public class ItemState
	{
		public string					item_id = "";						// 怨좎쑀 id.
		public ItemController.State		state = ItemController.State.None;	// ?ㅽ듃?뚰겕???곹깭.
		public string 					owner = "";							// 留뚮뱺 ?щ엺(怨꾩젙 ?대쫫).
	}

	//private Hashtable	item_table = new Hashtable();

	private List<ItemState>			item_request = new List<ItemState>();

	// MonoBehaviour?먯꽌 ?곸냽.
	// ================================================================ //

	void	Start()
	{
		// Network ?대옒?ㅼ쓽 而댄룷?뚰듃 ?띾뱷.
		GameObject obj = GameObject.Find("Network");
		if (obj != null) {
			m_network = obj.GetComponent<Network> ();
			if (m_network != null) {
				m_network.RegisterReceiveNotification (PacketId.ItemData, OnReceiveItemPacket);
				m_network.RegisterReceiveNotification (PacketId.UseItem, OnReceiveUseItemPacket);
			}
		}

		if (obj == null || m_network == null) {
			Debug.LogError("ItemManager can't find Netowrk gameobject or Network component on the current scene.");
		}
	}
	
	void	Update()
	{
		process_item_request();

		//this.process_item_query();
	}
	
	// ================================================================ //
	
	public void		createItem(string type, string owner)
	{
		this.createItem(type, type, owner);
	}

	// ?꾩씠???앹꽦.
	public void		createItem(string type, string name, string owner)
	{
		do {

			ItemController	item = (GameObject.Instantiate(this.item_base_prefab) as GameObject).GetComponent<ItemController>();

			// 紐⑤뜽???꾨━??李얘린.
	
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
			item_model_go.transform.localPosition = Vector3.zero;
	
			item.type         = type;
			item.owner_account = owner;
			item.id           = name;
			item.name         = item.id;
	
			item.transform.parent        = this.gameObject.transform;
			item.transform.localPosition = Vector3.zero;
	
			item.GetComponent<Rigidbody>().isKinematic = true;

			// 鍮꾪뿤?대퉬??

			// 而ㅼ뒪? 鍮꾪뿤?대퉬?대뒗 而⑦듃濡ㅻ윭???먯떇(紐⑤뜽???꾨━????異붽??섏뼱 ?덉쓣 寃껋씠??
			item.behavior = item.gameObject.GetComponentInChildren<ItemBehaviorBase>();

			if(item.behavior == null) {

				item.behavior = item.gameObject.AddComponent<ItemBehaviorBase>();

			} else {

				// 鍮꾪뿤?대퉬?닿? 泥섏쓬遺???덉쓣 ?뚮뒗 留뚮뱾吏 ?딅뒗??
			}

			item.behavior.controll = item;
			item.behavior.initialize();

		} while(false);
	}

	// Delete Item (FIXME)
	public void		deleteItem(string id)
	{
		foreach (var item in GetComponentsInChildren<ItemController>()) {
			if (item.id == id) {
				Destroy(item.gameObject);
			}
		}
	}

	// ?꾩씠???꾩튂 媛?몄삤湲?
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

	// ?꾩씠?쒖쓽 而щ━???ш린 ?띾뱷.
	public bool		getItemSize(out Vector3 size, string item_id)
	{
		bool	ret = false;

		size = Vector3.zero;

		do {

			ItemController	item = this.find_item(item_id);

			if(item == null) {
	
				break;
			}

			size = item.gameObject.GetComponent<Collider>().bounds.size;

			ret = true;

		} while(false);

		return(ret);
	}

	// ?꾩씠???꾩튂 ?ㅼ젙.
	public void		setPositionToItem(string item_id, Vector3 position)
	{
		do {

			ItemController	item = this.find_item(item_id);

			if(item == null) {
	
				break;
			}

			// 吏硫댁뿉 ?⑥뼱吏?吏?먯쓽 ?꾩튂猷?援ы븳??

			Ray			ray    = new Ray(position + Vector3.up*1000.0f, Vector3.down);
			float		radius = item.GetComponent<SphereCollider>().radius;
			RaycastHit	hit;

			// ?먭린?먯떊?먭쾶 ?덉씠媛 異⑸룎?섏? ?딄쾶.
			item.GetComponent<Collider>().enabled = false;

			if(Physics.SphereCast(ray, radius, out hit)) {

				position = hit.point;
			}

			item.GetComponent<Collider>().enabled = true;

			item.transform.position = position;

		} while(false);
	}

	// ?⑥뼱???덈뒗 ?꾩씠??以띻린.
	public ItemController	pickItem(QueryItemPick query, string owner_id, string item_id)
	{
		// ?꾨줈洹몃옩??踰꾧렇瑜?留됯퀬??荑쇰━媛 ?놁쑝硫?
		// 以띿? 紐삵븯寃??덈떎.

		ItemController	item = null;
		
		do {
			
			// ?쇰떒 荑쇰━??寃곌낵??泥댄겕.
			if(!query.isSuccess()) {

				break;
			}
	
			item = this.find_item(item_id);
			
			if(item == null) {
				
				break;
			}

			item.picker = owner_id;
			item.startPicked();

			if (item_id.StartsWith("key")) {
				PartyControl.get().pickKey(item_id, owner_id);
			}

		} while(false);

		return(item);
	}

	// 媛吏怨??덈뒗 ?꾩씠?쒖쓣 踰꾨┛??
	public void 	dropItem(string owner_id, string item_id)
	{
		ItemController item = this.find_item(item_id);
		
		if(item == null) {
			
			return;
		}

		item.picker = "";

		if (item_id.StartsWith("key")) {
			PartyControl.get().dropKey(item_id, "");
		}
	}


	// ?꾩씠??李얘린.
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

	// ?꾩씠??李얘린.
	public ItemController	findItem(string name)
	{
		ItemController	item = null;

		do {

			GameObject[]	items = GameObject.FindGameObjectsWithTag("Item");
			if(items == null) {

				break;
			}

			GameObject	go = System.Array.Find(items, x => x.name == name);
			if(go == null) {

				break;
			}

			item = go.GetComponent<ItemController>();

		} while(false);

		return(item);
	}

	// ?꾩씠??李얘린.
	public List<ItemController>	findItems(System.Predicate<ItemController> pred)
	{
		GameObject[]			gos   = GameObject.FindGameObjectsWithTag("Item");

		List<ItemController>	items = new List<ItemController>();

		foreach(var go in gos) {

			var	controller = go.GetComponent<ItemController>();

			if(controller == null) {

				continue;
			}
			if(!pred(controller)) {

				continue;
			}

			items.Add(go.GetComponent<ItemController>());
		}

		return(items);
	}

	// ?꾩씠???곹깭 蹂寃?
	public void	setItemState(string name, ItemController.State state, string owner)
	{
		GameObject[]	items = GameObject.FindGameObjectsWithTag("Item");

		if (items.Length == 0) {
			return;
		}

		GameObject go = System.Array.Find(items, x => x.name == name);

		if (go == null) {
			return;
		}

		ItemController	item = go.GetComponent<ItemController>();
	
		if (item == null) {
			return;
		}

		item.state = state;
		item.owner_account = owner;

		string log = "Item state changed => " + 
					 "[item:" + name + "]" + 
					 "[state:" + state.ToString() + "]" +
					 "[owner:" + owner + "]";
		Debug.Log(log);
		dbwin.console().print(log);
	}
	
	// ?꾩씠???ъ슜.
	public void		useItem(int slot_index, Item.Favor item_favor, string user_name, string target_name, bool is_local)
	{
		do {

			chrController	user = CharacterRoot.getInstance().findPlayer(user_name);

			if(user == null) {

				break;
			}

			chrController	target = CharacterRoot.getInstance().findPlayer(target_name);

			if(target == null) {

				break;
			}

			//

			if(user_name == PartyControl.get().getLocalPlayer().getAcountID()) {

				user.onUseItemSelf(slot_index, item_favor);

			} else {

				target.onUseItemByFriend(item_favor, user);
			}

			// ?꾩씠???ъ슜???뚮┝.
			if (is_local) {

				SendItemUseData(item_favor, user_name, target_name);
			}

		} while(false);
	}

	public void SendItemUseData(Item.Favor item_favor, string user_name, string target_name)
	{
		ItemUseData data = new ItemUseData();
		
		Debug.Log("[CLIENT] SendItemUseData: user:" + user_name + " target:" + target_name);
		dbwin.console().print("[CLIENT] SendItemUseData: user:" + user_name + " target:" + target_name);

		data.userId = user_name;
		data.targetId = target_name;
		data.itemCategory = (int)item_favor.category;

		ItemUsePacket packet = new ItemUsePacket(data);
		
		if (m_network != null) {
			int serverNode = m_network.GetServerNode();
			m_network.SendReliable<ItemUseData>(serverNode, packet);
		}
	}


	// ================================================================ //
	// 鍮꾪뿤?대퉬?댁슜 而ㅻ㎤??
	// 荑쇰━ 怨?

	// ?⑥뼱???덈뒗 ?꾩씠?쒖쓣 二쇱썙???섎뒗媛?.
	public QueryItemPick	queryPickItem(string owner_id, string item_id, bool is_local, bool forece_pickup)
	{
		ItemController	item = null;
		QueryItemPick	query = null;
		bool			needMediation = is_local;

		do {
			
			item = this.find_item(item_id);
			
			if(item == null) {

				needMediation = false;
				break;
			}

			// ?깆옣 以묒씤 寃껋? 二쇱슱 ???녿떎.
			if(!item.isPickable() && !forece_pickup) {

				needMediation = false;
				break;
			}

			// ?대? ?꾧뎔媛 媛吏怨??덈뒗 寃껋? 二쇱슱 ???녿떎.
			if(item.picker != "") {

				needMediation = false;
				break;
			}

			query = new QueryItemPick(owner_id, item_id);

			QueryManager.get().registerQuery(query);

			// ?띾뱷 以??곹깭濡?蹂寃쏀븳??
			this.setItemState(item_id, ItemController.State.PickingUp, owner_id);

		} while(false);

		if (GameRoot.get().isNowCakeBiking() == false) {
			if(needMediation) {
				// ?꾩씠???띾뱷 臾몄쓽瑜??쒕떎.
				SendItemStateChanged(item_id, ItemController.State.PickingUp, owner_id);
			}
		}

		return(query);
	}

	// ?뚯? 以묒씤 ?꾩씠???먭린.
	public void	cmdDropItem(string owner_id, string item_id, bool local)
	{
		// ?꾩씠???ㅽ뀒?댄듃瑜??ㅽ듃?뚰겕濡??꾩넚?쒕떎.
		SendItemStateChanged(item_id, ItemController.State.Dropping, owner_id);
	}
	

	// ================================================================ //
	
	private void	process_item_request()
	{
		this.item_request.Clear();
	}

	// ================================================================ //
	
	public ItemState FindItemState(string item_name) 
	{
		foreach (ItemState state in GlobalParam.get().item_table.Values) {
			if (item_name.Contains(state.item_id)) {
				return state;
			}
		}

		return default(ItemState);
	}
	
	
	// ================================================================ //
	// ?듭떊??愿???⑥닔.
	
	
	// ?꾩씠???곹깭 蹂寃??듭? ?⑥닔.
	private void SendItemStateChanged(string item_id, ItemController.State state, string owner_id)
	{
		if(m_network == null) {
			return;
		}
		
		Debug.Log("SendItemStateChanged.");
		
		// ?꾩씠???띾뱷 臾몄쓽.
		ItemData data = new ItemData();
		
		data.itemId = item_id;
		data.ownerId = owner_id;
		data.state = (int)state;
		
		ItemPacket packet = new ItemPacket(data);

		int serverNode = m_network.GetServerNode();
		Debug.Log("ServerNode:" + serverNode);
		m_network.SendReliable<ItemData>(serverNode, packet);
		
		string log = "[CLIENT] SendItemStateChanged " +
			"itemId:" + item_id +
				" state:" + state.ToString() + 
				" ownerId:" + owner_id;
		Debug.Log(log);
		dbwin.console().print(log);
	}

	// ================================================================ //
	
	// ?꾩씠???뺣낫 ?⑦궥 痍⑤뱷 ?⑥닔.
	public void OnReceiveItemPacket(int node, PacketId id, byte[] data)
	{
		ItemPacket packet = new ItemPacket(data);
		ItemData item = packet.GetPacket();

		// ?쒕쾭 ?곹깭? ?숆린??
		ItemState istate = new ItemState();
		istate.item_id = item.itemId;
		ItemController.State state = (ItemController.State)item.state;
		istate.state = (state == ItemController.State.Dropped)? ItemController.State.None : state;
		istate.owner = item.ownerId;
		if (GlobalParam.getInstance().item_table.ContainsKey(istate.item_id)) {
			GlobalParam.getInstance().item_table.Remove(istate.item_id); 
		}
		GlobalParam.getInstance().item_table.Add(istate.item_id, istate);
		
		string log = "[CLIENT] Receive itempacket [" +
			"itemId:" + item.itemId +
				" state:" + state.ToString() +
				" ownerId:" + item.ownerId + "]";
		Debug.Log(log);
		dbwin.console().print(log);

		if (state == ItemController.State.Picked) {
			Debug.Log("Receive item pick.");
			dbwin.console().print("Receive item pick.");

			// ?묐떟???덈뒗  荑쇰━瑜??먯깋.
			QueryItemPick	query_pick = QueryManager.get().findQuery<QueryItemPick>(x => x.target == item.itemId);

			bool remote_pick = true;
			
			if (query_pick != null) {
				string account_name = PartyControl.get().getLocalPlayer().getAcountID();
				if (item.ownerId == account_name) {                                                 
					Debug.Log("Receive item pick local:" + item.ownerId);
					dbwin.console().print("Receive item pick local:" + item.ownerId);

					item_query_done(query_pick, true);
					remote_pick = false;
				}
				else {
					Debug.Log("Receive item pick remote:" + item.ownerId);
					dbwin.console().print("Receive item pick remote:" + item.ownerId);

					item_query_done(query_pick, false);
				}
			}
			
			if (remote_pick == true) {
				Debug.Log("Remote pick item:" + item.ownerId);
				dbwin.console().print("Remote pick item:" + item.ownerId);

				// 由щえ??罹먮┃?곌? 媛吏寃??쒕떎.
				chrController remote = CharacterRoot.getInstance().findPlayer(item.ownerId);
				if (remote) {
					// ?꾩씠???띾뱷 荑쇰━ 諛쒗뻾.
					QueryItemPick query = remote.cmdItemQueryPick(item.itemId, false, true);
					if (query != null) {
						item_query_done(query, true);
					}
				}
			}

			// ?꾩씠???띾뱷 ?곹깭 蹂寃?
			this.setItemState(item.itemId, ItemController.State.Picked, item.ownerId);
		}
		else if (state == ItemController.State.Dropped) {
			Debug.Log("Receive item drop.");	

			// ?묐떟???덈뒗 荑쇰━瑜?寃??
			QueryItemDrop	query_drop = QueryManager.get().findQuery<QueryItemDrop>(x => x.target == item.itemId);

			
			bool remote_drop = true;
			
			if (query_drop != null) {
				// ?붿껌??????묐떟???덈떎.
				string account_name = AccountManager.get().getAccountData(GlobalParam.get().global_account_id).avator_id;
				if (item.ownerId == account_name) { 
					// ?먯떊???띾뱷.
					Debug.Log("Receive item drop local:" + item.ownerId);
					item_query_done(query_drop, true);
					remote_drop = false;
				}
				else {
					// ?곷?媛 ?띾뱷.
					Debug.Log("Receive item pick remote:" + item.ownerId);
					item_query_done(query_drop, false);
				}
			}
			
			if (remote_drop == true) {                                                 
				// 由щえ??罹먮┃?곌? ?띾뱷.
				chrController remote = CharacterRoot.getInstance().findPlayer(item.ownerId);
				if (remote) {
					// ?꾩씠?쒗쉷??荑쇰━ 諛쒗뻾.
					Debug.Log ("QuetyitemDrop:cmdItemQueryDrop");
				 	remote.cmdItemDrop(item.itemId, false);
				}
			}
		}
		else {
			Debug.Log("Receive item error.");
		}
	}

	public void OnReceiveUseItemPacket(int node, PacketId id, byte[] data)
	{
		ItemUsePacket packet = new ItemUsePacket(data);
		ItemUseData useData = packet.GetPacket();

		Debug.Log ("Receive UseItemPacket:" + useData.userId + " -> " + 
		           								useData.targetId + " (" + useData.itemCategory + ")");

		chrController	user = CharacterRoot.getInstance().findPlayer(useData.userId);

		AccountData	account_data = AccountManager.get().getAccountData(GlobalParam.getInstance().global_account_id);

		if (user != null && account_data.avator_id != useData.userId) {
			Debug.Log("use item. favor:" + useData.itemFavor);

			Item.Favor	favor = new Item.Favor();
			favor.category = (Item.CATEGORY) useData.itemCategory;
			this.useItem(-1, favor, useData.userId, useData.targetId, false);
		}
		else {
			Debug.Log("Receive packet is already done.");
		}
	}
	
	private void item_query_done(QueryBase query, bool success)
	{
		query.set_done(true);
		query.set_success(success);
		
		Debug.Log("cmdItemQuery done.");
	}
	

	// ================================================================ //
	// ?몄뒪?댁뒪.

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
