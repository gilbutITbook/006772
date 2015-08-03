using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameObjectExtension;

// 플레이어.
// 로컬 플레이어와 네트워크 플레이어의 공통 처리.
// (chrBehaviorLocal와 chrBehaviorNet).
public class chrBehaviorPlayer : chrBehaviorBase {

	public DoorControl			door = null;

	protected BulletShooter		bullet_shooter = null;
	protected SHOT_TYPE			shot_type      = SHOT_TYPE.NONE;

	public Vector3				position_in_formation = Vector3.zero;

	public const float		CHARITY_SPHERE_RADIUS = 3.5f;				// 체력회복 아이템의 효과가 미치는 거리.
	public const float		ICE_DIGEST_TIME       = 2.0f;				// [sec] 이 이상 짧은 시간에 연속으로 아이스를 사용하면 과식(머리 아픔).
	public const float		SHOT_BOOST_TIME       = 10.0f;				// [sec] 캔디로 파워업하는 시간.
	public const float		MELEE_ATTACK_POWER    = 10.0f;				// 근접 공격의 공격력.

	protected const float	BLOW_OUT_TIME = 0.1f;						// [sec] 날라가는 시간.

	// ---------------------------------------------------------------- //
	
	public Item.SlotArray			item_slot = new Item.SlotArray();	// 플레이어가 주울 수 있는 아이템.

	protected	Vector3		initial_local_position_model;				// 모델의 로컬 포지션(초기상태에서의).

	// 아이스 과식으로 머리 아픈 상태 타이머.
	public class JinJinTimer {

		public float	current  = -1.0f;
		public float	duration = -1.0f;
	};

	public JinJinTimer	jin_jin_timer  = new JinJinTimer();				// 아이스 과식으로 머리 아픈 상태.
	public GameObject	jin_jin_effect = null;

	protected MeleeAttack	melee_attack;

	public SkinColorControl	skin_color_control = null;					// 모델의 컬러 체인지(체력 회복, 아이스 과식 등).

	protected int		cake_count = 0;									// 케이크 획득 수..
	protected float		ice_timer = -1.0f;								// [sec] 아이스를 사용한 후 경과 시간.

	protected float		shot_boost_timer = 0.0f;						// [sec] 파워업용 타이머.

	protected bool		is_shot_enable = true;							// 쏠 수 있는가?.

	protected int		melee_count = 0;								// 근접 공격으로 쓰러진 적의 수.

	// 체력이 0이 되었을 때 연출.
	protected class StepBatanQ {

