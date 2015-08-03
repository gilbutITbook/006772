using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 비헤이비어 - 네트워크 플레이어(게스트)용.
public class chrBehaviorNet : chrBehaviorPlayer {

	protected string	move_target_item = "";	// 아이템을 목표로 이동할 때.

	protected string	collision = "";

	public chrBehaviorLocal	local_player          = null;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 평상 시.
		MELEE_ATTACK,		// 근접 공격.
		OUTER_CONTROL,		// 외부 제어.

		NUM,
	};

	//public Step<STEP>	step = new Step<STEP>(STEP.NONE);
	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;


	// ---------------------------------------------------------------- //
	
	// 3차 스플라인 보간에서 사용할 점의 개수.
	private const int PLOT_NUM = 4;
	
	// 솎아낸 좌표의 프레임 수.
	private const int CULLING_NUM = 10;
	
	// 현재 플롯 인덱스.
	private int 	m_plotIndex = 0;
	
	// 솎아낸 좌표를 보존.
	private List<CharacterCoord>	m_culling = new List<CharacterCoord>();
	// 보간한 좌표를 보존.
	private List<CharacterCoord>	m_plots = new List<CharacterCoord>();

	// 공격 플래그.
	private	bool	m_shot = false;

	// 보행 플래그.
	private struct WalkMotion {

		public bool		is_walking;
		public float	timer;
	};
	private	WalkMotion	walk_motion;

	private const float	STOP_WALK_WAIT = 0.1f;		// [sec] 걷기 -> 서기 모션으로 이행할 때의 유예 시간.

	// ================================================================ //

	// 로컬 플레이어?.
	public override bool	isLocal()
	{
		return(false);
	}

	// 외부에서의 제어를 시작한다.
	public override void 	beginOuterControll()
	{
		this.next_step = STEP.OUTER_CONTROL;
	}

	// 외부에서의 제어를 종료한다.
	public override void		endOuterControll()
	{
		this.next_step = STEP.MOVE;
	}


	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// 콜리전에 충돌하는 동안 호출되는 메소드.
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

	// 트리거에 충돌한 순간만 호출되는 메소드.
	void	OnTriggerEnter(Collider other)
	{
		this.on_trigger_common(other);
	}
	// 트리거에 충돌하는 동안 호출되는 메소드.
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

					// 케이크 무한제공 이외(보통 게임 중)일 때는 리모트는.
					// 아이템을 주울 수 없다.
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

		// 케이크 무한제공일 때만 리모트가 아이템을 주을 수 있으므로 콜리전 처리를 한다.]
		// 그 이외(보통 게임 중)에는 대미지를 입거나 아이템 취득은 하지 않는다.
		this.resolve_collision();

		this.update_item_queries();

		// ---------------------------------------------------------------- //
		// 스텝 내의 경과시간을 진행한다.
		
		this.step_timer += Time.deltaTime;
		
		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크한다.
		
		if(this.next_step == STEP.NONE) {
			
			switch(this.step) {

				case STEP.MOVE:
				{
				}
				break;
				
				// 2014.10.14 추가.
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
		// 상태 전환 시 초기화.
		
		while(this.next_step != STEP.NONE) {

			STEP prev	   = this.step;
			this.step      = this.next_step;
			this.next_step = STEP.NONE;
			
			switch(this.step) {
				
				case STEP.OUTER_CONTROL:
				{
					this.rigidbody.Sleep();
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
		// 각 상태에서의 실행처리.
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

	// 이동에 관한 처리.
	protected void	exec_step_move()
	{
		Vector3		new_position = this.control.getPosition();
		if(m_plots.Count > 0) {
			CharacterCoord coord = m_plots[0];
			new_position = new Vector3(coord.x, new_position.y, coord.z);
			m_plots.RemoveAt(0);
		}

		// 한 순간 멈추었을 뿐이라면 걷기 모션을 멈추지 않게 한다.

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
			
			// 걷기 모션.
			this.control.cmdSetMotion("m001_walk", 0);
			
		} else {
			
			// 멈춤 모션.
			this.control.cmdSetMotion("m002_idle", 0);
		}
	}

	// ---------------------------------------------------------------- //
	// 콜리전의 의미 부여.
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

						// 케이크 무한제공 이외(평소 게임 중)에는 리모트는.
						//  아이템은 주울 수 없다.
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
	// 아이템 쿼리를 갱신한다.

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

			// 아이템 효과만 복사하고 삭제한다.

			ItemController	item = this.control.cmdItemPick(query, query.target);

			if(item == null) {

				break;
			}

			// 이펙트.
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
		// 수신한 좌표를 보존.
		do {

			// 데이터가 텅빔(만일을 위해).
			if(data.Length <= 0) {

				break;
			}

			// 새로운 데이터가 없다.
			if(index <= m_plotIndex) {
	
				break;
			}

			// m_plotIndex ... m_culling[]의 마지막 정점 인덱스.
			// index       ... data[]의 마지막 정점의 인덱스.
			//
			// index - m_plotIndex ... 이번에 새로 추가된 정점의 수.
			//
			int		s = data.Length - (index - m_plotIndex);

			if(s < 0) {

				break;
			}

			for(int i = s;i < data.Length;i++) {
	
				m_culling.Add(data[i]);
			}

			// m_culling[]의 마지막 정점 인덱스.
			m_plotIndex = index;

			// 스플라인 곡선을 구해서 보간한다.	
			SplineData	spline = new SplineData();
			spline.CalcSpline(m_culling);
			
			// 구한 스플라인 보간을 좌표 정보로서 보존한다.
			CharacterCoord plot = new CharacterCoord();
			for (int i = 0; i < spline.GetPlotNum(); ++i) {
				spline.GetPoint(i, out plot);

				m_plots.Add(plot);
			}
			
			// 가장 오래된 좌표를 삭제.
			if (m_culling.Count > PLOT_NUM) {

				m_culling.RemoveRange(0, m_culling.Count - PLOT_NUM);
			}

		} while(false);
	
	}

}
