using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 비헤이비어   네트 플레이어용(게스트).
public class chrBehaviorFakeNet : chrBehaviorPlayer {

	private Vector3		move_target;			// 이동할 윛.
	private Vector3		heading_target;			// 방향.

	protected string	move_target_item = "";	// 아이템을 목표로 이동할 때.

	protected string	collision = "";

	public chrBehaviorLocal	local_player          = null;

	public	bool		in_formation = false;	// 로컬 플레이어와 함께 이동한다(디버그용).

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 통상 시.
		OUTER_CONTROL,		// 외부 제어.

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;


	// ================================================================ //

	// 로컬 플레이어?.
	public override bool	isLocal()
	{
		return(false);
	}

	// 외부에서의 컨트롤을 시작한다.
	public override void 	beginOuterControll()
	{
		this.next_step = STEP.OUTER_CONTROL;
	}

	// 외부로부터의 컨트롤러를 종료한다.
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

	// 컬리전에 히트하는 동안 호출되는 메소드.
	void 	OnCollisionStay(Collision other)
	{
		switch(other.gameObject.tag) {

			case "Item":
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

	// 트리거에 히트한 순간만 호출되는 메소드.
	void	OnTriggerEnter(Collider other)
	{
		this.on_trigger_common(other);
	}
	// 트리거에 히트하는 동안 호출되는 메소드.
	void	OnTriggerStay(Collider other)
	{
		this.on_trigger_common(other);
	}

	protected	void	on_trigger_common(Collider other)
	{
		switch(other.gameObject.tag) {

			case "Door":
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

	// ================================================================ //

	public override void	initialize()
	{
		base.initialize();

		this.move_target = this.transform.position;
	}
	public override void	start()
	{
		base.start();

		this.next_step = STEP.MOVE;
	}
	public override	void	execute()
	{
		base.execute();
		
		// ---------------------------------------------------------------- //
		// 스텝 내 경과 시간을 진행한다.
		
		this.step_timer += Time.deltaTime;
		
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.
		
		if(this.next_step == STEP.NONE) {
			
			switch(this.step) {
				
				case STEP.MOVE:
				{
				}
				break;
			}
		}
		
		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.
		
		while(this.next_step != STEP.NONE) {
			
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
					this.move_target = this.transform.position;
					this.heading_target = this.transform.TransformPoint(Vector3.forward);
				}
				break;
				
			}
			
			this.step_timer = 0.0f;
		}
		
		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.
		
		GameInput	gi = GameInput.getInstance();

		switch(this.step) {
			
			case STEP.MOVE:
			{
				if(this.in_formation) {
					
					chrBehaviorLocal	player = PartyControl.get().getLocalPlayer();
					
					Vector3		leader_position = player.transform.TransformPoint(this.position_in_formation);
					
					if(Vector3.Distance(this.control.getPosition(), leader_position) > 1.0f) {
						
						this.move_target    = leader_position;
						this.heading_target = this.move_target;
					}
				}
				
				this.exec_step_move();
			}
			break;
		}
		
		this.bullet_shooter.execute(gi.shot.current);
		
		this.collision = "";
		
		// ---------------------------------------------------------------- //
	}

	// ================================================================ //

	// STEP.MOVE 실행.
	// 이동.
	protected void	exec_step_move()
	{

		// ---------------------------------------------------------------- //
		// 이동(위치 좌표 보간).

		Vector3		position  = this.control.getPosition();
		float		cur_dir   = this.control.getDirection();

		Vector3		dist = this.move_target - position;

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

		// 방향 보간.

		float	tgt_dir;

		if(Vector3.Distance(this.heading_target, position) > 0.01f) {

			tgt_dir = Quaternion.LookRotation(this.heading_target - position).eulerAngles.y;

		} else {

			tgt_dir = cur_dir;
		}

		float	dir_diff = tgt_dir - cur_dir;

		if(dir_diff > 180.0f) {

			dir_diff = dir_diff - 360.0f;

		} else if(dir_diff < -180.0f) {

			dir_diff = dir_diff + 360.0f;
		}

		GameInput	gi = GameInput.getInstance();
		if(!gi.pointing.current && gi.shot.trigger_on) {

		} else {

			dir_diff *= 0.1f;
		}

		if(Mathf.Abs(dir_diff) < 1.0f) {

			cur_dir = tgt_dir;

		} else {

			cur_dir += dir_diff;
		}

		position.y = this.control.getPosition().y;

		this.control.cmdSetPosition(position);
		this.control.cmdSetDirection(cur_dir);

	}

	// ================================================================ //

}
