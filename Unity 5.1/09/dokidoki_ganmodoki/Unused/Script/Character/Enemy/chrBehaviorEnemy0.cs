using UnityEngine;
using System.Collections;

// ビヘイビアー　敵テスト用.
public class chrBehaviorEnemy0 : chrBehaviorEnemy {

	public float	move_dir = 0.0f;					// 移動方向.

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 動く.
		REST,				// 止まってる.
		VANISH,				// やられた.

		NUM,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	// ================================================================ //
	// MonoBehaviour からの継承.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// やられたことにする.
	public override void		causeVanish()
	{
		this.next_step = STEP.VANISH;
	}

	// ================================================================ //

	public override void	initialize()
	{
		base.initialize();

	}
	public override void	start()
	{
		this.control.vital.hit_point = 5.0f;

		this.next_step = STEP.MOVE;
	}

	public override	void	execute()
	{

		// ---------------------------------------------------------------- //
		// ステップ内の経過時間を進める.

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //

		if(this.control.trigger_damage) {

			this.control.damage_effect.startDamage();

			if(this.control.vital.hit_point <= 0.0f) {

				this.next_step = STEP.VANISH;
			}
		}

		// ---------------------------------------------------------------- //
		// 次の状態に移るかどうかを、チェックする.

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.MOVE:
				{
					if(this.step_timer > 1.0f) {

						this.next_step = STEP.REST;
					}
				}
				break;

				case STEP.REST:
				{
					if(this.step_timer > 1.0f) {

						this.next_step = STEP.MOVE;
					}
				}
				break;

				case STEP.VANISH:
				{
					if(this.control.damage_effect.isDone()) {

						EnemyRoot.getInstance().deleteEnemy(this.control);
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 状態が遷移したときの初期化.

		while(this.next_step != STEP.NONE) {

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			switch(this.step) {
	
				case STEP.MOVE:
				{
					float	turn = Random.Range(-90.0f, 90.0f);

					this.move_dir = this.control.getDirection() + turn;

					this.control.cmdSetMotion("m001_walk", 0);
				}
				break;

				case STEP.REST:
				{
					float	turn = Random.Range(-90.0f, 90.0f);

					this.move_dir = this.control.getDirection() + turn;

					this.control.cmdSetMotion("m002_idle", 0);
				}
				break;

				case STEP.VANISH:
				{
					this.control.cmdBeginVanish();
				}
				break;
			}

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 各状態での実行処理.

		switch(this.step) {

			case STEP.MOVE:
			{
				this.exec_step_move();
			}
			break;

			case STEP.VANISH:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //

	}

	// STEP.MOVE の実行.
	// 移動.
	protected void	exec_step_move()
	{
		// ---------------------------------------------------------------- //
		// 移動（位置座標の補間）.

		Vector3		position  = this.control.getPosition();
		float		cur_dir   = this.control.getDirection();

		float		speed = 2.0f;
		float		speed_per_frame = speed*Time.deltaTime;

		Vector3		move_vector = Quaternion.AngleAxis(this.move_dir, Vector3.up)*Vector3.forward;

		position += move_vector*speed_per_frame;

		// 向きの補間.

		float	dir_diff = this.move_dir - cur_dir;

		if(dir_diff > 180.0f) {

			dir_diff = dir_diff - 360.0f;

		} else if(dir_diff < -180.0f) {

			dir_diff = dir_diff + 360.0f;
		}

		dir_diff *= 0.1f;

		if(Mathf.Abs(dir_diff) < 1.0f) {

			cur_dir = this.move_dir;

		} else {

			cur_dir += dir_diff;
		}

		position.y = this.control.getPosition().y;

		this.control.cmdSetPosition(position);
		this.control.cmdSetDirection(cur_dir);
	}

	// 自分が近接攻撃.
	public override void		onMeleeAttack(chrBehaviorPlayer player)
	{
		// プレイヤーと接触したら消滅する（テスト用）.
		this.causeVanish();
	}
}
