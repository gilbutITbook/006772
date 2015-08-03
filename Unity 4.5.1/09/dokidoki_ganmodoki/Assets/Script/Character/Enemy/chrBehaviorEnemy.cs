using UnityEngine;
using System.Collections;
using MathExtension;

// 적의 비헤이비어 기저 클래스.
public class chrBehaviorEnemy : chrBehaviorBase {


	public bool		is_attack_motion_finished = false;		// 공격 모션이 끝났는가?
	public bool		is_attack_motion_impact = false;

	// ---------------------------------------------------------------- //

	public Character.BasicAction	basic_action  = new Character.BasicAction();		// 적 공통 기본 액션
	public Enemy.BEHAVE_KIND		behave_kind   = Enemy.BEHAVE_KIND.BOTTACHI;
	public Character.ActionBase		unique_action = new Character.BottachiAction();

	// 적을 헤치웠을 때 나타나는 아이템.
	protected struct RewardItem {

		public string		type;		// 타입.
		public string		name;		// 이름.
		public object		option0;	// 옵션인수０
	}
	protected RewardItem	reward_item;

	// ================================================================ //

	/// <summary>
	/// 참일 때 적의 사고 판단은 정지한다.
	/// </summary>
	protected bool isPaused;

	public RoomController	room;		// 자신이 있는 방.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
	}

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	void 	OnCollisionEnter(Collision other)
	{
		switch(other.gameObject.tag) {

			case "Player":
			case "Wall":
			{
				CollisionResult	result = new CollisionResult();
		
				result.object0    = this.gameObject;
				result.object1    = other.gameObject;
				result.is_trigger = false;

				if(other.contacts.Length > 0) {

					result.option0 = (object)other.contacts[0];
				}

				this.control.collision_results.Add(result);

			}
			break;
		}
	}

	// ================================================================ //

	// 생성 직후에 호출된다.
	public override void	initialize()
	{
		this.reward_item.type = "";
		this.reward_item.name = "";

		this.rigidbody.useGravity = false;
		
		this.basic_action.create(this);
	}

	// 게임 시작 시에 한 번만 호출된다.
	public override void	start()
	{
		this.basic_action.start();
	}

	// 매 프레임 호출된다.
	public override void	execute()
	{
	}

	// AI의 종류(행동 패턴)을 설정한다.
	public void		setBehaveKind(Enemy.BEHAVE_KIND behave_kind, Character.ActionBase.DescBase desc_base = null)
	{
		Character.ActionBase	unique_action = null;

		switch(behave_kind) {

			default:
			case Enemy.BEHAVE_KIND.BOTTACHI:		unique_action = new Character.BottachiAction();			break;
			case Enemy.BEHAVE_KIND.OUFUKU:			unique_action = new Character.OufukuAction();			break;
			case Enemy.BEHAVE_KIND.UROURO:			unique_action = new Character.UroUroAction();			break;
			case Enemy.BEHAVE_KIND.TOTUGEKI:		unique_action = new Character.TotugekiAction();			break;
			case Enemy.BEHAVE_KIND.SONOBA_DE_FIRE:	unique_action = new Character.SonobaDeFireAction();		break;
			case Enemy.BEHAVE_KIND.WARP_DE_FIRE:	unique_action = new Character.WarpDeFireAction();		break;
			case Enemy.BEHAVE_KIND.JUMBO:			unique_action = new Character.JumboAction();			break;
			case Enemy.BEHAVE_KIND.GOROGORO:		unique_action = new Character.GoroGoroAction();			break;
		}

		if(unique_action != null) {

			if(desc_base == null) {

				desc_base = new Character.ActionBase.DescBase();
			}

			this.behave_kind = behave_kind;

			this.unique_action = unique_action;
			this.unique_action.create(this, desc_base);

			this.basic_action.unique_action = this.unique_action;
		}
	}

	// ================================================================ //

	// 대미지.
	public virtual void		causeDamage()
	{
	}

	// 죽는 걸로 한다.
	public virtual void		causeVanish()
	{
		this.basic_action.step.set_next(Character.BasicAction.STEP.VANISH);
	}

	// ================================================================ //

	// 자신이 근접 공격.
	public virtual void		onMeleeAttack(chrBehaviorPlayer player)
	{
	}
	// 죽을 때 호출된다.
	public override void		onVanished()
	{
	}

	// 삭제 직전에 호출된다.
	public override void		onDelete()
	{
		base.onDelete();

		// 아이템을 생성한다.
		if(this.reward_item.type != "" && this.reward_item.name != "") {

			string	local_player_id = PartyControl.get().getLocalPlayer().getAcountID();
	
			ItemManager.get().createItem(this.reward_item.type, this.reward_item.name, local_player_id);
			ItemManager.get().setPositionToItem(this.reward_item.name, this.control.getPosition());

			var		item = ItemManager.get().findItem(this.reward_item.name);

			if(item != null) {

				item.behavior.setFavorOption(this.reward_item.option0);
				item.behavior.beginSpawn();
			}
		}

		if(this.room != null) {

			LevelControl.get().onDeleteEnemy(this.room, this.control);
		}
	}

	// 대미지를 받았을 때 호출된다.
	public override void		onDamaged()
	{
		if(this.control.vital.hit_point <= 0.0f) {

			if(this.basic_action.step.get_current() != Character.BasicAction.STEP.VANISH) {

				this.basic_action.step.set_next(Character.BasicAction.STEP.VANISH);
			}
		}
	}

	// 그 자리에 멈춰선다.
	public void		beginStill()
	{
		this.basic_action.step.set_next(Character.BasicAction.STEP.STILL);
	}

	// 그 자리에 멈춰서기를 끝낸다.
	public void		endStill(float delay = 0.0f)
	{
		this.basic_action.step.set_next_delay(Character.BasicAction.STEP.UNIQUE, delay);
	}

	// ================================================================ //
	// 외부에서 호출되는 메소드.
		
	/// <summary>
	/// 비헤이비어의 일시정지를 설정한다.
	/// </summary>
	/// <param name="newPause">참 일 때 일시정지 거짓일 때 일시 정지 해제</param>
	public void SetPause(bool newPause)
	{
		isPaused = newPause;
	}

	// 스폰(상자로부터 튀어나온다) 액션을 시작한다.
	public void		beginSpawn(Vector3 start, Vector3 dir_vector)
	{
		this.basic_action.beginSpawn(start, dir_vector);
	}

	// 적을 쓰러뜨렸을 때 출현하는 아이템을 설정한다.
	public void		setRewardItem(string type, string name, object favor_option0)
	{
		this.reward_item.type    = type;
		this.reward_item.name    = name;
		this.reward_item.option0 = favor_option0;
	}

	// 생성된 룸을 설정한다.
	public void		setRoom(RoomController room)
	{
		this.room = room;
	}

	// 자신을 삭제한다.
	public void		deleteSelf()
	{
		this.onDelete();

		EnemyRoot.getInstance().deleteEnemy(this.control);
	}

	// ================================================================ //

	// 공격할 플레이어를 선택한다.
	public chrBehaviorPlayer	selectTargetPlayer(float distance_limit, float angle_limit)
	{
		chrBehaviorPlayer	target = null;

		// 공격 가능 범위에서 가장 가깝고 정면에 있는 플레이어를.
		// 찾는다.

		var players = PartyControl.get().getPlayers();

		float	min_score = -1.0f;

		foreach(var player in players) {

			// 거리를 조사한다.

			float	distance = (this.control.getPosition() - player.control.getPosition()).Y(0.0f).magnitude;

			if(distance >= distance_limit) {
					
					continue;
			}

			// 정면에서 어느 정도 방향이 어긋나는가?.

			float	angle = MathUtility.calcDirection(this.control.getPosition(), player.control.getPosition());

			angle = Mathf.Abs(MathUtility.snormDegree(angle - this.control.getDirection()));

			if(angle >= angle_limit) {

				continue;
			}

			//

			float	score = distance*MathUtility.remap(0.0f, angle_limit, angle, 1.0f, 2.0f);

			if(target == null) {

				target    = player;
				min_score = score;

			} else {

				if(score < min_score) {

					target    = player;
					min_score = score;
				}
			}
		}

		return(target);
	}

	// 상대가 공격할 수 있는 범위에 있는가?.
	public bool		isInAttackRange(chrController target)
	{
		bool	ret = false;

		do {
	
			chrController	mine = this.control;

			Vector3		to_enemy = target.getPosition() - mine.getPosition();
	
			to_enemy.y = 0.0f;
	
			if(to_enemy.magnitude >= mine.vital.getAttackDistance()) {

				break;
			}
			to_enemy.Normalize();

			Vector3		heading = Quaternion.AngleAxis(mine.getDirection(), Vector3.up)*Vector3.forward;
	
			heading.y = 0.0f;
			heading.Normalize();
	
			float	dp = Vector3.Dot(to_enemy, heading);
	
			if(dp < Mathf.Cos(Mathf.PI/4.0f)) {
	
				break;
			}

			ret = true;

		} while(false);

		return(ret);
	}
}

