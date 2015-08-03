using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameObjectExtension;
using MathExtension;


// 도착 이벤트.
public class EnterEvent : EventBase {


	// ================================================================ //

	public enum STEP {

		NONE = -1,

		IDLE = 0,			// 실행 중이 아님.

		START,				// 이벤트 시작.
		OPEN_DOOR,			// 배추가 옆으로 움직이고 & 대야를 타고 플레이어가 등장.
		GET_OFF_TARAI_0,	// 대야에서 내립니다(기슭으로 점프).
		GET_OFF_TARAI_1,	// 대야에서 내립니다(조금 걷기).
		CLOSE_DOOR,			// 배추가 돌아오고 & 대야가 밖으로 이동.
		END,				// 이벤트 종료.

		NUM,
	};
	protected Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //

	protected HakusaiSet	hakusai_set;

	protected GameObject	tarai_fune;

	protected chrBehaviorPlayer	player = null;
	protected bool				is_local_player = true;

	protected ipModule.Simple2Points	player_move  = new ipModule.Simple2Points();

	protected ipModule.Jump		tarai_jump  = new ipModule.Jump();
	protected ipModule.Jump		player_jump = new ipModule.Jump();

	protected Vector3	initial_player_position;

	protected SimpleSplineObject	tarai_enter_spline = null;					// 대야가 등장할 때의 이동 경로.
	protected SimpleSplineObject	tarai_leave_spline = null;					// 대야가 화면 밖으로 빠져나갈 때의 이동 경로.
	protected SimpleSpline.Tracer	tarai_tracer = new SimpleSpline.Tracer();
	protected ipModule.FCurve		tarai_fcurve = new ipModule.FCurve();
	
	protected SimpleSplineObject	hakusai_spline = null;
	protected SimpleSpline.Tracer	hakusai_tracer = new SimpleSpline.Tracer();
	protected ipModule.FCurve		hakusai_fcurve = new ipModule.FCurve();

	// 대야 뒤에 나오는 물결.
	protected LeaveEvent.RippleEffect	ripple_effect = new LeaveEvent.RippleEffect();

	// ================================================================ //

	public EnterEvent()
	{
	}

	public override void	initialize()
	{
		this.hakusai_set = new HakusaiSet();
		this.hakusai_set.attach();

		this.tarai_fune = GameObject.Find("tarai_boat").gameObject;

		GameObject		map_go = GameObject.Find(MapCreator.get().getCurrentMapName()).gameObject;

		this.data_holder = map_go.transform.FindChild("LeaveEventData").gameObject;

		this.tarai_enter_spline = this.data_holder.gameObject.findDescendant("tarai_enter_spline").GetComponent<SimpleSplineObject>();
		this.tarai_leave_spline = this.data_holder.gameObject.findDescendant("tarai_leave_spline").GetComponent<SimpleSplineObject>();
		this.tarai_tracer.attach(this.tarai_leave_spline.curve);
		
		this.hakusai_spline = this.data_holder.gameObject.findDescendant("hakusai_spline").GetComponent<SimpleSplineObject>();
		this.hakusai_tracer.attach(this.hakusai_spline.curve);

		this.step.set_next(STEP.IDLE);
		this.execute();
	}

