using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameObjectExtension;
using MathExtension;

// 비헤이비어　로컬 플레이어용.
// 마우스로 컨트롤한다.
public class chrBehaviorLocal : chrBehaviorPlayer {

	public static float	MOVE_SPEED = 3.0f;

	private Vector3		move_target;			// 이동할 위치.
	//private string		serif = "";				// 대사（말풍선 안에 표시되는 텍스트）.

	protected string	move_target_item = "";	// 아이템을 목표로 이동하고 있을 때.

	protected string	collision = "";

	// 솎아낸 좌표를 보존.
	private List<CharacterCoord>	m_culling = new List<CharacterCoord>();
	// 현재 플롯의 인덱스.
	private int 		m_plotIndex = 0;
	// 정지 상태일 때는 데이터를 송신하지 않도록 한다.
	private Vector3		m_prev;


	// 3차 스플라인 보간으로 사용할 점의 수.
	private const int	PLOT_NUM = 4;

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 이동（멈춰있을 때도 포함).
		HOUSE_MOVE,			// 이사.
		OUTER_CONTROL,		// 외부 제어.

		WAIT_QUERY,			// 쿼리 대기.

		NUM,
	};
	Step<STEP>		step = new Step<STEP>(STEP.NONE);

	protected bool				is_within_house_move_event_box = false;		// 이사 시작 이벤트 박스에 들어왔는가？.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// 컬리전에 충돌하는 동안 호출되는 메소드.
	void 	OnCollisionStay(Collision other)
	{
		if(other.gameObject.tag == "Item" || other.gameObject.tag == "Charactor") {

			this.collision = other.gameObject.name;
		}
	}

	// ================================================================ //

	public override void	initialize()
	{
		this.move_target = this.transform.position;
	}
	public override void	start()
	{
		this.controll.balloon.setPriority(-1);

		// 게임 시작 직후에 EnterEvent가 시작되면, 여기서 next_step에
		// OuterControll이 설정되어 있다. 그때 덮어쓰지 않도록 
		// next == NONE의 체크를 넣는다.
		if(this.step.get_next() == STEP.NONE) {

			this.step.set_next(STEP.MOVE);
		}
	}
	public override	void	execute()
	{
		// 아이템과 캐릭터를 클릭했을 때
		//
		// 클릭한 아이템과 캐릭터의 컬리전에 부딪혔으면
		// 멈춘다.
		//
		if(this.move_target_item != "") {

			if(this.move_target_item == this.collision) {

				this.move_target = this.controll.getPosition();
			}
		}

		// ---------------------------------------------------------------- //
		// 조정이 끝난 쿼리 실행.

		base.execute_queries();

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크한다.

		switch(this.step.do_transition()) {

			case STEP.MOVE:
			{
			}
			break;

			case STEP.WAIT_QUERY:
			{
				if(this.controll.queries.Count <= 0) {

					this.step.set_next(STEP.MOVE);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.OUTER_CONTROL:
				{
					this.rigidbody.Sleep();
				}
				break;

				case STEP.MOVE:
				{
					this.move_target = this.transform.position;
				}
				break;

				case STEP.HOUSE_MOVE:
				{
					this.initialize_step_house_move();
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 결과.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.MOVE:
			{
				this.exec_step_move();
			}
			break;

			case STEP.HOUSE_MOVE:
			{
				this.execute_step_house_move();
			}
			break;
		}


		this.collision = "";

		// ---------------------------------------------------------------- //

		GameInput	gi = this.controll.game_input;

		if(gi.serif_text.trigger_on) {

			// 어미(장비 아이템 특전).

			ItemFavor	item_favor = this.controll.getItemFavor();

			gi.serif_text.text += item_favor.term_word;

			this.controll.cmdQueryTalk(gi.serif_text.text, true);
		}

		// ---------------------------------------------------------------- //
		// 10 프레임에 1회, 좌표를 네트워크로 전송한다(테스트).

		{
			do {
	
				if(GameRoot.get().net_player == null) {
	
					break;
				}

				if(this.step.get_current() == STEP.OUTER_CONTROL) {
					break;
				}
	
				m_send_count = (m_send_count + 1)%SplineData.SEND_INTERVAL;
	
				if(m_send_count != 0) {
	
					break;
				}

				// 통신용 좌표 송신.
				Vector3 target = this.controll.getPosition() + Vector3.left;
				CharacterCoord coord = new CharacterCoord(target.x, target.z);

				Vector3 diff = m_prev - target;
				if (diff.sqrMagnitude > 0.0001f) {

					m_culling.Add(coord);

					//Debug.Log("SendCharacterCoord[index:" + m_plotIndex + "]");
					CharacterRoot.get().SendCharacterCoord(controll.account_name, m_plotIndex, m_culling); 
					++m_plotIndex;

					if (m_culling.Count >= PLOT_NUM) {

						m_culling.RemoveAt(0);
					}

					m_prev = target;
				}
	
			} while(false);
		}
	}

	protected int	m_send_count = 0;

	// ================================================================ //

	// STEP.MOVE 실행.
	// 이동.
	protected void	exec_step_move()
	{
		// ---------------------------------------------------------------- //
		// 이동(위치좌표 보간).

		Vector3		position  = this.controll.getPosition();

		Vector3		dist = (this.move_target - position).Y(0.0f);

		float		speed = MOVE_SPEED;
		float		speed_per_frame = speed*Time.deltaTime;

		if(dist.magnitude < speed_per_frame) {

			// 목표위치가 아주 가까울 때는 현재위치＝목표위치로 한다.
			position = this.move_target;

			// 스톱 모션을 재생한다.
			this.stopWalkMotion();

		} else {

			// 목표위치가 멀 때는 이동 속도만큼 이동한다 .

			dist *= (speed_per_frame)/dist.magnitude;

			position += dist;

			// 걷기 모션을 재생한다.
			this.playWalkMotion();
		}

		position.y = this.controll.getPosition().y;

		this.controll.cmdSetPosition(position);

		if(this.step.get_current() == STEP.HOUSE_MOVE) {

		} else {

			// 방향 보간.
			this.controll.cmdSmoothHeadingTo(this.move_target);
		}

		// ---------------------------------------------------------------- //

		GameInput	gi = this.controll.game_input;

		// 드래그하는 동안 이동 목표위치를 갱신한다.
		if(gi.pointing.current) {

			if(gi.pointing.pointee != GameInput.POINTEE.NONE) {
	
				this.move_target = gi.pointing.position_3d;
			}
		}

		if(gi.pointing.trigger_on) {

			if(gi.pointing.pointee == GameInput.POINTEE.ITEM) {

				if(this.controll.item_carrier.isCarrying()) {

					// 픽업 중인 아이템을 클릭했을 때는 일단 아무것도 하지 않는다.
					if(gi.pointing.pointee_name == this.controll.item_carrier.item.name) {
	
						this.move_target    = this.controll.getPosition();
						gi.pointing.pointee = GameInput.POINTEE.NONE;
					}
				}
			}

			// 아이템/캐릭터가 클릭되었다.
			if(gi.pointing.pointee == GameInput.POINTEE.ITEM || gi.pointing.pointee == GameInput.POINTEE.CHARACTER) {

				Vector3		item_pos;
				Vector3		item_size;

				if(gi.pointing.pointee == GameInput.POINTEE.ITEM) {

					this.controll.item_man.getItemPosition(out item_pos, gi.pointing.pointee_name);
					this.controll.item_man.getItemSize(out item_size, gi.pointing.pointee_name);

				} else {

					chrController	chr = CharacterRoot.getInstance().findCharacter(gi.pointing.pointee_name);

					item_pos  = chr.transform.position;

					if(chr.GetComponent<BoxCollider>() != null) {

						item_size = chr.GetComponent<BoxCollider>().size;

					} else {

						item_size = chr.collider.bounds.size;
					}
				}

				dist = item_pos - this.transform.position;
				dist.y = 0.0f;

				item_size.y = 0.0f;

				float	distance_to_pick = (this.gameObject.collider.bounds.size.x + item_size.magnitude)/2.0f;

				if(this.step.get_current() == STEP.HOUSE_MOVE) {

					// 이사 중에 집이 클릭되면 이사 끝.
					if(gi.pointing.pointee_name == this.name) {

						this.controll.cmdQueryHouseMoveEnd();
						this.step.set_next(STEP.WAIT_QUERY);
					}

				} else {

					if(dist.magnitude < distance_to_pick) {
	
						// 가까울 때.

						if(gi.pointing.pointee == GameInput.POINTEE.ITEM) {
	
							// 아이템이면 픽업한다.
							this.controll.cmdItemQueryPick(gi.pointing.pointee_name);
							this.step.set_next(STEP.WAIT_QUERY);

						} else if(gi.pointing.pointee == GameInput.POINTEE.CHARACTER) {
							
							// 캐릭터.

							// 집일 때는 이사.
							if(gi.pointing.pointee_name.ToLower().StartsWith("house")) {

								if(this.isEnableHouseMove()) {

									this.controll.cmdQueryHouseMoveStart(gi.pointing.pointee_name);
									this.step.set_next(STEP.WAIT_QUERY);
								}
							}
						}
	
						// 아이템을 픽업한 직후에 이동을 시작하지 않도록、
						// 이동 목표 위치를 클릭해 둔다.
						gi.pointing.pointee = GameInput.POINTEE.NONE;
						this.move_target    = this.controll.getPosition();
	
					} else {
	
						// 멀 때는 그곳까지 이동.
						this.move_target      = gi.pointing.position_3d;
						this.move_target_item = gi.pointing.pointee_name;
					}
				}
			}
		}
	}

	// 매 프레임 LateUpdate()에서 호출된다.
	public override void	lateExecute()
	{
#if false
		GameObject	head = this.gameObject.findDescendant("anim_neck");

		Vector3		camera_position = head.transform.parent.InverseTransformPoint(CameraControl.get().transform.position);
		Vector3		up              = head.transform.parent.InverseTransformDirection(Vector3.up);

		camera_position.Normalize();

		head.transform.localRotation = Quaternion.LookRotation(camera_position, up)*Quaternion.AngleAxis(-90.0f, Vector3.forward);
#endif
	}

	// ================================================================ //

	// 로컬 플레이어?.
	public override bool		isLocal()
	{
		return(true);
	}

	// 외부에서 컨트롤을 시작한다.
	public override void 	beginOuterControll()
	{
		base.beginOuterControll();

		this.controll.cmdSetMotion("Take 002", 0);
		this.step.set_next(STEP.OUTER_CONTROL);
	}

	// 외부 컨트롤을 종료한다.
	public override void		endOuterControll()
	{
		base.endOuterControll();

		this.move_target = this.transform.position;
		this.step.set_next(STEP.MOVE);
	}

	// ================================================================ //
	// 이사（HOUSE_MOVE）.

	// 이사 시작 이벤트 박스에 들어왔을 때 호출된다.
	public void		onEnterHouseMoveEventBox()
	{
		this.is_within_house_move_event_box = true;
	}

	// 이사 시작 이벤트 박스에서 나왔을 때 호출된다.
	public void		onLeaveHouseMoveEventBox()
	{
		this.is_within_house_move_event_box = false;
	}

	// 이사 시작.
	public override void		beginHouseMove(chrBehaviorNPC_House house)
	{
		base.beginHouseMove(house);

		this.step.set_next(STEP.HOUSE_MOVE);
	}

	// 이사 종료 시 처리.
	public override void		endHouseMove()
	{
		base.endHouseMove();

		this.step.set_next(STEP.MOVE);
	}

	// 이사 중？.
	public override bool		isNowHouseMoving()
	{
		return(this.step.get_current() == STEP.HOUSE_MOVE);
	}

	// 이사할 수 있는가?.
	public bool		isEnableHouseMove()
	{
		bool	ret = false;

		do {

			if(!this.is_within_house_move_event_box) {

				break;
			}

			ItemFavor	favor = this.controll.getItemFavor();

			if(favor == null) {

				break;
			}	

			ret = favor.is_enable_house_move;

		} while(false);

		return(ret);
	}

	// STEP.HOUSE_MOVE 초기화.
	protected void	initialize_step_house_move()
	{
		this.initialize_step_house_move_common();

		this.move_target = this.transform.position;
	}

	// STEP.HOUSE_MOVE 실행.
	protected void	execute_step_house_move()
	{
		this.execute_step_house_move_common();

		this.exec_step_move();
	}

}
