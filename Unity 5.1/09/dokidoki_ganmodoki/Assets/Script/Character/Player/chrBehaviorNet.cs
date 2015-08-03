using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 鍮꾪뿤?대퉬??- ?ㅽ듃?뚰겕 ?뚮젅?댁뼱(寃뚯뒪????
public class chrBehaviorNet : chrBehaviorPlayer {

	protected string	move_target_item = "";	// ?꾩씠?쒖쓣 紐⑺몴濡??대룞????

	protected string	collision = "";

	public chrBehaviorLocal	local_player          = null;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// ?됱긽 ??
		MELEE_ATTACK,		// 洹쇱젒 怨듦꺽.
		OUTER_CONTROL,		// ?몃? ?쒖뼱.

		NUM,
	};

	//public Step<STEP>	step = new Step<STEP>(STEP.NONE);
	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;


	// ---------------------------------------------------------------- //
	
	// 3李??ㅽ뵆?쇱씤 蹂닿컙?먯꽌 ?ъ슜???먯쓽 媛쒖닔.
	private const int PLOT_NUM = 4;
	
	// ?롮븘??醫뚰몴???꾨젅????
	private const int CULLING_NUM = 10;
	
	// ?꾩옱 ?뚮’ ?몃뜳??
	private int 	m_plotIndex = 0;
	
	// ?롮븘??醫뚰몴瑜?蹂댁〈.
	private List<CharacterCoord>	m_culling = new List<CharacterCoord>();
	// 蹂닿컙??醫뚰몴瑜?蹂댁〈.
	private List<CharacterCoord>	m_plots = new List<CharacterCoord>();

	// 怨듦꺽 ?뚮옒洹?
	private	bool	m_shot = false;

	// 蹂댄뻾 ?뚮옒洹?
	private struct WalkMotion {

		public bool		is_walking;
		public float	timer;
	};
	private	WalkMotion	walk_motion;

	private const float	STOP_WALK_WAIT = 0.1f;		// [sec] 嫄룰린 -> ?쒓린 紐⑥뀡?쇰줈 ?댄뻾???뚯쓽 ?좎삁 ?쒓컙.

	// ================================================================ //

	// 濡쒖뺄 ?뚮젅?댁뼱?.
	public override bool	isLocal()
	{
		return(false);
	}

	// ?몃??먯꽌???쒖뼱瑜??쒖옉?쒕떎.
	public override void 	beginOuterControll()
	{
		this.next_step = STEP.OUTER_CONTROL;
	}

	// ?몃??먯꽌???쒖뼱瑜?醫낅즺?쒕떎.
	public override void		endOuterControll()
	{
		this.next_step = STEP.MOVE;
	}


	// ================================================================ //
	// MonoBehaviour?먯꽌 ?곸냽.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// 肄쒕━?꾩뿉 異⑸룎?섎뒗 ?숈븞 ?몄텧?섎뒗 硫붿냼??
	void 	OnCollisionStay(Collision other)
	{
		switch(other.gameObject.tag) {


			case "Enemy":
			case "Boss":
			{
				CollisionResult	result = new CollisionResult();
		
				result.object0    = this.gameObject;
				result.object1    = other.gameObject;
				result.is_trigger = false;

				CollisionManager.getInstance().results.Add(result);
			}
			break;
		}
	}

	// ?몃━嫄곗뿉 異⑸룎???쒓컙留??몄텧?섎뒗 硫붿냼??
	void	OnTriggerEnter(Collider other)
	{
		this.on_trigger_common(other);
	}
	// ?몃━嫄곗뿉 異⑸룎?섎뒗 ?숈븞 ?몄텧?섎뒗 硫붿냼??
	void	OnTriggerStay(Collider other)
	{
		this.on_trigger_common(other);
	}

	protected	void	on_trigger_common(Collider other)
	{
		switch(other.gameObject.tag) {

			case "Item":
			{
				if(GameRoot.get().isNowCakeBiking()) {

					CollisionResult	result = new CollisionResult();
			
					result.object0    = this.gameObject;
					result.object1    = other.gameObject;
					result.is_trigger = true;
	
					this.control.collision_results.Add(result);

				} else {

					// 耳?댄겕 臾댄븳?쒓났 ?댁쇅(蹂댄넻 寃뚯엫 以????뚮뒗 由щえ?몃뒗.
					// ?꾩씠?쒖쓣 二쇱슱 ???녿떎.
				}
			}
			break;

			case "Door":
			{
				CollisionResult	result = new CollisionResult();
		
				result.object0    = this.gameObject;
				result.object1    = other.gameObject;
				result.is_trigger = true;

				CollisionManager.getInstance().results.Add(result);
			}
			break;
		}
	}

	// ================================================================ //

	public override void	initialize()
	{
		base.initialize();

		this.walk_motion.is_walking = false;
		this.walk_motion.timer      = 0.0f;
	}
	public override void	start()
	{
		base.start();

		this.next_step = STEP.MOVE;
	}
	public override	void	execute()
	{
		base.execute();

		// 耳?댄겕 臾댄븳?쒓났???뚮쭔 由щえ?멸? ?꾩씠?쒖쓣 二쇱쓣 ???덉쑝誘濡?肄쒕━??泥섎━瑜??쒕떎.]
		// 洹??댁쇅(蹂댄넻 寃뚯엫 以??먮뒗 ?誘몄?瑜??낃굅???꾩씠??痍⑤뱷? ?섏? ?딅뒗??
		this.resolve_collision();

		this.update_item_queries();

		// ---------------------------------------------------------------- //
		// ?ㅽ뀦 ?댁쓽 寃쎄낵?쒓컙??吏꾪뻾?쒕떎.
		
		this.step_timer += Time.deltaTime;
		
		// ---------------------------------------------------------------- //
		// ?ㅼ쓬 ?곹깭濡??대룞?좎? 泥댄겕?쒕떎.
		
		if(this.next_step == STEP.NONE) {
			
			switch(this.step) {

				case STEP.MOVE:
				{
				}
				break;
				
				// 2014.10.14 異붽?.
				case STEP.MELEE_ATTACK:
				{
					if(!this.melee_attack.isAttacking()) {
							
						//this.step.set_next(STEP.MOVE);
						this.next_step = STEP.MOVE;
					}
				}
				break;
			}
		}
		
		// ---------------------------------------------------------------- //
		// ?곹깭 ?꾪솚 ??珥덇린??
		
		while(this.next_step != STEP.NONE) {

			STEP prev	   = this.step;
			this.step      = this.next_step;
			this.next_step = STEP.NONE;
			
			switch(this.step) {
				
				case STEP.OUTER_CONTROL:
				{
					this.GetComponent<Rigidbody>().Sleep();
				}
				break;
				
				case STEP.MOVE:
				{
					if (prev == STEP.OUTER_CONTROL) {
						m_culling.Clear();
						m_plots.Clear();
					}
				}
				break;
				
				case STEP.MELEE_ATTACK:
				{
					this.melee_attack.setHasInput(true);
					this.melee_attack.attack(false);
				}
				break;
			}
			
			this.step_timer = 0.0f;
		}
		
		// ---------------------------------------------------------------- //
		// 媛??곹깭?먯꽌???ㅽ뻾泥섎━.
		this.melee_attack.setHasInput(false);

		switch(this.step) {
			
			case STEP.MOVE:
			{
				this.exec_step_move();
			}
			break;

			case STEP.MELEE_ATTACK:
			{
			}
			break;	
		}
	
		if(this.is_shot_enable) {
	
			this.bullet_shooter.execute(m_shot);
		}
		m_shot = false;
		
		this.collision = "";
		
		// ---------------------------------------------------------------- //
	}

	// ?대룞??愿??泥섎━.
	protected void	exec_step_move()
	{
		Vector3		new_position = this.control.getPosition();
		if(m_plots.Count > 0) {
			CharacterCoord coord = m_plots[0];
			new_position = new Vector3(coord.x, new_position.y, coord.z);
			m_plots.RemoveAt(0);
		}

		// ???쒓컙 硫덉텛?덉쓣 肉먯씠?쇰㈃ 嫄룰린 紐⑥뀡??硫덉텛吏 ?딄쾶 ?쒕떎.

		bool	is_walking = this.walk_motion.is_walking;

		if(Vector3.Distance(new_position, this.control.getPosition()) > 0.0f) {

			this.control.cmdSmoothHeadingTo(new_position);
			this.control.cmdSetPosition(new_position);

			is_walking = true;

		} else {

			is_walking = false;
		}

		if(this.walk_motion.is_walking && !is_walking) {

			this.walk_motion.timer -= Time.deltaTime;

			if(this.walk_motion.timer <= 0.0f) {

				this.walk_motion.is_walking = is_walking;
				this.walk_motion.timer      = STOP_WALK_WAIT;
			}

		} else {

			this.walk_motion.is_walking = is_walking;
			this.walk_motion.timer      = STOP_WALK_WAIT;
		}

		if(this.walk_motion.is_walking) {
			
			// 嫄룰린 紐⑥뀡.
			this.control.cmdSetMotion("m001_walk", 0);
			
		} else {
			
			// 硫덉땄 紐⑥뀡.
			this.control.cmdSetMotion("m002_idle", 0);
		}
	}

	// ---------------------------------------------------------------- //
	// 肄쒕━?꾩쓽 ?섎? 遺??
	private void	resolve_collision()
	{
		foreach(var result in this.control.collision_results) {

			if(result.object1 == null) {
							
				continue;
			}

			//GameObject		self  = result.object0;
			GameObject		other = result.object1;

			switch(other.tag) {


				case "Item":
				{
					do {

						// 耳?댄겕 臾댄븳?쒓났 ?댁쇅(?됱냼 寃뚯엫 以??먮뒗 由щえ?몃뒗.
						//  ?꾩씠?쒖? 二쇱슱 ???녿떎.
						if(!GameRoot.get().isNowCakeBiking()) {

							break;		
						}

						ItemController		item  = other.GetComponent<ItemController>();
	
						QueryItemPick query = this.control.cmdItemQueryPick(item.name, true, true);

					} while(false);
				}
				break;
			}
		}

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

			// ?꾩씠???④낵留?蹂듭궗?섍퀬 ??젣?쒕떎.

			ItemController	item = this.control.cmdItemPick(query, query.target);

			if(item == null) {

				break;
			}

			// ?댄럺??
			EffectRoot.get().createItemGetEffect(this.control.getPosition());

			SoundManager.get().playSE(Sound.ID.DDG_SE_SYS02);

			Debug.Log("Item favor category:" + item.behavior.item_favor.category);
			switch(item.behavior.item_favor.category) {

				case Item.CATEGORY.FOOD:
				{
					this.control.vital.healFull();
					
					this.skin_color_control.startHealing();

					this.cake_count++;
				}
				break;

				case Item.CATEGORY.KEY:
				{
					PartyControl.get().getLocalPlayer().control.consumeKey(item);
				}
				break;	
				
				case Item.CATEGORY.FLOOR_KEY:
				{
					PartyControl.get().getLocalPlayer().control.consumeKey(item);
				}
				break;	

				case Item.CATEGORY.CANDY:
				{
					this.startShotBoost();
				}
				break;

				case Item.CATEGORY.WEAPON:
				{
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

	public override void onDamaged()
	{
		this.control.cmdSetMotion("m005_damage", 1);
		
		EffectRoot.get().createHitEffect(this.transform.position);
	}

	// ================================================================ //

	public void cmdMeleeAttack()
	{
		this.next_step = STEP.MELEE_ATTACK;
		Debug.Log("Command Melee attack.");
	}

	public void cmdShotAttack()
	{
		m_shot = true;
	}

	public void CalcCoordinates(int index, CharacterCoord[] data)
	{
		// ?섏떊??醫뚰몴瑜?蹂댁〈.
		do {

			// ?곗씠?곌? ?낅퉼(留뚯씪???꾪빐).
			if(data.Length <= 0) {

				break;
			}

			// ?덈줈???곗씠?곌? ?녿떎.
			if(index <= m_plotIndex) {
	
				break;
			}

			// m_plotIndex ... m_culling[]??留덉?留??뺤젏 ?몃뜳??
			// index       ... data[]??留덉?留??뺤젏???몃뜳??
			//
			// index - m_plotIndex ... ?대쾲???덈줈 異붽????뺤젏????
			//
			int		s = data.Length - (index - m_plotIndex);

			if(s < 0) {

				break;
			}

			for(int i = s;i < data.Length;i++) {
	
				m_culling.Add(data[i]);
			}

			// m_culling[]??留덉?留??뺤젏 ?몃뜳??
			m_plotIndex = index;

			// ?ㅽ뵆?쇱씤 怨≪꽑??援ы빐??蹂닿컙?쒕떎.	
			SplineData	spline = new SplineData();
			spline.CalcSpline(m_culling);
			
			// 援ы븳 ?ㅽ뵆?쇱씤 蹂닿컙??醫뚰몴 ?뺣낫濡쒖꽌 蹂댁〈?쒕떎.
			CharacterCoord plot = new CharacterCoord();
			for (int i = 0; i < spline.GetPlotNum(); ++i) {
				spline.GetPoint(i, out plot);

				m_plots.Add(plot);
			}
			
			// 媛???ㅻ옒??醫뚰몴瑜???젣.
			if (m_culling.Count > PLOT_NUM) {

				m_culling.RemoveRange(0, m_culling.Count - PLOT_NUM);
			}

		} while(false);
	
	}

}
