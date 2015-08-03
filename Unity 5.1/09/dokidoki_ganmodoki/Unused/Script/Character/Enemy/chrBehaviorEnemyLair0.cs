using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// 敵の巣（ジェネレーター）.
public class chrBehaviorEnemyLair0 : chrBehaviorBase {

	public float		move_dir = 0.0f;				// 移動方向.


	public float		life = 5.0f;					// ヒットポイント.

	public bool			damage_trigger = false;

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
	public float		step_timer_prev = 0.0f;

	// ================================================================ //
	// MonoBehaviour からの継承.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// ダメージ.
	public void		causeDamage()
	{
		switch(this.step) {

			case STEP.MOVE:
			case STEP.REST:
			{
				this.damage_trigger = true;
			}
			break;
		}
	}

	// やられたことにする.
	public void		causeVanish()
	{
		this.next_step = STEP.VANISH;
	}

	public void		createEnemy()
	{
		chrController	enemy = EnemyRoot.getInstance().createEnemy();

		enemy.transform.position = this.transform.position;
	}

	// ================================================================ //

	public override void	initialize()
	{
	}
	public override void	start()
	{
		this.next_step = STEP.MOVE;
	}

	public override	void	execute()
	{

		// ---------------------------------------------------------------- //
		// ステップ内の経過時間を進める.

		this.step_timer_prev = this.step_timer;
		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //

		if(this.damage_trigger) {

			this.control.damage_effect.startDamage();

			this.life -= 1.0f;

			if(this.life <= 0.0f) {

				this.next_step = STEP.VANISH;
			}
		}

		// ---------------------------------------------------------------- //
		// 次の状態に移るかどうかを、チェックする.

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.MOVE:
				{
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
				}
				break;

				case STEP.VANISH:
				{
					this.control.damage_effect.startVanish();
				}
				break;
			}

			this.step_timer_prev = 0.0f;
			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 各状態での実行処理.

		switch(this.step) {

			case STEP.MOVE:
			{
				if(this.step_timer > 0.0f) {

					if(Mathf.FloorToInt(this.step_timer_prev/5.0f) != Mathf.FloorToInt(this.step_timer/5.0f)) {

						this.createEnemy();
					}
				}
			}
			break;

		}

		// ---------------------------------------------------------------- //

		this.damage_trigger = false;
	}

	public override void		onDamaged()
	{
		this.causeDamage();
	}
}
