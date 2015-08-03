using UnityEngine;
using System.Collections;
using MathExtension;

// 각 액션에서 공통으로 사용하는 서브 액션.

namespace Character {

// ============================================================================ //
//																				//
//		워프.																	//
//																				//
// ============================================================================ //
public class WarpAction : ActionBase {

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		WARP_IN = 0,			// 사라집니다.
		WARP_OUT,				// 나타납니다.
		FINISH,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	public chrBehaviorPlayer	target_player;

	// ================================================================ //

	public override void	start()
	{
		this.is_finished = false;

		// 지금은 도깨비 전용.

		chrBehaviorEnemy_Obake	obake = this.behavior as chrBehaviorEnemy_Obake;

		if(obake != null) {

			this.step.set_next(STEP.WARP_IN);

		} else {

			this.step.set_next(STEP.FINISH);
		}
	}

	public override void	execute()
	{
		BasicAction	basic_action = this.behavior.basic_action;

		float	warp_in_time = 0.2f;
		float	warp_out_time = 0.2f;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크합니다.

		switch(this.step.do_transition()) {

			// 사라집니다.
			case STEP.WARP_IN:
			{
				if(this.step.get_time() > warp_in_time) {

					this.step.set_next(STEP.WARP_OUT);
				}
			}
			break;

			// 나타납니다.
			case STEP.WARP_OUT:
			{
				if(this.step.get_time() > warp_out_time) {

					this.step.set_next_delay(STEP.FINISH, 1.0f);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 바뀌었을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 나타납니다.
				case STEP.WARP_OUT:
				{
					if(this.target_player != null) {

						Vector3		v = this.control.getPosition() - this.target_player.control.getPosition();
	
						if(v.magnitude > 5.0f) {
	
							v *= 5.0f/v.magnitude;
						}
	
						v = Quaternion.AngleAxis(45.0f, Vector3.up)*v;
	
						basic_action.position_xz = this.target_player.control.getPosition() + v;
						basic_action.move_dir    = Mathf.Atan2(-v.x, -v.z)*Mathf.Rad2Deg;

					} else {

						Vector3		v = Quaternion.AngleAxis(this.control.getDirection() + Random.Range(-30.0f, 30.0f), Vector3.up)*Vector3.forward;

						v *= 5.0f;

						basic_action.position_xz = this.control.getPosition() + v;
						basic_action.move_dir    = Mathf.Atan2(v.x, v.z)*Mathf.Rad2Deg;
					}

					this.control.cmdSetDirection(basic_action.move_dir);

					// 착지합니다.
					// 상자에서 튀어나온 직후는 공중에서 워프하므로.
					basic_action.jump.forceFinish();
				}
				break;

				// 끝.
				case STEP.FINISH:
				{
					this.is_finished = true;
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 사라집니다.
			case STEP.WARP_IN:
			{
				float	rate = Mathf.Clamp01(this.step.get_time()/warp_in_time);

				rate = Mathf.Pow(rate, 0.25f);

				float	xz_scale = Mathf.Lerp(1.0f, 0.0f, rate);
				float	y_scale  = Mathf.Lerp(1.0f, 2.0f, rate);

				this.control.transform.localScale = new Vector3(xz_scale, y_scale, xz_scale);

				this.control.GetComponent<Rigidbody>().Sleep();
			}
			break;

			// 나타납니다.
			case STEP.WARP_OUT:
			{
				float	rate = Mathf.Clamp01(this.step.get_time()/warp_out_time);

				rate = Mathf.Pow(rate, 0.25f);

				float	xz_scale = Mathf.Lerp(0.0f, 1.0f, rate);
				float	y_scale  = Mathf.Lerp(2.0f, 1.0f, rate);

				this.control.transform.localScale = new Vector3(xz_scale, y_scale, xz_scale);

				this.control.GetComponent<Rigidbody>().Sleep();
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}
}
// ============================================================================ //
//																				//
//		발사.																	//
//																				//
// ============================================================================ //
public class ShootAction : ActionBase {

	// --------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		SHOOT = 0,			// 발사.
		FINISH,				// 끝.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //

	public override void	start()
	{
		this.is_finished = false;

		// 지금은 도깨비 전용.

		chrBehaviorEnemy_Obake	obake = this.behavior as chrBehaviorEnemy_Obake;

		if(obake != null) {

			this.step.set_next(STEP.SHOOT);

		} else {

			this.step.set_next(STEP.FINISH);
		}
	}

	public override void	execute()
	{
		BasicAction	basic_action = this.behavior.basic_action;

		chrBehaviorEnemy_Obake	obake = this.behavior as chrBehaviorEnemy_Obake;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크합니다.

		switch(this.step.do_transition()) {

			// 발사.
			case STEP.SHOOT:
			{
				if(this.behavior.is_attack_motion_finished) {

					this.step.set_next_delay(STEP.FINISH, 1.0f);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 바뀌었을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 발사.
				case STEP.SHOOT:
				{
					basic_action.setMoveMotionSpeed(0.0f);
					basic_action.animator.SetTrigger("Attack");
				}
				break;

				// 끝.
				case STEP.FINISH:
				{
					this.is_finished = true;
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 발사.
			case STEP.SHOOT:
			{
				if(this.behavior.is_attack_motion_impact) {

					obake.shootBullet();
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}
}
// ============================================================================ //
//																				//
//		근접공격.																//
//																				//
// ============================================================================ //
public class MeleeAttackAction : ActionBase {

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		ATTACK = 0,			// 공격 중.
		FINISH,				// 끝.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	public chrBehaviorPlayer	target_player = null;

	// ================================================================ //

	public override void	start()
	{
		this.is_finished = false;

		this.step.set_next(STEP.ATTACK);
	}

	public override void	execute()
	{
		BasicAction	basic_action = this.behavior.basic_action;

		if(this.target_player == null) {

			this.target_player = PartyControl.get().getLocalPlayer();
		}

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크합니다.

		switch(this.step.do_transition()) {

			// 공격 중.
			case STEP.ATTACK:
			{
				if(this.behavior.is_attack_motion_finished) {

					this.step.set_next_delay(STEP.FINISH, 1.0f);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 바뀌었을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 공격 중.
				case STEP.ATTACK:
				{
					basic_action.setMoveMotionSpeed(0.0f);
					basic_action.animator.SetTrigger("Attack");
				}
				break;

				// 끝.
				case STEP.FINISH:
				{
					this.is_finished = true;
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 공격 중.
			case STEP.ATTACK:
			{
				if(this.behavior.is_attack_motion_impact) {

					if(this.behavior.isInAttackRange(this.target_player.control)) {

						if(this.target_player.isLocal()) {

							this.target_player.control.causeDamage(this.behavior.control.vital.getAttackPower(), -1);

						} else {

							// 원격 플레이어에게는 대미지를 주지 않는다.
						}
					}
				}
				basic_action.move_dir = MathUtility.calcDirection(this.behavior.control.getPosition(), this.target_player.control.getPosition());
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}
}



}
