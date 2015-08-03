using UnityEngine;
using System.Collections;
using MathExtension;
using GameObjectExtension;

namespace Character {

// ============================================================================ //
//																				//
//		// JUMBO,			// 점보		.										//
//																				//
// ============================================================================ //
public class JumboAction : ActionBase {

	protected MeleeAttackAction		melee_attack;

	protected ipModule.Spring	spring;				// 스케일 제어용 스프링.
	protected float				scale = 1.0f;		// 스케일.

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 걷는 중.
		REST,				// 멈춰있는 중.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //

	public override void		create(chrBehaviorEnemy behavior, ActionBase.DescBase desc_base = null)
	{
		base.create(behavior, desc_base);

		this.melee_attack = new MeleeAttackAction();
		this.melee_attack.create(this.behavior);

		this.spring = new ipModule.Spring();
		this.spring.k      = 300.0f;
		this.spring.reduce = 0.90f;

		// 스폰 → 착지 시에 바운드하지 않게 합니다.
		this.behavior.basic_action.jump.bounciness.y = 0.0f;
	}

	public override void	start()
	{
		this.spring.start(-1.0f);

		this.control.vital.setHitPoint(chrBehaviorPlayer.MELEE_ATTACK_POWER*2.5f);
		this.control.vital.setAttackPower(chrBehaviorEnemy_Kumasan.ATTACK_POWER*3.0f);

		this.step.set_next(STEP.REST);
	}

