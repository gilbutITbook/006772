using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathExtension;

//! 체력, 공격력.
public class Vital {

	public Vital()
	{
		this.hit_point  = 10.0f;
		this.shot_power   = 1.0f;
		this.attack_power = 10.0f;
		this.attack_distance = 2.0f;
	}

	// 체력 만땅.
	public void		healFull()
	{
		this.hit_point = 100.0f;
		SoundManager.getInstance().playSE(Sound.ID.DDG_SE_SYS04);
	}

	// 체력 풀로 채운다  연출 없음.
	public void		healFullInternal()
	{
		this.hit_point = 100.0f;
	}

	// 대미지를 준다.
	public void		causeDamage(float damage)
	{
		this.hit_point -= damage;
		this.hit_point = Mathf.Max(0.0f, this.hit_point);
	}

	// 슈팅 공격력을 구한다.
	public float	getShotPower()
	{
		return(this.shot_power);
	}

	// 근접 공격 공격력을 구한다.
	public float	getAttackPower()
	{
		return(this.attack_power);
	}
	// 근접 공격의 공격력을 설정한다.
	public void		setAttackPower(float power)
	{
		this.attack_power = power;
	}

	// 근접 공격이 닿는 거리를 구한다.
	public float	getAttackDistance()
	{
		return(this.attack_distance);
	}
	// 근접 공격이 닿는 거리를 설정한다.
	public void		setAttackDistance(float distance)
	{
		this.attack_distance = distance;
	}

	// 히트 포인트를 설정한다.
	public void		setHitPoint(float hp)
	{
		this.hit_point = hp;
	}

	// 히트 포인트를 구한다.
	public float	getHitPoint()
	{
		return(this.hit_point);
	}

	public float		hit_point;				// 체력.
	public float		shot_power;				// 슈팅 공격력.
	protected float		attack_power;			// 근접 공격의 공격력.

	protected float		attack_distance;		// 근접 공격이 닿는 거리.
};


public class chrController : MonoBehaviour {

	public int		global_index = -1;							// 글로벌 유니크 어카운트 id.
	public int		local_index  = -1;							// 이 PC 내에서의 인덱스（로컬 플레이어는 0）.

	protected Vector3	previous_position;						// 이전 프레임 위치.
	protected Vector3	move_vector = Vector3.zero;				// [m/dt] 이전 프레임으로부터의 위치 이동 벡터.


	public chrBehaviorBase		behavior = null;				/// 비헤이비어　마우스 제어, NPC의 AI 등.

	public chrBalloon			balloon = null;					// 말풍선.

	public Vital				vital = new Vital();			// 체력, 공격력.

	public bool					is_accept_damage = true;		// 대미지를 받는가？.

	public float				damage_after_timer = 0.0f;
	public bool					trigger_damage = false;

	public DamageEffect			damage_effect = null;

	public List<CollisionResult>	collision_results = new List<CollisionResult>();

	protected struct Motion {

		public string	name;
		public int		layer;
		public float	previous_time;				// [sec] 이전 프레임의 재생 시각.

	};

	protected	Animation	anim_player = null;				// 애니메이션.
	protected	Motion		current_motion = new Motion();

	public Animation	getAnimationPlayer()
	{
		return(this.anim_player);
	}

	// ================================================================ //
	// 이 클래스를 상속하는 클래스를 위한 인터페이스.

	// 디폴트 프로퍼티 변경에.
	protected virtual void _awake()
	{
	}

	// 첫 프레임의 업데이트 전에.
	protected virtual void _start()
	{
	}
	
	// 그리기 프레임마다 매번 호출된다.
	protected virtual void	execute()
	{
	}

	// 고정 업데이트.
	protected virtual void fixedExecute()
	{
	}

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		// 말풍선.
		this.balloon = this.gameObject.AddComponent<chrBalloon>();

		// 대미지를 먹었을 때 효과(임시).
		this.damage_effect = new DamageEffect(this.gameObject);

