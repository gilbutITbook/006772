using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Controller には操作源（ビヘイビア）によって変化のない不変の要素について記述する
public class chrControllerEnemyLairMinato : chrControllerEnemyBase {
	public string		spawn_class_name = "Enemy_Obake";	// なにをジェネレートするか.

	private bool		bHasPlayedDeathAnimation;			// 死亡アニメーションをトリガー済みかどうか
	private bool		bHasFinishedDeathAnimation;			// 死亡アニメーションが再生終了済みかどうか
	private bool		bHasStartedVanishing;				// ホワイトアウトがトリガーされたかどうか
	public  float		DelayForVanish = 0.2f;				// 死亡アニメ再生終了からホワイトアウト開始までに少し間を空ける（sec）

	// ---------------------------------------------------------------- //
	// 生成に絡んだプロパティ
	// ---------------------------------------------------------------- //
	private float		SPAWN_DISTANCE = 2.0f;
	private float		SPAWN_COLLIDE_CHECK_RADIUS = 0.1f;
	private float		SPAWN_COLLIDE_CHECK_POS_Y = 0.48f; // FIXME

// 同じものが基底クラスにもあるようなので、コメントアウトしました.
#if false	
	private RoomController	room;
#endif
	private bool		activated = false;

	//===================================================================
	// アニメーション周り
	// ライフなどのデフォルトプロパティの変更など
	override protected void _awake()
	{
		base._awake();
		animator = GetComponent<Animator>();
	}
	
	// ================================================================ //
	// このコントローラ（敵種）に固有のメソッド
	/**
	 * spawn_class_nameプロパティで指定された敵を生成する
	 * @return 生成に成功した場合は true 、生成を見送った場合は false を戻す
	 */
	public bool createEnemy()
	{
		if (!activated) {
			return false;
		}

		// 放出する方向が空いているかどうかを調べる
		Vector3[] spawnDirections = new Vector3[] { Vector3.back, Vector3.left, Vector3.forward, Vector3.right };
		Vector3 spawnFrom;
		
		spawnFrom = this.transform.position;
		spawnFrom.y = SPAWN_COLLIDE_CHECK_POS_Y;
		
		foreach (Vector3 dir in spawnDirections) {
			if (!Physics.SphereCast (new Ray (spawnFrom, dir), SPAWN_COLLIDE_CHECK_RADIUS, SPAWN_DISTANCE, 1 << LayerMask.NameToLayer ("Enemy"))) {
				playGenerate();

				// 敵を生成する
				//FIXME: The following 3 parameters should be a property.
				chrControllerEnemyBase enemy = EnemyRoot.getInstance().createEnemy("Enemy_Obake", "chrControllerEnemyGhost", "chrBehaviorEnemyGhost") as chrControllerEnemyBase;

				if (room != null) {
					room.RegisterEnemy (enemy);
				}

				enemy.transform.position = spawnFrom + dir * SPAWN_DISTANCE;
				enemy.transform.rotation = Quaternion.LookRotation (dir);
				
				return true;
			}
		}

		return false;
	}

	protected virtual void playGenerate()
	{
		animator.SetTrigger("Generate");
	}

	public void Activate()
	{
		this.activated = true;
	}

	public void Deactivate()
	{
		this.activated = false;
	}

	// ================================
	// ジェネレータはアニメーションを再生し終わってからホワイトアウトする. そのためのオーバーライド関数群.


	protected override void exec_step_vanish()
	{
		if (!bHasPlayedDeathAnimation)
		{
			animator.SetTrigger("Death");
			bHasPlayedDeathAnimation = true;
		}
		else if (bHasFinishedDeathAnimation && !bHasStartedVanishing)
		{
			DelayForVanish -= Time.deltaTime;
			if (DelayForVanish <= 0.0f)
			{
				this.damage_effect.startVanish();
				bHasStartedVanishing = true;
			}
		}
		else if (bHasStartedVanishing)
		{
			base.exec_step_vanish();
		}
	}
	
	sealed protected override void goToVanishState()
	{
		// 移行を許すステート元かをチェックして状態遷移実行.
		if (this.state != EnemyState.VANISH)
		{
			this.behavior.onVanished();
			
			playDying();
			
			this.state = EnemyState.VANISH;
			this.localStateTimer = 0.0f;

			// ヒットを抜く
			this.GetComponent<Collider>().enabled = false;
			this.GetComponent<Rigidbody>().Sleep();
		}
	}
	
	// 死亡アニメーション終了の通知をアニメーションから受け取る
	public void NotifyFinishedDeathAnimation()
	{
		bHasFinishedDeathAnimation = true;
	}
}
