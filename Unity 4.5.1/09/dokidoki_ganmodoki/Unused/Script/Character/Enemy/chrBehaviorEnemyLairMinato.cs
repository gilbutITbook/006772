using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// 敵の巣（ジェネレーター）.
public class chrBehaviorEnemyLairMinato : chrBehaviorEnemy {
	public float		life = 5.0f;					// ヒットポイント.
	
	public bool			damage_trigger = false;
	
	// ---------------------------------------------------------------- //
	
	public enum STEP {
		
		NONE = -1,

		WAIT = 0,		// 通常
		SPAWN,				// スポーン
		VANISH,				// やられた.
		
		NUM,
	};
	
	public STEP			step      = STEP.NONE;		// 現在のステート
	public STEP			next_step = STEP.NONE;		// 次のステート
	public float		step_timer = 0.0f;			// 時間で移行するステートのタイマー
	public float		step_timer_prev = 0.0f;		// 前回のタイマー

	private chrControllerEnemyLairMinato myController; //< 前提とするコントローラクラスへのキャスト済参照の保持

	// ---------------------------------------------------------------- //
	// スペック
	// ---------------------------------------------------------------- //
	private float		SPAWN_INTERVAL = 2.0f;

	// ================================================================ //

	// このコントローラが前提とするコントローラクラスへの参照を返す
	private chrControllerEnemyLairMinato getMyController()
	{
		if (myController == null) {
			myController = this.control as chrControllerEnemyLairMinato;
		}
		return myController;
	}

	// ダメージ.
	// FIXME ダメージ処理がビヘイビアのほうにある. コントローラへ移すべきか？ とりあえずバイパスさせる
	public override void		causeDamage()
	{
		getMyController().causeDamage();
	}
	
	// やられたことにする.
	// FIXME 不変のふるまい（死亡演出および後の処理）なのでコントローラのほうにあるべき. とりあえずバイパスさせる
	public override void		causeVanish()
	{
		getMyController().causeVanish();
	}
	
	// やられたときに呼ばれる.
	public override void		onVanished()
	{
		this.next_step = STEP.VANISH;
	}

	// ================================================================ //
	
	public override void	initialize()
	{
	}
	public override void	start()
	{
		this.next_step = STEP.WAIT;
	}
	
	public override	void	execute()
	{
		if (isPaused) {
			return;
		}
		
		// ---------------------------------------------------------------- //
		// ステップ内の経過時間を進める.
		this.step_timer_prev = this.step_timer;
		this.step_timer += Time.deltaTime;
		
		// ---------------------------------------------------------------- //
		// 次の状態に移るかどうかを、チェックする.

		if(this.next_step == STEP.NONE) {
			switch(this.step) {
				
			case STEP.WAIT:
				if (step_timer >= SPAWN_INTERVAL)
				{
					this.next_step = STEP.SPAWN;
				}
				break;

			default:
				break;
			}
		}
		
		// ---------------------------------------------------------------- //
		// 状態が遷移したときの初期化.
		
		while(this.next_step != STEP.NONE) {
		
			this.step      = this.next_step;
			this.next_step = STEP.NONE;
			
			switch(this.step) {
			default:
				break;
			}
			
			this.step_timer_prev = 0.0f;
			this.step_timer = 0.0f;
		}
		
		// ---------------------------------------------------------------- //
		// 各状態での実行処理.

		switch (this.step) {
			case STEP.SPAWN:
				getMyController().createEnemy ();
				this.next_step = STEP.WAIT;
				break;
			default:
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
