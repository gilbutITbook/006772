using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathExtension;

public class chrController : MonoBehaviour {

	public static float		CARRIED_ITEM_HEIGHT = 2.0f;			// 운반 중인 아이템의 높이.

	// ================================================================ //

	public chrBehaviorBase		behavior = null;				// 비헤이비어　마우스 제어, NPC의 AI 등.
	public ChattyBalloon		balloon = null;					// 말풍선.
	public GameObject			model = null;					// 모델.

	public int		global_index = -1;							// 글로벌에서 고유한 어카운트 id.
	public int		local_index  = -1;							// 이 PC 내에서의 인덱스(로컬 플레이어가 0).

	public ItemManager			item_man;						// 아이템 매니저.
	public AcountManager		account_man;					// 어카운트 매니저
	public GameInput			game_input;						// 마우스 등의 입력.

	public	string				account_name = "";				// 어카운트 이름.
	public	AcountData			account_data = null;			// 어카운트 정보.

	public Vector3				prev_position;

	protected struct Motion {

		public string	name;
		public int		layer;
	};

	protected	Animation	anim_player = null;		// 애니메이션.
	protected	Motion		current_motion = new Motion();
	protected	bool		is_player = true;

	public ItemCarrier		item_carrier = null;	// 운반 중인 아이템.

	public	List<QueryBase>	queries = new List<QueryBase>();

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void 	Awake()
	{
		this.balloon = BalloonRoot.get().createBalloon();
		
		this.item_man    = ItemManager.getInstance();
		
		this.item_carrier = new ItemCarrier(this);
	}

	void	Start()
	{
		if(this.is_player) {

			this.account_data = AcountManager.get().getAccountData(account_name);
		}

		// 애니메이션 컴포넌트를 찾아둔다.
		this.anim_player = this.transform.GetComponentInChildren<Animation>();

		this.current_motion.name  = "";
		this.current_motion.layer = -1;

		this.behavior.start();
	}

	void	LateUpdate()
	{
		this.behavior.lateExecute();
	}

	void	Update()
	{
		// ---------------------------------------------------------------- //

		if(this.current_motion.name != "") {

			if(!this.anim_player.isPlaying) {
	
				this.current_motion.name  = "";
				this.current_motion.layer = -1;
			}
		}

		// 운반 중인 아이템.
		//
		this.item_carrier.execute();

		// ---------------------------------------------------------------- //
		// 비헤이비어 실행.
		//
		// （마우스로 이동(로컬), 네트워크에서 수신한 데이터로 이동(원격)).
		//

		this.behavior.execute();

		// ---------------------------------------------------------------- //
		// 속도를 클리어해둔다.

		if(!this.rigidbody.isKinematic) {

			this.rigidbody.velocity = this.rigidbody.velocity.XZ(0.0f, 0.0f);
			this.rigidbody.angularVelocity = Vector3.zero;
		}

		// ---------------------------------------------------------------- //
		// 말풍선 위치/색상.

		if(this.balloon.getText() != "") {

			Vector3		on_screen_position = Camera.main.WorldToScreenPoint(this.transform.position + Vector3.up*3.0f);

			this.balloon.setPosition(new Vector2(on_screen_position.x, Screen.height - on_screen_position.y));

			if(this.account_data != null) {

				this.balloon.setColor(this.account_data.favorite_color);
			}
		}


		// ---------------------------------------------------------------- //

		// 실행이 끝난 쿼리를 삭제한다. 
		this.queries.RemoveAll(x => x.isExpired());
	}

	// ================================================================ //

	public Vector3		getPosition()
	{
		return(this.transform.position);
	}
	public float		getDirection()
	{
		return(this.transform.rotation.eulerAngles.y);
	}

	// 장비 아이템을 가져온다.
	public T	getCarriedItem<T>() where T : ItemBehaviorBase
	{
		T	item_behavior = null;

		do {

			ItemController	item = this.item_carrier.item;

			if(item == null) {

				break;
			}

			if(item.behavior.GetType() != typeof(T)) {

				break;
			}

			item_behavior = item.behavior as T;

		} while(false);

		return(item_behavior);
	}

