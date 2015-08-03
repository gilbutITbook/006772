using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameObjectExtension;

// 비헤이비어 로컬 플레이어용.
// 마우스로 컨트롤한다.
public class chrBehaviorLocal : chrBehaviorPlayer {

	private Vector3		move_target;				// 이동할 위치.
	private Vector3		heading_target;				// 방향.

	protected chrBehaviorEnemy	melee_target;		// 근접 공격할 상대.

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 평상 시.
		MELEE_ATTACK,		// 근접 공격.
		USE_ITEM,			// 아이템 사용.

		BLOW_OUT,			// 대미지를 받아 날라가는 중.

		BATAN_Q,			// 체력0.
		WAIT_RESTART,		// 리스타트 대기.

		OUTER_CONTROL,		// 외부제어.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	protected StepUseItem	step_use_item = new StepUseItem();		// 아이템 사용 중 제어.


	// 솎아낸 좌표 보존.
	private List<CharacterCoord>	m_culling = new List<CharacterCoord>();
	// 현재 프롯의 인덱스.
	private int 		m_plotIndex = 0;
	// 정지 상태일 때는 데이터를 송신하지 않게 한다.
	private Vector3		m_prev;


	// 3차 스플라인 보간에서 사용할 점의 수.
	private const int	PLOT_NUM = 4;

	// 송신 횟수.
	private int	m_send_count = 0;

	// ================================================================ //

	// 외부에서의 컨트롤을 시작한다.
	public override void 	beginOuterControll()
	{
		base.beginOuterControll();

		this.step.set_next(STEP.OUTER_CONTROL);
	}

	// 외부에서의 컨트롤을 종료한다.
	public override void		endOuterControll()
	{
		base.endOuterControll();

		this.step.set_next(STEP.MOVE);
	}

	// 대미지를 받고 날라가기 시작한다.
	public override void		beginBlowOut(Vector3 center, float radius)
	{
		this.step_blow_out.begin(center, radius);
		this.step.set_next(STEP.BLOW_OUT);
	}

	// 대미지를 받고 날라가기 시작한다(가로방향한정).
	public override void		beginBlowOutSide(Vector3 center, float radius, Vector3 direction)
	{
		this.step_blow_out.begin(center, radius, direction);
		this.step.set_next(STEP.BLOW_OUT);
	}

	// 아이템을 사용했을 때 호출된다.
	public override void		onUseItem(int slot, Item.Favor favor)
	{
		base.onUseItem(slot, favor);

		this.step_use_item.player     = this;
		this.step_use_item.slot_index = slot;
		this.step_use_item.item_favor = favor;

		this.step.set_next(STEP.USE_ITEM);
	}

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// 컬리전에 충돌한 동안 호출되는 메소드.
	void 	OnCollisionStay(Collision other)
	{
		switch(other.gameObject.tag) {

			case "Item":
			case "Enemy":
			case "EnemyLair":
			case "Boss":
			{
				CollisionResult	result = new CollisionResult();
		
				result.object0    = this.gameObject;
				result.object1    = other.gameObject;
				result.is_trigger = false;

				this.control.collision_results.Add(result);
			}
			break;
		}
	}

	// 트리거에 충돌한 순간만 호출되는 메소드.
	void	OnTriggerEnter(Collider other)
	{

		this.on_trigger_common(other);
	}
	// 트리거에 충돌한 동안 호출되는 메소드.
	void	OnTriggerStay(Collider other)
	{
		this.on_trigger_common(other);
	}

	protected	void	on_trigger_common(Collider other)
	{
		switch(other.gameObject.tag) {

			case "Door":
			case "Item":
			{
				CollisionResult	result = new CollisionResult();
		
				result.object0    = this.gameObject;
				result.object1    = other.gameObject;
				result.is_trigger = true;

				this.control.collision_results.Add(result);
			}
			break;
		}
	}

	// ================================================================ //

