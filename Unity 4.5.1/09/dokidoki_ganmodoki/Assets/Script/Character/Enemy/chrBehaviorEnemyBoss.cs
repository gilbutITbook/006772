using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 적 보스의 사고 루틴.
// 호스트에서만 동작한다.
public class chrBehaviorEnemyBoss : chrBehaviorEnemy {

	private chrControllerEnemyBoss myController; 			// 전제로하는 컨트롤러 클래스로 캐스팅 된 참조 유지.

	// ---------------------------------------------------------------- //
	
	public enum STEP {
		
		NONE = -1,
		
		MOVE = 0,			// 움직인다.
		REST,				// 멈춘다.
		ACTION,				// 액션 종료 대기.
		VANISH,				// 죽음.
		
		NUM,
	};
	
	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;
	
	// ---------------------------------------------------------------- //
	protected List<chrBehaviorPlayer> targetPlayers;		// 싸울 상대들(계정 이름으로 나열).
	protected int indexOfTargets;							// 현재 노리는 대상(targetPlayers의 인덱스).
	protected chrBehaviorPlayer	focus;						// 현재 노리는 대상(targetPlayers[indexOfTargets]의 캐시).

	// ---------------------------------------------------------------- //
	// 통신 관련.

	protected Network	m_network = null;

	protected bool		m_isHost = false;
	
	// ---------------------------------------------------------------- //
	
	// 3차 스플라인 보간에서 사용할 점의 수.
	protected const int	PLOT_NUM = 4;
	
	// 송신 회수.
	protected int	m_send_count = 0;
	
	// 현재 플롯의 인덱스.
	protected int 	m_plotIndex = 0;
	
	// 정지 상태일 때는 데이터를 보내지 않게 한다.
	protected Vector3		m_prev;

	// 솎아낸 좌표를 보존.
	protected List<CharacterCoord>	m_culling = new List<CharacterCoord>();
	// 보간한 좌표를 보존.
	protected List<CharacterCoord>	m_plots = new List<CharacterCoord>();

	// ---------------------------------------------------------------- //
	// 캐릭터 스펙
	// ---------------------------------------------------------------- //
	private float		REST_TIME = 1.0f;
	private float		MOVE_TIME = 3.0f;//5.0f;		// 최소한 이정도로 움직인다.
	
	// ================================================================ //
	// 이 컨트롤러가 전제로 하는 컨트롤러 클래스로의 참조를 반환한다.
	protected chrControllerEnemyBoss getMyController()
	{
		if (myController == null) {
			myController = this.control as chrControllerEnemyBoss;
		}
		return myController;
	}
	
	// ================================================================ //
	// 비헤이비어 쪽에서 탐지한 대미지를  컨트롤러로 바이패스시킨다.
	public override void		onDamaged()
	{
		this.getMyController().causeDamage();
	}

	// 당한 걸로 한다.
	public override void		causeVanish()
	{
		this.getMyController().causeVanish();
	}

	// 사망 통지에 의한 대미지
	public void 				dead()
	{
		this.getMyController().life = 0;
		this.getMyController().causeDamage();
	}
	
	// ================================================================ //
	
	public override void	initialize()
	{
		base.initialize();
		initializeTargetPlayers();
	}

	public override void	start()
	{
		this.next_step = STEP.MOVE;

		GameObject go = GameObject.Find("Network");
		if (go != null) {
			m_network = go.GetComponent<Network>();
		}

		m_isHost = (GlobalParam.get().global_account_id == 0)? true : false;
	}
	
	public override	void	execute()
	{
		if (isPaused) {
			return;
		}
		
		// ---------------------------------------------------------------- //
		// 스텝 내의 경과 시간을 진행한다.
		
		this.step_timer += Time.deltaTime;
		
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.
		
		if(this.next_step == STEP.NONE) {
            //Debug.Log(this.step);
			switch(this.step) {
				
				case STEP.MOVE:
				{
					if(this.step_timer >= MOVE_TIME && this.getMyController().CanBeControlled()) {
						
						this.next_step = STEP.REST;
					}
				}
				break;
				
				case STEP.REST:
				{
					if(this.step_timer >= REST_TIME) {
						decideNextStep();
					}
				}
				break;
			
				default:
					break;
			}
		}
		
		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.
		
		while(this.next_step != STEP.NONE) {
			
			this.step      = this.next_step;
			this.next_step = STEP.NONE;
			switch(this.step) {
			case STEP.REST:
				this.getMyController().acceleration = 0.0f;
				this.getMyController().velocity = 0.0f;
				break;
				
			case STEP.MOVE:
				break;

			default:
				break;
			}
			
			this.step_timer = 0.0f;
		}
		
		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.
		
		switch(this.step) {
			
		case STEP.MOVE:
			updateMoving();
			break;
			
		case STEP.VANISH: 
			break;
		}
		
		// ---------------------------------------------------------------- //
	}

