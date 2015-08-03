using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossLevelSequenceCake : SequenceBase {

	public enum STEP {

		NONE = -1,

		IDLE = 0,
		IN_ACTION,
		FINISH,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	protected float		time_limit = 30.0f;		// [sec] 제한시간(임시).

	// ================================================================ //

	// 디버그 창 생성 시 호출.
	public override void		createDebugWindow(dbwin.Window window)
	{
		window.createButton("케이크 타임 끝")
			.setOnPress(() =>
			{
				this.step.do_execution(this.time_limit);
			});
	}

	// 레벨 시작 시 호출.
	public override void		start()
	{
		this.step.set_next(STEP.IN_ACTION);
	}

	// 매 프레임 호출.
	public override void		execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크.

		switch(this.step.do_transition()) {

			case STEP.IN_ACTION:
			{
				// 타임오버.
				if(this.step.get_time() > this.time_limit) {

					Navi.get().dispatchYell(YELL_WORD.TIMEUP);

					CakeTrolley.get().stopServe();

					// 남아있는 게이크를 전부 지움.
					CakeTrolley.get().deleteAllCakes();

					this.step.set_next(STEP.FINISH);
				}
			}
			break;
		}
				
		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.IN_ACTION:
				{
					// 쏠 수 없게 한다.

					var	players = PartyControl.get().getPlayers();

					foreach(var player in players) {

						player.setShotEnable(false);
					}

					Navi.get().dispatchYell(YELL_WORD.OYATU);
					Navi.get().createCakeTimer();

					CakeTrolley.get().startServe();
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		float	current_time = 1.0f;

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.IN_ACTION:
			{
				current_time = this.step.get_time()/this.time_limit;
			}
			break;
		}

		Navi.get().getCakeTimer().setTime(current_time);

		//dbPrint.setLocate(20, 15);
		//dbPrint.print("남은 시간." + Mathf.FloorToInt(this.time_limit - this.step.get_time()).ToString());

		//dbPrint.setLocate(20, 20);
		//dbPrint.print("획득 수(디버그용)). " + PartyControl.get().getLocalPlayer().getCakeCount().ToString());

		// ---------------------------------------------------------------- //
	}

	public override bool	isFinished()
	{
		return(this.step.get_current() == STEP.FINISH);
	}
}