		public GameObject	tears_effect;		// 눈물 효과.
	};
	protected StepBatanQ	step_batan_q = new StepBatanQ();

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		this.initial_local_position_model = this.getModel().transform.localPosition;
	}

	void	Start()
	{
	}

	void	Update()
	{
	}

	// ================================================================ //

	public override void	initialize()
	{
		// Shoot.
		this.changeBulletShooter(SHOT_TYPE.NEGI);

		// 근접공격.
		this.melee_attack = this.gameObject.AddComponent<MeleeAttack>();
		this.melee_attack.behavior = this;

		this.skin_color_control = new SkinColorControl();
		this.skin_color_control.create(this);
	}

	public override void	start()
	{
		this.control.vital.healFullInternal();
		this.control.resetMotion();
	}

	public override	void	execute()
	{
		this.door = null;

		// 파워업 타이머
		// (캔디를 획득했을 때).
		if(this.shot_boost_timer > 0.0f) {

			this.shot_boost_timer -= Time.deltaTime;

			if(this.shot_boost_timer <= 0.0f) {

				// 타임아웃으로 효과 끝남.

				this.shot_boost_timer = 0.0f;

				ItemWindow.get().clearItem(Item.SLOT_TYPE.CANDY, 0);

				this.item_slot.candy.initialize();
			}
		}

		this.melee_attack.execute();

		if(this.ice_timer >= 0.0f) {

			this.ice_timer -= Time.deltaTime;
		}

		this.executeJinJin();

		this.skin_color_control.execute();

		this.execute_queries();
	}

	// 로컬 플레이어?.
	public virtual bool	isLocal()
	{
		return(true);
	}

	// 외부에서의 제어를 시작한다.
	public virtual void		beginOuterControll()
	{
	}

	// 외부에서의 제어를 종료한다.
	public virtual void		endOuterControll()
	{
	}

	// 아이템을 사용했을 때 호출된다.
	public virtual void		onUseItem(int slot, Item.Favor favor)
	{
	}

	//  아이템을 사용해 주었을 때(동료가 자신에게) 호출된다.
	public virtual void		onUseItemByFriend(Item.Favor favor, chrBehaviorPlayer friend)
	{
		switch(favor.category) {

			case Item.CATEGORY.SODA_ICE:
			{
				this.skin_color_control.startHealing();
			}
			break;
		}
	}

	// 근접 공격이 통했을 때 호출된다.
	public override void		onMeleeAttackHitted(chrBehaviorBase other)
	{
		// 근접 공격으로 적을 10마리 쓰러뜨릴 때마다 캔디가 나온다.
		do {

			if(other.control.vital.getHitPoint() > 0) {

				break;
			}

			this.melee_count++;
			
			if(this.melee_count%10 != 0) {

				break;
			}

			chrBehaviorEnemy	enemy = other as chrBehaviorEnemy;

			if(enemy == null) {

				break;
			}

			enemy.setRewardItem("candy00", "candy00", null);

		} while(false);
	}

	// 대미지를 받아 날라가기 시작한다.
	public virtual void		beginBlowOut(Vector3 center, float radius)
	{
	}

	// 대미지를 받아 날라가기 시작한다(가로 방향 한정).
	public virtual void		beginBlowOutSide(Vector3 center, float radius, Vector3 direction)
	{
	}

	// ================================================================ //
	// 비헤이비어가 사용하는 커맨드.
	// 이벤트 박스.

	// 문 앞에 있는 동안 호출한다.
	public void		cmdNotiryStayDoorBox(DoorControl door)
	{
		this.door = door;
	}

	// ================================================================ //

	// 쏠 수 있다 / 쏠 수 없다를 설정한다?.
	public void		setShotEnable(bool is_enable)
	{
		this.is_shot_enable = is_enable;
	}

	// 모델의 게임 오브젝트를 취득한다.
	public GameObject	getModel()
	{
		return(this.gameObject.transform.FindChild("model").gameObject);
	}

	// 모델의 초기 상태에서의 로컬 포지션을 가져온다.
	public Vector3	getInitialLocalPositionModel()
	{
		return(this.initial_local_position_model);
	}

	// 계정 이름을 가져온다.
	public string	getAcountID()
	{
		AccountData		account_data = AccountManager.get().getAccountData(this.control.global_index);

		return(account_data.account_id);
	}

	// 글로벌 인덱스를 가져온다.
	public int		getGlobalIndex()
	{
		return(this.control.global_index);
	}

	// SHOT_TYPE을 가져온다.
	public SHOT_TYPE	getShotType()
	{
		return(this.shot_type);
	}

	// SHOT_TYPE을 변경한다.
	public void		changeBulletShooter(SHOT_TYPE shot_type)
	{
		if(shot_type != this.shot_type) {

			if(this.bullet_shooter != null) {

				GameObject.Destroy(this.bullet_shooter);
			}

			this.shot_type = shot_type;
			this.bullet_shooter = BulletShooter.createShooter(this.control, this.shot_type);
		}
	}

	// 파워업 타임을 시작한다.
	public void		startShotBoost()
	{
		this.shot_boost_timer = SHOT_BOOST_TIME;
	}

	// 파워업 중?.
	public bool		isShotBoosted()
	{
		return(this.shot_boost_timer > 0.0f);
	}

	// 케이크를 획득한 수를 가져온다.
	public int	getCakeCount()
	{
		return(this.cake_count);
	}

	// ================================================================ //

	// 머리 아픈 상태(아이스 과식)를 시작한다.
	public void		startJinJin()
	{
		this.jin_jin_timer.current = 0.0f;
		this.jin_jin_timer.duration = 5.0f;

		if(this.jin_jin_effect == null) {

			this.jin_jin_effect = EffectRoot.get().createJinJinEffect(this.control.getPosition() + new Vector3(0.0f, 2.5f, -0.5f));
		}

		this.skin_color_control.startJinJin();
	}

	// 머리 아픈 상태(아이스 과식)를 끝낸다.
	public void		stopJinJin()
	{
		this.jin_jin_timer.current  = -1.0f;
		this.jin_jin_timer.duration = -1.0f;

		if(this.jin_jin_effect != null) {

			this.jin_jin_effect.destroy();
			this.jin_jin_effect         = null;
		}

		this.skin_color_control.stopJinJin();
	}

	// 머리 아픈 상태(아이스 과식) 실행.
	public void		executeJinJin()
	{
		if(this.jin_jin_timer.current >= 0.0f) {

			this.jin_jin_timer.current += Time.deltaTime;

			if(this.jin_jin_timer.current >= this.jin_jin_timer.duration) {

				this.stopJinJin();
			}
		}

		if(this.jin_jin_timer.current >= 0.0f) {

			// 효과 위치를 플레이어에게 따르게 한다.
			if(this.jin_jin_effect != null) {
	
				this.jin_jin_effect.transform.position = this.control.getPosition() + new Vector3(0.0f, 2.5f, -0.5f);
			}
		}
	}

	// 머리 아픈 상태??.
	public bool		isNowJinJin()
	{
		return(this.jin_jin_timer.current >= 0.0f);
	}

	// ---------------------------------------------------------------- //

	// 크림 범벅을 시작한다.
	public void		startCreamy()
	{
		this.skin_color_control.startCreamy();
	}

	// 크림 범벅을 끝낸다.
	public void		stopCreamy()
	{
		this.skin_color_control.stopCreamy();
	}

	// 크림 범벅 중?.
	public bool		isNowCreamy()
	{
		return(this.skin_color_control.isNowCreamy());
	}

	// ---------------------------------------------------------------- //

	// 체력회복을 시작한다.
	public void		startHealing()
	{
		this.skin_color_control.startHealing();
	}

	// 체력 회복을 끝낸다.
	public void		stopHealing()
	{
		this.skin_color_control.stopHealing();
	}

	// 체력 회복 중인가?.
	public bool		isNowHealing()
	{
		return(this.skin_color_control.isNowHealing());
	}

	public void		execute_queries()
	{
		// 조정이 끝난 쿼리를 찾는다.
		List<QueryBase> queries = QueryManager.get().findDoneQuery(this.control.getAccountID());

		foreach(QueryBase query in queries) {

			switch(query.getType()) {
				
				case "talk":
				{
				Debug.Log("query talk: " + PartyControl.get().getLocalPlayer().getAcountID());

					if(query.isSuccess()) {
						
						QueryTalk		query_talk = query as QueryTalk;
						
						this.control.cmdDispBalloon(query_talk.words);
					}
					query.set_expired(true);
				}
				break;
			}
			
			// 용무가 끝났으므로 삭제한다.
			query.set_expired(true);
			
			if(!query.isSuccess()) {
				
				continue;
			}
		}

	}
	// ================================================================ //

	// 대미지를 받고 날라가는 중.
	protected class StepBlowOut {

		public Vector3	center;
		public float	radius;

		public bool		is_side_only;		// 가로 방향만.
		public Vector3	direction;

		public void		begin(Vector3 center, float radius)
		{
			this.center       = center;
			this.radius       = radius;
			this.is_side_only = false;
			this.direction    = Vector3.zero;
		}

		public void		begin(Vector3 center, float radius, Vector3 direction)
		{
			this.center       = center;
			this.radius       = radius;
			this.is_side_only = true;
			this.direction    = direction;
	
			this.direction.y = 0.0f;
			this.direction.Normalize();

			if(this.direction.magnitude == 0.0f) {

				this.is_side_only = false;
			}
		}
	}
	protected StepBlowOut	step_blow_out = new StepBlowOut();

	protected void		exec_step_blow_out()
	{
		Vector3		distance_vector = this.control.getPosition() - this.step_blow_out.center;

		distance_vector.y = 0.0f;

		if(this.step_blow_out.is_side_only) {

			Vector3		parallel = Vector3.Dot(distance_vector, this.step_blow_out.direction)*this.step_blow_out.direction;

			distance_vector -= parallel;
		}

		Rect	room_rect = MapCreator.get().getRoomRect(PartyControl.get().getCurrentRoom().getIndex());

		// 방 끝 벽에 가까울 때는 역방향으로 날린다.

		if(room_rect.min.y - this.step_blow_out.center.z > -this.step_blow_out.radius) {

			if(distance_vector.z < 0.0f) {

				distance_vector.z *= -1.0f;
			}

		} else if(room_rect.max.y - this.step_blow_out.center.z < this.step_blow_out.radius) {

			if(distance_vector.z > 0.0f) {

				distance_vector.z *= -1.0f;
			}
		}

		if(room_rect.min.x - this.step_blow_out.center.x > -this.step_blow_out.radius) {

			if(distance_vector.x < 0.0f) {

				distance_vector.x *= -1.0f;
			}

		} else if(room_rect.max.x - this.step_blow_out.center.x < this.step_blow_out.radius) {

			if(distance_vector.x > 0.0f) {

				distance_vector.x *= -1.0f;
			}
		}

		//

		float		base_speed = this.step_blow_out.radius/BLOW_OUT_TIME;
		float		speed = base_speed*(Mathf.Max(0.0f, this.step_blow_out.radius - distance_vector.magnitude) + 1.0f)/this.step_blow_out.radius;

		distance_vector.Normalize();

		if(distance_vector.magnitude == 0.0f) {

			distance_vector = Vector3.forward;
		}

		distance_vector *= speed*Time.deltaTime;

		this.control.cmdSetPosition(this.control.getPosition() + distance_vector);
		this.control.cmdSmoothHeadingTo(this.step_blow_out.center, 0.5f);
	}

}