	protected void decideNextStep()
	{
		if(m_isHost) {

			// 가까이에 있는(퀵 공격 할 수 있는) 플레이어를 찾는다.
			chrBehaviorPlayer	target = this.find_close_player();
	
			if(target != null)
			{
				this.next_step = STEP.ACTION;
				EnemyRoot.getInstance().RequestBossQuickAttack(target.getAcountID(), 1.0f);
			} 
			else
			{
	
				// FIXME: 통신에 대응시킬 것.
				float randomValue = Random.value * 4;
		
				if (randomValue < 1.0f)
				{
		            Debug.Log("DirectAttack");
					this.next_step = STEP.ACTION;

		            EnemyRoot.getInstance().RequestBossDirectAttack(focus.getAcountID(), 1.0f);
				}
				else if (randomValue < 2.0f)
				{
		            Debug.Log("RangeAttack");
					this.next_step = STEP.ACTION;

					EnemyRoot.getInstance().RequestBossRangeAttack(1.0f, 5.0f);
				}
				else
				{
					this.next_step = STEP.MOVE;
				}
			}

		} else {

			this.next_step = STEP.MOVE;
		}
        
		// ---------------------------------------------------------------- //
		// 캐릭터 좌표를 보낸다.
		sendCharacterCoordinates();
	}

	// 가까이에 있는(퀵 공격할 수 있는) 플레이어를 찾는다.
	protected chrBehaviorPlayer		find_close_player()
	{
		chrBehaviorPlayer	target = null;

		foreach(var result in this.control.collision_results) {

			if(result.object1.tag != "Player") {

				continue;
			}

			chrBehaviorPlayer	player = chrBehaviorBase.getBehaviorFromGameObject<chrBehaviorPlayer>(result.object1);

			if(player == null) {

				continue;
			}

			Vector3		to_player = player.control.getPosition() - this.control.getPosition();

			to_player.Normalize();

			float		pinch = Mathf.Acos(Vector3.Dot(to_player, this.transform.forward))*Mathf.Rad2Deg;

			if(pinch > 90.0f) {

				continue;
			}

			target = player;
			break;
		}

		return(target);
	}

    //범위 공격하면 true.
    private bool rangeAttackEnable(float attackRange) {
        //공격 범위 안에 있는 플레이어를 세서 판단한다.
        int rangeInPlayerCount = 0;
        foreach (chrBehaviorPlayer p in targetPlayers) {
            Vector3 diff = transform.position - p.transform.position;
            if (diff.magnitude < attackRange) {
                rangeInPlayerCount++;       //유효 범위에 들어온 플레이어를 센다.
            }
        }
        
        //유효 범위에 몇 명 있는지로 공격할지 결정한다.
        int rangeAttackThreshold = Random.Range(0, targetPlayers.Count);    //한계값은 랜덤.
        if (rangeInPlayerCount > rangeAttackThreshold) {
            Debug.Log("rangeInPlayerCount:" + rangeInPlayerCount);
            return true;
        }
        return false;
    }

    //+-findAngle범위에서 정면 타깃을 검색해서 반환한다(index).탐색에 실패하면-1.
    private int findForwardTarget(float findAngle) {
        //자신(정면)과 타깃의 각도로 판정한다.
        int findIndex = -1;
        
        Vector2 forward = new Vector2( transform.forward.x, transform.forward.z ).normalized;
        for(int i=0; i < targetPlayers.Count; ++i){
            chrBehaviorPlayer p = targetPlayers[i];
            Vector3 diff = p.transform.position - transform.position;
            Vector2 targetVec = new Vector2(diff.x, diff.z).normalized;

            float angle = Vector2.Angle(forward, targetVec);
            //Debug.Log(angle);

            //정면에서 각도 범위 내라면 찾은 걸로 한다.
            if (angle < findAngle) {
                findAngle = angle;  //가장 정면에 있는 대상을 찾고자 검색 범위를 갱신하여 처리를 계속한다.
                findIndex = i;
            }
        }
        Debug.Log("findForwardTarget:" + findIndex);
        return findIndex;
    }


	// ---------------------------------------------------------------- //
	// 10 프레임에 1회, 좌표를 네트워크에 전송한다.
	private void sendCharacterCoordinates()
	{

		if(m_network == null) {
			
			return;
		}
		
		if(this.step != STEP.MOVE) {

			return;
		}
		
		m_send_count = (m_send_count + 1)%SplineData.SEND_INTERVAL;
		
		if(m_send_count != 0) {
			
			return;
		}
		
		// 통신용 좌표 송신.
		Vector3 target = this.control.getPosition() + Vector3.left;
		CharacterCoord coord = new CharacterCoord(target.x, target.z);
		
		Vector3 diff = m_prev - target;
		if (diff.sqrMagnitude > 0.0001f) {
			
			m_culling.Add(coord);
			
			AccountData	account_data = AccountManager.get().getAccountData(GlobalParam.getInstance().global_account_id);
			
			CharacterRoot.get().SendCharacterCoord(account_data.avator_id, m_plotIndex, m_culling); 
			++m_plotIndex;
			
			if (m_culling.Count >= PLOT_NUM) {
				
				m_culling.RemoveAt(0);
			}
			
			m_prev = target;
		}
	}

