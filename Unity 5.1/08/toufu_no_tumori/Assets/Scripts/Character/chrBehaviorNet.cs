using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 비헤이비어  네트워크 플레이어(게스트)용.
// 네트워크에서 수신한 데이터로 컨트롤할 예정.
public class chrBehaviorNet : chrBehaviorPlayer {

	public enum STEP {

		NONE = -1,

		MOVE = 0,			// 이동(멈춘 때도 포함).
		HOUSE_MOVE,			// 이사.
		OUTER_CONTROL,		// 외부 제어.

		WAIT_QUERY,			// 쿼리 제어.

		NUM,
	};
	Step<STEP>		step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	// 3차 스플라인 보간에서 사용할 점 수.
	private const int PLOT_NUM = 4;

	// 솎아낼 좌표의 프레임 수.
	private const int CULLING_NUM = 10;

	// 현재 플롯 인덱스.
	private int 	m_plotIndex = 0;

	// 추출한 좌표를 보존.
	private List<CharacterCoord>	m_culling = new List<CharacterCoord>();
	// 보간한 좌표를 보존.
	private List<CharacterCoord>	m_plots = new List<CharacterCoord>();
	
	// 걷기 모션.
	private struct WalkMotion {

		public bool		is_walking;
		public float	timer;
	};
	private	WalkMotion	walk_motion;

	private const float	STOP_WALK_WAIT = 0.1f;		// [sec] 걷기 -> 서기 모션으로 이행할 때의 유예 기간.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	public override void	initialize()
	{
		this.walk_motion.is_walking = false;
		this.walk_motion.timer      = 0.0f;
	}

	public override void	start()
	{
		this.controll.balloon.setPriority(-1);

		// 게임 시작 직후에 EnterEvent가 시작되면 여기서 next_step에.
		// OuterControll이 설정됩니다。그때 덮어쓰지 않도록.
		// next == NONE을 체크합니다.
		if(this.step.get_next() == STEP.NONE) {

			this.step.set_next(STEP.MOVE);
		}
	}
	public override	void	execute()
	{
		// ---------------------------------------------------------------- //
		// 조정이 끝난 쿼리 실행.
		
		base.execute_queries();


		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크합니다.

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
		// 상태가 전환됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.OUTER_CONTROL:
				{
					this.GetComponent<Rigidbody>().Sleep();
				}
				break;

				case STEP.MOVE:
				{
					//this.move_target = this.transform.position;
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
		// 각 상태에서의 실행 처리.

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

	}

	// 이동에 관한 처리.
	protected void	exec_step_move()
	{
		Vector3		new_position = this.controll.getPosition();
		if(m_plots.Count > 0) {
			CharacterCoord coord = m_plots[0];
			new_position = new Vector3(coord.x, new_position.y, coord.z);
			m_plots.RemoveAt(0);
		}

		// 순간적으로 멈추기만 했을 때는 걷기 모션이 정지하지 않게 합니다.

		bool	is_walking = this.walk_motion.is_walking;

		if(Vector3.Distance(new_position, this.controll.getPosition()) > 0.0f) {

			if(this.step.get_current() == STEP.HOUSE_MOVE) {
	
			} else {

				this.controll.cmdSmoothHeadingTo(new_position);
			}
			this.controll.cmdSetPosition(new_position);

			is_walking = true;

		} else {

			is_walking = false;
		}

		if(this.walk_motion.is_walking && !is_walking) {

			this.walk_motion.timer -= Time.deltaTime;

			if(this.walk_motion.timer <= 0.0f) {

				this.walk_motion.is_walking = is_walking;
				this.walk_motion.timer      = STOP_WALK_WAIT;
			}

		} else {

			this.walk_motion.is_walking = is_walking;
			this.walk_motion.timer      = STOP_WALK_WAIT;
		}
		
		if(this.walk_motion.is_walking) {
			
			this.playWalkMotion();
			
		} else {
			
			this.stopWalkMotion();
		}
	}

	// ================================================================ //

	// 로컬 플레이어?.
	public override bool		isLocal()
	{
		return(false);
	}

	// 외부로부터의 컨트롤을 시작합니다.
	public override void 	beginOuterControll()
	{
		base.beginOuterControll();

		this.controll.cmdSetMotion("Take 002", 0);
		this.step.set_next(STEP.OUTER_CONTROL);
	}

	// 외부로부터의 컨트롤을 종료합니다.
	public override void		endOuterControll()
	{
		base.endOuterControll();

		this.step.set_next(STEP.MOVE);
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

	// 이사 중?.
	public override bool		isNowHouseMoving()
	{
		return(this.step.get_current() == STEP.HOUSE_MOVE);
	}

	// ================================================================ //

	// STEP.HOUSE_MOVE 초기화
	protected void	initialize_step_house_move()
	{
		this.initialize_step_house_move_common();
	}

	// STEP.HOUSE_MOVE 실행.
	protected void	execute_step_house_move()
	{
		this.execute_step_house_move_common();

		this.exec_step_move();
	}

	// ================================================================ //

	public void		ReceivePointFromNet(Vector3 point)
	{
		CharacterCoord	coord;

		coord.x = point.x;
		coord.z = point.z;

		m_culling.Add(coord);

		SplineData	spline = new SplineData();

		spline.CalcSpline(m_culling, 4);

		m_plots.Clear();

		if(spline.GetPlotNum() > 0) {

			for(int i = 0;i < spline.GetPlotNum();i++) {

				CharacterCoord	plot;

				spline.GetPoint(i, out plot);

				m_plots.Add(plot);
			}
		}
	}

	public void CalcCoordinates(int index, CharacterCoord[] data)
	{
		SplineData	spline = new SplineData();
		
		for (int i = 0; i < data.Length; ++i) {
			int p = index - PLOT_NUM - i + 1;
			if (p < m_plotIndex) {
				m_culling.Add(data[i]);
			}
		}
		
		// 최신 좌표를 설정.
		m_plotIndex = index;
		
		// 스플라인 곡선을 구해서 보간합니다.	
		spline.CalcSpline(m_culling, CULLING_NUM);
		
		// 구한 스플라인 보간을 좌표 정보로서 저장합니다.
		CharacterCoord plot = new CharacterCoord();
		for (int i = 0; i < spline.GetPlotNum(); ++i) {
			spline.GetPoint(i, out plot);
			m_plots.Add(plot);
		}
		
		// 가장 오래된 좌표를 삭제.
		if (m_culling.Count > PLOT_NUM) {
			m_culling.RemoveAt(0);
		}
	}
}
