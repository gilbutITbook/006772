using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 히트 이펙트(대미지를 받았을 때).
public class HitEffectControl : MonoBehaviour {

	private float	get_random(float min, float max)
	{
		float	rand;

		rand = Random.Range(min, max);

		if(Random.Range(0, 2) == 0) {
			rand *= -1.0f;
		}

		return(rand);
	}

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
	}

	void	Start()
	{
		GameObject	go;

		Vector3		center = this.transform.position + Vector3.up*1.0f;
		float		y_angle;
		float		radius;

		y_angle = this.get_random(5.0f, 10.0f);

		// 아얏

		radius = 1.5f;

		go = EffectRoot.get().createHitIteEffect(center);
		go.transform.parent = this.transform;

		go.transform.localRotation  = Quaternion.identity;
		go.transform.localRotation *= Quaternion.AngleAxis(-y_angle, Vector3.up);
		go.transform.Translate(Vector3.forward*radius);
		
		// ☆.

		foreach(var i in System.Linq.Enumerable.Range(0, 3)) {

			go = EffectRoot.get().createHitHosiEffect(center);
			go.transform.parent = this.transform;

			y_angle = 45.0f + (float)i*135.0f;
			y_angle += this.get_random(5.0f, 15.0f);

			radius = 1.0f;

			go.transform.localRotation = Quaternion.identity;
			go.transform.localRotation *= Quaternion.AngleAxis(-y_angle, Vector3.up);
			go.transform.Translate(Vector3.forward*radius);
		}
	}
	
	void	Update()
	{
		if(this.transform.childCount == 0) {

			GameObject.Destroy(this.gameObject);
		}
	}
}
