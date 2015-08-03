using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameObjectExtension;
using MathExtension;

// 이사 시작 이벤트.
public class HouseMoveStartEvent : EventBase {


	// ================================================================ //

	public enum STEP {

		NONE = -1,

		IDLE = 0,			// 실행 중이 아닙니다.

		START,				// 이벤트 시작.
		WALK_TO_DOOR,		// 현관 앞까지 걷습니다.
		OPEN_DOOR,			// 문이 열립니다.
		ENTER,				// 집 안에 들어갑니다.
		CLOSE_DOOR,			// 문이 닫힙니다.
		HOUSE_TURN,			// 집이 빙글 돕니다.
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

	public HouseMoveStartEvent()
	{
	}

	public override void	initialize()
	{
		this.cat = this.player.controll.getCarriedItem<ItemBehaviorCat>();
			
		// 디버그 용.

		if(this.cat == null) {

			this.cat = ItemManager.getInstance().findItem("Cat").behavior as ItemBehaviorCat;

			this.player.controll.item_carrier.beginCarryAnon(this.cat.controll);
		}

		this.step.set_next(STEP.IDLE);
		this.execute();
	}

	public const float	HIDE_DISTANCE = 0.5f;	// 집의 중심에서 이 거리 이하가 되면 캐릭터를 표시하지 않습니다.

