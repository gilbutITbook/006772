using UnityEngine;
using System.Collections;

// FIXME このクラス自体がゴーストのコードコピーだからクリンナップしないとだめ
public class chrBehaviorEnemyDemon : chrBehaviorEnemy {
	//private Renderer[]	renders = null;					// レンダー.
	//private Material[]	org_materials = null;			// もともとのモデルにアサインされていたマテリアル.
	
	
	private chrControllerEnemyDemon myController; //< 前提とするコントローラクラスへのキャスト済参照の保持

	// ---------------------------------------------------------------- //
	
	public enum STEP {
		
		NONE = -1,
		
		MOVE = 0,			// 動く.
		REST,				// 止まってる.
		VANISH,				// やられた.
		FIRE,				// 発射

		NUM,
	};
	
	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;
	
	// ---------------------------------------------------------------- //
	private GameObject	focus;							// 狙ってる対象
	private int			cachedLayerMask;				// 弾丸を発射するかどうかの判定を行うラインテストの設定をキャッシュ

	// ---------------------------------------------------------------- //
	// キャラクタースペック
	// ---------------------------------------------------------------- //
	private float		REFACING_TIME = 1.0f;			// この時間ぶんは直進する
	private float		REST_TIME = 0.0f;
	private float		FIRE_TIME = 3.0f;
	private float		AIM_ANGLE = 7.0f;				// この角度以下なら発射する.
	private float		AIM_DISTANCE = 10.0f;			// この距離以下なら発射する.
	
	// ================================================================ //
	// このコントローラが前提とするコントローラクラスへの参照を返す
	private chrControllerEnemyDemon getMyController()
	{
		if (myController == null) {
			myController = this.control as chrControllerEnemyDemon;
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
				if (checkShouldFire(focus.transform.position))
				{
					this.getMyController().Fire();
					this.next_step = STEP.FIRE;
				}
				else if(this.step_timer >= REFACING_TIME) {
					
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

			case STEP.FIRE:
			{
				if(this.step_timer >= FIRE_TIME) {
					
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
				break;
			case STEP.FIRE:
				// 動きをストップ
				this.getMyController().velocity = 0.0f;
				this.getMyController().move_dir = 0.0f;
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
		case STEP.REST:
			break;

		case STEP.MOVE:
			break;
			
		case STEP.VANISH:
			break;
		}
		
		// ---------------------------------------------------------------- //
		
	}

	// ターゲットが一直線上にいるかどうかレイチェックでテストする
	private bool checkShouldFire(Vector3 targetPosition)
	{
		chrControllerEnemyDemon ctrl = getMyController ();

		targetPosition.y = ctrl.transform.position.y;

		// 発射可能なときだけ距離・角度チェック→ラインチェック
		if (ctrl.CanFire ()) {
			Vector3 localPosition = ctrl.transform.InverseTransformPoint (targetPosition);
			localPosition.y = 0.0f;
			if (localPosition.magnitude <= AIM_DISTANCE && Mathf.Abs(Vector3.Angle(Vector3.forward, localPosition)) <= AIM_ANGLE) {
				// ラインチェック
				RaycastHit hit;
				// FIXME エネミーもチェックして、できるだけ先頭の敵だけが発射するようにする
				if (cachedLayerMask == 0) {
					cachedLayerMask = 1 << LayerMask.NameToLayer ("Wall") | 1 << LayerMask.NameToLayer ("Player");
				}

				Vector3 rayDirection = (targetPosition - ctrl.transform.position).normalized;

				if (Physics.Raycast(ctrl.transform.position, rayDirection, out hit, Mathf.Infinity, cachedLayerMask)) {
					return hit.collider.gameObject.tag == "Player";
				}
			}
		}

		return false;
	}
}
