using UnityEngine;
using System.Collections;

// ビヘイビアー　敵　初代ガントレット再現用.
public class chrBehaviorEnemyMinato : chrBehaviorEnemy {
	
	
	public float	move_dir = 0.0f;					// 移動方向.
	
	
//	private Renderer[]	renders = null;					// レンダー.
//	private Material[]	org_materials = null;			// もともとのモデルにアサインされていたマテリアル.
	
	
	public float		life = 5.0f;					// ヒットポイント.
	
	public bool			trigger_damage = false;
	
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

	// ---------------------------------------------------------------- //
	private GameObject	focus;							// 狙ってる対象

	// ---------------------------------------------------------------- //
	// キャラクタースペック
	// ---------------------------------------------------------------- //
	private float		REST_TIME = 0.0f;
	
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
	public override void		causeDamage()
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
	public override void		causeVanish()
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

		// この敵が狙うプレイヤーを決定する
		// ひとまず一番最初に決まったプレイヤーにする
		focus = GameObject.FindGameObjectWithTag ("Player");
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
				if(this.step_timer >= REST_TIME) {
					
					this.next_step = STEP.MOVE;
				}
			}
				break;
				
			case STEP.VANISH:
			{
				if(this.control.damage_effect.isVacant()) {
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
				float turn;
				// FIXME controll からマトリクス(transform)を取得する関数かなんか作らないといけなさそう
				Vector3 focal_local_pos = this.control.transform.InverseTransformPoint(focus.transform.position);
				focal_local_pos.y = 0.0f; // 高さはみない

				//float	turn = Random.Range(-90.0f, 90.0f);
				if (Vector3.Dot(Vector3.right, focal_local_pos) > 0)
				{
					turn = Vector3.Angle(focal_local_pos, Vector3.forward);
				}
				else
				{
					turn = -Vector3.Angle(focal_local_pos, Vector3.forward);
				}
				turn = Mathf.Clamp(turn, -90, 90);

				// move_dir に回転の角度を入れる
				this.move_dir = this.control.getDirection() + turn;
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
		
		dir_diff *= 0.1f;
		
		if(Mathf.Abs(dir_diff) < 1.0f) {
			
			cur_dir = this.move_dir;
			
		} else {
			
			cur_dir += dir_diff;
		}

		// position.y = this.controll.getPosition().y;
		// FIXME 動的なフロアコンタクトにするべき
		position.y = 0.48f;		

		this.control.cmdSetPosition(position);
		this.control.cmdSetDirection(cur_dir);
	}
	
	public override void		onDamaged()
	{
		this.causeDamage();
	}
}
