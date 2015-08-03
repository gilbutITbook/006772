using UnityEngine;
using System.Collections;
using GameObjectExtension;

// 플레이어의 슈팅 컨트롤  유자 폭탄.
public class BulletShooter_yuzu : BulletShooter {

	protected PlayerBulletControl_yuzu		bullet = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		this.bullet_prefab = CharacterRoot.get().player_bullet_yuzu_prefab;
	}

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	public const float	MAX_RADIUS = 4.0f;

	public override void		execute(bool is_shooting)
	{
		do {

			if(!is_shooting) {

				break;
			}

			if(this.bullet != null) {

				break;
			}

			//

			GameObject	go = this.bullet_prefab.instantiate();

			go.transform.position = this.transform.TransformPoint(new Vector3(0.0f, 2.0f, 0.4f));
			go.transform.rotation = Quaternion.AngleAxis(this.player.getDirection(), Vector3.up);

			this.bullet = go.GetComponent<PlayerBulletControl_yuzu>();

			this.bullet.player = this.player;

			if(this.isBoosted()) {

				this.bullet.max_radius = MAX_RADIUS*1.5f;

			} else {

				this.bullet.max_radius = MAX_RADIUS;
			}

		} while(false);

	}
}
