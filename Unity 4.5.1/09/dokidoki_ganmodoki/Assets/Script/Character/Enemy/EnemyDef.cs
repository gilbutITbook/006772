using UnityEngine;
using System.Collections;

namespace Enemy {

	public enum BEHAVE_KIND {

		NONE = -1,

		BOTTACHI = 0,		// 서있기만 함. 디버그용.
		OUFUKU,				// 두 지점을 왕복한다.
		UROURO,				// 멈춘다→걷는다.
		TOTUGEKI,			// 플레이어에게 다가가 근접 공격.
		SONOBA_DE_FIRE,		// 그 자리에서 발사.
		WARP_DE_FIRE,		// 워프를 반복한다.
		JUMBO,				// 점보.
		GOROGORO,			// 뒹굴뒹굴 굴러가 벽에서 반사.

		NUM,
	}

} // namespace Map

public class EnemyDef : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