	//========================================================================
	// 컨트롤로로부터 메시징된 액션 종료 통지(AI힌트).

	public void NotifyFinishedCharging()
	{
		// 돌진 공격의 대상인 인덱스의 다음 인덱스의 캐릭터에 포커스.
		indexOfTargets = (indexOfTargets + 1) % targetPlayers.Count;
		focus = targetPlayers[indexOfTargets];

		this.next_step = STEP.MOVE;
	}

	public void NotifyFinishedJumping()
	{
		indexOfTargets = (indexOfTargets + 1) % targetPlayers.Count;
		focus = targetPlayers[indexOfTargets];

		this.next_step = STEP.MOVE;
	}

	public void NotifyFinishedQuickAttack()
	{
		indexOfTargets = (indexOfTargets + 1) % targetPlayers.Count;
		focus = targetPlayers[indexOfTargets];

		this.next_step = STEP.MOVE;
	}

	public void NotifyFinishedTyphoon()
	{
		this.next_step = STEP.MOVE;
	}

	public void NotifyDied()
	{
		this.next_step = STEP.VANISH;
	}

	//========================================================================
	// AI에 의한 패드 조작.

	// 목표를 정면으로 향한다.
	protected void updateMoving()
	{
		float turn;

		Vector3 focal_local_pos = this.control.transform.InverseTransformPoint(focus.transform.position);
		focal_local_pos.y = 0.0f; // 높이는 보지 않는다.

		if(!m_isHost && m_plots.Count > 0) {
			CharacterCoord coord = m_plots[0];
			focal_local_pos = new Vector3(coord.x, focal_local_pos.y, coord.z);
			if (m_plots.Count > 0) {
				m_plots.RemoveAt(0);
			}
		}

		//float	turn = Random.Range(-90.0f, 90.0f);
		if (Vector3.Dot(Vector3.right, focal_local_pos) > 0)
		{
			turn = Vector3.Angle(focal_local_pos, Vector3.forward);
		}
		else
		{
			turn = -Vector3.Angle(focal_local_pos, Vector3.forward);
		}
		turn = Mathf.Clamp(turn, -90, 90);
		
		// move_dir에 회전 각도를 넣는다.
		this.getMyController().SetMoveDirection(this.control.getDirection() + turn);
		this.getMyController().acceleration = this.getMyController().maxSpeed;
	}

	//========================================================================

	// 보스가 싸우는 플레이어 리스트를 가져온다.
	protected void initializeTargetPlayers()
	{
		// 계정 이름으로 ABC순으로 정렬해 둔다.
		targetPlayers = new List<chrBehaviorPlayer>(PartyControl.getInstance().getPlayers());
		targetPlayers.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
		
		indexOfTargets = 0;
		
		// 노릴 플레이어를 결정한다. ABC 정렬 전의 선두 계정 이름을 가진 플레이어가 최초의 표적이 된다.
		focus = targetPlayers[indexOfTargets];
	}

	// [개발용] 최신 파티 정보를 가져와 플레이어 목록을 갱신한다.
	public virtual void updateTargetPlayers()
	{
		// 본 게임에서는 호출되지 말아야 하므로 개발용 서브 클래스에서 구현한다.
	}

	// ================================================================ //
	// 
	// 게스트 단말에서의 보스의 이동.
	//
	
	public void CalcCoordinates(int index, CharacterCoord[] data)
	{
		// 수신한 좌표를 보존.
		do {
			
			// 데이터가 빔(만일을 위해).
			if(data.Length <= 0) {
				
				break;
			}
			
			// 새로운 데이터가 없다.
			if(index <= m_plotIndex) {
				
				break;
			}
			
			// m_plotIndex ... m_culling[]의 마지막 정점의 인덱스.
			// index       ... data[]의 마지막 정점의 인덱스.
			//
			// index - m_plotIndex ... 이번에 새로 추가된 정점 수.
			//
			int		s = data.Length - 1 - (index - m_plotIndex);
			
			if(s < 0) {
				
				break;
			}
			
			for(int i = s;i < data.Length;i++) {
				
				m_culling.Add(data[i]);
			}
			
			// m_culling[]의 마지막 정점의 인덱스.
			m_plotIndex = index;
			
			// 스플라인 곡선을 구해서 보간한다.	
			SplineData	spline = new SplineData();
			spline.CalcSpline(m_culling);
			
			// 구한 스플라인 곡선을 좌표 정보로 저장한다.
			CharacterCoord plot = new CharacterCoord();
			for (int i = 0; i < spline.GetPlotNum(); ++i) {
				spline.GetPoint(i, out plot);
				m_plots.Add(plot);
			}
			
			// 가장 오래된 좌표를 삭제한다.
			if (m_culling.Count > PLOT_NUM) {
				
				m_culling.RemoveRange(0, m_culling.Count - PLOT_NUM);
			}
			
		} while(false);
		

		// 수신한 좌표를 저장한다.
		for (int i = 0; i < data.Length; ++i) {
			int p = index - PLOT_NUM - i + 1;
			if (p < m_plotIndex) {
				m_culling.Add(data[i]);
			}
		}
	}

}
