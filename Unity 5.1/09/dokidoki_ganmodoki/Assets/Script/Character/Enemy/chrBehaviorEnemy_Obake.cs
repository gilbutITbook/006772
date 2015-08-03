using UnityEngine;
using System.Collections;

// 적의 비헤이비어  　도깨비.
public class chrBehaviorEnemy_Obake : chrBehaviorEnemy {

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
	}

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	public override void	initialize()
	{
		base.initialize();

		base.setBehaveKind(Enemy.BEHAVE_KIND.UROURO);
	}

	public override void	start()
	{
		base.start();

		this.control.vital.setHitPoint(5.0f);
		this.control.vital.setAttackPower(2.0f);
		this.control.vital.setAttackDistance(2.0f);
	}


	public override	void	execute()
	{
		base.execute();
		this.basic_action.execute();

		this.is_attack_motion_finished = false;
		this.is_attack_motion_impact = false;

		/*if(Input.GetKeyDown(KeyCode.A)) {

			this.shootBullet();
		}*/
	}

	// 진행 방향으로 돌아 EnemyBullet을 발사한다.
	public void 	shootBullet()
	{
		GameObject	go = GameObject.Instantiate(CharacterRoot.get().enemy_bullet_prefab) as GameObject;
		
		go.transform.position = this.transform.TransformPoint(new Vector3(0.0f, 0.25f, 1.0f));
		go.transform.rotation = Quaternion.AngleAxis(this.control.getDirection(), Vector3.up);
		
		EnemyBulletControl		bullet = go.GetComponent<EnemyBulletControl>();

		bullet.owner = this.control;
	}

	// ================================================================ //
	// 애니메이션 이벤트.

	// Attack 모션으로 타격이 상대방에게 닿는 순간.
	public void 	NotifyAttackImpact()
	{
		this.is_attack_motion_impact = true;
	}

	// Attack 모션이 끝났을 때.
	public void		NotifyFinishedAttack()
	{
		this.is_attack_motion_finished = true;
	}

	// Attack 모션이 무기를 발사한다 & 상대방에게 닿는 순간.
	public void 	PerformFire()
	{
		this.is_attack_motion_impact = true;
	}

	// Attack 모션이 끝났을 때.
	public void 	NotifyFinishedFire()
	{
		this.is_attack_motion_finished = true;
	}

}

