using UnityEngine;
using System.Collections;

namespace Character {

// 적 캐릭터 액션의 기저 클래스.
public class ActionBase {

	public chrController		control  = null;
	public chrBehaviorEnemy		behavior = null;

	public ActionBase			parent   = null;
	public ActionBase			child    = null;

	public bool					is_finished = false;

	public class DescBase {

	}

	// ================================================================ //

	// 생성합니다.
	public virtual void		create(chrBehaviorEnemy behavior, ActionBase.DescBase desc_base = null)
	{
		this.behavior = behavior;
		this.control  = behavior.control;
	}

	public virtual void		start() {}				// 스타트.
	public virtual void		resume() {}				// 자식 계층부터 복귀.
	public virtual void		execute() {}			// 매 프레임 실행.
	public virtual void		stealth() {}			// 부모가 실행 중에도 실행.

	// 자식 계층을 시작합니다.
	public void		push(ActionBase child)
	{
		this.child = child;
		this.child.start();
	}

	// 자식 계층 실행.
	public void		execute_child()
	{
		if(this.child != null) {

			this.child.execute();
		}
	}

	// 자식 계층 종료 체크.
	public bool		finish_child()
	{
		bool	ret = false;

		do {

			if(this.child == null) {

				break;
			}
			if(!this.child.is_finished) {

				break;
			}

			//

			this.child = null;
			ret = true;

		} while(false);

		return(ret);
	}
}

// 적 공통 기본 액션.
public class BasicAction : ActionBase {

	public const float	MOVE_SPEED_DEFAULT = 2.0f;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		IDLE  = 0,	
		SPAWN,				// 상자(제네레이터)에서 튀어나오는 중(착지까지).
		VANISH,				// 바이바~이.
		STILL,				// 그 자리에 멈춤.

		UNIQUE,				// 적의 사고 타입별 고유 액션.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	public ActionBase	unique_action = new ActionBase();
	public Animator		animator;

	// 이동 모션의 속도.
	protected struct MotionSpeed {

		public float	current;
		public float	goal;
	}
	protected MotionSpeed		motion_speed;

	// ---------------------------------------------------------------- //

	public float		move_dir   = 0.0f;					// 이동방향.
	public float		move_speed = 1.0f;
	public float		turn_rate  = 0.0f;
	public Vector3		position_xz;

	public struct WallColi {

		public bool		is_valid;
		public Vector3	normal;
	}
	public WallColi		wall_coli;

	public ipModule.Jump	jump;

	public bool			is_spawn_from_lair = false;
	public Vector3		outlet_position    = Vector3.zero;		// 제네레이터에서 튀어나올 때의 시작 위치.
	public Vector3		outlet_vector      = Vector3.forward;	// 제네레이터에서 튀어나올 때의 속도.


	// ================================================================ //

	// 생성합니다.
	public override void		create(chrBehaviorEnemy behavior, ActionBase.DescBase desc_base = null)
	{
		base.create(behavior, desc_base);

		this.motion_speed.current = 0.0f;
		this.motion_speed.goal    = 0.0f;

		this.animator = this.behavior.gameObject.GetComponent<Animator>();

		this.jump = new ipModule.Jump();
		this.jump.gravity *= 3.0f;
		this.jump.bounciness = new Vector3(1.0f, -0.4f, 1.0f);
	}

	// 시작.
	public override void	start()
	{
		if(this.step.get_next() == STEP.NONE) {

			if(this.is_spawn_from_lair) {
	
				// 제네레이터에서 튀어나올 때.
	
				this.position_xz = this.outlet_position;
	
				Vector3		start = this.outlet_position;
				Vector3		goal  = start + this.outlet_vector*3.0f;
	
				goal.y = MapCreator.get().getFloorHeight();

				this.jump.start(start, goal, start.y + 2.0f);
	
				this.move_dir = Mathf.Atan2(this.outlet_vector.x, this.outlet_vector.z)*Mathf.Rad2Deg;
	
				this.step.set_next(STEP.SPAWN);
	
			} else {
	
				this.move_dir    = this.control.getDirection();
				this.position_xz = this.control.getPosition();
				this.position_xz.y = 0.0f;
	
				this.step.set_next(STEP.UNIQUE);
			}

		} else {

			// 생성 직후에 단계가 지정되어 있을 때 변경해버리지 않도록.

			this.move_dir    = this.control.getDirection();
			this.position_xz = this.control.getPosition();
			this.position_xz.y = 0.0f;
		}
	
		this.control.cmdSetPositionAnon(this.position_xz);
		this.control.cmdSetDirectionAnon(this.move_dir);
	}

	// 자식 계층에서 복귀합니다.
	public override void		resume()
	{
		if(this.control.vital.hit_point <= 0.0f) {

			this.step.set_next(STEP.VANISH);

		} else {

			this.step.set_next(STEP.IDLE);
		}
	}

