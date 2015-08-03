using UnityEngine;
using System.Collections;
using GameObjectExtension;

// 플레이어 모델의 색상을 효과로 변화.
// (머리 아픔, 크림 투성이 등).
public class SkinColorControl {

	public enum STEP {

		NONE = -1,

		IDLE = 0,			// 실행 중이 아니다.
		JINJIN,				// 아이스 과식해서 머리가 아픔.
		CREAMY,				// 크림 투성이.
		HEALING,			// 체력 회복 중.

		END,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	protected chrBehaviorPlayer	player = null;

	// ================================================================ //

	public void		create(chrBehaviorPlayer player)
	{
		this.player = player;
	}

	public void		execute()
	{
		float	healing_time = 2.0f;

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다].

		switch(this.step.do_transition()) {

			case STEP.HEALING:
			{
				if(this.step.get_time() >= healing_time) {

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
					this.player.gameObject.setMaterialProperty("_BlendRate", 0.0f);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.HEALING:
			{
				float	h0, h1;
				float	blend;
				float	cycle = 2.0f;

				// 레인보우 컬러.
				{
					h0 = Mathf.Repeat(this.step.get_time(), cycle);
					h0 = h0/cycle*360.0f;
					h1 = h0 - 10.0f;
				}

				// 노멀 →　레인보우 　→　노멀.
				{
					float	t0 = 0.1f*healing_time;
					float	t1 = 0.8f*healing_time;

					blend = Mathf.Repeat(this.step.get_time(), cycle);

					if(blend < t0) {
	
						blend = Mathf.InverseLerp(0.0f, t0, blend);
						blend = Mathf.Sin(Mathf.PI/2.0f*blend);
	
					} else if(blend < t1) {

						blend = 1.0f;

					} else {
	
						blend = Mathf.InverseLerp(t1, healing_time*1.0f, blend);
						blend = Mathf.Lerp(0.0f, Mathf.PI, blend);
						blend = Mathf.Cos(blend);
						blend = Mathf.InverseLerp(-1.0f, 1.0f, blend);
					}
				}

				Color	color0 = DoorMojiControl.HSVToRGB(h0, 1.0f, 1.0f);
				Color	color1 = DoorMojiControl.HSVToRGB(h1, 1.0f, 1.0f);

				this.player.gameObject.setMaterialProperty("_SecondColor",   color0);
				this.player.gameObject.setMaterialProperty("_ThirdColor",    color1);
				this.player.gameObject.setMaterialProperty("_BlendRate",     blend);
				this.player.gameObject.setMaterialProperty("_MaskAffection", 0.0f);
			}
			break;

			case STEP.CREAMY:
			{
				this.player.gameObject.setMaterialProperty("_SecondColor",   Color.white);
				this.player.gameObject.setMaterialProperty("_ThirdColor",   Color.white);
				this.player.gameObject.setMaterialProperty("_BlendRate",     0.9f);
				this.player.gameObject.setMaterialProperty("_MaskAffection", 1.0f);
			}
			break;

			case STEP.JINJIN:
			{
				float	cycle = 1.0f;
				float	rate  = ipCell.get().setInput(this.step.get_time())
									.repeat(cycle).normalize().uradian().sin().lerp(0.2f, 0.5f).getCurrent();

				this.player.gameObject.setMaterialProperty("_SecondColor",   Color.blue);
				this.player.gameObject.setMaterialProperty("_ThirdColor",    Color.blue);
				this.player.gameObject.setMaterialProperty("_BlendRate",     rate);
				this.player.gameObject.setMaterialProperty("_MaskAffection", 0.0f);
			}
			break;
		}
	}

	// ---------------------------------------------------------------- //

	// 머리 아픈 상태(아이스 과식)를 시작한다.
	public void		startJinJin()
	{
		this.step.set_next(STEP.JINJIN);
	}

	// 머리 아픈 상태(아이스 과식)를 마친다.
	public void		stopJinJin()
	{
		this.step.set_next(STEP.IDLE);
	}

	// ---------------------------------------------------------------- //

	// 크림 범벅을 시작한다.
	public void		startCreamy()
	{
		this.step.set_next(STEP.CREAMY);
	}

	// 크림 범벅을 끝낸다.
	public void		stopCreamy()
	{
		this.step.set_next(STEP.IDLE);
	}

	// 크림 범벅 중?.
	public bool		isNowCreamy()
	{
		return(this.step.get_current() == STEP.CREAMY);
	}

	// ---------------------------------------------------------------- //

	// 체력 회복을 시작한다.
	public void		startHealing()
	{
		this.step.set_next(STEP.HEALING);
	}

	// 체력 회복을 끝낸다.
	public void		stopHealing()
	{
		this.step.set_next(STEP.IDLE);
	}

	// 체력 회복 중?.
	public bool		isNowHealing()
	{
		return(this.step.get_current() == STEP.HEALING);
	}
}
