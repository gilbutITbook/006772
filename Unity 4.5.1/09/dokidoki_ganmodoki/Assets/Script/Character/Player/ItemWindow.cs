using UnityEngine;
using System.Collections;

// 로컬 플레이어의 아이템 창.
public class ItemWindow : MonoBehaviour {

	// ---------------------------------------------------------------- //
	// 텍스처.

	public Texture			texture_waku = null;
	public Texture			texture_key_waku = null;

	public Texture[]		item_icon_keys;			// 열쇄 ×５.
	public Texture			item_icon_candy;		// 캔디.
	public Texture			item_icon_ice;			// 소다 아이스.

	// ---------------------------------------------------------------- //
	
	public chrBehaviorLocal	player = null;

	public Rect[]		item_rects = null;

	public int			clicked_slot = -1;

	// 스프라이트.
	protected	Sprite2DControl[]	sprite_key;				// 방 열쇠 ×４.
	protected	Sprite2DControl		sprite_key_waku;		// 방 열쇠 받침.
	protected	Sprite2DControl		sprite_floor_key;		// 플로어 키.
	protected	Sprite2DControl		sprite_floor_key_waku;	// 플로어 키 받침.
	protected	Sprite2DControl		sprite_candy;
	protected	Sprite2DControl		sprite_candy_waku;
	protected	Sprite2DControl[]	sprite_miscs;
	protected	Sprite2DControl[]	sprite_wakus;

