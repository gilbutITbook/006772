using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameObjectExtension;
using MathExtension;

// 이사 종료 이벤트.
public class HouseMoveEndEvent : EventBase {


	// ================================================================ //

	public enum STEP {

		NONE = -1,

		IDLE = 0,			// 실행 중이 아닙니다.

		START,				// 이벤트 시작.
		HOUSE_TURN,			// 집이 빙글 돕니다.
		OPEN_DOOR,			// 문이 열립니다.
		EXIT,				// 집 밖으로 나옵니다.
		EXIT_CAT,			// 고양이가 집 밖으로 나옵니다.
		CLOSE_DOOR,			// 문이 닫힙니다.

		END,				// 이벤트 종료.

		NUM,
	};
	protected Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //

	protected chrBehaviorPlayer	player          = null;
	protected bool				is_local_player = true;

	protected chrBehaviorNPC_House	house = null;
	protected ItemBehaviorCat		cat   = null;

	protected ipModule.Simple2Points	player_move  = new ipModule.Simple2Points();
	protected ipModule.Simple2Points	cat_move     = new ipModule.Simple2Points();
	protected ipModule.Jump				cat_jump     = new ipModule.Jump();

	protected ipModule.FCurve			house_fcurve = new ipModule.FCurve();

	protected Vector3	initial_player_position;
	
	
	// ================================================================ //

	public HouseMoveEndEvent()
	{
	}

	public override void	initialize()
	{
		var		cat_go = this.house.gameObject.findChildGameObject("Cat");

		if(cat_go != null) {

			this.cat = cat_go.GetComponentInChildren<ItemBehaviorCat>();
		}

		// ---------------------------------------------------------------- //
		// 디버그할 때를 위해(원래 필요 없음).
		{
			if(this.cat == null) {
		
				this.cat = ItemManager.getInstance().findItem("Cat").behavior as ItemBehaviorCat;
			}

			// 고양이.
			if(this.cat != null) {
	
				// 툇마루에 앉힙니다.
				this.cat.controll.setVisible(true);
				this.cat.controll.cmdSetCollidable(false);
				this.cat.controll.setParent(this.house.controll.gameObject);		
				this.cat.controll.transform.localPosition = new Vector3(0.06996871f, 0.2095842f, -0.4440203f);
				this.cat.controll.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.up);
				this.cat.controll.transform.localScale    = Vector3.one*0.6f;
			}

			this.house.transform.rotation = Quaternion.AngleAxis(315.0f, Vector3.up);
		}
		// 여기까지~ 디버그용.
		// ---------------------------------------------------------------- //

