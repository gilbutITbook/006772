using UnityEngine;
using System.Collections;

public class BossLevelSequence : SequenceBase {

	public enum STEP {

		NONE = -1,

		IDLE = 0,
		VS_BOSS,			// 보스와 전투 중.
		CAKE_BIKING,		// 케이크 무한제공 중.
		RESULT,				// 결과.
		FINISH,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	//===================================================================

	dbwin.Window	window = null;

	// 디버그 창 생성 시에 호출.
	public override void		createDebugWindow(dbwin.Window window)
	{
		this.window = window;

		/*window.createButton("다음")
			.setOnPress(() =>
			{
				switch(this.step.get_current()) {

					case STEP.VS_BOSS:
					{
						this.boss.causeVanish();
					}
					break;

					case STEP.CAKE_BIKING:
					{
						this.is_cake_time_over = true;
					}
					break;

					case STEP.RESULT:
					{
						this.is_result_done = true;
					}
					break;
				}
			});*/
	}

	protected bool	is_result_done = false;			// 테스트용   결과 끝?.

	// 레벨 시작 시 호출.
	public override void		start()
	{
		this.step.set_next(STEP.VS_BOSS);
	}

	// 케이크 무한제공(보스전 후 보너스)중?.
	public bool	isNowCakeBiking()
	{
		bool	ret = false;

		if(this.child != null) {

			ret = this.child is BossLevelSequenceCake;
		}

		return(ret);
	}

	// 매 프레임 호출.
	public override void		execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크.

		switch(this.step.do_transition()) {

			case STEP.VS_BOSS:
			{
				if(this.child.isFinished()) {

					GameObject.Destroy(this.child);
					this.child = null;

					this.step.set_next(STEP.CAKE_BIKING);
				}
			}
			break;

			case STEP.CAKE_BIKING:
			{
				if(this.child.isFinished()) {

					GameObject.Destroy(this.child);
					this.child = null;

					this.step.set_next(STEP.RESULT);
				}
			}
			break;

			case STEP.RESULT:
			{
				if(this.child.isFinished()) {

					GameObject.Destroy(this.child);
					this.child = null;

					this.step.set_next(STEP.FINISH);
				}
			}
			break;
		}
				
		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.VS_BOSS:
				{
					this.child = this.gameObject.AddComponent<BossLevelSequenceBoss>();
					this.child.parent = this;

					this.child.createDebugWindow(this.window);

					this.child.start();

					Navi.get().dispatchPlayerMarker();
				}
				break;

				case STEP.CAKE_BIKING:
				{
					this.child = this.gameObject.AddComponent<BossLevelSequenceCake>();
					this.child.parent = this;

					this.child.createDebugWindow(this.window);

					this.child.start();
				}
				break;

				case STEP.RESULT:
				{
					this.child = this.gameObject.AddComponent<BossLevelSequenceResult>();
					this.child.parent = this;

					this.child.createDebugWindow(this.window);

					this.child.start();
				}
				break;

				case STEP.FINISH:
				{
					// Network 클래스의 컴포넌트 획득.
					GameObject	obj = GameObject.Find("Network");
					Network		network = null;	

					if(obj != null) {
					
						network = obj.GetComponent<Network>();
					}
	
					if (network != null) {

					if (GameRoot.get().isHost()) {
							network.StopGameServer();
						}
						
						network.StopServer();
						network.Disconnect();
						GameObject.Destroy(network);
					}

					GameRoot.get().setNextScene("TitleScene");
				}
				break;

			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.VS_BOSS:
			{
				this.child.execute();
			}
			break;

			case STEP.CAKE_BIKING:
			{
				this.child.execute();
			}
			break;

			case STEP.RESULT:
			{
				this.child.execute();
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

}
