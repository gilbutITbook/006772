using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoorMojiControl : MonoBehaviour {

	protected	class Moji {

		public GameObject	go;
	};

	protected List<Moji>	mojis;

	protected float			time = 0.0f;

	public static Color	HSVToRGB(float h, float s, float v)
	{
	    int			sel;
	    float		fl;
	    float		m, n;
		Color		color = Color.black;

		h = Mathf.Repeat(h, 360.0f);

	    sel = Mathf.FloorToInt(h/60.0f);
	
	    fl = (h/60.0f) - (float)sel;
	
	    if(sel%2 == 0) {
	
			fl = 1.0f - fl;
		}
	
	    m = v*(1.0f - s);
	    n = v*(1.0f - s*fl);
	
		switch(sel) {
	
			default:
			case 0: color.r = v;	color.g = n;	color.b = m;	break;
			case 1: color.r = n;	color.g = v;	color.b = m;	break;
			case 2: color.r = m;	color.g = v;	color.b = n;	break;
			case 3: color.r = m;	color.g = n;	color.b = v;	break;
			case 4: color.r = n;	color.g = m;	color.b = v;	break;
			case 5: color.r = v;	color.g = m;	color.b = n;	break;
		}

		return(color);
	}

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		this.mojis = new List<Moji>();

		foreach(var i in System.Linq.Enumerable.Range(0, this.transform.childCount)) {

			Moji	moji = new Moji();

			moji.go = this.transform.GetChild(i).gameObject;

			this.mojis.Add(moji);

			Color	color = DoorMojiControl.HSVToRGB((float)i/(float)this.transform.childCount*360.0f, 0.7f, 1.0f);

			moji.go.GetComponentInChildren<Renderer>().materials[0].SetColor("_MainColor", color);
		}

		// 문자순으로 정렬.
		this.mojis.Sort((x, y) => string.Compare(x.go.name, y.go.name));
	}

	void	Start()
	{
	}
	
	void	Update()
	{
		float	radius = 3.0f;
		float	cycle  = 4.0f;
		float	yangle = Mathf.Repeat(this.time, cycle)/cycle*360.0f;

		float	yangle_add = 360.0f/(float)this.mojis.Count;

		float	up_down_cycle = 4.0f;
		float	up_down_angle;

		float	up_down_time = this.time;

		foreach(var moji in this.mojis) {

			up_down_time += 1.1f;

			float	up_down_ratio = Mathf.Repeat(up_down_time, up_down_cycle)/up_down_cycle;

			if(up_down_ratio < 0.5f) {

				up_down_ratio = Mathf.InverseLerp(0.0f, 0.5f, up_down_ratio);
				up_down_angle = up_down_ratio*Mathf.PI;

			} else {

				up_down_angle = 0.0f;
			}

			float	up_down = Mathf.Sin(up_down_angle)*0.5f;

			moji.go.transform.localPosition = Vector3.zero;
			moji.go.transform.localRotation = Quaternion.AngleAxis(yangle, Vector3.up);
			moji.go.transform.Translate(new Vector3(0.0f, 0.5f + up_down, -radius));

			yangle -= yangle_add;
		}

		this.time += Time.deltaTime;
	}
}
