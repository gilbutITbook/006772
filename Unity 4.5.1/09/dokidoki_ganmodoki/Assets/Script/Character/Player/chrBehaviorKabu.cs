using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 비헤이비어-　NPC용.
public class chrBehaviorKabu : chrBehaviorBase {


	public enum STEP {

		NONE = -1,

		IDLE = 0,			// 등장.
		MOVE,				// 플레이어 뒤를 쫒아 이동 중.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	protected ipModule.Hover	hover = new ipModule.Hover();

	protected chrBehaviorLocal	player;
	protected CameraControl		camera_control;
	protected float				radius;

	protected Vector3			door_position;			// 도어 위치.
	protected bool				is_in_event = false;	// 플로어 이동 이벤트 중?.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// 데모에서 대사를 말할 때의 위치.
	public static Vector3	getStayPosition()
	{
		return(new Vector3(0.0f, 3.0f, -5.0f));
	}

	// 데모에서 대사를 말할 때의 방향.
	public static float		getStayDirection()
	{
		return(180.0f);
	}

	// 데모에서 등장할 때의 높이.
	public static float	getStartHeight()
	{
		return(6.0f);
	}

	// 플로어 이동 이벤트가 시작되었을 때 호출된다.
	public void		onBeginTransportEvent()
	{
		TransportEvent	ev = EventRoot.get().getCurrentEvent<TransportEvent>();

		this.door_position = ev.getDoor().getPosition();
		this.is_in_event   = true;
	}

	// ================================================================ //

	// 생성 직후에 호출된다.
	public override sealed void	initialize()
	{
	}

	// 게임 시작 시에 한 번만 호출된다.
	public override void	start()
	{
		this.player = PartyControl.get().getLocalPlayer();

		this.camera_control = CameraControl.get();

		// ---------------------------------------------------------------- //

		this.control.rigidbody.isKinematic = true;
		this.control.rigidbody.Sleep();

		this.control.setVisible(false);

		this.hover.gravity     = Physics.gravity.y*0.1f;
		this.hover.accel_scale = 2.0f;

		this.hover.zero_level = 3.0f;
		this.hover.hard_limit.max = 100.0f;

		this.radius = 5.0f;

		//

		this.step.set_next(STEP.IDLE);
	}

	// 매 프레임 호출된다.
	public override	void	execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.


		switch(this.step.do_transition()) {

			case STEP.IDLE:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.IDLE:
				{
					Vector3		stay_position = chrBehaviorKabu.getStayPosition();
					float		start_height  = chrBehaviorKabu.getStartHeight();

					this.hover.height      = stay_position.y + start_height;
					this.hover.gravity     = Physics.gravity.y*2.0f;
					this.hover.accel_scale = 1.5f;

					this.control.setVisible(true);

					this.control.cmdSetPositionAnon(stay_position);
					this.control.cmdSetDirectionAnon(chrBehaviorKabu.getStayDirection());
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.IDLE:
			{
				this.hover.execute(Time.deltaTime);

				// 낙하가 멈춘 곳에서 파라미터를 바꾼다.
				// (천천히 움직이고 싶으므로).		
				if(this.hover.trigger_down_peak) {

					this.hover.gravity     = Physics.gravity.y*0.1f;
					this.hover.accel_scale = 2.0f;
				}

				Vector3		position = this.control.getPosition();
		
				position.y = this.hover.height;
		
				this.control.cmdSetPosition(position);
			}
			break;

			case STEP.MOVE:
			{
				this.execute_step_move();
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

	protected void	execute_step_move()
	{
		this.hover.execute(Time.deltaTime);

		Vector3		position = this.control.getPosition();
		Vector3		next_position = position;

		// ---------------------------------------------------------------- //

		Vector3		camera_position = this.camera_control.getModule().getPosture().position;
		Vector3		player_position = this.player.control.getPosition();

		if(this.is_in_event) {

			player_position = this.door_position;
		}

		float	rate = (this.control.getPosition().y - player_position.y)/(camera_position.y - player_position.y);

		Vector3	interest = Vector3.Lerp(player_position, camera_position, rate);

		// ---------------------------------------------------------------- //

		Vector3		v = position - interest;

		float		current_distance = v.magnitude;

		current_distance = Mathf.Lerp(current_distance, this.radius, 0.05f);

		v.Normalize();
		v *= current_distance;

		next_position = interest + v;

		next_position.y = this.hover.height;

		this.control.cmdSetPosition(next_position);
		this.control.cmdSmoothHeadingTo(interest);
	}

	// ================================================================ //

	public void		beginMove()
	{
		this.step.set_next(STEP.MOVE);
	}
}