using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameObjectExtension;

// 鍮꾪뿤?대퉬??濡쒖뺄 ?뚮젅?댁뼱??
// 留덉슦?ㅻ줈 而⑦듃濡ㅽ븳??
public class chrBehaviorLocal : chrBehaviorPlayer {

	private Vector3		move_target;				// ?대룞???꾩튂.
	private Vector3		heading_target;				// 諛⑺뼢.

	protected chrBehaviorEnemy	melee_target;		// 洹쇱젒 怨듦꺽???곷?.

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// ?됱긽 ??
		MELEE_ATTACK,		// 洹쇱젒 怨듦꺽.
		USE_ITEM,			// ?꾩씠???ъ슜.

		BLOW_OUT,			// ?誘몄?瑜?諛쏆븘 ?좊씪媛??以?

		BATAN_Q,			// 泥대젰0.
		WAIT_RESTART,		// 由ъ뒪????湲?

		OUTER_CONTROL,		// ?몃??쒖뼱.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	protected StepUseItem	step_use_item = new StepUseItem();		// ?꾩씠???ъ슜 以??쒖뼱.


	// ?롮븘??醫뚰몴 蹂댁〈.
	private List<CharacterCoord>	m_culling = new List<CharacterCoord>();
	// ?꾩옱 ?꾨’???몃뜳??
	private int 		m_plotIndex = 0;
	// ?뺤? ?곹깭???뚮뒗 ?곗씠?곕? ?≪떊?섏? ?딄쾶 ?쒕떎.
	private Vector3		m_prev;


	// 3李??ㅽ뵆?쇱씤 蹂닿컙?먯꽌 ?ъ슜???먯쓽 ??
	private const int	PLOT_NUM = 4;

	// ?≪떊 ?잛닔.
	private int	m_send_count = 0;

	// ================================================================ //

	// ?몃??먯꽌??而⑦듃濡ㅼ쓣 ?쒖옉?쒕떎.
	public override void 	beginOuterControll()
	{
		base.beginOuterControll();

		this.step.set_next(STEP.OUTER_CONTROL);
	}

	// ?몃??먯꽌??而⑦듃濡ㅼ쓣 醫낅즺?쒕떎.
	public override void		endOuterControll()
	{
		base.endOuterControll();

		this.step.set_next(STEP.MOVE);
	}

	// ?誘몄?瑜?諛쏄퀬 ?좊씪媛湲??쒖옉?쒕떎.
	public override void		beginBlowOut(Vector3 center, float radius)
	{
		this.step_blow_out.begin(center, radius);
		this.step.set_next(STEP.BLOW_OUT);
	}

	// ?誘몄?瑜?諛쏄퀬 ?좊씪媛湲??쒖옉?쒕떎(媛濡쒕갑?ν븳??.
	public override void		beginBlowOutSide(Vector3 center, float radius, Vector3 direction)
	{
		this.step_blow_out.begin(center, radius, direction);
		this.step.set_next(STEP.BLOW_OUT);
	}

	// ?꾩씠?쒖쓣 ?ъ슜?덉쓣 ???몄텧?쒕떎.
	public override void		onUseItem(int slot, Item.Favor favor)
	{
		base.onUseItem(slot, favor);

		this.step_use_item.player     = this;
		this.step_use_item.slot_index = slot;
		this.step_use_item.item_favor = favor;

		this.step.set_next(STEP.USE_ITEM);
	}

	// ================================================================ //
	// MonoBehaviour?먯꽌 ?곸냽.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// 而щ━?꾩뿉 異⑸룎???숈븞 ?몄텧?섎뒗 硫붿냼??
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

	// ?몃━嫄곗뿉 異⑸룎???쒓컙留??몄텧?섎뒗 硫붿냼??
	void	OnTriggerEnter(Collider other)
	{

		this.on_trigger_common(other);
	}
	// ?몃━嫄곗뿉 異⑸룎???숈븞 ?몄텧?섎뒗 硫붿냼??
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