	public override void	execute()
	{

		float	enter_hide_distance = HIDE_DISTANCE;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이행할지 체크합니다.

		switch(this.step.do_transition()) {

			// 이벤트 시작.
			case STEP.START:
			{
				this.step.set_next(STEP.WALK_TO_DOOR);
			}
			break;

			// 현관 앞까지 걷습니다.
			case STEP.WALK_TO_DOOR:
			{
				if(!this.player_move.isMoving()) {

					this.player.stopWalkMotion();
					this.step.set_next(STEP.OPEN_DOOR);
				}
			}
			break;

			// 현관 문이 열립니다.
			case STEP.OPEN_DOOR:
			{
				if(!this.house.controll.isMotionPlaying()) {

					this.step.set_next_delay(STEP.ENTER, 0.2f);
				}
			}
			break;

			// 집 안으로 들어갑니다.
			case STEP.ENTER:
			{
				do {

					if(Vector3.Distance(this.player.controll.getPosition(), this.house.controll.getPosition()) >= enter_hide_distance) {

						break;
					}
					if(Vector3.Distance(this.cat.controll.transform.position, this.house.controll.getPosition()) >= enter_hide_distance) {

						break;
					}
					if(this.cat_jump.isMoving()) {

						break;
					}
					if(this.cat_move.isMoving()) {

						break;
					}

					this.player.controll.setVisible(false);
					this.step.set_next_delay(STEP.CLOSE_DOOR, 0.5f);

				} while(false);
			}
			break;

			// 문이 닫힙니다.
			case STEP.CLOSE_DOOR:
			{
				if(!this.house.controll.isMotionPlaying()) {

					this.step.set_next_delay(STEP.HOUSE_TURN, 0.2f);
				}
			}
			break;

			// 집이 빙글 돕니다.
			case STEP.HOUSE_TURN:
			{
				if(!this.house_fcurve.isMoving()) {

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
		// 상태가 전환되면 초기화합니다.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				// 이벤트 시작 .
				case STEP.START:
				{
					this.player.beginOuterControll();
					this.player.GetComponent<Rigidbody>().isKinematic = true;
				}
				break;

				// 현관 앞까지 걷습니다.
				case STEP.WALK_TO_DOOR:
				{
					this.player_move.position.start = this.player.controll.getPosition();
					this.player_move.position.goal  = this.house.transform.TransformPoint(Vector3.forward*1.5f);
					this.player_move.startConstantVelocity(chrBehaviorLocal.MOVE_SPEED);
				}
				break;

				// 현관 문이 열립니다.
				case STEP.OPEN_DOOR:
				{
					this.house.openDoor();
				}
				break;

				// 집 안으로 들어갑니다.
				case STEP.ENTER:
				{
					this.player_move.position.start = this.player.controll.getPosition();
					this.player_move.position.goal  = this.house.controll.getPosition();
					this.player_move.startConstantVelocity(chrBehaviorLocal.MOVE_SPEED);

					// 고양이.
					if(this.cat != null) {

						this.player.controll.item_carrier.endCarry();
						this.cat.controll.gameObject.setParent(this.house.gameObject);		
						this.cat.controll.cmdSetVisible(true);
						this.cat.controll.cmdSetCollidable(false);
			
						Vector3		start = this.cat.controll.transform.localPosition;
						Vector3		goal  = start.Y(0.0f);

						this.cat_jump.setBounciness(-0.5f*Vector3.up);	
						this.cat_jump.start(start, goal, start.y);

						this.cat_move.reset();
					}
				}
				break;

				// 문이 닫힙니다.
				case STEP.CLOSE_DOOR:
				{
					this.house.closeDoor();
				}
				break;

				// 집이 빙글 돕니다.
				case STEP.HOUSE_TURN:
				{
					this.house_fcurve.setSlopeAngle(70.0f, 0.0f);
					this.house_fcurve.setDuration(1.5f);
					this.house_fcurve.start();

					// 고양이.
					// 만일을 위해.
					if(this.cat != null) {

						// 툇마루에 앉힙니다.
						this.cat.controll.setVisible(true);			
						this.cat.controll.transform.localPosition = new Vector3(0.06996871f, 0.2095842f, -0.4440203f);
						this.cat.controll.transform.localRotation = Quaternion.AngleAxis(0.0f, Vector3.up);
						this.cat.controll.transform.localScale    = Vector3.one*0.6f;
					}
				}
				break;

				case STEP.END:
				{
					this.player.beginHouseMove(this.house);

					// '이사 중~'이라는 말풍선 표시.
					this.house.startHouseMove();
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 현관 앞까지 걷습니다.
			case STEP.WALK_TO_DOOR:
			{
				this.player_move.execute(Time.deltaTime);
				this.player.controll.cmdSetPosition(this.player_move.position.current);
				this.player.controll.cmdSmoothHeadingTo(this.player_move.position.goal);
				this.player.playWalkMotion();
			}
			break;

			//현관 문이 열립니다.
			case STEP.OPEN_DOOR:
			{
				this.player.controll.cmdSmoothHeadingTo(this.house.transform.position);

			}
			break;

			// 집 안으로 들어갑니다.
			case STEP.ENTER:
			{
				this.player_move.execute(Time.deltaTime);
				this.player.controll.cmdSetPosition(this.player_move.position.current);
				this.player.controll.cmdSmoothHeadingTo(this.player_move.position.goal);

				if(Vector3.Distance(this.player.controll.getPosition(), this.house.controll.getPosition()) > enter_hide_distance) {

					this.player.playWalkMotion();

				} else {

					this.player.controll.setVisible(false);
					this.player.stopWalkMotion();
				}

				// 고양이.
				if(this.cat != null) {

					this.cat_jump.execute(Time.deltaTime);

					this.cat.controll.gameObject.setLocalPosition(this.cat_jump.position);

					if(this.cat_move.isStarted()) {

						this.cat_move.execute(Time.deltaTime);
						this.cat.controll.gameObject.setLocalPosition(this.cat_move.position.current.Y(this.cat_jump.position.y));
						//this.cat.controll.cmdSmoothHeadingTo(this.cat_move.position.goal);

						if(Vector3.Distance(this.cat.controll.transform.position, this.house.controll.getPosition()) <= enter_hide_distance) {

							this.cat.controll.setVisible(false);
						}

					} else {

						if(this.cat_jump.is_trigger_bounce) {

							this.cat_move.position.start = this.cat.controll.gameObject.transform.localPosition;
							this.cat_move.position.goal  = Vector3.zero;
							this.cat_move.startConstantVelocity(chrBehaviorLocal.MOVE_SPEED);
						}
					}
				}
			}
			break;

			// 집이 빙글 돕니다.
			case STEP.HOUSE_TURN:
			{
				this.house_fcurve.execute(Time.deltaTime);

				float	y_angle = Mathf.LerpAngle(135.0f, 315.0f, this.house_fcurve.getValue());

				this.house.transform.rotation = Quaternion.AngleAxis(y_angle, Vector3.up);
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

	public override  void		onGUI()
	{
	}

	// 이벤트 실행 중?.
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

	// 집을 설정합니다.
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
