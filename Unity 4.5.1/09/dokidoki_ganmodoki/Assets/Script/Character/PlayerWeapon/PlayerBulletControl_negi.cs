using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 플레이어가 쏘는 탄환.
public class PlayerBulletControl_negi : PlayerBulletControl {

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
		bool	is_damage = false;

		do {

			// 벽에 부딪히면 사라진다.
			// 벽 너머의 적에 닿지 않다록 먼저 벽을 조사한다.
			if(this.collision_results.Exists(x => x.object1.layer ==  LayerMask.NameToLayer("Wall"))) {

				is_damage = true;
				break;
			}

			// 적에게 닿으면 대미지를 준다.
			foreach(var result in this.collision_results) {

				if(result.object1 == null) {

					continue;
				}

				int		enemy_layer      = LayerMask.NameToLayer("Enemy");
				int		enemy_lair_layer = LayerMask.NameToLayer("EnemyLair");

				if(result.object1.layer == enemy_layer || result.object1.layer == enemy_lair_layer) {

					if((this.player.behavior as chrBehaviorLocal) != null) {

						result.object1.GetComponent<chrController>().causeDamage(1.0f, this.player.global_index);
					}
					result.object1 = null;
					is_damage = true;
				}
			}
			if(is_damage) {

				break;
			}

			// 화면 밖으로 나가면 사라진다.
			if(!this.is_in_screen()) {

				is_damage = true;
				break;
			}

		} while(false);

		this.collision_results.Clear();

		if(is_damage) {

			GameObject.Destroy(this.gameObject);

		} else {

			// 이동.

			float	speed = 0.2f;

			this.transform.Translate(Vector3.forward*speed*(Time.deltaTime/(1.0f/60.0f)));
		}
	}
}
