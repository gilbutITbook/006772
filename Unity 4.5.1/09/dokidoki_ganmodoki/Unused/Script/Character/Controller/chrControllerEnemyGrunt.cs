using UnityEngine;
using System.Collections;

// 敵に体あたりするとダメージをあたえることができる
// そのかわり、消滅する
public class chrControllerEnemyGrunt : chrControllerEnemyBase {
	// ライフなどのデフォルトプロパティの変更など
	override protected void _awake()
	{
		base._awake();
		life = 3.0f;		// 一撃で死ぬ
		maxSpeed = 2.0f;
	}

	override protected void _start()
	{
		base._start();
		
		// RigidBody をキネティックに
		rigidbody.isKinematic = true;
	}
}
