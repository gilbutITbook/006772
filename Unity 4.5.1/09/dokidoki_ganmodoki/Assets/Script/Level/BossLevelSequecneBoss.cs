using UnityEngine;
using System.Collections;

// 보스 플로어 시퀀스   보스와 전투 중.
public class BossLevelSequenceBoss : SequenceBase {

	public enum STEP {

		NONE = -1,

		IDLE = 0,
		IN_ACTION,			// 보스와 전투 중.

		FINISH,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	protected chrBehaviorEnemyBoss	boss;

	// ================================================================ //

	// 디버그 창 생성시에 호출.
	public override void		createDebugWindow(dbwin.Window window)
	{
		window.createButton("보스 쓰러뜨리기")
			.setOnPress(() =>
			{
				this.boss.causeVanish();
			});
		window.createButton("보스의 적 리스트 갱신")
			.setOnPress(() =>
			            {
				this.boss.updateTargetPlayers();
			});
	}

	// 레벨 시작 시에 호출.
	public override void		start()
	{
		this.boss = CharacterRoot.get().findCharacter<chrBehaviorEnemyBoss>("Boss1");

		this.step.set_next(STEP.IN_ACTION);
	}

	// 매 프레임 호출.
	public override void		execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.

		switch(this.step.do_transition()) {

			case STEP.IN_ACTION:
			{
				if(this.boss == null) {

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
					Navi.get().dispatchYell(YELL_WORD.READY);
				}
				break;

			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.IN_ACTION:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

	public override bool	isFinished()
	{
		return(this.step.get_current() == STEP.FINISH);
	}

}