	public override void	execute()
	{

		CameraControl		camera = CameraControl.get();

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크합니다.

		switch(this.step.do_transition()) {

			// 이벤트 시작.
			case STEP.START:
			{
				this.step.set_next(STEP.OPEN_DOOR);
				//camera.module.parallelMoveTo(this.get_locator_position("cam_loc_0"));

				Debug.Log("Name:" + this.player.controll.account_name);

				foreach (ItemManager.ItemState istate in GlobalParam.get().item_table.Values) {

					Debug.Log("Item:" + istate.item_id + " Own:" + istate.owner + " State:" + istate.state);

					if (istate.owner == this.player.controll.account_name &&
				    	istate.state == ItemController.State.Picked) {
						// 이미 아이템을 획득했다면 가지고 갈 수 있게 합니다.
						ItemManager.get().activeItme(istate.item_id, true);
						ItemManager.get().finishGrowingItem(istate.item_id);
						QueryItemPick query = this.player.controll.cmdItemQueryPick(istate.item_id, false, true);
						if (query != null) {
							query.is_anon = true;
							query.set_done(true);
							query.set_success(true);
						}
						ItemManager.get().setVisible(istate.item_id, true);
					}
				}

				// 리모트에서 이사 중은 로컬도 이사합니다.
				do {

					MovingData moving = GlobalParam.get().remote_moving;
					if(!moving.moving) {

						break;
					}

					chrController remote = CharacterRoot.get().findCharacter(moving.characterId);
					if(remote == null) {

						break;
					}

					chrBehaviorNet	remote_player = remote.behavior as chrBehaviorNet;
					if(remote_player == null) {

						break;
					}

					chrBehaviorNPC_House	house = CharacterRoot.getInstance().findCharacter<chrBehaviorNPC_House>(moving.houseId);
					if(house == null) {

						break;
					}

					Debug.Log("House move event call:" + moving.characterId + ":" + moving.houseId);

					remote_player.beginHouseMove(house);

					// '이사중~' 말풍선 표시.
					house.startHouseMove();

				} while(false);	
			}
			break;

			// 배추가 옆으로 움직이고 & 대야를 타고 플레이어가 등장.
			case STEP.OPEN_DOOR:
			{
				if(this.hakusai_fcurve.isDone() && this.tarai_fcurve.isDone()) {
					
					this.step.set_next(STEP.GET_OFF_TARAI_0);
				}
			}
			break;

			// 대야에서 내립니다(기슭으로 점프).
			case STEP.GET_OFF_TARAI_0:
			{
				if(!this.player_jump.isMoving()) {

					this.step.set_next(STEP.GET_OFF_TARAI_1);
				}
			}
			break;

			// 대야에서 내립니다(조금 걷기).
			case STEP.GET_OFF_TARAI_1:
			{
				if(!this.player_move.isMoving()) {

					this.step.set_next(STEP.CLOSE_DOOR);
				}
			}
			break;


			// 배추가 돌아오고 & 대야가 밖으로 이동.
			case STEP.CLOSE_DOOR:
			{
				if(this.hakusai_fcurve.isDone() && this.tarai_fcurve.isDone()) {
					
					this.step.set_next(STEP.END);
				}
			}
			break;

			case STEP.END:
			{
				camera.module.popPosture();

				this.step.set_next(STEP.IDLE);
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			dbwin.console().print(this.step.ToString());

			switch(this.step.do_initialize()) {
	
				// 이벤트 시작.
				case STEP.START:
				{
					camera.module.pushPosture();

					this.player.beginOuterControll();
					this.player.controll.cmdSetPosition(this.tarai_leave_spline.curve.cvs.back().position);
				
					this.tarai_fune.transform.position = this.tarai_leave_spline.curve.cvs.back().position;
				
					if(!this.is_local_player) {

						SoundManager.get().playSE(Sound.ID.SMN_JINGLE01);
					}
				}
				break;

				// 배추가 옆으로 이동하고 & 대야를 타고 플레이어가 등장.
				case STEP.OPEN_DOOR:
				{
					this.hakusai_set.setControl(true);
					this.hakusai_fcurve.setSlopeAngle(10.0f, 10.0f);
					this.hakusai_fcurve.setDuration(4.0f);
					this.hakusai_fcurve.start();
					this.hakusai_tracer.restart();

					this.tarai_fcurve.setSlopeAngle(60.0f, 5.0f);
					this.tarai_fcurve.setDuration(3.5f);
					this.tarai_fcurve.setDelay(0.5f);
					this.tarai_fcurve.start();
				}
				break;

				// 대야에서 내립니다(기슭으로 점프).
				case STEP.GET_OFF_TARAI_0:
				{
					Vector3		start = this.player.controll.getPosition();
					Vector3		goal  = this.get_locator_position("chr_loc_0");

					this.player_jump.start(start, goal, 1.0f);

				}
				break;

				// 대야에서 내립니다(조금 걷기).
				case STEP.GET_OFF_TARAI_1:
				{
					this.player_move.position.start = this.player.controll.getPosition();
					this.player_move.position.goal  = this.get_locator_position("chr_loc_1");
					this.player_move.startConstantVelocity(chrBehaviorLocal.MOVE_SPEED);
				}
				break;

				// 배추가 돌아오고 & 대야가 밖으로 이동.
				case STEP.CLOSE_DOOR:
				{
					this.hakusai_fcurve.setSlopeAngle(10.0f, 10.0f);
					this.hakusai_fcurve.setDuration(4.0f);
					this.hakusai_fcurve.setDelay(1.0f);
					this.hakusai_fcurve.start();
					this.hakusai_tracer.restart();
					this.hakusai_tracer.setCurrentByDistance(this.hakusai_tracer.curve.calcTotalDistance());
				
					this.tarai_tracer.attach(this.tarai_enter_spline.curve);
					this.tarai_tracer.restart();
					this.tarai_fcurve.reset();
					this.tarai_fcurve.setSlopeAngle(10.0f, 60.0f);
					this.tarai_fcurve.setDuration(2.5f);
					this.tarai_fcurve.start();
				
					this.ripple_effect.is_created = false;
				}
				break;

				case STEP.END:
				{
					// 이벤트 종료.
					this.hakusai_set.reset();
					this.hakusai_set.setControl(false);

					this.player.endOuterControll();

					this.player = null;
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 배추가 옆으로 움직인다 & 대야를 타고 플레이어가 등장..
			case STEP.OPEN_DOOR:
			{
				this.hakusai_fcurve.execute(Time.deltaTime);
				this.hakusai_tracer.proceedToDistance(this.hakusai_tracer.curve.calcTotalDistance()*this.hakusai_fcurve.getValue());
				this.hakusai_set.setPosition(this.hakusai_tracer.cv.position);
			
				this.tarai_fcurve.execute(Time.deltaTime);
				this.tarai_tracer.proceedToDistance(this.tarai_tracer.curve.calcTotalDistance()*(1.0f - this.tarai_fcurve.getValue()));

				SimpleSpline.ControlVertex	cv = this.tarai_tracer.getCurrent();
				
				this.tarai_fune.setPosition(cv.position);
				this.player.controll.cmdSetPosition(cv.position);
				this.player.controll.cmdSmoothHeadingTo(cv.position - cv.tangent.Y(0.0f));

				if(this.tarai_fcurve.isTriggerDone()) {
					
					this.ripple_effect.is_created = false;
				}
			}
			break;

			// 대야에서 내립니다(기슭을 향해 점프).
			case STEP.GET_OFF_TARAI_0:
			{
				this.player_jump.execute(Time.deltaTime);
				this.player.controll.cmdSetPosition(this.player_jump.position);
				this.player.controll.cmdSmoothHeadingTo(this.player_jump.goal);
			}
			break;

			// 대야에서 내립니다(조금 걷는다).
			case STEP.GET_OFF_TARAI_1:
			{
				this.player_move.execute(Time.deltaTime);
				this.player.controll.cmdSetPosition(this.player_move.position.current);
				this.player.controll.cmdSmoothHeadingTo(this.player_move.position.goal);
				this.player.playWalkMotion();
			}
			break;

			// 배추가 돌아옵니다 & 대야가 밖으로 이동.
			case STEP.CLOSE_DOOR:
			{
				this.hakusai_fcurve.execute(Time.deltaTime);
				this.hakusai_tracer.proceedToDistance(this.hakusai_tracer.curve.calcTotalDistance()*(1.0f - this.hakusai_fcurve.getValue()));
				this.hakusai_set.setPosition(this.hakusai_tracer.getCurrent().position);
			
				this.tarai_fcurve.execute(Time.deltaTime);
				this.tarai_tracer.proceedToDistance(this.tarai_tracer.curve.calcTotalDistance()*(1.0f - this.tarai_fcurve.getValue()));
				this.tarai_fune.transform.position = this.tarai_tracer.getCurrent().position;
				this.player.stopWalkMotion();
			}
			break;
		}

		// 대야 뒤에 나오는 물결.
		if(!this.ripple_effect.is_created || Vector3.Distance(this.ripple_effect.last_position, this.tarai_fune.getPosition()) > 2.0f) {
			
			this.ripple_effect.is_created = true;
			this.ripple_effect.last_position = this.tarai_fune.transform.position;
			
			EffectRoot.get().createRipple(this.ripple_effect.last_position);
		}

		// ---------------------------------------------------------------- //
	}

	public override void		onGUI()
	{
	}

	// 이벤트가 실행 중?.
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

	// 로컬 플레이어가 주역?.
	public void		setIsLocalPlayer(bool is_local_player)
	{
		this.is_local_player = is_local_player;
	}

	// ================================================================ //

	// 배추와 물결 효과.
	protected class HakusaiSet {

		public TransformModifier	hakusai;
		public TransformModifier	nami0;
		public TransformModifier	nami1;

		public void attach()
		{
			this.hakusai = GameObject.Find("hakusai2").gameObject.AddComponent<TransformModifier>();
			this.nami0   = GameObject.Find("nami_00").gameObject.AddComponent<TransformModifier>();
			this.nami1   = GameObject.Find("nami_01").gameObject.AddComponent<TransformModifier>();

			this.hakusai.setWriteMask("xz");
		}

		public void detach()
		{
			GameObject.Destroy(this.hakusai);
			GameObject.Destroy(this.nami0);
			GameObject.Destroy(this.nami1);
		}

		public void setControl(bool is_control)
		{
			this.hakusai.setControl(is_control);
			this.nami0.setControl(is_control);
			this.nami1.setControl(is_control);
		}

		public void	setPosition(Vector3 position)
		{
			this.hakusai.setPosition(position);
			this.nami0.setPosition(this.nami0.getInitialPosition() - this.hakusai.getInitialPosition() + position);
			this.nami1.setPosition(this.nami1.getInitialPosition() - this.hakusai.getInitialPosition() + position);
		}

		public void reset()
		{
			this.hakusai.reset();
			this.nami0.reset();
			this.nami1.reset();
		}
	};
}
