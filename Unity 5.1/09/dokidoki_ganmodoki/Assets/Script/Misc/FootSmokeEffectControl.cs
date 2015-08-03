using UnityEngine;
using System.Collections;

// 매우 단순한 동작 컨트롤.
public class FootSmokeEffectControl : MonoBehaviour {
	public Vector3 velocity;

	// Update is called once per frame
	void Update () {
		transform.position += velocity * Time.deltaTime;
	}
}
