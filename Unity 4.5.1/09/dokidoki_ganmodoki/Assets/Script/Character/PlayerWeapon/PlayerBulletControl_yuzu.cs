using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathExtension;
using GameObjectExtension;

// 플레이어가 쏘는 탄환(유자폭탄).
public class PlayerBulletControl_yuzu : PlayerBulletControl {

	public GameObject		coli_node = null;
	public GameObject		model_node = null;
	public float			max_radius = 2.0f;

	protected float			coli_sphere_radius = 1.0f;

	//

	protected Vector3	velocity = Vector3.zero;

	protected const float	PEAK_HEIGHT = 5.0f;
	protected const float	REACH = 5.0f;			// 도달거리.

	protected enum STEP {

		NONE = -1,

		FLYING = 0,
		EXPLODE,
		END,

		NUM,
	};
	protected Step<STEP>		step = new Step<STEP>(STEP.NONE);

	protected GameObject		explode_effect = null;


	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		float	t = Mathf.Sqrt((2.0f*PEAK_HEIGHT)/Mathf.Abs(Physics.gravity.y));

		float	v_velocity = Mathf.Abs(Physics.gravity.y)*t/2.0f;
		float	h_velocity = REACH/(t*2.0f);

		this.velocity = this.transform.forward*h_velocity + Vector3.up*v_velocity;

		// 플레이어의 이동량을 더해둔다.
		this.velocity += this.player.getMoveVector().Y(0.0f)/Time.deltaTime;

		//

		var		coli = this.coli_node.GetComponent<SphereCollider>();

		if(coli != null) {

			this.coli_sphere_radius = coli.radius;
		}

		//

		this.step.set_next(STEP.FLYING);
	}

	void	Update()
	{
		this.resolve_collision();

		float	explode_time = 0.5f;

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.

		switch(this.step.do_transition()) {

			case STEP.FLYING:
			{
				if(this.trigger_damage) {

					this.step.set_next(STEP.EXPLODE);
				}
			}
			break;

			case STEP.EXPLODE:
			{
				//if(this.explode_effect == null) {
				if(this.step.get_time() > explode_time) {

					this.step.set_next(STEP.END);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.FLYING:
				{
				}
				break;

				case STEP.EXPLODE:
				{
					this.model_node.renderer.enabled = false;
					this.explode_effect = EffectRoot.get().createYuzuExplode(this.gameObject.getPosition().Y(0.5f));

					this.coli_node.setLocalScale(Vector3.one*this.max_radius*0.0f);
				}
				break;

				case STEP.END:
				{
					this.gameObject.destroy();
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.FLYING:
			{
				// 이동.
				this.velocity += Physics.gravity*Time.deltaTime;
				this.transform.position += this.velocity*Time.deltaTime;
			}
			break;

			case STEP.EXPLODE:
			{
				float	rate = this.step.get_time()/explode_time;

				float	scale = rate*this.max_radius;

				this.coli_node.setLocalScale(Vector3.one*scale);

				// 폭풍과의 충돌.
				// 콜라이더가 바닥에 충돌하면 그 다음 폭풍이 충돌해도 OnTriggerEnter가.
				// 호출되지 않으므로 직접 조사한다.

				RaycastHit	hit;

				if(Physics.SphereCast(this.transform.position, scale*this.coli_sphere_radius, Vector3.up, out hit, float.MinValue, LayerMask.GetMask("Enemy", "EnemyLair"))) {

					CollisionResult	result = new CollisionResult();
		
					result.object0 = this.gameObject;
					result.object1 = hit.collider.gameObject;
					result.is_trigger = true;
		
					this.collision_results.Add(result);
				}

			}
			break;
		}
	}

	// 충돌 결과 처리.
	protected void	resolve_collision()
	{
		do {

			// 벽에 닿았으므로 지운다.
			// 벽 너머의 적에게 닿지 않게 먼저 벽을 조사한다.
			if(this.collision_results.Exists(x => x.object1.layer ==  LayerMask.NameToLayer("Wall"))) {

				this.trigger_damage = true;
			}

			// 적에게 닿으면 대미지를 준다.
			foreach(var result in this.collision_results) {

				if(result.object1 == null) {

					continue;
				}

				int		enemy_layer      = LayerMask.NameToLayer("Enemy");
				int		enemy_lair_layer = LayerMask.NameToLayer("EnemyLair");

				if(result.object1.layer == enemy_layer || result.object1.layer == enemy_lair_layer) {

					if(this.step.get_current() == STEP.EXPLODE) {

						if((this.player.behavior as chrBehaviorLocal) != null) {

							result.object1.GetComponent<chrController>().causeDamage(5.0f, this.player.global_index);
						}
						result.object1 = null;
					}
					this.trigger_damage = true;
				}
			}

			// 지면에 떨어졌다.
			if(this.transform.position.y < 0.0f) {

				this.trigger_damage = true;
			}

		} while(false);

		this.collision_results.Clear();
	}
	
}