	// 장비 아이템의 특전을 반환한다.
	public ItemFavor	getItemFavor()
	{
		ItemFavor	favor = null;

		do {

			ItemController	item = this.item_carrier.item;

			if(item == null) {

				break;
			}

			if(item.behavior.item_favor == null) {

				break;
			}

			favor = item.behavior.item_favor;

		} while(false);

		if(favor == null) {

			favor = new ItemFavor();
		}

		return(favor);
	}

	// 다른 캐릭터에 터치된(옆에서 클릭) 때 호출된다.
	public void		touchedBy(chrController toucher)
	{
		this.behavior.touchedBy(toucher);
	}
	
	public void		setPlayer(bool is_player)
	{
		this.is_player = is_player;
	}

	// 표시/비표시 설정.
	public void		setVisible(bool is_visible)
	{
		Renderer[]	renderers = this.model.gameObject.GetComponentsInChildren<Renderer>();

		foreach(var renderer in renderers) {

			renderer.enabled = is_visible;
		}

		// 둥근 그림자도.
		Projector[]	projectors = this.gameObject.GetComponentsInChildren<Projector>();

		foreach(var projector in projectors) {

			projector.enabled = is_visible;
		}
	}

	// ================================================================ //
	// 비헤이비어가 사용하는 커맨드.

	// 위치를 설정한다.
	public void		cmdSetPosition(Vector3 position)
	{
		this.transform.position = position;
	}

	// 방향을 설정한다.
	public void		cmdSetDirection(float angle)
	{
		this.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
	}

	// 타깃 쪽으로 조금 회전한다.
	// (매 프레임 호출하면 매끄럽게 타깃 방향으로 향한다).
	public float		cmdSmoothHeadingTo(Vector3 target)
	{
		float	cur_dir = this.getDirection();

		do {

			Vector3		dir_vector = target - this.transform.position;

			dir_vector.y = 0.0f;

			dir_vector.Normalize();

			if(dir_vector.magnitude == 0.0f) {

				break;
			}

			float	tgt_dir = Quaternion.LookRotation(dir_vector).eulerAngles.y + 180.0f;

			cur_dir = Mathf.LerpAngle(cur_dir, tgt_dir, 0.1f);

			this.cmdSetDirection(cur_dir);

		} while(false);

		return(cur_dir);
	}

	// ================================================================ //
	// 비헤이비어가 사용하는 커맨드.
	// 모션계.

	// 모션 재생 시작.
	public void		cmdSetMotion(string motion_name, int layer)
	{
		do {

			if(this.anim_player == null) {

				break;
			}
			if(this.current_motion.layer > layer) {

				break;
			}
			if(motion_name == this.current_motion.name) {

				break;
			}

			this.current_motion.name  = motion_name;
			this.current_motion.layer = layer;

			this.anim_player[motion_name].speed = 1.0f;
			this.anim_player[motion_name].time  = 0.0f;
			this.anim_player.CrossFade(this.current_motion.name, 0.1f);

		} while(false);

	}

	// 모션を逆再生する（最終フレームから逆向きに）.
	public void		cmdSetMotionRewind(string motion_name, int layer)
	{
		do {

			if(this.anim_player == null) {

				break;
			}
			if(this.current_motion.layer > layer) {

				break;
			}
			if(motion_name == this.current_motion.name) {

				break;
			}

			this.current_motion.name  = motion_name;
			this.current_motion.layer = layer;

			this.anim_player[motion_name].speed = -1.0f;
			this.anim_player[motion_name].time  = this.anim_player[motion_name].length - 0.1f;
			this.anim_player.Play(this.current_motion.name);

		} while(false);

	}

	// 현재 모션을 가져온다.
	public string	cmdGetMotion()
	{
		return(this.current_motion.name);
	}

	// 모션 재생 중?.
	public bool		isMotionPlaying()
	{
		return(this.anim_player.isPlaying);
	}

	// ================================================================ //
	// 비헤이비어가 사용하는 커맨드.
	// 말풍선.

	// 말풍선을 표시한다.
	public void		cmdDispBalloon(string text)
	{
		this.balloon.setText(text);
	}