	protected	bool		is_active = true;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		this.sprite_key   = new Sprite2DControl[4];
		this.sprite_miscs = new Sprite2DControl[Item.SlotArray.MISC_NUM];
		this.sprite_wakus = new Sprite2DControl[Item.SlotArray.MISC_NUM];
	}

	public static Vector2	WINDOW_SIZE = new Vector2(424.0f, 48.0f);
	public static Vector2	BASE_POS    = new Vector2(0.0f,  -Screen.height/2.0f + (WINDOW_SIZE.y/2.0f + 16.0f));

	public static Vector2 ICON_SIZE  = new Vector2(32.0f, 32.0f);
	public static Vector2 ICON_PITCH = new Vector2(32.0f, 32.0f);

	public static Vector2 KEY_ICON_SIZE       = new Vector2(24.0f, 24.0f);
	public static Vector2 FLOOR_KEY_ICON_SIZE = new Vector2(36.0f, 36.0f);

	void	Start()
	{
		this.item_rects = new Rect[Item.SlotArray.MISC_NUM];

		Vector2		base_pos = BASE_POS;

		base_pos.x += 8.0f;
		base_pos.y += 8.0f;

		// ---------------------------------------------------------------- //
		// 열쇠.

		Vector2		pos;

		// 방 열쇠.

		pos = base_pos - WINDOW_SIZE/2.0f;

		pos.x += KEY_ICON_SIZE.x*0.5f;
		pos.y += KEY_ICON_SIZE.y*0.5f;

		this.sprite_key_waku = Sprite2DRoot.get().createSprite(this.texture_key_waku, true);
		this.sprite_key_waku.setSize(ICON_SIZE);
		this.sprite_key_waku.setPosition(pos);

		foreach(var i in System.Linq.Enumerable.Range(0, 4)) {

			pos = base_pos - WINDOW_SIZE/2.0f;

			pos.x += KEY_ICON_SIZE.x*(i%2);
			pos.y += KEY_ICON_SIZE.y*(i/2);

			this.sprite_key[i] = Sprite2DRoot.get().createSprite(null, true);
			this.sprite_key[i].setSize(KEY_ICON_SIZE);
			this.sprite_key[i].setPosition(pos);
			this.sprite_key[i].setVisible(false);
		}

		// 플로어 키.

		pos.y = (base_pos - WINDOW_SIZE/2.0f).y;
		pos.y += KEY_ICON_SIZE.y*0.5f;
		pos.x += KEY_ICON_SIZE.x*1.2f;

		this.sprite_floor_key_waku = Sprite2DRoot.get().createSprite(this.texture_key_waku, true);
		this.sprite_floor_key_waku.setSize(ICON_SIZE);
		this.sprite_floor_key_waku.setPosition(pos);

		this.sprite_floor_key = Sprite2DRoot.get().createSprite(null, true);
		this.sprite_floor_key.setSize(FLOOR_KEY_ICON_SIZE);
		this.sprite_floor_key.setPosition(pos);
		this.sprite_floor_key.setVisible(false);

		// ---------------------------------------------------------------- //
		// 캔디.

		pos.y = (base_pos - WINDOW_SIZE/2.0f).y;
		pos.y += 16.0f;
		pos.x += 64.0f;

		this.sprite_candy_waku = Sprite2DRoot.get().createSprite(this.texture_waku, true);
		this.sprite_candy_waku.setSize(ICON_SIZE);
		this.sprite_candy_waku.setPosition(pos);

		this.sprite_candy = Sprite2DRoot.get().createSprite(null, true);
		this.sprite_candy.setSize(ICON_SIZE);
		this.sprite_candy.setPosition(pos);
		this.sprite_candy.setVisible(false);

		// ---------------------------------------------------------------- //
		// 기타.

		pos.x += 48.0f;

		for(int i = 0;i < (int)Item.SlotArray.MISC_NUM;i++) {

			this.sprite_wakus[i] = Sprite2DRoot.get().createSprite(this.texture_waku, true);
			this.sprite_wakus[i].setSize(ICON_SIZE);
			this.sprite_wakus[i].setPosition(pos);

			this.sprite_miscs[i] = Sprite2DRoot.get().createSprite(null, true);
			this.sprite_miscs[i].setSize(ICON_SIZE);
			this.sprite_miscs[i].setPosition(pos);
			this.sprite_miscs[i].setVisible(false);

			pos.x += ICON_SIZE.x + ICON_PITCH.x;
		}

		this.setActive(true);

	}

	void	Update()
	{
		// ---------------------------------------------------------------- //

		if(this.player == null) {

			this.player = PartyControl.getInstance().getLocalPlayer();
		}

		// ---------------------------------------------------------------- //

		// 아이템 아이콘(슬롯)을 클릭했을 때.
		if(this.clicked_slot >= 0) {

			GameInput	gi = GameInput.getInstance();
	
			if(gi.pointing.trigger_off) {

				do {

					// 아이템 사용 중.
					if(this.player.step.get_next() == chrBehaviorLocal.STEP.USE_ITEM) {

						break;
					}

					Item.Slot	slot = this.player.item_slot.miscs[this.clicked_slot];

					if(slot.favor == null) {

						break;
					}

					// 사용 중인 아이템(조작은 할 수 있게 됐지만 슬롯에서는 아직.
					// 삭제되지 않은 때).
					if(slot.is_using) {

						break;
					}

					this.player.control.cmdUseItemSelf(this.clicked_slot, slot.favor, true);

					SoundManager.getInstance().playSE(Sound.ID.DDG_SE_SYS03);

				} while(false);

				this.clicked_slot = -1;
			}
		}
	}

	// ================================================================ //

	// 룸 이동 후에 호출되고 싶다.
	public void		onRoomChanged(RoomController room)
	{
		// 열쇠 아이콘을 대응하는 도어 방향으로 표시한다.

		if(room != null) {

			Map.EWSN[]		key_dir = new Map.EWSN[4];
	
			for(int i = 0;i < 4;i++) {
	
				key_dir[i] = Map.EWSN.NONE;
			}
			for(int i = 0;i < (int)Map.EWSN.NUM;i++) {
	
				DoorControl	door = room.getDoor((Map.EWSN)i);
	
				if(door == null) {
	
					continue;
				}
	
				int	key_type = door.KeyType;

				if(key_type >= 4) {

					continue;
				}
				key_dir[key_type] = (Map.EWSN)i;
			}
	
			Vector2		base_pos = BASE_POS;
			Vector2		pos;
	
			base_pos.x += KEY_ICON_SIZE.x;
			base_pos.y += KEY_ICON_SIZE.y;

			for(int i = 0;i < 4;i++) {
	
				pos = base_pos - WINDOW_SIZE/2.0f;

				this.sprite_key[i].setVisible(true);

				// 도어가 없을 때는 비표시로.
				if(key_dir[i] == Map.EWSN.NONE) {
	
					this.sprite_key[i].setVisible(false);
					continue;
				}

				// 언락되지 않음 = 열쇠를 아직 줍지 않았으면 비표시.
				DoorControl	door = room.getDoor(key_dir[i]);

				if(!door.IsUnlocked()) {

					this.sprite_key[i].setVisible(false);
				}

				//

				float	offset = KEY_ICON_SIZE.x*0.6f;

				switch(key_dir[i]) {

					case Map.EWSN.NORTH:	pos += new Vector2(0.0f,  offset);	break;
					case Map.EWSN.SOUTH:	pos += new Vector2(0.0f, -offset);	break;
					case Map.EWSN.EAST:		pos += new Vector2( offset, 0.0f);	break;
					case Map.EWSN.WEST:		pos += new Vector2(-offset, 0.0f);	break;
				}
				this.sprite_key[i].setPosition(pos);
			}

		}
	}

	// ================================================================ //

	// 아이템 창의 좌표?？.
	public bool	isPositionInWindow(Vector2 position)
	{
		bool	ret = false;

		Vector2		p = Sprite2DRoot.get().convertScreenPosition(position);

		do {

			if(p.x < BASE_POS.x - WINDOW_SIZE.x/2.0f || BASE_POS.x + WINDOW_SIZE.x/2.0f < p.x) {

				break;
			}
			if(p.y < BASE_POS.y - WINDOW_SIZE.y/2.0f || BASE_POS.y + WINDOW_SIZE.y/2.0f < p.y) {

				break;
			}

			ret = true;

		} while(false);

		return(ret);
	}

	// 창이 클릭되었다.
	public bool	clickWindow(Vector2 position)
	{
		bool	is_clicked = false;

		this.clicked_slot = -1;

		do {

			// 이동 중이 아닐 때는 아이템을 사용할 수 없다.
			if(this.player.step.get_current() != chrBehaviorLocal.STEP.MOVE) {

				break;
			}

			is_clicked = true;

			Vector2		p = Sprite2DRoot.get().convertScreenPosition(position);

			this.clicked_slot = System.Array.FindIndex(this.sprite_miscs, x => x.isContainPoint(p));

			if(this.clicked_slot >= 0) {

				break;
			}

			is_clicked = false;

		} while(false);

		return(is_clicked);
	}

	// 표시/비표시.
	public void		setActive(bool is_active)
	{
		this.is_active = is_active;

		// 방 열쇠.
		foreach(var key in this.sprite_key) {

			if(key.getTexture() != null) {
				
				key.setVisible(this.is_active);

			} else {

				key.setVisible(false);
			}
		}
		this.sprite_key_waku.setVisible(this.is_active);

		// 플로어 키.
		if(this.sprite_floor_key.getTexture() != null) {
				
			sprite_floor_key.setVisible(this.is_active);
		}
		this.sprite_floor_key_waku.setVisible(this.is_active);

		// 캔디.
		if(this.sprite_candy.getTexture() != null) {

			this.sprite_candy.setVisible(this.is_active);

		} else {

			this.sprite_candy.setVisible(false);
		}
		this.sprite_candy_waku.setVisible(this.is_active);

		// 아이템.
		foreach(var misc in this.sprite_miscs) {

			if(misc.getTexture() != null) {

				misc.setVisible(this.is_active);

			} else {

				misc.setVisible(false);
			}
		}	
		foreach(var waku in this.sprite_wakus) {

			waku.setVisible(this.is_active);
		}	
	}
	
	// 아이템을 설정한다(아이콘을 표시한다).
	public void		setItem(Item.SLOT_TYPE slot, int slot_index, Item.Favor favor)
	{
		switch(slot) {

			//방 열쇠..
			case Item.SLOT_TYPE.KEY:
			{
				this.sprite_key[slot_index].setTexture(this.item_icon_keys[slot_index]);
				this.sprite_key[slot_index].setVisible(this.is_active);
			}
			break;

			// 플로어 이동 도어 열쇠.
			case Item.SLOT_TYPE.FLOOR_KEY:
			{
				this.sprite_floor_key.setTexture(this.item_icon_keys[(int)Item.KEY_COLOR.PURPLE]);
				this.sprite_floor_key.setVisible(this.is_active);
			}
			break;

			case Item.SLOT_TYPE.CANDY:
			{
				this.sprite_candy.setTexture(this.item_icon_candy);
				this.sprite_candy.setVisible(this.is_active);
			}
			break;

			case Item.SLOT_TYPE.MISC:
			{
				this.sprite_miscs[slot_index].setTexture(this.item_icon_ice);
				this.sprite_miscs[slot_index].setVisible(this.is_active);
			}
			break;
		}
	}

	// 아이템 클리어(슬롯을 비움)한다.
	public void		clearItem(Item.SLOT_TYPE slot, int slot_index)
	{
		switch(slot) {

			case Item.SLOT_TYPE.KEY:
			{
			if (this.sprite_key[slot_index] != null) {
					this.sprite_key[slot_index].setTexture(this.texture_waku);
					this.sprite_key[slot_index].setVisible(false);
				}
			}
			break;

			case Item.SLOT_TYPE.CANDY:
			{
				if (this.sprite_candy != null) {
					this.sprite_candy.setTexture(null);
					this.sprite_candy.setVisible(false);
				}
			}
			break;

			case Item.SLOT_TYPE.MISC:
			{
				if (this.sprite_miscs[slot_index] != null) {
					this.sprite_miscs[slot_index].setTexture(null);
					this.sprite_miscs[slot_index].setVisible(false);
				}
			}
			break;
		}
	}

	// 슬롯의 아이콘 위치를 가져온다.
	public Vector2		getIconPosition(Item.SLOT_TYPE slot, int slot_index)
	{
		Vector2		position = Vector2.zero;

		switch(slot) {

			case Item.SLOT_TYPE.KEY:
			{
				position = this.sprite_key[slot_index].getPosition();
			}
			break;

			case Item.SLOT_TYPE.CANDY:
			{
				position = this.sprite_candy.getPosition();
			}
			break;

			case Item.SLOT_TYPE.MISC:
			{
				position = this.sprite_miscs[slot_index].getPosition();
			}
			break;
		}

		return(position);
	}
	// ================================================================ //
	// 인스턴스.

	private	static ItemWindow	instance = null;

	public static ItemWindow	get()
	{
		if(ItemWindow.instance == null) {

			ItemWindow.instance = GameObject.Find("Item Window").GetComponent<ItemWindow>();
		}

		return(ItemWindow.instance);
	}
}
