using UnityEngine;
using System.Collections;
using GameObjectExtension;

// 플레이어 슈팅 컨트롤　 대파 발칸.
public class BulletShooter_negi : BulletShooter {


	public static float	REST_TIME = 0.4f;		// 연사 간격.

	private	float	rest_timer = 0.0f;			// 타이머.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		this.bullet_prefab = CharacterRoot.get().player_bullet_negi_prefab;
	}

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	public override void		execute(bool is_shooting)
	{
		this.rest_timer = Mathf.Max(0.0f, this.rest_timer - Time.deltaTime);

		if(is_shooting) {

			if(this.rest_timer <= 0.0f) {

				GameObject	go = this.bullet_prefab.instantiate();

				go.transform.position = this.transform.TransformPoint(new Vector3(0.0f, 0.5f, 1.0f));
				go.transform.rotation = Quaternion.AngleAxis(this.player.getDirection(), Vector3.up);
	
				PlayerBulletControl		bullet = go.GetComponent<PlayerBulletControl>();
	
				bullet.player = this.player;

				// 연사 간격.
				if(this.isBoosted()) {

					// 파워 업 중엔 슈팅 간격이 짧아진다.
					this.rest_timer = REST_TIME*0.5f;

				} else {

					this.rest_timer = REST_TIME;
				}
			}
		}
	}
}
