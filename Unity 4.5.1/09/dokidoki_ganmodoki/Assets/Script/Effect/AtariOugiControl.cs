using UnityEngine;
using System.Collections;

// 아이스 당첨시 부채.
public class AtariOugiControl : MonoBehaviour {

	public float	timer = 0.0f;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
	}

	void	Start()
	{
	}
	
	void	Update()
	{
		// UV 스크롤.

		float	cycle = 1.0f;

		float	u_offset = ipCell.get().setInput(this.timer).repeat(cycle).quantize(0.25f).getCurrent();

		Renderer[]	renderers = this.gameObject.GetComponentsInChildren<Renderer>();

		foreach(var renderer in renderers) {

			renderer.material.SetTextureOffset("_MainTex", new Vector2(u_offset, 0.0f));
		}

		this.timer += Time.deltaTime;
	}
}
