using UnityEngine;
using System.Collections;

public class chrBehaviorEnemyGhost : chrBehaviorEnemy {
	//private Renderer[]	renders = null;					// レンダー.
	//private Material[]	org_materials = null;			// もともとのモデルにアサインされていたマテリアル.
	
	
	private chrControllerEnemyGhost myController; //< 前提とするコントローラクラスへのキャスト済参照の保持

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
	// このコントローラが前提とするコントローラクラスへの参照を返す
	private chrControllerEnemyGhost getMyController()
	{
		if (myController == null) {
			myController = this.control as chrControllerEnemyGhost;
		}
		return myController;
	}
	
	public override void		onDamaged()
	{
		this.getMyController().causeDamage();
	}

	// やられたことにする.
	public override void		causeVanish()
	{
		this.getMyController().causeVanish();
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
		if (isPaused) {
			return;
		}
		// ---------------------------------------------------------------- //
		// ステップ内の経過時間を進める.
		
		this.step_timer += Time.deltaTime;
		
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
			case STEP.REST:
				this.getMyController().velocity = 0.0f;
				if(REST_TIME <= 0.0f) {

					this.next_step = STEP.MOVE;
				}
				break;
				
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
				this.getMyController().move_dir = this.control.getDirection() + turn;
				this.getMyController().velocity = this.getMyController().maxSpeed;
			}
				break;

			default:
				break;
			}
			
			this.step_timer = 0.0f;
		}
		
		// ---------------------------------------------------------------- //
		// 各状態での実行処理.
		
		switch(this.step) {
			
		case STEP.MOVE:
			break;
			
		case STEP.VANISH:
			break;
		}
		
		// ---------------------------------------------------------------- //
		this.resolve_collision();
	}

	//========================================================================
	// コリジョンに接触開始したときに呼ばれるメソッド
	void 	OnCollisionEnter(Collision other)
	{
		switch(other.gameObject.tag) {
			case "Player":
			{
				CollisionResult	result = new CollisionResult();
				
				result.object0    = this.gameObject;
				result.object1    = other.gameObject;
				result.is_trigger = false;
				
				this.control.collision_results.Add(result);
			}
			break;
		}
	}
	
	private void resolve_collision()
	{
		foreach (var result in this.control.collision_results) {
			if (result.object1 == null) {
				continue;
			}

			//GameObject self = result.object0;
			GameObject other = result.object1;

			if (other.tag == "Player")
			{
				// 敵との直接接触に関するダメージ処理はプレイヤー側で行われるはずなので
				// ここでは自死のみ処理
				this.control.causeDamage(100.0f);
			}
		}
		this.control.collision_results.Clear();	
	}
}
