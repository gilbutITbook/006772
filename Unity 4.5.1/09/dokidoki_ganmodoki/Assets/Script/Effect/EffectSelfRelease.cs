using UnityEngine;
using System.Collections;

// 이펙트 종료 시에 게임 오브젝트를 삭제.
public class EffectSelfRelease : MonoBehaviour {

	// ================================================================ //
	// MonoBehaviour에서 상속.

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
