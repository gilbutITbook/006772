using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 자동으로 사라지는 효과.
public class EffectControl : MonoBehaviour {

	void FixedUpdate(){ 
        ParticleSystem[] effects = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in effects) {
            if (ps.isStopped == false) {
                return;
            }
        }
        //효과가 끝났으므로 제거합니다.
        Destroy(this.gameObject);
	}
}
