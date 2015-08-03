using UnityEngine;
using System.Collections;

// 소환수  개.
public class chrBehaviorBeast_Dog : chrBehaviorBase {

	public Vector3		position_in_formation = Vector3.zero;

	private Vector3		move_target;			// 이동할 위치.
	private Vector3		heading_target;			// 향할 곳.

	//protected string	move_target_item = "";	// 아이템을 목표로 이동할 때.

	protected string	collision = "";

	//public chrBehaviorLocal	local_player          = null;

	public	bool		in_formation = true;	// 로컬 플레이어와 함께 이동한다(디버그용).

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,		// 이동.
		STOP,			// 정지.

		NUM,
	};
	public	Step<STEP>	step = new Step<STEP>(STEP.NONE);

	//public STEP			step      = STEP.NONE;
	//public STEP			next_step = STEP.NONE;
	//public float		step_timer = 0.0f;

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

	// ================================================================ //

	public override void	initialize()
	{

		base.initialize();

		this.move_target = this.transform.position;
	}
	public override void	start()
	{
		base.start();

		this.step.set_next(STEP.STOP);
	}
	public override	void	execute()
	{
		base.execute();

		float	stop_to_move = 5.0f;
		float	move_to_stop = 3.0f;

		chrBehaviorLocal	player = PartyControl.get().getLocalPlayer();

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.

		switch(this.step.do_transition()) {

			case STEP.STOP:
			{
				Vector3		ditance_vector = player.control.getPosition() - this.control.getPosition();

				ditance_vector.y = 0.0f;

				if(ditance_vector.magnitude >= stop_to_move) {

					this.step.set_next(STEP.MOVE);
				}
			}
			break;

			case STEP.MOVE:
			{
				Vector3		ditance_vector = player.control.getPosition() - this.control.getPosition();

				ditance_vector.y = 0.0f;

				if(ditance_vector.magnitude <= move_to_stop) {

					this.step.set_next(STEP.STOP);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.MOVE:
				{
					this.move_target    = player.control.getPosition();
					this.heading_target = this.move_target;
				}
				break;

			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.MOVE:
			{
				this.move_target    = player.control.getPosition();
				this.heading_target = this.move_target;

				this.exec_step_move();
			}
			break;
		}

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

		// 방향 보간

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

		//if(!gi.pointing.current && gi.shot.trigger_on) {
		
		//} else {

			dir_diff *= 0.1f;
		//}

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