		// 寃뚯엫 ?쒖옉 吏곹썑??EnterEvent媛 ?쒖옉?섎㈃, ?ш린??next_step??
		// OuterControll???ㅼ젙?쒕떎. 洹몃븣 ??뼱?곗? ?딄쾶.
		// next == NONE 泥댄겕瑜??ｋ뒗??
		if(this.step.get_next() == STEP.NONE) {

			this.step.set_next(STEP.MOVE);
		}

		this.control.cmdSetAcceptDamage(true);

		this.GetComponent<Rigidbody>().WakeUp();
	}

	public override	void	execute()
	{
		base.execute();

		this.resolve_collision();

		this.update_item_queries();

		// ---------------------------------------------------------------- //
		// ?ㅼ쓬 ?곹깭濡??꾪솚?좎? 泥댄겕?쒕떎.


		switch(this.step.do_transition()) {

			// ?됱긽 ??
			case STEP.MOVE:
			{
				if(this.control.vital.getHitPoint() <= 0.0f) {

					this.step.set_next(STEP.BATAN_Q);
				}
			}
			break;

			// 洹쇱젒 怨듦꺽.
			case STEP.MELEE_ATTACK:
			{
				if(!this.melee_attack.isAttacking()) {

					this.step.set_next(STEP.MOVE);
				}
			}
			break;

			// ?꾩씠???ъ슜.
			case STEP.USE_ITEM:
			{
				if(this.step_use_item.transition_check()) {

					this.ice_timer = ICE_DIGEST_TIME;
				}
			}
			break;

			// ?誘몄?瑜?諛쏆븘 ?좊씪媛??以?
			case STEP.BLOW_OUT:
			{
				// ?쇱젙 嫄곕━瑜??섏븘媛嫄곕굹 ??꾩븘?껋쑝濡?醫낅즺.

				float	distance = MathUtility.calcDistanceXZ(this.control.getPosition(), this.step_blow_out.center);

				if(distance >= this.step_blow_out.radius || this.step.get_time() > BLOW_OUT_TIME) {

					this.control.cmdSetAcceptDamage(true);
					this.step.set_next(STEP.MOVE);
				}
			}
			break;

			// 泥대젰0.
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
		// ?곹깭 ?꾪솚 ??珥덇린??

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// ?됱긽 ??	
				case STEP.MOVE:
				{
					this.GetComponent<Rigidbody>().WakeUp();
					this.move_target = this.transform.position;
					this.heading_target = this.transform.TransformPoint(Vector3.forward);
				}
				break;

				// 洹쇱젒 怨듦꺽.
				case STEP.MELEE_ATTACK:
				{
					this.melee_attack.setTarget(this.melee_target);
					this.melee_attack.attack(true);
					this.melee_target = null;
				}
				break;

				// ?꾩씠???ъ슜.
				case STEP.USE_ITEM:
				{
					int		slot_index = this.step_use_item.slot_index;

					if(this.ice_timer > 0.0f) {

						// ?꾩씠?ㅻ? 吏㏃? 媛꾧꺽?쇰줈 ?곗냽 ?ъ슜????						// 癒몃━媛 吏?덉??덊빐???뚮났?섏? ?딅뒗??
					
						// ?꾩씠?쒖쓣 ??젣?쒕떎.
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

				// ?誘몄?瑜?諛쏆븘 ?좊씪媛??以?
				case STEP.BLOW_OUT:
				{
					this.GetComponent<Rigidbody>().Sleep();
					this.control.cmdSetAcceptDamage(false);
				}
				break;

				// 泥대젰 0
				case STEP.BATAN_Q:
				{
					this.GetComponent<Rigidbody>().Sleep();
					this.control.cmdSetAcceptDamage(false);
					this.control.cmdSetMotion("m006_out", 1);
				}
				break;

				// 由ъ뒪????湲?
				case STEP.WAIT_RESTART:
				{
					this.step_batan_q.tears_effect.destroy();
				}
				break;

				// ?몃? ?쒖뼱.
				case STEP.OUTER_CONTROL:
				{
					this.GetComponent<Rigidbody>().Sleep();
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 媛??곹깭?먯꽌???ㅽ뻾 泥섎━.

		// 洹쇱젒 怨듦꺽.
		// ?쇰떒 false濡??대몢怨??대룞 ?낅젰???덉쓣 ?뚮쭔.
		// true濡??쒕떎.
		this.melee_attack.setHasInput(false);

		GameInput	gi = GameInput.getInstance();

		switch(this.step.do_execution(Time.deltaTime)) {

			// ?됱긽 ??
			case STEP.MOVE:
			{
				this.exec_step_move();

				// ?룔깾?껁깉.
				if(this.is_shot_enable) {

					this.bullet_shooter.execute(gi.shot.current);

					if(gi.shot.current) {
	
						CharacterRoot.get().SendAttackData(PartyControl.get().getLocalPlayer().getAcountID(), 0);
					}
				}

				// 泥대젰 ?뚮났 吏곹썑(?덉씤蹂댁슦而щ윭 以???臾댁쟻.
				if(this.skin_color_control.isNowHealing()) {

					this.control.cmdSetAcceptDamage(false);

				} else {

					this.control.cmdSetAcceptDamage(true);
				}
			}
			break;

			// ?꾩씠???ъ슜.
			case STEP.USE_ITEM:
			{
				this.step_use_item.execute();
			}
			break;

			// ?誘몄?瑜?諛쏆븘 ?좊씪媛??以?
			case STEP.BLOW_OUT:
			{
				this.exec_step_blow_out();
			}
			break;

			// 泥대젰0.
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
		// 10 ?꾨젅?꾩뿉 ??踰?醫뚰몴瑜??ㅽ듃?뚰겕??蹂대궦??
		
		{
			do {

				if(this.step.get_current() == STEP.OUTER_CONTROL) {

					break;
				}
				
				m_send_count = (m_send_count + 1)%SplineData.SEND_INTERVAL;
				
				if(m_send_count != 0 && m_culling.Count < PLOT_NUM) {
					
					break;
				}
				
				// ?듭떊??醫뚰몴 ?≪떊.
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

	// ?誘몄?瑜?諛쏆븯?????몄텧?쒕떎.
	public override void		onDamaged()
	{
		this.control.cmdSetMotion("m005_damage", 1);

		EffectRoot.get().createHitEffect(this.transform.position);
	}

	// ================================================================ //

	// 肄쒕━?꾩쓽 ?섎? 遺??
	protected void	resolve_collision()
	{
		foreach(var result in this.control.collision_results) {

			if(result.object1 == null) {
							
				continue;
			}

			//GameObject		self  = result.object0;
			GameObject		other = result.object1;

			// ?뚮젅?댁뼱媛 蹂댁뒪??肄쒕━?꾩뿉 ?뚮춱? ?吏곸씪 ???녾쾶 ?섏? ?딄쾶.
			// 諛⑹쓽 以묒떖 諛⑺뼢?쇰줈 諛?대궦??
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

						// ?꾩씠?쒖쓣 二쇱슱?섏엳??議곗궗?쒕떎.
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
		
								// ?щ’??媛??=  ??媛吏????놁쓣 ??	
								if(slot_index < 0) {
	
									is_pickable = false;
								}
							}
							break;

							case Item.CATEGORY.WEAPON:
							{
								// ?ъ슜 以묒씤 ?룰낵 媛숈쑝硫?二쇱슱 ???녿떎.
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

	// ?뚮젅?댁뼱媛 蹂댁뒪??肄쒕━?꾩뿉 臾삵????吏곸씪 ???녾쾶 ?섏? ?딅룄濡?
	// 諛⑹쓽 以묒떖 諛⑺뼢?쇰줈 諛?대궦??
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

			// ???댁긽 媛源뚯슦硫?媛뺤젣濡?諛?대궦??
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
	// ?꾩씠??荑쇰━瑜?媛깆떊?쒕떎.

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

			// ?꾩씠???⑤뒫留?蹂듭궗?섍퀬 ??젣?쒕떎.

			ItemController	item = this.control.cmdItemPick(query, query.target);

			if(item == null) {

				break;
			}

			// ?④낵.
			EffectRoot.get().createItemGetEffect(this.control.getPosition());

			SoundManager.get().playSE(Sound.ID.DDG_SE_SYS02);

			switch(item.behavior.item_favor.category) {

				case Item.CATEGORY.CANDY:
				{
					// ?꾩씠??李쎌뿉 ?꾩씠肄??쒖떆.
					this.item_slot.candy.favor = item.behavior.item_favor.clone();

					ItemWindow.get().setItem(Item.SLOT_TYPE.CANDY, 0, this.item_slot.candy.favor);

					// ?룹쓽 ?쇱젙?쒓컙 ?뚯썙??					this.startShotBoost();
				}
				break;

				case Item.CATEGORY.SODA_ICE:
				case Item.CATEGORY.ETC:
				{
					// 鍮??щ’???꾩씠???ㅼ젙.
					int		slot_index = this.item_slot.getEmptyMiscSlot();

					if(slot_index >= 0) {

						this.item_slot.miscs[slot_index].item_id = query.target;
						this.item_slot.miscs[slot_index].favor   = item.behavior.item_favor.clone();

						// ?꾩씠??李쎌뿉 ?꾩씠肄??쒖떆.
						ItemWindow.get().setItem(Item.SLOT_TYPE.MISC, slot_index, this.item_slot.miscs[slot_index].favor);
					}
				}
				break;

				case Item.CATEGORY.FOOD:
				{
					// 泥대젰 ?뚮났.
					if(GameRoot.get().isNowCakeBiking()) {

						this.control.vital.healFullInternal();

					} else {

						this.control.vital.healFull();

						// ?덉씤蹂댁슦 移쇰윭 ?④낵.
						this.skin_color_control.startHealing();
					}

					// 泥대젰 ?뚮났???뚮┝.
					CharacterRoot.get().NotifyHitPoint(this.getAcountID(), this.control.vital.getHitPoint());

					// ?꾩씠???먭린瑜??뚮┝.
					this.control.cmdItemDrop(query.target);

					// 耳?댄겕瑜?癒뱀? ??耳?댄겕 臾댄븳?쒓났??.
					this.cake_count++;
				}
				break;

				// 諛??댁뇿.
				case Item.CATEGORY.KEY:
				{
					PartyControl.get().getLocalPlayer().control.consumeKey(item);

					Item.KEY_COLOR	key_color = Item.Key.getColorFromInstanceName(item.name);

					// ?꾩씠??李쎌뿉 ?꾩씠肄??쒖떆.
					if(key_color != Item.KEY_COLOR.NONE) {

						ItemWindow.get().setItem(Item.SLOT_TYPE.KEY, (int)key_color, item.behavior.item_favor);
					}
				}
				break;

				// ?뚮줈???대룞 臾??댁뇿.
				case Item.CATEGORY.FLOOR_KEY:
				{
					MapCreator.getInstance().UnlockBossDoor();

					// ?꾩씠??李쎌뿉 ?꾩씠肄??쒖떆.
					ItemWindow.get().setItem(Item.SLOT_TYPE.FLOOR_KEY, 0, item.behavior.item_favor);
				}
				break;

				case Item.CATEGORY.WEAPON:
				{
					// ??蹂寃????諛쒖뭏 / ?좎옄 ??깂).
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

	// STEP.MOVE ?ㅽ뻾.
	// ?대룞.
	protected void	exec_step_move()
	{
		GameInput	gi = GameInput.getInstance();

		// ---------------------------------------------------------------- //
		//?대룞 紐⑺몴 ?꾩튂瑜?媛깆떊?쒕떎.

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

			// 洹쇱젒 怨듦꺽.
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
		// ?대룞(?꾩튂 醫뚰몴 蹂닿컙).

		Vector3		position  = this.control.getPosition();
		Vector3		dist      = this.move_target - position;

		dist.y = 0.0f;

		float		speed = 5.0f;
		float		speed_per_frame = speed*Time.deltaTime;

		if(dist.magnitude < speed_per_frame) {

			// 硫덉텣??
			this.control.cmdSetMotion("m002_idle", 0);

			dist = Vector3.zero;

		} else {

			// 嫄룸뒗??
			this.control.cmdSetMotion("m001_walk", 0);

			dist *= (speed_per_frame)/dist.magnitude;
		}

		position += dist;
		position.y = this.control.getPosition().y;

		this.control.cmdSetPosition(position);

		// 諛⑺뼢 蹂닿컙.

		float	turn_rate = 0.1f;

		if(!gi.pointing.current && gi.shot.trigger_on) {

			turn_rate = 1.0f;
		}

		this.control.cmdSmoothHeadingTo(this.heading_target, turn_rate);
	}

	// ================================================================ //

	// ?꾩씠???ъ슜 以??쒖뼱.
	protected class StepUseItem {

		public int					slot_index;		//?ъ슜 以묒씤 ?꾩씠?쒖씠 ?ㅼ뼱?덈뒗 ?щ’.
		public Item.Favor			item_favor;
		public chrBehaviorLocal		player;

		public static float	use_motion_delay  = 0.4f;
		public static float	heal_effect_delay = 0.9f;

		// ================================================================ //

		// ?쒖옉.
		public void		initialize()
		{
			EffectRoot.get().createHealEffect(this.player.control.getPosition());
		}

		// 醫낅즺 泥댄겕.
		public bool		transition_check()
		{
			bool	is_transit = false;
	
			if(this.player.step.get_time() >= use_motion_delay && this.player.control.getMotion() != "m004_use") {

				ItemWindow.get().clearItem(Item.SLOT_TYPE.MISC, this.slot_index);

				bool	is_atari = (bool)this.item_favor.option0;

				if(is_atari) {

					// 異⑸룎 ?대깽??
					EventIceAtari	event_atari = EventRoot.get().startEvent<EventIceAtari>();

					event_atari.setItemSlotAndFavor(this.slot_index, this.item_favor);

					this.player.step.set_next(STEP.MOVE);

				} else {

					this.player.step.set_next(STEP.MOVE);
				}

				// ?꾩씠?쒖쓣 ??젣?쒕떎.
				if(this.slot_index >= 0) {

					if(is_atari) {

						// 異⑸룎?덉쓣 ???꾩씠?쒖쓣 ??젣?섏? ?딅뒗??
						// 異⑸룎 ????嫄몃줈 ?섎룎由곕떎.
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

		// 留??꾨젅???ㅽ뻾.
		public void		execute()
		{
			// 議곌툑 ??쾶 紐⑥뀡 ?쒖옉.
			if(this.player.step.is_acrossing_time(use_motion_delay)) {

				this.player.control.cmdSetMotion("m004_use", 1);
			}

			// ?뚮? ?꾨줈 ?щ┛ ??대컢???꾩씠???④낵 ?곸슜.
			if(this.player.step.is_acrossing_time(heal_effect_delay)) {

				switch(this.item_favor.category) {

					case Item.CATEGORY.SODA_ICE:
					{
						this.player.control.vital.healFull();
						this.player.skin_color_control.startHealing();

						// 媛源뚯씠???덈뜕 ?숇즺??泥대젰 ?뚮났.
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
