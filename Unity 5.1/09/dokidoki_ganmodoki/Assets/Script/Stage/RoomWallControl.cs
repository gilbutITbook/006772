using UnityEngine;
using System.Collections;

public class RoomWallControl : MonoBehaviour {

	public Material 		OpacityMaterial;
	public Material 		TransparencyMaterial;
	public MeshRenderer 	WallRenderer;

	public static float		OPAQUE_ALPHA = 1.0f;
	public static float		TRANS_ALPHA  = 0.3f;

	// 페이드 인 / 아웃.
	protected struct Fade{

		public bool		is_fading;				// 페이드 중？.
		public bool		fade_in;				// true ... 페이드 인	false ... 페이드 아웃.

		public float	alpha_ratio;			// 0.0 ... TRANS_ALPHA		1.0 ... OPAQUE_ALPHA.
	};
	protected Fade	fade;

	void	Awake()
	{
		this.WallRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
		this.fade.is_fading = false;
		this.fade.alpha_ratio = 1.0f;
	}

	void	Start()
	{
	}
	
	void	Update()
	{
		// 페이드 인/아웃의 종료 체크.
		if(this.fade.is_fading) {

			if(this.fade.fade_in) {

				if(this.fade.alpha_ratio >= 1.0f) {
	
					this.fade.is_fading   = false;
					this.fade.alpha_ratio = 1.0f;
	
					WallRenderer.material = OpacityMaterial;
				}

			} else {

				if(this.fade.alpha_ratio <= 0.0f) {
	
					this.fade.is_fading   = false;
					this.fade.alpha_ratio = 0.0f;
				}
			}
		}

		// 알파값 계산.
		if(this.fade.is_fading) {

			if(this.fade.fade_in) {

				this.fade.alpha_ratio += 1.0f*Time.deltaTime;
	
			} else {

				this.fade.alpha_ratio -= 1.0f*Time.deltaTime;
			}

			this.fade.alpha_ratio = Mathf.Clamp01(this.fade.alpha_ratio);
		}

		// 알파값을 머티리얼에 설정한다.
		if(this.fade.is_fading) {

			float	alpha = Mathf.Lerp(TRANS_ALPHA, OPAQUE_ALPHA, this.fade.alpha_ratio);

			WallRenderer.material.SetFloat("_Alpha", alpha);
		}
	}

	// 페이드 인을 시작한다.
	public void	FadeIn()
	{
		this.fade.is_fading = true;
		this.fade.fade_in   = true;

		if(this.fade.alpha_ratio == 1.0f) {

			WallRenderer.material = TransparencyMaterial;
		}
	}

	// 페이드 아웃을 시작한다.
	public void	FadeOut()
	{
		this.fade.is_fading = true;
		this.fade.fade_in   = false;

		if(this.fade.alpha_ratio == 1.0f) {

			WallRenderer.material = TransparencyMaterial;
		}
	}
}