	// 프리셋을 사용해 말풍선을 표시한다.
	public void		cmdDispBalloon(int text_id)
	{
		this.balloon.setText(this.behavior.getPresetText(text_id));
	}


	// ================================================================ //
	// 비헤이비어가 사용하는 커맨드.
	// 아이템 계열.

	// 아이템를 만든다.
	public string		cmdItemCreate(string type)
	{
		return(this.item_man.createItem(type, this.account_name));
	}
	// 아이템의 위치를 설정한다.
	public void		cmdItemSetPosition(string item_id, Vector3 position)
	{
		this.item_man.setPositionToItem(item_id, position);
	}

	// 아이템를 픽업한다.
	public bool		cmdItemPick(QueryItemPick query, string owner_id, string item_id)
	{
		bool	ret = false;
	
		do {

			if(this.item_carrier.isCarrying()) {

				break;
			}

			ItemController	item = this.item_man.pickItem(query, owner_id, item_id);

			if(item == null) {

				break;
			}

			if(query.is_anon) {

				this.item_carrier.beginCarryAnon(item);

			} else {

				this.item_carrier.beginCarry(item);
			}

			ret = true;

		} while(false);

		return(ret);
	}
	// 아이템을 버린다.
	public bool		cmdItemDrop(string owner_id)
	{
		bool	ret = false;
	
		do {

			ItemController	item = this.item_carrier.item;

			if(item == null) {

				break;
			}

			// 아이템 매니저에 버린 사실을 알린다.
			if(owner_id == this.account_name) {

				this.item_man.dropItem(owner_id, item);
			}

			item.gameObject.rigidbody.isKinematic   = true;
			item.gameObject.transform.localPosition += Vector3.left*1.0f;
			item.gameObject.transform.parent        = this.item_man.transform;

			// 리스폰한다.
			//
			// 현재 사양에서는 버려진 아이템은 최초의 위치에 리스폰된다.
			// （그 자리에 버리지 않는다）
			//
			item.startRespawn();

			this.item_carrier.endCarry();

			ret = true;

		} while(false);

		return(ret);

	}

	// 쿼리　아이템을 픽업한다?.
	public QueryItemPick	cmdItemQueryPick(string item_id, bool local = true, bool force = false)
	{
		QueryItemPick	query = null;
	
		do {

			query = this.item_man.queryPickItem(this.account_name, item_id, local, force);

			if(query == null) {

				break;
			}

			this.queries.Add(query);

		} while(false);

		return(query);
	}

	// 쿼리　아이템을 버려도 되나?.
	public QueryItemDrop	cmdItemQueryDrop(bool local = true)
	{
		QueryItemDrop	query = null;
	
		do {

			if(!this.item_carrier.isCarrying()) {

				break;
			}

			query = this.item_man.queryDropItem(this.account_name, this.item_carrier.item, local);

			if(query == null) {

				break;
			}

			this.queries.Add(query);

		} while(false);

		return(query);
	}

	// 쿼리　대화（말풍선）.
	public QueryTalk	cmdQueryTalk(string words, bool local = false)
	{
		QueryTalk	query = null;
	
		do {

			query = CharacterRoot.get().queryTalk(words, local);

			if(query == null) {

				break;
			}

			this.queries.Add(query);

		} while(false);

		return(query);
	}

	// 쿼리　이사를 시작해도 되는가?.
	public QueryHouseMoveStart	cmdQueryHouseMoveStart(string house_name, bool local = true)
	{
		QueryHouseMoveStart	query = null;
	
		do {

			query = CharacterRoot.get().queryHouseMoveStart(house_name, local);

			if(query == null) {

				break;
			}

			this.queries.Add(query);

		} while(false);

		return(query);
	}

	// 쿼리　이사를 마쳐도 되는가?.
	public QueryHouseMoveEnd	cmdQueryHouseMoveEnd(bool local = true)
	{
		QueryHouseMoveEnd	query = null;
	
		do {

			query = CharacterRoot.get().queryHouseMoveEnd(local);

			if(query == null) {

				break;
			}

			this.queries.Add(query);

		} while(false);

		return(query);
	}

}