		this.step.set_next(STEP.IDLE);
		this.execute();
	}

	public override void	execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크합니다.

		switch(this.step.do_transition()) {

			// 이벤트 시작.
			case STEP.START:
			{
				this.step.set_next(STEP.HOUSE_TURN);
			}
			break;

			// 집이 빙글 돕니다.
			case STEP.HOUSE_TURN:
			{
				if(!this.house_fcurve.isMoving()) {

					this.step.set_next_delay(STEP.OPEN_DOOR, 0.2f);
				}
			}
			break;

			// 현관 문이 열립니다.
			case STEP.OPEN_DOOR:
			{
				if(!this.house.controll.isMotionPlaying()) {

					this.step.set_next_delay(STEP.EXIT, 0.2f);
				}
			}
			break;

			// 집 밖으로 나옵니다.
			case STEP.EXIT:
			{
				if(this.player_move.isDone()) {

					this.step.set_next(STEP.EXIT_CAT);
				}
			}
			break;

			// 고양이가 집 밖으로 나옵니다.
			case STEP.EXIT_CAT:
			{
				if(Vector3.Distance(this.cat.controll.getPosition(), this.player.controll.getPosition()) <= 1.0f) {

					this.player.controll.item_carrier.beginCarry(this.cat.controll);
					this.step.set_next(STEP.CLOSE_DOOR);
				}
			}
			break;

			// 문이 닫힙니다.
			case STEP.CLOSE_DOOR:
			{
				if(!this.house.controll.isMotionPlaying()) {

					this.step.set_next_delay(STEP.END, 0.2f);
				}
			}
			break;


			case STEP.END:
			{
				this.step.set_next(STEP.IDLE);
			}
			break;

		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				// 이벤트 시작.
				case STEP.START:
				{
					this.player.beginOuterControll();
					this.player.GetComponent<Rigidbody>().isKinematic = true;

					this.house.controll.setParent(null);
				}
				break;

				// 집이 빙글 돕니다.
				case STEP.HOUSE_TURN:
				{
					this.house_fcurve.setSlopeAngle(70.0f, 0.0f);
					this.house_fcurve.setDuration(1.5f);
					this.house_fcurve.start();
				}
				break;

				// 현관 문이 열립니다..
				case STEP.OPEN_DOOR:
				{
					this.house.openDoor();
				}
				break;

				// 집 밖으로 나옵니다.
				case STEP.EXIT:
				{
					Vector3		start = this.house.controll.getPosition();
					Vector3		goal  = this.house.controll.transform.TransformPoint(Vector3.forward*2.0f);

					start = start + (goal - start).normalized*HouseMoveStartEvent.HIDE_DISTANCE;

					this.player.controll.cmdSetDirection(this.house.controll.getDirection() + 180.0f);
					this.player.controll.setVisible(true);
					this.player_move.position.start = start;
					this.player_move.position.goal  = goal;
					this.player_move.startConstantVelocity(chrBehaviorLocal.MOVE_SPEED);
				}
				break;

				// 고양이가 집 밖으로 나옵니다.
				case STEP.EXIT_CAT:
				{
					// 고양이.
					if(this.cat != null) {

						Vector3		start = this.house.controll.getPosition();
						Vector3		goal  = this.house.controll.transform.TransformPoint(Vector3.forward*2.0f);

						this.cat.controll.transform.localScale = Vector3.one;
						this.cat.controll.cmdSetDirection(this.house.controll.getDirection() + 180.0f);
						this.cat.controll.setParent(null);

						this.cat_move.position.start = start;
						this.cat_move.position.goal  = goal;
						this.cat_move.startConstantVelocity(chrBehaviorLocal.MOVE_SPEED);
					}
				}
				break;

				// 문이 닫힙니다.
				case STEP.CLOSE_DOOR:
				{
					this.house.closeDoor();
				}
				break;

				case STEP.END:
				{
					this.player.endOuterControll();
					this.player.GetComponent<Rigidbody>().isKinematic = false;

					this.player.endHouseMove();

					this.house.endHouseMove();
				}
				break;

			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 집이 빙들 돕니다.
			case STEP.HOUSE_TURN:
			{
				this.house_fcurve.execute(Time.deltaTime);

				float	y_angle = Mathf.LerpAngle(315.0f, 135.0f, this.house_fcurve.getValue());

				this.house.transform.rotation = Quaternion.AngleAxis(y_angle, Vector3.up);
			}
			break;

			//현관 문이 열립니다.
			case STEP.OPEN_DOOR:
			{
			}
			break;

			// 집 밖으로 나옵니다.
			case STEP.EXIT:
			{
				this.player_move.execute(Time.deltaTime);
				this.player.controll.cmdSetPosition(this.player_move.position.current);
				this.player.controll.cmdSmoothHeadingTo(this.player_move.position.goal);

				if(!this.player_move.isDone()) {

					this.player.playWalkMotion();

				} else {

					this.player.stopWalkMotion();
				}
			}
			break;

			// 고양이가 밖으로 나옵니다.
			case STEP.EXIT_CAT:
			{
				// 고양이.
				if(this.cat != null) {

					this.cat_move.execute(Time.deltaTime);
					this.cat.controll.gameObject.setLocalPosition(this.cat_move.position.current);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

	public override  void		onGUI()
	{
	}

	// 이벤트가 실행중?.
	public override bool	isInAction()
	{
		bool	ret = !(this.step.get_current() == STEP.IDLE && this.step.get_next() == STEP.NONE);

		return(ret);
	}

	// 이벤트 시작.
	public override void	start()
	{
		if(this.player != null) {

			this.initial_player_position = this.player.transform.position;

			this.step.set_next(STEP.START);
		}
	}

	// ================================================================ //

	// 주역이 될 플레이어(로컬/리모트)를 설정합니다.
	public void		setPrincipal(chrBehaviorPlayer player)
	{
		this.player = player;
	}

	// 집을 설정합니다..
	public void		setHouse(chrBehaviorNPC_House house)
	{
		this.house = house;
	}

	// 로컬 플레이어가 주역인가?.
	public void		setIsLocalPlayer(bool is_local_player)
	{
		this.is_local_player = is_local_player;
	}

	// ================================================================ //
}
