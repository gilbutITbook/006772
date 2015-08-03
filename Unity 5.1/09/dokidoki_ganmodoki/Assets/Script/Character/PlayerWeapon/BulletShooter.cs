using UnityEngine;
using System.Collections;
using GameObjectExtension;

public enum SHOT_TYPE {

	NONE = -1,

	EMPTY = 0,		// 탄환을 쏠 수 없다(무기 선택 스테이지용).

	NEGI,
	YUZU,

	NUM,
};

// 플레이어 슈팅 컨트롤.
public class BulletShooter : MonoBehaviour {

	public chrController		player = null;

	protected GameObject	bullet_prefab = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.player = this.GetComponent<chrController>();
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	public virtual void		execute(bool is_shooting)
	{
	}

	// ================================================================ //

	// 파워 업 중?.
	protected bool isBoosted()
	{
		bool	is_boosted = false;

		do {

			chrBehaviorPlayer	behavior = this.player.behavior as chrBehaviorPlayer;

			if(behavior == null) {

				break;
			}

			is_boosted = behavior.isShotBoosted();

		} while(false);

		return(is_boosted);
	}

	public static BulletShooter	createShooter(chrController chr, SHOT_TYPE type)
	{
		BulletShooter	bullet_shooter = null;

		switch(type) {

			case SHOT_TYPE.EMPTY:
			{
				bullet_shooter = chr.gameObject.AddComponent<BulletShooter>();
			}
			break;

			case SHOT_TYPE.NEGI:
			{
				bullet_shooter = chr.gameObject.AddComponent<BulletShooter_negi>();
			}
			break;

			case SHOT_TYPE.YUZU:
			{
				bullet_shooter = chr.gameObject.AddComponent<BulletShooter_yuzu>();
			}
			break;
		}

		bullet_shooter.player = chr;

		return(bullet_shooter);
	}

}