		// 애니메이션 컴포넌트를 찾아둔다.
		this.anim_player = this.transform.GetComponentInChildren<Animation>();
		if (this.anim_player != null) {
			this.anim_player.cullingType = AnimationCullingType.AlwaysAnimate;
		}

		this.current_motion.name  = "";
		this.current_motion.layer = -1;

		this._start();

		this.behavior.start();

		this.previous_position = this.transform.position;
	}
	
	void FixedUpdate()
	{
		// ---------------------------------------------------------------- //
		// 속도를 클리어해 둔다.

		if(!this.rigidbody.isKinematic) {

			this.rigidbody.velocity = this.rigidbody.velocity.XZ(0.0f, 0.0f);
			this.rigidbody.angularVelocity = Vector3.zero;
		}

		this.fixedExecute();

		this.collision_results.Clear();
	}

	void 	Awake()
	{
		_awake();
	}

	void	Update()
	{
		this.damage_after_timer = Mathf.Max(0.0f, this.damage_after_timer - Time.deltaTime);

		if(this.trigger_damage) {
	
			this.damage_effect.startDamage();

			this.damage_after_timer = 1.0f;
		}

		this.move_vector       = this.getPosition() - this.previous_position;
		this.previous_position = this.getPosition();

		this.trigger_damage = false;

		// ---------------------------------------------------------------- //

		if(this.current_motion.name != "" && this.anim_player != null) {

			if(!this.anim_player.IsPlaying(this.current_motion.name)) {
	
				this.current_motion.name  = "";
				this.current_motion.layer = -1;
			}
		}

		// ---------------------------------------------------------------- //
		// 상속 클래스의 컨트롤러 실행.
		//
		this.execute();

		// ---------------------------------------------------------------- //
		// 비헤이비어 실행.
		//
		// （마우스 이동(로컬), 네트워크로 수신한 데이터로 이동(원격)）.
		//

		this.behavior.execute();

		this.damage_effect.execute();

		// ---------------------------------------------------------------- //
		// 말풍선 위치 / 색상.

		if(this.balloon.text != "") {

			Camera	camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

			Vector3		on_screen_position = camera.WorldToScreenPoint(this.transform.position + Vector3.up*0.0f);

			this.balloon.position = new Vector2(on_screen_position.x, Screen.height - on_screen_position.y);


			AccountData account_data = AccountManager.get().getAccountData(this.getAccountID());
			this.balloon.setColor(account_data.favorite_color);
		}

		// ---------------------------------------------------------------- //

		this.current_motion.previous_time = this.getMotionCurrentTime();
	}

	// ================================================================ //

	// 표시 / 비표시 설정한다.
	public void			setVisible(bool is_visible)
	{
		Transform		model = this.transform.FindChild("model");

		if(model != null) {

			model.gameObject.SetActive(is_visible);

		} else {

			for(int i = 0;i < this.transform.childCount;i++) {

				this.transform.GetChild(i).gameObject.SetActive(is_visible);
			}
		}
	}

	// １프레임 전 위치를 구한다.
	public Vector3		getPreviousPosition()
	{
		return(this.previous_position);
	}

	// 위치를 구한다.
	public Vector3		getPosition()
	{
		return(this.transform.position);
	}

	// [degree] 캐릭터 방향을 구한다.
	public float		getDirection()
	{
		return(this.transform.rotation.eulerAngles.y);
	}

	// [m/dt] 이전 프레임에서의 위치 이동 벡터를 구한다.
	public Vector3	getMoveVector()
	{
		return(this.move_vector);
	}

	// 어카운트 이름을 구한다.
	public string	getAccountID()
	{
		return(AccountManager.get().getAccountData(this.global_index).account_id);
	}

	// 컬리전 반경(XZ).
	public float	getCollisionRadius()
	{
		float	radius = -1.0f;

		do {

			Collider	coli = this.gameObject.transform.GetComponent<Collider>();

			if(coli == null) {

				break;
			}

			Vector3		scale = this.gameObject.transform.localScale;

			radius = new Vector2(coli.bounds.size.x*scale.x, coli.bounds.size.z*scale.z).magnitude/2.0f;

		} while(false);

		return(radius);
	}

	// ================================================================ //
	// 비헤이비어가 사용하는 커맨드.

	// 위치를 설정한다.
	public void		cmdSetPosition(Vector3 position)
	{
		if(this.rigidbody != null && !this.rigidbody.IsSleeping()) {

			this.rigidbody.MovePosition(position);

		} else {

			this.transform.position = position;
		}
	}

	// 위치를 바로 설정한다.
	public void		cmdSetPositionAnon(Vector3 position)
	{
		this.transform.position = position;
	}

	// 방향을 설정한다.
	public void		cmdSetDirection(float angle)
	{
		if(this.rigidbody != null && !this.rigidbody.IsSleeping()) {

			this.rigidbody.MoveRotation(Quaternion.AngleAxis(angle, Vector3.up));

		} else {

			this.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
		}
	}

	// 방향을 바로 설정한다.
	public void		cmdSetDirectionAnon(float angle)
	{
		this.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
	}

	// 타깃 쪽으로 조금만 회전한다.
	// （매 프레임 호출하면 매끄럽게 타깃 쪽을 향한다).
	public float		cmdSmoothHeadingTo(Vector3 target, float turn_rate = 0.1f)
	{
		float	cur_dir = this.getDirection();

		do {

			Vector3		dir_vector = target - this.transform.position;

			dir_vector.y = 0.0f;

			dir_vector.Normalize();

			if(dir_vector.magnitude == 0.0f) {

				break;
			}

			float	tgt_dir = Quaternion.LookRotation(dir_vector).eulerAngles.y;

			cur_dir = Mathf.LerpAngle(cur_dir, tgt_dir, turn_rate);

			this.cmdSetDirection(cur_dir);

		} while(false);

		return(cur_dir);
	}
	// 지정한 방향으로 조금만 회전한다.
	// （매 프레임 호출하면 매끄럽게 타깃 쪽을 향한다).
	public float		cmdSmoothDirection(float target_dir, float turn_rate = 0.1f)
	{
		float	cur_dir = this.getDirection();

		float	dir_diff = target_dir - cur_dir;

		if(dir_diff > 180.0f) {

			dir_diff = dir_diff - 360.0f;

		} else if(dir_diff < -180.0f) {

			dir_diff = dir_diff + 360.0f;
		}

		if(Mathf.Abs(dir_diff*(1.0f - turn_rate)) < 1.0f) {

			cur_dir = target_dir;

		} else {

			dir_diff *= turn_rate;
			cur_dir += dir_diff;
		}

		this.cmdSetDirection(cur_dir);

		return(cur_dir);
	}

	// ================================================================ //
	// 비헤이비어가 사용하는 커맨드.
	// 모션.

	// 모션을 설정한다.
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
			this.current_motion.previous_time = -Time.deltaTime;

			this.anim_player.Play(this.current_motion.name);

		} while(false);

	}

	// 재생 중인 모션을 가져온다.
	public string	getMotion()
	{
		return(this.current_motion.name);
	}

	// [sec] 모션의 현재 재생 시각을 구한다.
	public float	getMotionCurrentTime()
	{
		float	time = 0.0f;

		do {

			if(this.current_motion.name == "") {

				break;
			}

			if(this.anim_player[this.current_motion.name] == null) {

				break;
			}

			time = this.anim_player[this.current_motion.name].time;

		} while(false);

		return(time);
	}

	// [sec] 이전 프레임의 모션 재생 시각을 구한다.
	public float	getMotionPreviousTime()
	{
		return(this.current_motion.previous_time);
	}

	// 모션이 지정한 재생 시각을 지나는 순간?？.
	public bool		isMotionAcrossingTime(float time)
	{
		return(this.getMotionPreviousTime() < time && time <= this.getMotionCurrentTime());
	}

	// 모션 재생 정보를 리셋한다.
	public void		resetMotion()
	{
		this.current_motion.name  = "";
		this.current_motion.layer = -1;
	}

	// ================================================================ //
	// 비헤이비어가 사용하는 커맨드.
	// 아이템.

	// 아이템을 만든다.
	public void		cmdItemCreate(string type)
	{
		ItemManager.getInstance().createItem(type, type, AccountManager.get().getAccountData(this.global_index).avator_id);
	}

	// 아이템 위치를 설정한다.
	public void		cmdItemSetPosition(string item_id, Vector3 position)
	{
		ItemManager.getInstance().setPositionToItem(item_id, position);
	}

	// 아이템 줍는다.
	public ItemController		cmdItemPick(QueryItemPick query, string item_id)
	{
		ItemController	item = null;

		do {

			string account_id = PartyControl.get().getLocalPlayer().getAcountID();
			item = ItemManager.getInstance().pickItem(query, account_id, item_id);

			if(item == null) {

				break;
			}

			item.gameObject.transform.parent        = this.gameObject.transform;
			item.gameObject.transform.localPosition = Vector3.up*300.0f;
			item.gameObject.rigidbody.isKinematic   = true;

			Debug.Log("cmdItemPick:" + item_id);
			dbwin.console().print("cmdItemPick:" + item_id);

		} while(false);

		return(item);
	}

	// 아이템을 버린다.
	public void		cmdItemDrop(string item_id, bool is_local = true)
	{
		ItemManager.getInstance().cmdDropItem(this.getAccountID(), item_id, is_local);
	}

	// 자신에게 아이템을 사용한다.
	public void		cmdUseItemSelf(int slot, Item.Favor item_favor, bool is_local)
	{
		// 이 부분은 ItemWindow 등에서 아이템을 사용하고 싶을 때.
		// 호출하는 메소드다.

		string	account_id = AccountManager.get().getAccountData(this.global_index).account_id;

		ItemManager.getInstance().useItem(slot, item_favor, account_id, account_id, is_local);
	}

	// 동료에게 아이템을 사용한다.
	public void		cmdUseItemToFriend(Item.Favor item_favor, int friend_global_index, bool is_local)
	{
		// 이 부분은 ItemWindow 등에서 아이템을 사용하고 싶을 때.
		// 호출하는 메시지다.

		string	account_id        = AccountManager.get().getAccountData(this.global_index).account_id;
		string	friend_account_id = AccountManager.get().getAccountData(friend_global_index).account_id;

		ItemManager.getInstance().useItem(-1, item_favor, account_id, friend_account_id, is_local);
	}

	// 쿼리 　아이템을 줍는가？.
	public QueryItemPick	cmdItemQueryPick(string item_id, bool is_local, bool force_pickup)
	{
		QueryItemPick	query = null;

		do {


			query = ItemManager.get().queryPickItem(this.getAccountID(), item_id, is_local, force_pickup);

			if(query == null) {

				break;
			}

		} while(false);

		return(query);
	}

	// ================================================================ //
	// 비헤이비어가 사용하는 커맨드.
	// 말풍선.
	
	// 말풍선을 표시한다.
	public void		cmdDispBalloon(string text)
	{
		this.balloon.setText(text, 8.0f);
	}
	
	// 프리셋을 사용해서 말풍선을 표시한다.
	public void		cmdDispBalloon(int text_id)
	{
		this.balloon.text = this.behavior.getPresetText(text_id);
	}
	
	// 쿼리 　대화(말풍선).
	public QueryTalk	cmdQueryTalk(string words, bool local = false)
	{
		QueryTalk	query = CharacterRoot.get().queryTalk(this.getAccountID(), words, local);

		return(query);
	}

	// ================================================================ //


	// 퇴장한다.
	public void		cmdBeginVanish()
	{
		// 애니메이션을 멈춘다.
		Animation[] animations = this.gameObject.GetComponentsInChildren<Animation>();
		
		foreach(var animation in animations) {
			
			animation.Stop();
		}
		
		this.damage_effect.startVanish();

		this.rigidbody.Sleep();
		this.collider.enabled = false;
	}

	// 컬리전을 ON/OFF 한다.
	public void		cmdEnableCollision(bool is_enable)
	{
		if(this.rigidbody != null) {

			if(is_enable) {
	
				this.rigidbody.WakeUp();
	
			} else {
	
				this.rigidbody.Sleep();
			}
		}

		if(this.collider != null) {

			this.collider.enabled = is_enable;
		}
	}

	// 대미지를 받는다 / 받지 않는다를 설정한다.
	public void		cmdSetAcceptDamage(bool is_accept)
	{
		this.is_accept_damage = is_accept;
	}

	// ================================================================ //

	// 대미지.
	// attacker_gidx ...	대미지를 준 플레이어의 global_index
	//						적이 플레이어에게 대미지를 주었을 때는 -1
	public void		causeDamage(float damage, int attacker_gidx, bool is_local = true)
	{
		if(!this.is_accept_damage && is_local) {

			return;
		}

		//string log = "[CLIENT][" + ((is_local)? "Local" : "Remote") + "]causeDamage called:" + damage + "[" + attacker_gidx + "]";
		//Debug.Log(log);

		if (!is_local && attacker_gidx == GlobalParam.get().global_account_id) {
			// 자신이 공격한 정보일 때는 이미 반영되었으므로 아무것도 하지 않는다.
			return;
		}

		this.trigger_damage = true;

		this.vital.causeDamage(damage);

		this.behavior.onDamaged();

		if (is_local) {
			//Debug.Log("[CLIENT][Local]Send damage data:" + this.name + "[" + damage + "]");

			if (this.name.StartsWith("Player")) {	
				CharacterRoot.get().NotifyHitPoint(this.behavior.name, this.vital.hit_point);
			}
			else {
				CharacterRoot.get().NotifyDamage(this.behavior.name, GlobalParam.get().global_account_id, damage);
			}
		}
	}

	// HP를 설정한다.
	public void		setHitPoint(float hp)
	{
		this.vital.setHitPoint(hp);
		
		this.behavior.onDamaged();
	}

	// 아이템을 사용한다(스스로 자신에게).
	public void		onUseItemSelf(int slot_index, Item.Favor favor)
	{
		// 이 부분은 실제로 아이템을 사용할 때 아이템 매니저에서.
		// 호출되는 메소드다.

		var	player = this.behavior as chrBehaviorPlayer;
		
		switch(favor.category) {

			case Item.CATEGORY.SODA_ICE:
			{
				if(player != null) {

					player.onUseItem(slot_index, favor);
				}
			}
			break;
		}
	}
	// 아이템을 사용했다(동려가 자신에게).
	//
	// in:	friend	아이템을 사용한 동료.
	//
	public void		onUseItemByFriend(Item.Favor favor, chrController friend)
	{
		// 이 부분은 실제로 아이템을 사용할 때 아이템 매니저에서.
		// 호출되는 메소드다.

		var	player = this.behavior as chrBehaviorPlayer;

		switch(favor.category) {

			case Item.CATEGORY.SODA_ICE:
			{
				//this.vital.healFull();

				if(player != null) {

					player.onUseItemByFriend(favor, friend.behavior as chrBehaviorPlayer);
				}
			}
			break;
		}
	}

	// ================================================================ //
	
	public void	consumeKey(ItemController item)
	{
		Debug.Log ("consumeKey");

		if(item.type == "key04") {

			// 플로어 이동키.
			Debug.Log("UNLOCK FLOOR DOOR!!");
			MapCreator.get().UnlockBossDoor();

		} else {

			RoomController	room = MapCreator.get().getRoomFromPosition(transform.position);

			room.OnConsumedKey(item.type);
		}
	}
}
