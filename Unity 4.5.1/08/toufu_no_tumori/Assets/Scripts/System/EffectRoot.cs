using UnityEngine;
using System.Collections;

public class EffectRoot : MonoBehaviour {

	public GameObject	smoke01Prefab = null;
	public GameObject	smoke02Prefab = null;
	public GameObject	ripplePrefab  = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 아이템이 실제로 성장할 때의 연기 효과를 만듭니다.
	public GameObject	createSmoke01(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.smoke01Prefab) as GameObject;

		// 효과가 끝나면 게임 오브젝트를 삭제합니다.
		effect.AddComponent<EffectSelfRelease>();
		effect.transform.position = position;

		return(effect);
	}

	// 개가 달려올 때 등 연기 효과를 만듭니다.
	public GameObject	createSmoke02(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.smoke02Prefab) as GameObject;

		effect.AddComponent<EffectSelfRelease>();
		effect.transform.position = position;

		return(effect);
	}

	// 물결 효과를 만듭니다.
	public GameObject	createRipple(Vector3 position)
	{
		GameObject	effect = GameObject.Instantiate(this.ripplePrefab) as GameObject;

		effect.AddComponent<EffectSelfRelease>();
		effect.transform.position = position;

		return(effect);
	}

	// ================================================================ //
	// 인스턴스.

	private	static EffectRoot	instance = null;

	public static EffectRoot	getInstance()
	{
		if(EffectRoot.instance == null) {

			EffectRoot.instance = GameObject.Find("Effect Root").GetComponent<EffectRoot>();
		}

		return(EffectRoot.instance);
	}

	public static EffectRoot	get()
	{
		return(EffectRoot.getInstance());
	}
}
