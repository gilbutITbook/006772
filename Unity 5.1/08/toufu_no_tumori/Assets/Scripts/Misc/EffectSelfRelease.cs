using UnityEngine;
using System.Collections;

// 이펙트 종료 시 게임 오브젝트를 제거합니다.
public class EffectSelfRelease : MonoBehaviour {

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
		if(this.GetComponentInChildren<ParticleSystem>().isStopped) {

			GameObject.Destroy(this.gameObject);
		}
	}
}
