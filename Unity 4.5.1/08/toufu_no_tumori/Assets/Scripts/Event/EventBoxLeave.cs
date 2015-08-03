using UnityEngine;
using System.Collections;

public class EventBoxLeave : MonoBehaviour {

	public enum STEP {

		NONE = -1,

		IDLE = 0,
		WAIT_ENTER,				// 실행 중이 아님.
		ENTERED,
		WAIT_LEAVE,
		LEAVE,

		NUM,
	};
	protected Step<STEP>	step = new Step<STEP>(STEP.NONE);

	protected chrBehaviorLocal	player = null;

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
		this.step.set_next(STEP.WAIT_ENTER);
	}
	
	void	Update()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크합니다.

		switch(this.step.do_transition()) {

			case STEP.WAIT_ENTER:
			{
				if(this.player != null) {

					this.step.set_next(STEP.ENTERED);
				}
			}
			break;

			case STEP.ENTERED:
			{
				if(YesNoAskDialog.get().isSelected()) {

					if(YesNoAskDialog.get().getSelection() == YesNoAskDialog.SELECTION.YES) {

						LeaveEvent	leave_event = EventRoot.get().startEvent<LeaveEvent>();

						if(leave_event != null) {
			
							leave_event.setPrincipal(this.player);
							leave_event.setIsLocalPlayer(true);

							// 정원 이동 요청 발행.
							if (GameRoot.get().net_player)
							{
								GameRoot.get().NotifyFieldMoving();
								GlobalParam.get().request_move_home = false;
							}
							else {
								GlobalParam.get().request_move_home = true;
							}

						}

						this.step.set_next(STEP.IDLE);

					} else {

						this.step.set_next(STEP.WAIT_LEAVE);
					}

				} else if(this.player == null) {

					this.step.set_next(STEP.LEAVE);
				}
			}
			break;

			case STEP.WAIT_LEAVE:
			{
				if(this.player == null) {

					this.step.set_next(STEP.LEAVE);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환되면 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.WAIT_ENTER:
				case STEP.IDLE:
				{
					this.player = null;

					YesNoAskDialog.get().close();
				}
				break;

				case STEP.ENTERED:
				{
					if(GlobalParam.get().is_in_my_home) {

						YesNoAskDialog.get().setText("친구 정원에 놀러갈까요?");
						YesNoAskDialog.get().setButtonText("간다", "안 간다");

					} else {

						YesNoAskDialog.get().setText("집에 돌아갈까요?");
						YesNoAskDialog.get().setButtonText("돌아간다", "더 논다");
					}
					YesNoAskDialog.get().dispatch();
				}
				break;

				case STEP.WAIT_LEAVE:
				{
					YesNoAskDialog.get().close();
				}
				break;

				case STEP.LEAVE:
				{
					YesNoAskDialog.get().close();

					this.step.set_next(STEP.WAIT_ENTER);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.WAIT_ENTER:
			{
			}
			break;
		}
	}

	void	OnTriggerEnter(Collider other)
	{
		do {

			var		player = other.gameObject.GetComponent<chrBehaviorLocal>();

			if(player == null) {

				break;
			}

			// 이사 중엔 맵 이동을 할 수 없습니다.
			if(player.isNowHouseMoving()) {

				break;
			}

			// 통신 상대와 접속할 때까지 이동할 수 없습니다.
			if(GameRoot.get().isConnected() == false) {

				break;
			}

			this.player = player;

		} while(false);
	}

	void	OnTriggerExit(Collider other)
	{
		do {

			var		player = other.gameObject.GetComponent<chrBehaviorLocal>();

			if(player == null) {

				break;
			}
			if(player != this.player) {

				break;
			}

			this.player = null;

		} while(false);
	}

	// ================================================================ //

	public void		activate()
	{
		this.step.set_next(STEP.WAIT_ENTER);
	}

	public void		deactivate()
	{
		this.step.set_next(STEP.IDLE);
	}
}
