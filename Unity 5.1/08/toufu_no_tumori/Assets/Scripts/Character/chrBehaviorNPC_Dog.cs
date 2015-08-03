using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 비헤이비어 NPC Dog 용.
public class chrBehaviorNPC_Dog : chrBehaviorNPC {

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		STAND = 0,			// 
		TURN,
		MOVE,
		DASH,

		NUM,
	};

	Step<STEP>		step = new Step<STEP>(STEP.NONE);

	private struct StepTurn {

		public float	target_dir;
		public float	start_dir;
	};
	private StepTurn	step_turn;

	protected	ItemBehaviorWan		item_wan = null;

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
		this.step.set_next(STEP.MOVE);

		this.step_turn.start_dir  = this.transform.rotation.eulerAngles.y;
		this.step_turn.target_dir = this.step_turn.start_dir;
	}
	
	void	Update()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크합니다.

		float	turn_time = 0.5f;

		switch(this.step.do_transition()) {

			case STEP.STAND:
			{
				if(this.step.get_time() > 1.0f) {

					this.step.set_next(STEP.TURN);
				}
			}
			break;

			case STEP.TURN:
			{
				if(this.step.get_time() > turn_time) {

					//this.step.set_next(STEP.MOVE);
					this.step.set_next(STEP.DASH);
				}
			}
			break;

			case STEP.MOVE:
			{
				if(this.step.get_time() > 1.0f) {

					this.step.set_next(STEP.STAND);
				}
			}
			break;

			case STEP.DASH:
			{
				if(this.step.get_time() > 1.0f) {

					this.step.set_next(STEP.STAND);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.STAND:
				{
					this.controll.cmdSetMotion("dog_idle", 0);

					// 말풍선.
					if(this.item_wan.isEnableDispatch()) {

						Vector3		dir = this.transform.TransformDirection(Vector3.forward);
						Vector3		pos = this.controll.getPosition() + Vector3.up*0.5f + 0.5f*dir;

						this.item_wan.beginDispatch(pos, dir);
					}
				}
				break;

				case STEP.TURN:
				{
					this.controll.cmdSetMotion("dog_idle", 0);

					this.step_turn.start_dir  = this.transform.rotation.eulerAngles.y;
					this.step_turn.target_dir = this.step_turn.start_dir + Random.Range(45.0f, 225.0f);
				}
				break;

				case STEP.MOVE:
				{
					this.controll.cmdSetMotion("dog_walk", 0);
				}
				break;

				case STEP.DASH:
				{
					this.controll.cmdSetMotion("dog_walk", 0);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.STAND:
			{
			}
			break;

			case STEP.TURN:
			{
				float	ratio = this.step.get_time()/turn_time;

				ratio = Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, ratio);
				ratio = Mathf.Sin(ratio);
				ratio = Mathf.InverseLerp(-1.0f, 1.0f, ratio);

				float	y_angle = Mathf.LerpAngle(this.step_turn.start_dir, this.step_turn.target_dir, ratio);

				this.transform.rotation = Quaternion.AngleAxis(y_angle, Vector3.up);
			}
			break;

			case STEP.MOVE:
			{
				float	speed = 1.0f*Time.deltaTime;

				this.transform.Translate(Vector3.forward*speed);
			}
			break;

			case STEP.DASH:
			{
				float	speed = 2.0f*Time.deltaTime;

				this.transform.Translate(Vector3.forward*speed);

				// 연기 이펙트.
				if(this.step.is_acrossing_cycle(0.2f)) {

					Vector3		back = Quaternion.AngleAxis(this.controll.getDirection(), Vector3.up)*Vector3.back;

					EffectRoot.getInstance().createSmoke02(this.controll.getPosition() + back*1.0f + Vector3.up*0.4f);
				}
			}
			break;
		}
	}

	// ================================================================ //

	// 생성 직후 호출됩니다. NPC 용.
	public override void	initialize_npc()
	{
		this.addPresetText("두부사세요");
	}

	// 게임을 시작할 때 한 번만 호출됩니다.
	public override void	start()
	{
	}

	// ================================================================ //

	public void		setItemWan(ItemBehaviorWan item_wan)
	{
		this.item_wan = item_wan;
	}

}