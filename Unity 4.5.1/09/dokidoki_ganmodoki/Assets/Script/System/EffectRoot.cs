using UnityEngine;
using System.Collections;

public class EffectRoot : MonoBehaviour {

	public GameObject	itemGetEffect_prefab		= null;		// 아이템을 획득했을 때의 효과.
	public GameObject	healEffect_prefab 			= null;		// 체력 회복 효과.
	public GameObject	candleFireEffect_prefab		= null;		// 양촛불  효과.
	public GameObject	doorMojiEffect_prefab		= null;		// 도어(도넛) 주위를 도는 문자.
	public GameObject	bossFootSmoke_prefab		= null;		// 보스의 발 연기 효과. 보통 상태보다 클지도.
	public GameObject	bossAngryMark_prefab		= null;		// 보스 분노 마크.
	public GameObject	bossSnort_prefab			= null;		// 보스 콧김 효과.
	public GameObject	bossChargeAura_prefab		= null;		// 보스의 공격 효과.
	public GameObject	effectItePrefab				= null;		// '아얏' 효과.
	public GameObject	effectHosiPrefab			= null;		// ☆ 효과.
	public GameObject	hitEffect_prefab			= null;		// 대미지 효과('아앗'과 ☆을 모아서 만든다).
	public GameObject	jinJinEffect_prefab			= null;		// 아이스를 과식했을 때 머리 아픈 효과.
	public GameObject	atariOugi_prefab			= null;		// 아이스 당첨 시 부채.
	public GameObject	yuzuExplode_prefab			= null;		// 유자 폭발 효과.
	public GameObject	effectSmokeMiddlePrefab		= null;		// 연기 중.
	public GameObject	effectSmokeSmallPrefab		= null;		// 연기 소.
	public GameObject	effectTearsPrefab			= null;		// 눈물.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 아이템을 획득했을 때의 효과를 만든다.
	public GameObject		createItemGetEffect(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.itemGetEffect_prefab) as GameObject;

		effect.transform.position = position;
		effect.AddComponent<EffectSelfRelease>();

		return(effect);
	}

	// 체력회복 효과를 만든다.
	public GameObject		createHealEffect(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.healEffect_prefab) as GameObject;

		effect.transform.position = position;
		effect.AddComponent<EffectSelfRelease>();

		return(effect);
	}

	// 양촛 불 효과를 만든다.
	public GameObject		createCandleFireEffect(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.candleFireEffect_prefab) as GameObject;

		effect.transform.position = position;

		return(effect);
	}

	// 문(도넛) 주변의 문자 효과를 만든다.
	public DoorMojiControl		createDoorMojisEffect(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.doorMojiEffect_prefab) as GameObject;

		effect.transform.position = position;

		return(effect.GetComponent<DoorMojiControl>());
	}

	// 보스용 발 연기 효과를 만든다.
	public GameObject		createBossFootSmokeEffect(Vector3 position)
	{
		GameObject effect = null;

		if (bossFootSmoke_prefab != null)
		{
			effect = GameObject.Instantiate(this.bossFootSmoke_prefab) as GameObject;
			effect.transform.position = position;
		}

		return effect;
	}

	// 보스의 분노 마크 효과를 만든다.
	public GameObject		createAngryMarkEffect(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.bossAngryMark_prefab) as GameObject;
		
		effect.transform.position = position;
		
		return(effect);
	}
	
	// 보스의 콧김 효과를 만든다.
	public GameObject		createSnortEffect(Vector3 position, Quaternion rotation)
	{
		GameObject	effect = GameObject.Instantiate(this.bossSnort_prefab) as GameObject;
		
		effect.transform.position = position;
		effect.transform.rotation = rotation;
		
		return(effect);
	}

	// 보스의 공격 오라 효과를 만들어 반환한다. 좌표는 받는 쪽에서 조정한다.
	public GameObject		createChargeAura()
	{
		return GameObject.Instantiate(this.bossChargeAura_prefab) as GameObject;
	}
	
	// ---------------------------------------------------------------- //

	// '아얏' 효과를 만든다.
	public GameObject		createHitIteEffect(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.effectItePrefab) as GameObject;

		effect.transform.position = position;
		effect.AddComponent<EffectSelfRelease>();

		return(effect);
	}

	// ☆ 효과를 만든다.
	public GameObject		createHitHosiEffect(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.effectHosiPrefab) as GameObject;

		effect.transform.position = position;
		effect.AddComponent<EffectSelfRelease>();

		return(effect);
	}

	// 대미지를 입었을 때 효과를 만든다.
	public GameObject		createHitEffect(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.hitEffect_prefab) as GameObject;

		effect.transform.position = position;

		return(effect);
	}

	// 아이스바를 너무 먹었을 때 머리가 아픈 효과를 만든다.
	public GameObject		createJinJinEffect(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.jinJinEffect_prefab) as GameObject;

		effect.transform.position = position;

		return(effect);
	}

	// 아이스바가 당첨됐을 때의 부채를 만든다.
	public GameObject		createAtariOugi(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.atariOugi_prefab) as GameObject;

		effect.transform.position = position;

		return(effect);
	}

	// 유자가 폭발했을 때의 효과를 만든다.
	public GameObject		createYuzuExplode(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.yuzuExplode_prefab) as GameObject;

		effect.transform.position = position;
		effect.AddComponent<EffectSelfRelease>();

		return(effect);
	}

	// 연기(중간 사이즈) 효과를 만든다.
	public GameObject		createSmokeMiddle(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.effectSmokeMiddlePrefab) as GameObject;

		effect.transform.position = position;
		effect.AddComponent<EffectSelfRelease>();

		return(effect);
	}

	//연기(작은 사이즈) 효과를 만든다.
	public GameObject		createSmokeSmall(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.effectSmokeSmallPrefab) as GameObject;

		effect.transform.position = position;
		effect.AddComponent<EffectSelfRelease>();

		return(effect);
	}

	// 우는 효과를 만든다.
	public GameObject		createTearsEffect(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.effectTearsPrefab) as GameObject;

		effect.transform.position = position;
		effect.transform.rotation = Quaternion.identity;

		return(effect);
	}

	// ================================================================ //
	// 인스턴스.

	private	static EffectRoot	instance = null;

	public static EffectRoot	getInstance()
	{
		if(EffectRoot.instance == null) {

			EffectRoot.instance = GameObject.Find("EffectRoot").GetComponent<EffectRoot>();
		}

		return(EffectRoot.instance);
	}
	public static EffectRoot	get()
	{
		return(EffectRoot.getInstance());
	}
}