	public override void	execute()
	{
		chrBehaviorEnemy	mine = this.behavior;
		BasicAction			basic_action = mine.basic_action;

		if(this.finish_child()) {

			this.step.set_next(STEP.MOVE);
		}

		chrBehaviorPlayer	target_player = this.behavior.selectTargetPlayer(float.MaxValue, float.MaxValue);

		this.melee_attack.target_player = target_player;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동하는지 체크합니다.

		switch(this.step.do_transition()) {

			// 걷는 중.
			case STEP.MOVE:
			{
				do {

					if(target_player == null) {
	
						break;
					}
					if(!this.behavior.isInAttackRange(target_player.control)) {

						break;
					}

					//

					this.push(this.melee_attack);
					this.step.sleep();

				} while(false);
			}
			break;

			// 멈춰 있다.
			case STEP.REST:
			{
				if(this.step.get_time() > 1.0f) {

					this.step.set_next(STEP.MOVE);
				}

				do {

					if(target_player == null) {
	
						break;
					}
					if(!this.behavior.isInAttackRange(target_player.control)) {

						break;
					}

					//

					this.push(this.melee_attack);
					this.step.sleep();

				} while(false);
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 걷는 중.
				case STEP.MOVE:
				{
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 걷는 중.
			case STEP.MOVE:
			{
				do {

					if(target_player == null) {

						break;
					}

					basic_action.move_dir = MathUtility.calcDirection(mine.control.getPosition(), target_player.control.getPosition());
	
					basic_action.setMoveMotionSpeed(1.0f);
					basic_action.setMoveSpeed(0.6f);

					basic_action.setMotionPlaySpeed(0.6f);

					basic_action.executeMove();

				} while(false);
			}
			break;

			// 멈춰있다.
			case STEP.REST:
			{
				basic_action.setMoveMotionSpeed(0.0f);
			}
			break;
		}

		this.spring.execute(Time.deltaTime);

		if(this.spring.isMoving()) {

			this.scale = Mathf.InverseLerp(-1.0f, 1.0f, this.spring.position);
			this.scale = Mathf.Lerp(1.0f, 2.0f, this.scale);
		}

		this.control.transform.localScale = Vector3.one*this.scale;

		// ---------------------------------------------------------------- //

		this.execute_child();

		if(this.child == this.melee_attack) {

			this.attack_motion_speed_control();
		}

	}

	// 공격 모션 속도.
	protected void	attack_motion_speed_control()
	{
		chrBehaviorEnemy	mine = this.behavior;
		BasicAction			basic_action = mine.basic_action;

		if(this.melee_attack.step.get_current() == MeleeAttackAction.STEP.ATTACK) {

			float	current_time = this.melee_attack.step.get_time();
			float	furikaburi_time = 0.38f + 0.3f;

			float	play_speed = 0.5f;

			if(current_time < furikaburi_time) {

				// 머리 위로 높이 쳐들 때까지 서서히 느리게.

				float	rate = Mathf.Clamp01(Mathf.InverseLerp(0.0f, furikaburi_time, current_time));

				play_speed = Mathf.Lerp(0.3f, 0.1f, rate);

			} else {

				// 내리 휘두를 때가지 단숨에 빨라진다.

				float	rate = Mathf.Clamp01(Mathf.InverseLerp(furikaburi_time, furikaburi_time + 0.3f, current_time));

				play_speed = Mathf.Lerp(0.1f, 0.7f, rate);
			}

			basic_action.setMotionPlaySpeed(play_speed);
		}
	}
}
// ============================================================================ //
//																				//
//		TOTUGEKI,			// 플레이어에게 다가가 근접공격.					//
//																				//
// ============================================================================ //
public class TotugekiAction : ActionBase {

	protected MeleeAttackAction		melee_attack;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 걷는 중.
		REST,				// 멈춤.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //

	public override void		create(chrBehaviorEnemy behavior, ActionBase.DescBase desc_base = null)
	{
		base.create(behavior, desc_base);

		this.melee_attack = new MeleeAttackAction();
		this.melee_attack.create(this.behavior);
	}

	public override void	start()
	{
		this.step.set_next(STEP.REST);
	}

	public override void	execute()
	{
		chrBehaviorEnemy	mine = this.behavior;
		BasicAction			basic_action = mine.basic_action;

		if(this.finish_child()) {

			this.step.set_next(STEP.MOVE);
		}

		chrBehaviorPlayer	target_player = this.behavior.selectTargetPlayer(float.MaxValue, float.MaxValue);

		this.melee_attack.target_player = target_player;

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크합니다.

		switch(this.step.do_transition()) {

			// 걷는 중.
			case STEP.MOVE:
			{
				do {

					if(target_player == null) {
	
						break;
					}
					if(!this.behavior.isInAttackRange(target_player.control)) {

						break;
					}

					//

					this.push(this.melee_attack);
					this.step.sleep();

				} while(false);
			}
			break;

			// 멈춤.
			case STEP.REST:
			{
				if(this.step.get_time() > 1.0f) {

					this.step.set_next(STEP.MOVE);
				}

				do {

					if(target_player == null) {
	
						break;
					}
					if(!this.behavior.isInAttackRange(target_player.control)) {

						break;
					}

					//

					this.push(this.melee_attack);
					this.step.sleep();

				} while(false);
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전한됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 걷는 중.
				case STEP.MOVE:
				{
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 걷는 중.
			case STEP.MOVE:
			{
				do {

					if(target_player == null) {

						break;
					}

					basic_action.move_dir = MathUtility.calcDirection(mine.control.getPosition(), target_player.control.getPosition());
	
					basic_action.executeMove();
	
					basic_action.setMoveMotionSpeed(1.0f);

				} while(false);
			}
			break;

			// 멈춤.
			case STEP.REST:
			{
				basic_action.setMoveMotionSpeed(0.0f);
			}
			break;
		}

		// ---------------------------------------------------------------- //

		this.execute_child();
	}
}
// ============================================================================ //
//																				//
//		WARP_DE_FIRE,		// 워프를 반복합니다.								//
//																				//
// ============================================================================ //
public class WarpDeFireAction : ActionBase {

	protected ShootAction		shoot;
	protected WarpAction		warp;

	protected chrBehaviorPlayer	target_player;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		READY = 0,			// 준비.
		WARP,				// 워프.
		TURN,				// 목표 방향으로 선회.
		SHOT,				// 탄환을 쏨.
		FINISH,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	protected STEP	resume_step = STEP.READY;

	// ================================================================ //

	public override void	start()
	{
		this.is_finished = false;

		this.step.set_next(STEP.READY);

		this.shoot = new ShootAction();
		this.shoot.create(this.behavior);

		this.warp = new WarpAction();
		this.warp.create(this.behavior);
	}

	// 매 프레임 실행.
	public override void	execute()
	{
		chrBehaviorEnemy	mine = this.behavior;
		BasicAction			basic_action = mine.basic_action;

		float	distance_limit = 10.0f;
		float	angle_limit    = 180.0f;

		if(this.finish_child()) {

			this.step.set_next(this.resume_step);
		}

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크합니다.

		switch(this.step.do_transition()) {

			// 준비.
			case STEP.READY:
			{
				// 공격 가능 범위에서 가장 가까이 정면에 있는 플레이어를.
				// 찾습니다.
				this.target_player = this.behavior.selectTargetPlayer(distance_limit, angle_limit);

				this.step.set_next(STEP.WARP);
			}
			break;

			// 목표 방향으로 선회.
			case STEP.TURN:
			{
				if(this.target_player == null) {

					this.step.set_next(STEP.READY);

				} else {

					basic_action.move_dir = MathUtility.calcDirection(mine.control.getPosition(), this.target_player.control.getPosition());
	
					float	dir_diff = MathUtility.snormDegree(basic_action.move_dir - mine.control.getDirection());
	
					if(Mathf.Abs(dir_diff) < 5.0f) {
	
						this.step.set_next(STEP.SHOT);
					}
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환했을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// ワープ.
				case STEP.WARP:
				{
					this.warp.target_player = this.target_player;
					this.resume_step = STEP.TURN;
					this.push(this.warp);
					this.step.sleep();
				}
				break;

				// 탄환을 쏩니다.
				case STEP.SHOT:
				{
					this.resume_step = STEP.READY;
					this.push(this.shoot);
					this.step.sleep();
				}
				break;

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

			// 목표 방향으로 선회.
			case STEP.TURN:
			{
				if(this.target_player != null) {

					basic_action.move_dir = MathUtility.calcDirection(mine.control.getPosition(), this.target_player.control.getPosition());
					basic_action.turn_rate = 0.5f;
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //

		this.execute_child();
	}

	// 부모가 실행 중에도 실행.
	public override void	stealth()
	{
		chrBehaviorEnemy	mine = this.behavior;
		BasicAction			basic_action = mine.basic_action;

		switch(basic_action.step.get_current()) {

			case BasicAction.STEP.SPAWN:
			{
				if(basic_action.jump.velocity.y < 0.0f) {

					basic_action.step.set_next(BasicAction.STEP.UNIQUE);
				}
			}
			break;
		}
	}

}
// ============================================================================ //
//																				//
//		SONOBA_DE_FIRE,		//	그 자리에서 쏜다.								//
//																				//
// ============================================================================ //
public class SonobaDeFireAction : ActionBase {

	protected ShootAction		shoot;

	protected chrBehaviorPlayer	target_player;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		READY = 0,			// 준비.
		TURN,				// 목표 방향으로 선회.
		FINISH,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //

	public override void	start()
	{
		this.is_finished = false;

		this.step.set_next(STEP.READY);

		this.shoot = new ShootAction();
		this.shoot.create(this.behavior);
	}


	public override void	execute()
	{
		chrBehaviorEnemy	mine = this.behavior;
		BasicAction			basic_action = mine.basic_action;

		float	distance_limit = 10.0f;
		float	angle_limit    = 45.0f;

		if(this.finish_child()) {

			this.step.set_next(STEP.READY);
		}

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크합니다.

		switch(this.step.do_transition()) {

			case STEP.READY:
			{
				// 공격 가능 범위에서 가장 가까이 정면에 있는 플레이어를.
				// 찾습니다.
				this.target_player = this.behavior.selectTargetPlayer(distance_limit, angle_limit);

				if(target_player != null) {

					this.step.set_next(STEP.TURN);
				}
			}
			break;

			// 목표 방향으로 선회.
			case STEP.TURN:
			{
				if(this.target_player == null) {

					this.step.set_next(STEP.READY);

				} else {

					basic_action.move_dir = MathUtility.calcDirection(mine.control.getPosition(), this.target_player.control.getPosition());
	
					float	dir_diff = MathUtility.snormDegree(basic_action.move_dir - mine.control.getDirection());
	
					if(Mathf.Abs(dir_diff) < 5.0f) {
	
						this.push(this.shoot);
						this.step.sleep();
					}
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 목표 방향으로 선회.
				case STEP.TURN:
				{
				}
				break;

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

			// 목표 방향으로 선회.
			case STEP.TURN:
			{
				basic_action.move_dir = MathUtility.calcDirection(mine.control.getPosition(), this.target_player.control.getPosition());
			}
			break;
		}

		// ---------------------------------------------------------------- //

		this.execute_child();
	}
}
// ============================================================================ //
//																				//
//		OUFUKU = 0,		// ２ 곳을 왕복합니다									//
//																				//
// ============================================================================ //
public class OufukuAction : ActionBase {

	protected MeleeAttackAction		melee_attack;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 걷는 중.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	// BEHAVE_KIND의 Action을 만들 때의 옵션.
	public class Desc : ActionBase.DescBase {
			
		public Desc() {}
		public Desc(Vector3 center)
		{
			this.position0 = center + Vector3.right;
			this.position1 = center - Vector3.right;
		}

		public Vector3	position0;
		public Vector3	position1;
	}

	protected Vector3[]	positions = new Vector3[2];
	protected int		next_position;

	// ================================================================ //

	public override void		create(chrBehaviorEnemy behavior, ActionBase.DescBase desc_base = null)
	{
		base.create(behavior, desc_base);

		Desc	desc = desc_base as Desc;

		if(desc == null) {

			desc = new Desc(this.control.getPosition());
		}

		this.positions[0]  = desc.position0;
		this.positions[1]  = desc.position1;

		this.melee_attack = new MeleeAttackAction();
		this.melee_attack.create(this.behavior);
	}

	public override void	start()
	{
		this.step.set_next(STEP.MOVE);
	}

	public override void	execute()
	{
		BasicAction	basic_action = this.behavior.basic_action;

		if(this.finish_child()) {

			this.step.set_next(STEP.MOVE);
		}

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크합니다.

		switch(this.step.do_transition()) {

			// 걷는 중.
			case STEP.MOVE:
			{
				chrBehaviorLocal	player = PartyControl.get().getLocalPlayer();

				if(this.behavior.isInAttackRange(player.control)) {

					this.push(this.melee_attack);
					this.step.sleep();
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 걷는 중.
				case STEP.MOVE:
				{
					this.next_position = 0;

					Vector3		move_vector = this.positions[this.next_position] - this.control.getPosition();

					basic_action.move_dir = Mathf.Atan2(move_vector.x, move_vector.z)*Mathf.Rad2Deg;
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 걷는 중.
			case STEP.MOVE:
			{
				Vector3		tgt_vector0 = this.positions[0] - this.control.getPosition();
				Vector3		tgt_vector1 = this.positions[1] - this.control.getPosition();
				Vector3		move_vector = this.control.getMoveVector();

				float	dp0 = Vector3.Dot(tgt_vector0, move_vector);
				float	dp1 = Vector3.Dot(tgt_vector1, move_vector);

				if(dp0 > 0.0f && dp1 > 0.0f) {

					if(tgt_vector0.sqrMagnitude < tgt_vector1.sqrMagnitude) {

						this.next_position = 0;

					} else {

						this.next_position = 1;
					}

				} else if(dp0 < 0.0f && dp1 < 0.0f) {

					if(tgt_vector0.sqrMagnitude > tgt_vector1.sqrMagnitude) {

						this.next_position = 0;

					} else {

						this.next_position = 1;
					}
				}

				Vector3		tgt_vector = this.positions[this.next_position] - this.control.getPosition();

				basic_action.move_dir = Mathf.Atan2(tgt_vector.x, tgt_vector.z)*Mathf.Rad2Deg;

				basic_action.executeMove();

				basic_action.setMoveMotionSpeed(1.0f);
			}
			break;
		}

		// ---------------------------------------------------------------- //

		this.execute_child();
	}
}
// ============================================================================ //
//																				//
//		GOROGORO,				// 뒹굴뒹굴 굴러서 벽에서 반사 					//
//																				//
// ============================================================================ //
public class GoroGoroAction : ActionBase {

	protected MeleeAttackAction		melee_attack;

	protected GameObject	model;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 걷는 중.
		REST,				// 멈춤.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //

	public override void		create(chrBehaviorEnemy behavior, ActionBase.DescBase desc_base = null)
	{
		base.create(behavior, desc_base);

		this.melee_attack = new MeleeAttackAction();
		this.melee_attack.create(this.behavior);

		this.model = this.behavior.gameObject.findChildGameObject("model");
	}

	public override void	start()
	{
		this.step.set_next(STEP.REST);
	}

	public override void	execute()
	{
		BasicAction	basic_action = this.behavior.basic_action;

		if(this.finish_child()) {

			this.step.set_next(STEP.MOVE);
		}

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크합니다.

		switch(this.step.do_transition()) {

			// 걷는 중.
			case STEP.MOVE:
			{
			}
			break;

			// 멈춤.
			case STEP.REST:
			{
				if(this.step.get_time() > 1.0f) {

					this.step.set_next(STEP.MOVE);
				}

				chrBehaviorLocal	player = PartyControl.get().getLocalPlayer();

				if(this.behavior.isInAttackRange(player.control)) {

					this.push(this.melee_attack);
					this.step.sleep();
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 걷는 중.
				case STEP.MOVE:
				{
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 걷는 중.
			case STEP.MOVE:
			{
				var coli = basic_action.control.collision_results.Find(x => x.object1.tag == "Wall" || x.object1.tag == "Player");

				// 벽(또는 플레이어)에 부딪히면 반사해서 방향을 바꿉니다.
				do {

					if(coli == null) {

						break;
					}
					if(coli.option0 == null) {

						break;
					}
					ContactPoint	cp = (ContactPoint)coli.option0;
					
					// 벽의 법선 방향을 90도 단위로 합니다.

					Vector3		normal = cp.normal;

					float	normal_angle = Mathf.Atan2(normal.x, normal.z)*Mathf.Rad2Deg;

					normal_angle = MathUtility.unormDegree(normal_angle);
					normal_angle = Mathf.Round(normal_angle/90.0f)*90.0f;

					normal = Quaternion.AngleAxis(normal_angle, Vector3.up)*Vector3.forward;

					Vector3		v = basic_action.getMoveVector();

					if(Vector3.Dot(v, normal) >= 0.0f) {

						break;
					}

					v -= 2.0f*Vector3.Dot(v, normal)*normal;
					basic_action.setMoveDirByVector(v);

					// 플레이어에게 부딪혔으면 대미지를 줍니다.
					do {

						if(coli.object1.tag != "Player") {
	
							break;
						}
	
						chrController	chr = coli.object1.GetComponent<chrController>();
	
						if(chr == null) {
	
							break;
						}
						if(!(chr.behavior is chrBehaviorLocal)) {
	
							break;
						}
	
						chr.causeDamage(this.control.vital.getAttackPower(), -1);
	
					} while(false);

				} while(false);

				basic_action.setMoveSpeed(4.0f);
				basic_action.setMoveMotionSpeed(0.0f);

				basic_action.executeMove();

				//

				this.model.transform.Rotate(new Vector3(7.0f, 0.0f, 0.0f));

			}
			break;

			// 멈춤.
			case STEP.REST:
			{
				basic_action.setMoveMotionSpeed(0.0f);
			}
			break;
		}

		// ---------------------------------------------------------------- //

		this.execute_child();
	}
}
// ============================================================================ //
//																				//
//		UROURO,				// 멈춤→걷기.										//
//																				//
// ============================================================================ //
public class UroUroAction : ActionBase {

	protected MeleeAttackAction		melee_attack;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 걷는 중.
		REST,				// 멈춤.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //

	public override void		create(chrBehaviorEnemy behavior, ActionBase.DescBase desc_base = null)
	{
		base.create(behavior, desc_base);

		this.melee_attack = new MeleeAttackAction();
		this.melee_attack.create(this.behavior);
	}

	public override void	start()
	{
		this.step.set_next(STEP.REST);
	}

	public override void	execute()
	{
		BasicAction	basic_action = this.behavior.basic_action;

		if(this.finish_child()) {

			this.step.set_next(STEP.MOVE);
		}

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크합니다.

		switch(this.step.do_transition()) {

			// 걷는 중.
			case STEP.MOVE:
			{
				if(this.step.get_time() > 1.0f) {

					this.step.set_next(STEP.REST);
				}

				chrBehaviorLocal	player = PartyControl.get().getLocalPlayer();

				if(this.behavior.isInAttackRange(player.control)) {

					this.push(this.melee_attack);
					this.step.sleep();
				}
			}
			break;

			// 멈춤.
			case STEP.REST:
			{
				if(this.step.get_time() > 1.0f) {

					this.step.set_next(STEP.MOVE);
				}

				chrBehaviorLocal	player = PartyControl.get().getLocalPlayer();

				if(this.behavior.isInAttackRange(player.control)) {

					this.push(this.melee_attack);
					this.step.sleep();
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 걷기 중.
				case STEP.MOVE:
				{
					basic_action.move_dir += Random.Range(-90.0f, 90.0f);

					if(basic_action.move_dir > 180.0f) {

						basic_action.move_dir -= 360.0f;

					} else if(basic_action.move_dir < -180.0f) {

						basic_action.move_dir += 360.0f;
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 걷기 중.
			case STEP.MOVE:
			{
				basic_action.executeMove();

				basic_action.setMoveMotionSpeed(1.0f);
			}
			break;

			// 멈춤.
			case STEP.REST:
			{
				basic_action.setMoveMotionSpeed(0.0f);
			}
			break;
		}

		// ---------------------------------------------------------------- //

		this.execute_child();
	}
}
// ============================================================================ //
//																				//
//		BOTTACHI = 0,		// 서있을 뿐. 디버그용								//
//																				//
// ============================================================================ //
public class BottachiAction : ActionBase {

	protected MeleeAttackAction		melee_attack;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		BOTTACHI = 0,			// 서있기.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //

	public override void		create(chrBehaviorEnemy behavior, ActionBase.DescBase desc_base = null)
	{
		base.create(behavior, desc_base);

		this.melee_attack = new MeleeAttackAction();
		this.melee_attack.create(this.behavior);
	}

	public override void	start()
	{
		this.step.set_next(STEP.BOTTACHI);
	}

	public override void	execute()
	{
		if(this.finish_child()) {

			this.step.set_next(STEP.BOTTACHI);
		}

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크합니다.

		switch(this.step.do_transition()) {

			// 서있기.
			case STEP.BOTTACHI:
			{
				chrBehaviorLocal	player = PartyControl.get().getLocalPlayer();

				if(this.behavior.isInAttackRange(player.control)) {

					this.push(this.melee_attack);
					this.step.sleep();
				}
			}
			break;

		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.BOTTACHI:
				{
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.BOTTACHI:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //

		this.execute_child();
	}
}

}
