using UnityEngine;
using System.Collections;

// ビヘイビアー　ボステスト用.
public class chrBehaviorBoss0 : chrBehaviorBase {


	private float	move_dir = 0.0f;					// 移動方向.


	//public float		damage_timer = 0.0f;			// ダメージ演出用タイマー.
	//private Renderer[]	renders = null;					// レンダー.
	//private Material[]	org_materials = null;			// もともとのモデルにアサインされていたマテリアル.

	//public static float	DAMAGE_FLUSH_TIME = 0.1f;		// ダメージを受けたときに、白くフラッシュする時間.

	private float		life = 50.0f;					// ヒットポイント.

	private bool			trigger_damage = false;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 動く.
		REST,				// 止まってる.
		VANISH,				// やられた.

		NUM,
	};

	private STEP			step      = STEP.NONE;
	private STEP			next_step = STEP.NONE;
	private float		step_timer = 0.0f;

	// ================================================================ //
	// MonoBehaviour からの継承.

	void	Start()
	{
	}
	
	void	Update()
	{
	}
#if false
	public bool		damage_trigger = false;
#endif
	void	OnTriggerEnter(Collider other)
	{
		if(other.gameObject.layer == LayerMask.NameToLayer("Player Bullet")) {

			this.causeDamage();
		}
	}

	// ================================================================ //

	// ダメージ.
	public void		causeDamage()
	{
		switch(this.step) {

			case STEP.MOVE:
			case STEP.REST:
			{
				this.trigger_damage = true;
			}
			break;
		}
	}

	// やられたことにする.
	public void		causeVanish()
	{
		this.next_step = STEP.VANISH;
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

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //

		if(this.trigger_damage) {

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
					if(this.step_timer > 1.0f) {

						this.next_step = STEP.REST;
					}
				}
				break;

				case STEP.REST:
				{
					if(this.step_timer > 3.0f) {

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
				}
				break;

				case STEP.VANISH:
				{
					// アニメーションとめる.
					Animation[] animations = this.gameObject.GetComponentsInChildren<Animation>();

					foreach(var animation in animations) {

						animation.Stop();
					}

					this.control.damage_effect.startVanish();
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
				chrController	player = CharacterRoot.getInstance().getPlayer();

				if(player != null) {

					var		dir = Quaternion.LookRotation(player.transform.position - this.transform.position);

					this.move_dir =  dir.eulerAngles.y;

				} else {

					float	turn = Random.Range(-90.0f, 90.0f);

					this.move_dir = this.control.getDirection() + turn;
				}

				this.exec_step_move();
			}
			break;

			case STEP.VANISH:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //

		this.trigger_damage = false;
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

		dir_diff *= 0.03f;

		if(Mathf.Abs(dir_diff) < 0.1f) {

			cur_dir = this.move_dir;

		} else {

			cur_dir += dir_diff;
		}

		position.y = this.control.getPosition().y;

		this.control.cmdSetPosition(position);
		this.control.cmdSetDirection(cur_dir);
	}
}
