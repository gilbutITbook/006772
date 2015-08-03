using UnityEngine;
using System.Collections;

public class NormalLevelSequence : SequenceBase {

	public enum STEP {

		NONE = -1,

		IDLE = 0,
		IN_ACTION,
		TRANSPORT,		// 방 이동 이벤트 중.
		READY,			// "レディ！" 표시
		RESTART,		// 쓰러진 후 재시작.

		WAIT_FINISH,
		FINISH,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	//===================================================================

	// 디버그 창 생성 시에 호출.
	public override void		createDebugWindow(dbwin.Window window)
	{
		window.createButton("다음 플로어로")
			.setOnPress(() =>
			{
				switch(this.step.get_current()) {

					case STEP.IN_ACTION:
					{
						this.step.set_next(STEP.FINISH);
					}
					break;
				}
			});
	}

	
	protected bool		is_floor_door_event = false;		// 룸 이동 이벤트의 도어가 플로어 이동 도어인가?.
	protected bool		is_first_ready = true;				// 레벨을 시작하고 최초의 STEP.READY

	// 쓰러진 후 재시작 위치.
	protected class RestartPoint {

		public Vector3		position  = Vector3.zero;
		public float		direction = 0.0f;
		public DoorControl	door      = null;
	}

	protected RestartPoint	restart_point = new RestartPoint();

	protected float			wait_counter = 0.0f;

	//===================================================================

	// 리벨 시작 시에 호출.
	public override void		start()
	{
		this.step.set_next(STEP.READY);
	}

	// 매 프레임 호출.
	public override void		execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크.

		switch(this.step.do_transition()) {

			case STEP.IN_ACTION:
			{
				var		player = PartyControl.get().getLocalPlayer();

				if(player.step.get_current() == chrBehaviorLocal.STEP.WAIT_RESTART) {

					// 체력이 0이 되면 재시작.

					player.start();
					player.control.cmdSetPositionAnon(this.restart_point.position);
					player.control.cmdSetDirectionAnon(this.restart_point.direction);

					if(this.restart_point.door != null) {

						this.restart_point.door.beginWaitLeave();
					}

					this.step.set_next(STEP.RESTART);

				} else {

					// 방 이동 이벤트 시작?.
	
					var	ev = EventRoot.get().getCurrentEvent<TransportEvent>();
	
					if(ev != null) {
	
						DoorControl	door = ev.getDoor();
	
						if(door.type == DoorControl.TYPE.FLOOR) {
	
							// 플로어 이동 도어일 때 .

							this.is_floor_door_event = true;
	
							ev.setEndAtHoleIn(true);
	
						} else {

							// 방 이동 도어일 때.
							this.restart_point.door = door.connect_to;

							this.is_floor_door_event = false;
						}
	
						this.step.set_next(STEP.TRANSPORT);
					}
				}
			}
			break;

			// 방 이동 이벤트 중.
			case STEP.TRANSPORT:
			{
				// 방 이동 이벤트가 끝나면 일반 모드로.

				var		ev = EventRoot.get().getCurrentEvent<TransportEvent>();

				wait_counter += Time.deltaTime;

				if(ev == null) {

					if(this.is_floor_door_event) {

						this.step.set_next(STEP.WAIT_FINISH);

					} else {

						this.step.set_next(STEP.READY);

						wait_counter = 0.0f;
					}

				} else {

					if(ev.step.get_current() == TransportEvent.STEP.READY) {

						this.step.set_next(STEP.READY);

						wait_counter = 0.0f;
					}
				}

			}
			break;

			// 'レディ!' 표시.
			// 쓰러졌을 때 재시작.
			case STEP.READY:
			case STEP.RESTART:
			{
				if(this.is_first_ready) {

					if(this.step.get_time() > 1.0f) {

						this.step.set_next(STEP.IN_ACTION);
						this.is_first_ready = false;
					}

				} else {

					var		ev = EventRoot.get().getCurrentEvent<TransportEvent>();
	
					if(ev == null) {
	
						this.step.set_next(STEP.IN_ACTION);
					}
				}
			}
			break;

			case STEP.WAIT_FINISH:
			{
				// 약긴 대기.
				wait_counter += Time.deltaTime;
				if (wait_counter > 3.0f) {
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
					Navi.get().dispatchPlayerMarker();

					LevelControl.get().endStillEnemies(null, 0.0f);
				}
				break;

				// 방 이동 이벤트 중.
				case STEP.TRANSPORT:
				{
					var current_room = PartyControl.get().getCurrentRoom();
					var next_room    = PartyControl.get().getNextRoom();

					// 다음 방에 적을 만든다.
					// 방 이동 이벤트 조료 시에 갑자기 적이 나타나지 않게.
					// 일짜김치 만들어 둔다.
					LevelControl.get().createRoomEnemies(next_room);

					LevelControl.get().beginStillEnemies(next_room);
					LevelControl.get().beginStillEnemies(current_room);
				}
				break;

				// 'レディ!표시.
				case STEP.READY:
				{
					var current_room = PartyControl.get().getCurrentRoom();
					var next_room    = PartyControl.get().getNextRoom();

					if(this.is_first_ready) {

						LevelControl.get().createRoomEnemies(current_room);
						LevelControl.get().beginStillEnemies(current_room);
						LevelControl.get().onEnterRoom(current_room);

					} else {

						LevelControl.get().onLeaveRoom(current_room);
						LevelControl.get().onEnterRoom(next_room);
					}

					// 'レディ!' 표시
					Navi.get().dispatchYell(YELL_WORD.READY);

					// 전에 있던 방의 적을 삭제..
					if(next_room != current_room) {

						LevelControl.get().deleteRoomEnemies(current_room);
					}

					ItemWindow.get().onRoomChanged(next_room);

					this.restart_point.position  = PartyControl.get().getLocalPlayer().control.getPosition();
					this.restart_point.direction = PartyControl.get().getLocalPlayer().control.getDirection();
				}
				break;

				// 쓰러진 후 재시작.
				case STEP.RESTART:
				{
					// 'レディ!' 표시.
					Navi.get().dispatchYell(YELL_WORD.READY);

					this.restart_point.position  = PartyControl.get().getLocalPlayer().control.getPosition();
					this.restart_point.direction = PartyControl.get().getLocalPlayer().control.getDirection();
				}
				break;

				case STEP.FINISH:
				{
					GameRoot.get().setNextScene("BossScene");
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

}
