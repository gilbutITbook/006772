using UnityEngine;
using System.Collections;

//데미지를 먹었을 때의 효과 (백색 플래시).
public class DamageEffect {

	public static float	DAMAGE_FLUSH_TIME = 0.1f;		// [sec] 대미지를 받았을 때, 하얗게 빛나는 시간.

	public static float	VANISH_TIME = 2.0f;

	protected Renderer[]	renders = null;				// 렌더.
	protected Material[]	org_materials = null;		// 원래 모델에 할당되어 있던 머티리얼.

	protected float			fade_duration = 1.0f;		// [sec] 페이드 인 / 아웃의 길이.

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		IDLE = 0,			// 평상시.
		DAMAGE,				// 대미지　하얗게 플래시.
		VANISH,				// 당했다　아라로 페이드 아웃.

		FADE_OUT,			// 페이드 아웃(일시적으로 비표시로 하고 싶을 때).
		FADE_IN,			// 페이드 인(일시적으로 비표시로 하고 싶을 때).

		VACANT,				// 페이드가 끝나고 삭제 대기 중.
		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //

	public DamageEffect(GameObject go)
	{
		this.renders = go.GetComponentsInChildren<Renderer>();
	
		this.org_materials = new Material[renders.Length];

		for(int i = 0;i < this.renders.Length;i++) {

			this.org_materials[i] = this.renders[i].material;
		}

		//

		this.step.set_next(STEP.IDLE);
	}

	public void		execute()
	{

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크.


		switch(this.step.do_transition()) {

			// 대미지  하얗게 플래시.
			case STEP.DAMAGE:
			{
				if(this.step.get_time() >= DAMAGE_FLUSH_TIME) {

					this.step.set_next(STEP.IDLE);
				}
			}
			break;

			// 페이드가 끝나고 삭제 대기 중.
			case STEP.VANISH:
			{
				if(this.step.get_time() >= VANISH_TIME) {
					
					this.step.set_next(STEP.VACANT);
				}
			}
			break;

			// 페이드인.
			case STEP.FADE_IN:
			{
				if(this.step.get_time() >= this.fade_duration) {
					
					this.step.set_next(STEP.IDLE);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.IDLE:
				{
					// 머티리얼을 원래대로 되돌린다.
					for(int i = 0;i < this.renders.Length;i++) {
			
						this.renders[i].material = this.org_materials[i];
					}
				}
				break;

				// 대미지　하얗게 플래시.
				case STEP.DAMAGE:
				{
					for(int i = 0;i < renders.Length;i++) {
			
						this.renders[i].material = CharacterRoot.getInstance().damage_material;
					}
				}
				break;

				// 당했다.
				case STEP.VANISH:
				{
					// 페이드 아웃용 머티리얼로 교체.
					for(int i = 0;i < renders.Length;i++) {
			
						this.renders[i].material = CharacterRoot.getInstance().vanish_material;
					}
				}
				break;

				case STEP.FADE_IN:
				case STEP.FADE_OUT:
				{
					// 페이드용 머티리얼로 교체.
					for(int i = 0;i < renders.Length;i++) {

						// 텍스처를 원래 머티리얼로부터 복사한다.
						Texture		texture = this.renders[i].material.GetTexture(0);

						this.renders[i].material = CharacterRoot.getInstance().vanish_material;

						this.renders[i].material.SetTexture(0, texture);
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 당했다.
			case STEP.VANISH:
			{
				float	rate = this.step.get_time()/VANISH_TIME;
	
				rate = Mathf.Pow(rate, 1.0f/2.0f);
		
				float	alpha = Mathf.Max(0.0f, 1.0f - rate);
		
				for(int i = 0;i < this.renders.Length;i++) {
		
					this.renders[i].material.color = new Color(1.0f, 1.0f, 1.0f, alpha);
				}
			}
			break;

			// 페이드 아웃.
			case STEP.FADE_IN:
			case STEP.FADE_OUT:
			{
				float	rate = this.step.get_time()/this.fade_duration;
	
				rate = Mathf.Pow(rate, 1.0f/2.0f);
		
				float	alpha;

				if(this.step.get_current() == STEP.FADE_IN) {

					alpha = Mathf.Min(rate, 1.0f);

				} else {

					alpha = Mathf.Max(0.0f, 1.0f - rate);
				}

				for(int i = 0;i < this.renders.Length;i++) {

					Color	color = this.renders[i].material.color;

					color.a = alpha;

					this.renders[i].material.color = color;
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

	// ================================================================ //

	// 대미지 효과 시작.
	public void		startDamage()
	{
		if(this.step.get_current() != STEP.VANISH) {

			this.step.set_next(STEP.DAMAGE);
		}
	}

	// 당한 효과 시작.
	public void		startVanish()
	{
		this.step.set_next(STEP.VANISH);
	}

	// 페이드인 시작.
	public void		startFadeIn(float duration)
	{
		this.fade_duration = duration;

		this.step.set_next(STEP.FADE_IN);
	}

	// 페이드 아웃 시작.
	public void		startFadeOut(float duration)
	{
		this.fade_duration = duration;

		this.step.set_next(STEP.FADE_OUT);
	}

	// 연출중?.
	public bool		isDone()
	{
		bool	is_done = (this.step.get_current() == STEP.IDLE);

		return(is_done);
	}

	// 소실 연출이 끝났는가?.
	public bool		isVacant()
	{
		bool	is_done = (this.step.get_current() == STEP.VACANT);

		return(is_done);
	}

}
