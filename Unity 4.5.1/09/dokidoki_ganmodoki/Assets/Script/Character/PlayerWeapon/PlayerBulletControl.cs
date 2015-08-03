using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 플레이어가 쏘는 탄환.
public class PlayerBulletControl : MonoBehaviour {

	public chrController	player = null;

	public bool		trigger_damage = false;

	public List<CollisionResult>	collision_results = new List<CollisionResult>();

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
	}

	protected bool	is_in_screen()
	{
		bool	is_in_screen = false;

		do {

			Vector3	viewport_position = Camera.main.WorldToViewportPoint(this.transform.position);

			if(-1.0f <= viewport_position.x && viewport_position.x <= 1.0f) {

				if(-1.0f <= viewport_position.y && viewport_position.y <= 1.0f) {

					is_in_screen = true;
				}
			}

		} while(false);

		return(is_in_screen);
	}

	void	OnTriggerEnter(Collider other)
	{
		if(other.gameObject.layer == LayerMask.NameToLayer("Enemy")) {

			CollisionResult	result = new CollisionResult();

			result.object0 = this.gameObject;
			result.object1 = other.gameObject;
			result.is_trigger = false;

			this.collision_results.Add(result);

		} else if(other.gameObject.layer == LayerMask.NameToLayer("EnemyLair")) {

			CollisionResult	result = new CollisionResult();

			result.object0 = this.gameObject;
			result.object1 = other.gameObject;
			result.is_trigger = false;

			this.collision_results.Add(result);

		} else if(other.gameObject.layer == LayerMask.NameToLayer("Wall")) {

			CollisionResult	result = new CollisionResult();

			result.object0 = this.gameObject;
			result.object1 = other.gameObject;
			result.is_trigger = false;

			this.collision_results.Add(result);
		}
	}
}