	public override void	initialize()
	{
		base.initialize();

		this.move_target = this.transform.position;
	}
	public override void	start()
	{
		base.start();

		// 게임 시작 직후에 EnterEvent가 시작되면, 여기서 next_step에.
		// OuterControll이 설정된다. 그때 덮어쓰지 않게.
		// next == NONE 체크를 넣는다.
		if(this.step.get_next() == STEP.NONE) {

			this.step.set_next(STEP.MOVE);
		}

		this.control.cmdSetAcceptDamage(true);

		this.rigidbody.WakeUp();
	}

	public override	void	execute()
	{
		base.execute();

		this.resolve_collision();

		this.update_item_queries();

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.


		switch(this.step.do_transition()) {

			// 평상 시.
			case STEP.MOVE:
			{
				if(this.control.vital.getHitPoint() <= 0.0f) {

					this.step.set_next(STEP.BATAN_Q);
				}
			}
			break;

			// 근접 공격.
			case STEP.MELEE_ATTACK:
			{
				if(!this.melee_attack.isAttacking()) {

					this.step.set_next(STEP.MOVE);
				}
			}
			break;

			// 아이템 사용.
			case STEP.USE_ITEM:
			{
				if(this.step_use_item.transition_check()) {

					this.ice_timer = ICE_DIGEST_TIME;
				}
			}
			break;

			// 대미지를 받아 날라가는 중.
			case STEP.BLOW_OUT:
			{
				// 일정 거리를 나아가거나 타임아웃으로 종료.

				float	distance = MathUtility.calcDistanceXZ(this.control.getPosition(), this.step_blow_out.center);

				if(distance >= this.step_blow_out.radius || this.step.get_time() > BLOW_OUT_TIME) {

					this.control.cmdSetAcceptDamage(true);
					this.step.set_next(STEP.MOVE);
				}
			}
			break;

			// 체력0.
			case STEP.BATAN_Q:
			{
				if(this.control.getMotion() == "") {

					this.control.cmdSetMotion("m007_out_lp", 1);
					this.step.set_next_delay(STEP.WAIT_RESTART, 1.0f);
				}
			}
			break;

		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 평상 시.	
				case STEP.MOVE:
				{
					this.rigidbody.WakeUp();
					this.move_target = this.transform.position;
					this.heading_target = this.transform.TransformPoint(Vector3.forward);
				}
				break;

				// 근접 공격.
				case STEP.MELEE_ATTACK:
				{
					this.melee_attack.setTarget(this.melee_target);
					this.melee_attack.attack(true);
					this.melee_target = null;
				}
				break;

				// 아이템 사용.
				case STEP.USE_ITEM:
				{
					int		slot_index = this.step_use_item.slot_index;

					if(this.ice_timer > 0.0f) {

						// 아이스를 짧은 간격으로 연속 사용할 때
						// 머리가 지끈지끈해서 회복되지 않는다.
					
						// 아이템을 삭제한다.
						ItemWindow.get().clearItem(Item.SLOT_TYPE.MISC, slot_index);
						this.item_slot.miscs[slot_index].initialize();

						this.startJinJin();
						this.step.set_next(STEP.MOVE);

						SoundManager.getInstance().playSE(Sound.ID.DDG_SE_SYS06);

					} else {

						this.item_slot.miscs[slot_index].is_using = true;

						this.control.cmdSetAcceptDamage(false);

						this.step_use_item.initialize();
					}
				}
				break;

				// 대미지를 받아 날라가는 중.
				case STEP.BLOW_OUT:
				{
					this.rigidbody.Sleep();
					this.control.cmdSetAcceptDamage(false);
				}
				break;

				// 체력 0
				case STEP.BATAN_Q:
				{
					this.rigidbody.Sleep();
					this.control.cmdSetAcceptDamage(false);
					this.control.cmdSetMotion("m006_out", 1);
				}
				break;

				// 리스타트 대기.
				case STEP.WAIT_RESTART:
				{
					this.step_batan_q.tears_effect.destroy();
				}
				break;

				// 외부 제어.
				case STEP.OUTER_CONTROL:
				{
					this.rigidbody.Sleep();
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		// 근접 공격.
		// 일단 false로 해두고 이동 입력이 있을 때만.
		// true로 한다.
		this.melee_attack.setHasInput(false);

		GameInput	gi = GameInput.getInstance();

		switch(this.step.do_execution(Time.deltaTime)) {

			// 평상 시.
			case STEP.MOVE:
			{
				this.exec_step_move();

				// ショット.
				if(this.is_shot_enable) {

					this.bullet_shooter.execute(gi.shot.current);

					if(gi.shot.current) {
	
						CharacterRoot.get().SendAttackData(PartyControl.get().getLocalPlayer().getAcountID(), 0);
					}
				}

				// 체력 회복 직후(레인보우컬러 중)는 무적.
				if(this.skin_color_control.isNowHealing()) {

					this.control.cmdSetAcceptDamage(false);

				} else {

					this.control.cmdSetAcceptDamage(true);
				}
			}
			break;

			// 아이템 사용.
			case STEP.USE_ITEM:
			{
				this.step_use_item.execute();
			}
			break;

			// 대미지를 받아 날라가는 중.
			case STEP.BLOW_OUT:
			{
				this.exec_step_blow_out();
			}
			break;

			// 체력0.
			case STEP.BATAN_Q:
			{
				if(this.step.is_acrossing_time(4.0f)) {

					this.step_batan_q.tears_effect = EffectRoot.get().createTearsEffect(this.control.getPosition());

					this.step_batan_q.tears_effect.setParent(this.gameObject);
					this.step_batan_q.tears_effect.setLocalPosition(Vector3.up);
				}
			}
			break;

		}

		// ---------------------------------------------------------------- //

		if(gi.serif_text.trigger_on) {

			this.control.cmdQueryTalk(gi.serif_text.text, true);
		}

		// ---------------------------------------------------------------- //
		// 10 프레임에 한 번 좌표를 네트워크에 보낸다.
		
		{
			do {

				if(this.step.get_current() == STEP.OUTER_CONTROL) {

					break;
				}
				
				m_send_count = (m_send_count + 1)%SplineData.SEND_INTERVAL;
				
				if(m_send_count != 0 && m_culling.Count < PLOT_NUM) {
					
					break;
				}
				
				// 통신용 좌표 송신.
				Vector3 target = this.control.getPosition();
				CharacterCoord coord = new CharacterCoord(target.x, target.z);
				
				Vector3 diff = m_prev - target;
				if (diff.sqrMagnitude > 0.0001f) {
					
					m_culling.Add(coord);

					AccountData	account_data = AccountManager.get().getAccountData(GlobalParam.getInstance().global_account_id);

					CharacterRoot.get().SendCharacterCoord(account_data.avator_id, m_plotIndex, m_culling); 
					++m_plotIndex;
	
					if (m_culling.Count >= PLOT_NUM) {
						
						m_culling.RemoveAt(0);
					}
					
					m_prev = target;
				}
				
			} while(false);
		}

	}

	// 대미지를 받았을 때 호출된다.
	public override void		onDamaged()
	{
		this.control.cmdSetMotion("m005_damage", 1);

		EffectRoot.get().createHitEffect(this.transform.position);
	}

	// ================================================================ //

	// 콜리전의 의미 부여.
	protected void	resolve_collision()
	{
		foreach(var result in this.control.collision_results) {

			if(result.object1 == null) {
							
				continue;
			}

			//GameObject		self  = result.object0;
			GameObject		other = result.object1;

			// 플레이어가 보스의 콜리전에 파뭍혀 움직일 수 없게 되지 않게.
			// 방의 중심 방향으로 밀어낸다.
			if(other.tag == "Boss") {

				if(this.force_push_out(result)) {

					continue;
				}
			}

			switch(other.tag) {

				case "Enemy":
				case "EnemyLair":
				case "Boss":
				{
					do {

						chrBehaviorEnemy	enemy = other.GetComponent<chrBehaviorEnemy>();

						if(enemy == null) {

							break;
						}
						if(!this.melee_attack.isInAttackRange(enemy.control)) {

							//break;
						}
						if(this.step.get_current() == STEP.MELEE_ATTACK) {

							break;
						}
						if(!this.melee_attack.isHasInput()) {

							break;
						}

						this.melee_target = enemy;
						this.step.set_next(STEP.MELEE_ATTACK);

					} while(false);

					result.object1 = null;
				}
				break;

				case "Door":
				{
					this.cmdNotiryStayDoorBox(other.gameObject.GetComponent<DoorControl>());
				}
				break;

				case "Item":
				{
					do {

						ItemController		item  = other.GetComponent<ItemController>();

						// 아이템을 주울수있나 조사한다.
						bool	is_pickable = true;

						switch(item.behavior.item_favor.category) {
			
							case Item.CATEGORY.CANDY:
							{
								is_pickable = this.item_slot.candy.isVacant();
							}
							break;
			
							case Item.CATEGORY.SODA_ICE:
							case Item.CATEGORY.ETC:
							{
								int		slot_index = this.item_slot.getEmptyMiscSlot();
		
								// 슬롯이 가득 =  더 가질 수 없을 때.	
								if(slot_index < 0) {
	
									is_pickable = false;
								}
							}
							break;

							case Item.CATEGORY.WEAPON:
							{
								// 사용 중인 샷과 같으면 주울 수 없다.
								SHOT_TYPE	shot_type = Item.Weapon.getShotType(item.name);
			
								is_pickable = (this.shot_type != shot_type);
							}
							break;
						}
						if(!is_pickable) {

							break;
						}
			
						this.control.cmdItemQueryPick(item.name, true, false);

					} while(false);
				}
				break;
			}
		}

	}

	// 플레이어가 보스의 콜리전에 묻혀서 움직일 수 없게 되지 않도록.
	// 방의 중심 방향으로 밀어낸다.
	protected bool	force_push_out(CollisionResult result)
	{
		bool	is_pushouted = false;


		do {

			if(result.is_trigger) {

				break;
			}

			GameObject	other = result.object1;

			chrControllerEnemyBoss	control_other = other.GetComponent<chrControllerEnemyBoss>();

			if(control_other == null) {

				break;
			}

			// 이 이상 가까우면 강제로 밀어낸다.
			float		distance_limit = 2.0f*control_other.getScale();

			if(Vector3.Distance(other.transform.position, this.control.getPosition()) >= distance_limit) {

				break;
			}

			Vector3		room_center = MapCreator.get().getRoomCenterPosition(PartyControl.get().getCurrentRoom().getIndex());

			Vector3 	v = room_center - other.transform.position;

			v.Normalize();

			if(v.magnitude == 0.0f) {

				v = Vector3.forward;
			}

			this.control.cmdSetPositionAnon(other.transform.position + v*distance_limit*2.0f);

			is_pushouted = true;

		} while(false);

		return(is_pushouted);
	}

	// ---------------------------------------------------------------- //
	// 아이템 쿼리를 갱신한다.

	private void	update_item_queries()
	{
		List<QueryBase>		done_queries = QueryManager.get().findDoneQuery(this.control.getAccountID());

		foreach(var query in done_queries) {

			switch(query.getType()) {

				case "item.pick":
				{
					dbwin.console().print("item query done.");
					this.resolve_pick_item_query(query as QueryItemPick);
				}
				break;
			}
		}
	}

	private void	resolve_pick_item_query(QueryItemPick query)
	{
		do {

			if(!query.isSuccess()) {

				break;
			}

			// 아이템 효능만 복사하고 삭제한다.

			ItemController	item = this.control.cmdItemPick(query, query.target);

			if(item == null) {

				break;
			}

			// 효과.
			EffectRoot.get().createItemGetEffect(this.control.getPosition());

			SoundManager.get().playSE(Sound.ID.DDG_SE_SYS02);

			switch(item.behavior.item_favor.category) {

				case Item.CATEGORY.CANDY:
				{
					// 아이템 창에 아이콘 표시.
					this.item_slot.candy.favor = item.behavior.item_favor.clone();

					ItemWindow.get().setItem(Item.SLOT_TYPE.CANDY, 0, this.item_slot.candy.favor);

					// 샷의 일정시간 파워업
					this.startShotBoost();
				}
				break;

				case Item.CATEGORY.SODA_ICE:
				case Item.CATEGORY.ETC:
				{
					// 빈 슬롯에 아이템 설정.
					int		slot_index = this.item_slot.getEmptyMiscSlot();

					if(slot_index >= 0) {

						this.item_slot.miscs[slot_index].item_id = query.target;
						this.item_slot.miscs[slot_index].favor   = item.behavior.item_favor.clone();

						// 아이템 창에 아이콘 표시.
						ItemWindow.get().setItem(Item.SLOT_TYPE.MISC, slot_index, this.item_slot.miscs[slot_index].favor);
					}
				}
				break;

				case Item.CATEGORY.FOOD:
				{
					// 체력 회복.
					if(GameRoot.get().isNowCakeBiking()) {

						this.control.vital.healFullInternal();

					} else {

						this.control.vital.healFull();

						// 레인보우 칼러 효과.
						this.skin_color_control.startHealing();
					}

					// 체력 회복을 알림.
					CharacterRoot.get().NotifyHitPoint(this.getAcountID(), this.control.vital.getHitPoint());

					// 아이템 폐기를 알림.
					this.control.cmdItemDrop(query.target);

					// 케이크를 먹은 수(케이크 무한제공용).
					this.cake_count++;
				}
				break;

				// 방 열쇠.
				case Item.CATEGORY.KEY:
				{
					PartyControl.get().getLocalPlayer().control.consumeKey(item);

					Item.KEY_COLOR	key_color = Item.Key.getColorFromInstanceName(item.name);

					// 아이템 창에 아이콘 표시.
					if(key_color != Item.KEY_COLOR.NONE) {

						ItemWindow.get().setItem(Item.SLOT_TYPE.KEY, (int)key_color, item.behavior.item_favor);
					}
				}
				break;

				// 플로어 이동 문 열쇠.
				case Item.CATEGORY.FLOOR_KEY:
				{
					MapCreator.getInstance().UnlockBossDoor();

					// 아이템 창에 아이콘 표시.
					ItemWindow.get().setItem(Item.SLOT_TYPE.FLOOR_KEY, 0, item.behavior.item_favor);
				}
				break;

				case Item.CATEGORY.WEAPON:
				{
					// 샷 변경(대파 발칸 / 유자 폭탄).
					SHOT_TYPE	shot_type = Item.Weapon.getShotType(item.name);

					if(shot_type != SHOT_TYPE.NONE) {

						this.changeBulletShooter(shot_type);
					}
				}
				break;
			}

			item.vanish();

		} while(false);

		query.set_expired(true);
	}

	// ================================================================ //

	// STEP.MOVE 실행.
	// 이동.
	protected void	exec_step_move()
	{
		GameInput	gi = GameInput.getInstance();

		// ---------------------------------------------------------------- //
		//이동 목표 위치를 갱신한다.

		if(gi.pointing.current) {

			switch(gi.pointing.pointee) {


				case GameInput.POINTEE.CHARACTOR:
				case GameInput.POINTEE.NONE:
				{
				}
				break;

				default:
				{
					if(GameRoot.getInstance().controlable[this.control.local_index]) {

						this.move_target = gi.pointing.position_3d;
					}
				}
				break;
			}

			// 근접 공격.
			this.melee_attack.setHasInput(true);

		} else {

			this.move_target = this.control.getPosition();
		}

		if(gi.shot.current) {

			if(gi.shot.pointee != GameInput.POINTEE.NONE) {

				this.heading_target = gi.shot.position_3d;
			}

		} else if(gi.pointing.current) {

			if(gi.pointing.pointee != GameInput.POINTEE.NONE) {

				this.heading_target = gi.pointing.position_3d;
			}
		}

		// ---------------------------------------------------------------- //
		// 이동(위치 좌표 보간).

		Vector3		position  = this.control.getPosition();
		Vector3		dist      = this.move_target - position;

		dist.y = 0.0f;

		float		speed = 5.0f;
		float		speed_per_frame = speed*Time.deltaTime;

		if(dist.magnitude < speed_per_frame) {

			// 멈춘다.
			this.control.cmdSetMotion("m002_idle", 0);

			dist = Vector3.zero;

		} else {

			// 걷는다.
			this.control.cmdSetMotion("m001_walk", 0);

			dist *= (speed_per_frame)/dist.magnitude;
		}

		position += dist;
		position.y = this.control.getPosition().y;

		this.control.cmdSetPosition(position);

		// 방향 보간.

		float	turn_rate = 0.1f;

		if(!gi.pointing.current && gi.shot.trigger_on) {

			turn_rate = 1.0f;
		}

		this.control.cmdSmoothHeadingTo(this.heading_target, turn_rate);
	}

	// ================================================================ //

	// 아이템 사용 중 제어.
	protected class StepUseItem {

		public int					slot_index;		//사용 중인 아이템이 들어있는 슬롯.
		public Item.Favor			item_favor;
		public chrBehaviorLocal		player;

		public static float	use_motion_delay  = 0.4f;
		public static float	heal_effect_delay = 0.9f;

		// ================================================================ //

		// 시작.
		public void		initialize()
		{
			EffectRoot.get().createHealEffect(this.player.control.getPosition());
		}

		// 종료 체크.
		public bool		transition_check()
		{
			bool	is_transit = false;
	
			if(this.player.step.get_time() >= use_motion_delay && this.player.control.getMotion() != "m004_use") {

				ItemWindow.get().clearItem(Item.SLOT_TYPE.MISC, this.slot_index);

				bool	is_atari = (bool)this.item_favor.option0;

				if(is_atari) {

					// 충돌 이벤트.
					EventIceAtari	event_atari = EventRoot.get().startEvent<EventIceAtari>();

					event_atari.setItemSlotAndFavor(this.slot_index, this.item_favor);

					this.player.step.set_next(STEP.MOVE);

				} else {

					this.player.step.set_next(STEP.MOVE);
				}

				// 아이템을 삭제한다.
				if(this.slot_index >= 0) {

					if(is_atari) {

						// 충돌했을 때 아이템을 삭제하지 않는다.
						// 충돌 안 한 걸로 되돌린다.
						this.player.item_slot.miscs[this.slot_index].favor.option0 = false;

					} else {

						this.player.item_slot.miscs[this.slot_index].initialize();
	
						this.slot_index = -1;
					}
				}

				is_transit = true;
			}

			return(is_transit);
		}

		// 매 프레임 실행.
		public void		execute()
		{
			// 조금 늦게 모션 시작.
			if(this.player.step.is_acrossing_time(use_motion_delay)) {

				this.player.control.cmdSetMotion("m004_use", 1);
			}

			// 파를 위로 올린 타이밍에 아이템 효과 적용.
			if(this.player.step.is_acrossing_time(heal_effect_delay)) {

				switch(this.item_favor.category) {

					case Item.CATEGORY.SODA_ICE:
					{
						this.player.control.vital.healFull();
						this.player.skin_color_control.startHealing();

						// 가까이에 있던 동료도 체력 회복.
						for(int i = 0;i < PartyControl.get().getFriendCount();i++) {

							chrBehaviorPlayer	friend = PartyControl.get().getFriend(i);

							float	distance = (friend.control.getPosition() - this.player.control.getPosition()).magnitude;

							if(distance > chrBehaviorPlayer.CHARITY_SPHERE_RADIUS) {

								continue;
							}

							this.player.control.cmdUseItemToFriend(this.item_favor, friend.control.global_index, true);
						}
					}
					break;
				}
			}
		}
	}

}