	// 매 프레임 실행.
	public override void		execute()
	{
		this.position_xz = this.control.getPosition();

		// ---------------------------------------------------------------- //
		// 다음 상태로 이행할지 체크합니다.

		switch(this.step.do_transition()) {

			// 상자(제네레이터)에서 튀어나오는 중.
			case STEP.SPAWN:
			{
				if(this.jump.isDone()) {

					this.step.set_next(STEP.UNIQUE);
				}
			}
			break;

			// 바이바~이~.
			case STEP.VANISH:
			{
				if(this.behavior.control.damage_effect.isVacant()) {

					// 자기 자신의 인스턴스를 삭제합니다.
					this.behavior.deleteSelf();
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환했을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.UNIQUE:
				{
					this.unique_action.parent = this;
					this.unique_action.start();
				}
				break;

				// 상자(제네레이터)에서 튀어나오는 중.
				case STEP.SPAWN:
				{
					this.control.cmdSetDirection(this.move_dir);
				}
				break;

				// 바이바~이~.
				case STEP.VANISH:
				{
					this.animator.speed = 0.0f;
					this.control.cmdBeginVanish();
				}
				break;

				// 그 자리에서 멈춥니다.
				case STEP.STILL:
				{
					this.setMoveMotionSpeed(0.0f);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.UNIQUE:
			{
				this.unique_action.execute();
			}
			break;

			// 상자(제네레이터)에서 튀어나오는 중(착지까지).
			case STEP.SPAWN:
			{
				this.position_xz += this.jump.xz_velocity()*Time.deltaTime;
			}
			break;
		}

		this.unique_action.stealth();

		// ---------------------------------------------------------------- //

		this.update_motion_speed();

		this.jump.execute(Time.deltaTime);
		this.position_xz.y = this.jump.position.y;

		// 방 외벽을 넘어가지 않도록.
		do {

			chrBehaviorEnemy	behavior = this.control.behavior as chrBehaviorEnemy;

			if(behavior == null) {

				break;
			}

			Rect	room_rect = MapCreator.get().getRoomRect(behavior.room.getIndex());
	
			this.wall_coli.is_valid = false;
	
			if(this.position_xz.x < room_rect.min.x) {
	
				this.position_xz.x      = room_rect.min.x;
				this.wall_coli.is_valid = true;
				this.wall_coli.normal   = Vector3.right;
			}
			if(this.position_xz.x > room_rect.max.x) {
	
				this.position_xz.x      = room_rect.max.x;
				this.wall_coli.is_valid = true;
				this.wall_coli.normal   = Vector3.left;
			}
			if(this.position_xz.z < room_rect.min.y) {
	
				this.position_xz.z      = room_rect.min.y;
				this.wall_coli.is_valid = true;
				this.wall_coli.normal   = Vector3.forward;
			}
			if(this.position_xz.z > room_rect.max.y) {
	
				this.position_xz.z      = room_rect.max.y;
				this.wall_coli.is_valid = true;
				this.wall_coli.normal   = Vector3.back;
			}

		} while(false);

		this.control.cmdSetPosition(this.position_xz);

		// 벽에 부딪히면 벽을 따라 이동 방향을 바꿉니다.
		do {

			if(!this.wall_coli.is_valid) {

				break;
			}

			Vector3		v = Quaternion.AngleAxis(this.move_dir, Vector3.up)*Vector3.forward;

			float	dp = Vector3.Dot(v, this.wall_coli.normal);

			if(dp > 0.0f) {

				break;
			}

			v = v - 2.0f*this.wall_coli.normal*dp;

			this.move_dir = Mathf.Atan2(v.x, v.z)*Mathf.Rad2Deg;

		} while(false);

		// 턴(방향 보간).
		if(this.turn_rate > 0.0f) {

			this.control.cmdSmoothDirection(this.move_dir, this.turn_rate);
			this.turn_rate = 0.0f;

		} else {

			this.control.cmdSmoothDirection(this.move_dir);
		}
	}

	// 移動.
	public void		executeMove()
	{
		// ---------------------------------------------------------------- //
		// 이동(위치 좌표 보간).

		Vector3		position  = this.control.getPosition();

		float		speed_per_frame = this.move_speed*MOVE_SPEED_DEFAULT*Time.deltaTime;

		Vector3		move_vector = Quaternion.AngleAxis(this.move_dir, Vector3.up)*Vector3.forward;

		position += move_vector*speed_per_frame;

		this.position_xz = position;
	}

	// 스폰 액션(상자에서 뿅하고 튀어나온다).
	public void		beginSpawn(Vector3 start, Vector3 dir_vector)
	{
		this.is_spawn_from_lair = true;
		this.outlet_position    = start;
		this.outlet_vector      = dir_vector;
	}

	// 이동 속도를 설정합니다.
	public void		setMoveSpeed(float speed)
	{
		this.move_speed = speed;
	}

	// 이동 모션의 속도를 설정합니다.
	// 정지 모션과 걷기 모션의 블렌딩비율.
	public void		setMoveMotionSpeed(float speed)
	{
		this.motion_speed.goal = speed;
	}

	// 모션 재생 속도를 설정합니다.
	public void		setMotionPlaySpeed(float speed)
	{
		this.animator.speed = speed;
	}

	// 이동 방향의 벡터를 설정합니다.
	public Vector3	getMoveVector()
	{
		return(Quaternion.AngleAxis(this.move_dir, Vector3.up)*Vector3.forward);
	}

	// 이동 방향 벡터 → 이동 방향(Y앵글).
	public void		setMoveDirByVector(Vector3 v)
	{
		this.move_dir = Mathf.Atan2(v.x, v.z)*Mathf.Rad2Deg;
	}

	// ================================================================ //

	// 모션 속도 갱신.
	public void		update_motion_speed()
	{
		if(this.motion_speed.current != this.motion_speed.goal) {

			float	delta = (1.0f/0.2f)*Time.deltaTime;

			if(this.motion_speed.current < this.motion_speed.goal) {

				this.motion_speed.current = Mathf.Min(this.motion_speed.current + delta, this.motion_speed.goal);

			} else {

				this.motion_speed.current = Mathf.Max(this.motion_speed.current - delta, this.motion_speed.goal);
			}
			this.animator.SetFloat("Motion_Speed", this.motion_speed.current*0.1f);
		}
	}

}

}
