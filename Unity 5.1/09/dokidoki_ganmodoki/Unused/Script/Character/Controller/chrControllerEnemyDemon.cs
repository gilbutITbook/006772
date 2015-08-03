using UnityEngine;
using System.Collections;

// 敵に体あたりするとダメージをあたえることができる
// そのかわり、消滅する
public class chrControllerEnemyDemon : chrControllerEnemyBase {
	// 再発射可能タイマー
	private float refireDelayTimer;

	// 再発射にかかる時間
	public float refireDelayTime = 3.0f;

	// ライフなどのデフォルトプロパティの変更など
	override protected void _awake()
	{
		base._awake();
		life = 2.0f;
		maxSpeed = 2.0f;
	}
	
	override protected void _start()
	{
		base._start();
		
		// RigidBody をキネティックに
		rigidbody.isKinematic = true;
	}
	
	override protected void execute()
	{
		base.execute ();
		if (refireDelayTimer > 0.0f) {
			refireDelayTimer = Mathf.Max (refireDelayTimer - Time.deltaTime, 0.0f);
		}
	}

	// AIヒントにも使用する. 弾丸が発射可能かどうか.
	public bool CanFire()
	{
		return refireDelayTimer <= 0.0f;
	}

	// 進行方向へ向けて EnemyBullet を発射する
	public void		Fire()
	{
		if (CanFire ()) {
			if (animator != null) {
				animator.SetTrigger("Attack");
			}
			else {
				// もしアニメーションコントロールができないなら、即座に弾を発射する
				PerformFire();
			}
			// 弾丸の発射待ち
			this.refireDelayTimer = refireDelayTime;
		}
	}

	// 進行方向へ向けて EnemyBullet を発射する
	public void PerformFire()
	{
		GameObject	go = GameObject.Instantiate(CharacterRoot.getInstance().enemy_bullet_prefab) as GameObject;
		
		go.transform.position = this.transform.TransformPoint(new Vector3(0.0f, 0.25f, 1.0f));
		go.transform.rotation = Quaternion.AngleAxis(this.getDirection(), Vector3.up);
		
		EnemyBulletControl		bullet = go.GetComponent<EnemyBulletControl>();
		bullet.owner = this;
		
		// 弾丸の発射待ち
		this.refireDelayTimer = refireDelayTime;
	}

	// 攻撃硬直の解除をアニメーションから通知受け
	public void NotifyFinishedFire()
	{

	}
}
