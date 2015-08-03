using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 적이 쏘는 탄환.
public class EnemyBulletControl : MonoBehaviour {

	public chrController	owner = null;

	public bool		trigger_damage = false;

	public List<CollisionResult>	collision_results = new List<CollisionResult>();

	// 탄환 생존 시간. 이 시간을 넘으면 지운다.
	public float lifeSpan = 10.0f;

	// lifeSpan까지의 시간을 재는 타이머.
	private float lifeTimer = 0.0f;
	
	/// <summary>
	/// 일시정지 명령이 내려져 있는가.
	/// </summary>
	protected bool isPaused = false;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	public void		causeDamage()
	{
		this.trigger_damage = true;
	}

	void	Start()
	{
	}
	
	void	Update()
	{
		bool	is_damaged = false;
		
		if (isPaused)
		{
			return;
		}

		lifeTimer += Time.deltaTime;

		// 충돌 체크.
		do {

			// 벽에 닿으면 지운다.
			// 벽 건너편 적에 맞지 않게 먼저 벽을 조사한다.
			if(this.collision_results.Exists(x => x.object1.layer ==  LayerMask.NameToLayer("Wall"))) {

				is_damaged = true;
				break;
			}

			// 플레이어 닿으면 대미지를 준다.
			foreach(var result in this.collision_results) {

				if(result.object1 == null) {

					continue;
				}

				if(result.object1.tag != "Player") {

					continue;
				}

				chrBehaviorPlayer	behavior = result.object1.GetComponent<chrController>().behavior as chrBehaviorPlayer;

				if(behavior == null) {

					continue;
				}
				if(!behavior.isLocal()) {

					// 리모트 플레이어에게는 대미지를 주지 않는다.
					continue;
				}
				result.object1.GetComponent<chrController>().causeDamage(this.owner.vital.getShotPower(), -1);
				result.object1 = null;
				is_damaged = true;
			}
			if(is_damaged) {

				break;
			}

			// 라이프 타이머가 초과되면 사라진다.
			if (lifeTimer >= lifeSpan) {

				is_damaged = true;
				break;
			}

		} while(false);

		this.collision_results.Clear();

		if(is_damaged) {

			GameObject.Destroy(this.gameObject);

		} else {

			// 이동.

			float	speed = 12.0f;

			Vector3		move_to = this.transform.position + this.transform.forward*speed*Time.deltaTime;

			this.GetComponent<Rigidbody>().MovePosition(move_to);				
		}
	}

	void 	OnTriggerEnter(Collider other)
	{
		// 여분의 것과 맞지 않는 설정이므로 일단 쌓아 버린다.
		CollisionResult	result = new CollisionResult();
		
		result.object0 = this.gameObject;
		result.object1 = other.gameObject;
		result.is_trigger = false;
		
		this.collision_results.Add(result);
	}
	
	//=================================================================================//
	// 밖에서 호출되는 메소드.
	public void SetPause(bool newPause)
	{
		isPaused = newPause;
	}
}
