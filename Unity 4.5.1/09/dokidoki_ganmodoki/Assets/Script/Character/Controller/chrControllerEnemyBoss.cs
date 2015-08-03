using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class chrControllerEnemyBoss : chrControllerEnemyBase {

	private float	JUMP_PREACTION_DURATION         = 1.5f;
	private float	CHARGE_PREACTION_DURATION       = 1.5f;
	private float	FINISH_CHARGE_REACTION_DURATION = 1.0f;

    private const float JUMP_ATTACK_POWER = 3.0f;   //점프 공격력 .
    private float rangeAttackArea;                  //범위 공격에서의 공격 범위(원의 둘레)(원의 반지름).


	private float	angularMaxSpeed = 90.0f;	// 회복 속도.

	// FIXME 3차원적인 움직임도 포함하기 위한 코드 개조용.
	private Vector3 TotalVelocity;


	//목표 각도 .
	protected float desired_dir;

	//===================================================================
	// 돌진공격함수의 속성.

	protected GameObject	targetToCharge;
    private Vector3 		chargeStartPoisition;			//공격 시작 위치.
    private const float 	CHARGE_DISTANCE = 13.0f;		//공격할 최대 거리.


	//===================================================================
	// 효과 관련 속성.
	private float		footSmokeRespawnTimer;			// 효과 재생성을 카운트하는 타이머. 
	public float		chargeFootSmokeRespawnDuration  = 1.0f / 10.0f;		// 꽤 많이.
	public float		typhoonFootSmokeRespawnDuration = 1.0f / 10.0f;		// 꽤 많이.

	private GameObject	chargeAura;						// 공격 오라를 몸에 .

	public Transform	neckSocket;						// 목뼈.
	public Transform	neckEndSocket;					// 코끝.
	private	float		snortSpawnTimer;				// 콧김 간격을 계산하는 타이머ー.
	private float		snortSpawnInterval;				// 콧김 실제 간격. 생성할 때마다 snortSpawnIntervalMin과 snortSpawnIntervalMax의 로컬 랜덤값으로 결정한다.
	public float		snortSpawnIntervalMin = 0.2f;	// 콧김을 내는 간격의 최솟값.
	public float		snortSpawnIntervalMax = 0.4f;	// 콧김을 내는 간격의 최댓값.
	private bool		enableSpawnSnortEffects;		// 콧김을 낼지 내부적으로 제어하는 플래그.

	public Transform	headSocket;						// 분노 마크를 표기한다. 좌표를 결정하기 위한 트랜스폼 참조.

	protected float		model_scale = 0.5f;

	public float		getScale()
	{
		return(this.model_scale);
	}

	//===================================================================
	// 애니메이션.
	private float		chargingSpeedModifier = 2.0f;
	private float		chargingForce = 10.0f;			// 충돌 가속도.

	private chrBehaviorBase getBehavior()
	{
		if (behavior == null)
		{
			behavior = GetComponent<chrBehaviorBase>();
		}

		return behavior;
	}

	// AI힌트.
	public bool CanBeControlled()
	{
		return state == EnemyState.MAIN;
	}

	//===========================================================================
	//
	// 비헤이비어로부터 속성 써넣기.
	//
	//
	public void SetMoveDirection(float newDir)
	{
		// 방향 전환 입력을 받아들이는 스테이트는 제한되어 있다.
		if (state == EnemyState.MAIN)
		{
			move_dir = newDir;
		}
	}

	override protected float getMaxSpeedModifier()
	{
		switch (state)
		{
			case EnemyState.CHARGING:
				return chargingSpeedModifier;
			default:
				return base.getMaxSpeedModifier();
		}
	}

	// 라이프 등의 기본 프로퍼티 변경 등.
	override protected void _awake()
	{
		base._awake();

		this.transform.localScale = Vector3.one*model_scale;

		// FIXME: 원래라면 프리팹에 맡기고 싶다.
		life = 100.0f;
		maxSpeed = 2.0f;
		floorY = -0.02f;
	}

	override protected void execute()
	{
		base.execute ();

		// 콧김을 나타낸다.
		if (enableSpawnSnortEffects)
		{
			snortSpawnTimer += Time.deltaTime;
			if (snortSpawnTimer >= snortSpawnInterval)
			{
				snortSpawnTimer = 0.0f;
				snortSpawnInterval = Random.Range(snortSpawnIntervalMin, snortSpawnIntervalMax);
				createSnortEffect ();
			}
		}

		localStateTimer += Time.deltaTime;

		//dbPrint.setLocate(20, 10);
		//dbPrint.print(state.ToString(), 0.0f);

		switch (state)
		{
	        //점프.
			case EnemyState.JUMP_PREACTION:
				acceleration = 0.0f;
				velocity = 0.0f;
				if (localStateTimer >= JUMP_PREACTION_DURATION)
				{
					goToJumping();
				}
				break;
			case EnemyState.JUMPING:
				execJump();
				break;

	        //돌격.
			case EnemyState.CHARGE_PREACTION:
				execChargePreaction(Time.deltaTime);
				break;
			case EnemyState.CHARGING:
				execCharge(Time.deltaTime);
				break;
			case EnemyState.CHARGE_FINISHED:
				if (localStateTimer >= FINISH_CHARGE_REACTION_DURATION)
				{
					getBehavior().SendMessage ("NotifyFinishedCharging");
					changeState(EnemyState.MAIN);
				}
				break;

			// 퀵 공격.
			case EnemyState.QUICK_ATTACK:
				this.executeQuickAttack();
				break;

			// 사라짐.
			case EnemyState.VANISH:
				enableSpawnSnortEffects = false;
				break;

			default:
				break;
		}

		updateAnimator();
	}

	override protected void updateAnimator()
	{
		base.updateAnimator();
		if (animator != null)
		{
			animator.SetBool("Motion_Is_Charging", state == EnemyState.CHARGING);
		}
	}

	protected void exec_rotate(float deltaTime, float alpha = 1.0f)
	{
		float		cur_dir   = this.getDirection();
		float		dir_diff  = this.desired_dir - cur_dir;

		if(dir_diff > 180.0f) {
			
			dir_diff = dir_diff - 360.0f;
			
		} else if(dir_diff < -180.0f) {
			
			dir_diff = dir_diff + 360.0f;
		}

		float		angular_velocity = dir_diff >= 0.0f ? angularMaxSpeed : -angularMaxSpeed;
		float		delta_dir = angular_velocity * deltaTime * alpha;

		if (Mathf.Abs(delta_dir) > Mathf.Abs(dir_diff)) {

			cur_dir = desired_dir;
		}
		else {

			cur_dir += delta_dir;
		}

		this.cmdSetDirection(cur_dir);
	}

	//===========================================================================
	//
	//  점프 / 애니메이션 제어로 공중에 갔다가 착지한다.
	//
	//
	public void PlayJump(float power, float attackRange)
	{
		if (state == EnemyState.MAIN)
		{
			changeState (EnemyState.JUMP_PREACTION);

            rangeAttackArea = attackRange;  //공격 범위 설정.
            vital.setAttackPower(JUMP_ATTACK_POWER);
		}
	}

	protected void goToJumping()
	{
		animator.SetTrigger("Jump");
		this.cmdEnableCollision(false);

		if(this.state != EnemyState.VANISH) {

			changeState(EnemyState.JUMPING);
		}
	}

	// 완전히 애니메이션으로 움직으로 움직이므로 딱히 아무것도 하지 않는다.
	protected void execJump()
	{
		// FIXME 각 단말에서 문제가 일어난 경우는 루트 모션의 위치를 동기시킬 필요가 있을지도 모릅니다.

		// 애니메이터의 이벤트 알림이 오지 않는 일이 있으므로 보험.
		do {

			if(this.animator.IsInTransition(0)) {

				break;
			}
			if(!this.animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle")) {

				break;
			}

			this.NotifyLanded();

		} while(false);
	}
	
	// 애니메이션으로부터 호출되는 공격 판정의 발생 타이밍.
	public void NotifyImpactLanded()
	{
		this.cmdEnableCollision(true);

		// 충돌판정: 범위 원 안에 있으면 대미지로 한다.
        List<chrBehaviorPlayer> targets = PartyControl.getInstance().getPlayers();
        foreach (var t in targets) {
            Vector3 toTarget = t.transform.position - transform.position;
            if (toTarget.magnitude < rangeAttackArea*this.getScale()) {
                //공격 범위 안에 있으므로 대미지 대상으로 한다.
                if (t.isLocal()) {
                    t.control.causeDamage(getBehavior().control.vital.getAttackPower(), -1);
                }
                else {
                    // 리모트 플레이어에는 대미지를 주지 않는다.
                }
				// 뒤로 날려버린다.
				t.beginBlowOut(this.transform.position, 3.0f*this.getScale());
            }
        }

		// 얀츨영 연기를 많이 만든다.
		for (float degree = 0; degree < 360; degree += 30.0f)
		{
			this.createFootSmoke(degree, 10.0f);
		}
	}

	// 애니메이션에서 호출되는 일련의 점프 행동 종료(일반 행동으로 이행가능).
	public void		NotifyLanded()
	{
		Debug.Log("NotifyLanded");
		// 비헤이비어에 알린다(AI힌트).
		getBehavior().SendMessage ("NotifyFinishedJumping");

		if(this.state != EnemyState.VANISH) {

			changeState(EnemyState.MAIN);
		}
	}

	//===========================================================================
	//
	// 돌격
	//
	//

	protected List<chrBehaviorPlayer>	tackled_players = new List<chrBehaviorPlayer>();	// 이미 대미지를 준 플레이어.

	public void PlayCharge(GameObject target_player, float attack_power)
	{
		if(this.state != EnemyState.VANISH) {

			targetToCharge = target_player;

			changeState (EnemyState.CHARGE_PREACTION);

			footSmokeRespawnTimer = 0.0f;
			enableSnortSpawnEffects();
			createAngryMarkEffect();
	
	        chargeStartPoisition = transform.position;  //돌격 시작 위치를 기억해 둔다.
	        vital.setAttackPower(attack_power);
	
			this.tackled_players.Clear();
		}
	}

	// 돌격 전 처리. 가능한 한 돌격 대상 캐릭터 방향으로 향한다.
	protected void execChargePreaction(float deltaTime)
	{
		acceleration = 0.0f;
		velocity = 0.0f;
		
		updateDesiredDir(targetToCharge);
		exec_rotate(deltaTime);
		
		if (localStateTimer >= CHARGE_PREACTION_DURATION)
		{
			goToCharging();
		}
	}
	
	protected void goToCharging()
	{
		acceleration = chargingForce;

		if(this.state != EnemyState.VANISH) {

			changeState(EnemyState.CHARGING);
		}
	}
	
	protected void finishedCharging()
	{
		Destroy (chargeAura);
		chargeAura = null;
		enableSpawnSnortEffects = false;

		if(this.state != EnemyState.VANISH) {

			changeState (EnemyState.CHARGE_FINISHED);
		}
	}

	protected void execCharge(float deltaTime)
	{
        // 일정 거리를 이동했으면 돌격 종료.
        if ((transform.position - chargeStartPoisition).magnitude > CHARGE_DISTANCE) {
            finishedCharging();
            return;
        }

		// ---------------------------------------------------------------- //
		// 이동(위치 좌표 보간).
		
		Vector3		position  = this.getPosition();
		float		rotateAlpha = 1.0f;	// 어느 정도 강력하게 진로를 바꾸는가. 속도가 오르면 방향을 바꾸기 어려워진다.
		float		currentMaxSpeed = maxSpeed * getMaxSpeedModifier();

		velocity += acceleration * deltaTime;
		velocity = Mathf.Clamp(velocity, 0.0f, maxSpeed * getMaxSpeedModifier());
		float		speed_per_frame = velocity * Time.deltaTime;
		
		Vector3		move_vector = this.transform.forward;
		
		position += move_vector * speed_per_frame * velocity;

		position.y = floorY;

		this.cmdSetPosition(position);

		// 스피드가 나오지 않은 동안은 타깃 방향으로 향한다.
		rotateAlpha = 1.0f - velocity / currentMaxSpeed;
		updateDesiredDir(targetToCharge);
		exec_rotate(deltaTime, rotateAlpha);
		
		//  연출 처리.

		// 발 연기 처리.
		footSmokeRespawnTimer += deltaTime;
		if (footSmokeRespawnTimer >= chargeFootSmokeRespawnDuration)
		{
			footSmokeRespawnTimer = 0.0f;
			// 여기 랜덤은 연출용.
			createFootSmoke(transform.rotation * Quaternion.AngleAxis(Random.Range(-30.0f, 30.0f), Vector3.up) * Vector3.back * 5.0f);
		}

		// FIXME. 어쨌든 오라는 최고속이 되고 난 후에 나온다.
		if (chargeAura == null && Mathf.Abs (velocity - maxSpeed * getMaxSpeedModifier ()) <= float.Epsilon)
		{
			// 오라 처리.
			chargeAura = EffectRoot.getInstance ().createChargeAura ();
			chargeAura.transform.parent = transform;
			chargeAura.transform.localPosition = new Vector3 (0.0f, 3.6f, 5.0f);
			chargeAura.transform.localRotation = Quaternion.identity;
			
			// 대신에 콧김은 이제 내뿜지 않는다.
			enableSpawnSnortEffects = false;
		}

		float	speed_rate = velocity/currentMaxSpeed;

		foreach(var result in this.collision_results) {

			if(result.object1 == null) {

				continue;
			}

			GameObject other = result.object1;

			// 충돌한 플레이어에게 대미지를 준다.
			if(other.tag == "Player") {

				chrBehaviorPlayer	player = chrBehaviorBase.getBehaviorFromGameObject<chrBehaviorPlayer>(other);

				if(player == null) {

					continue;
				}
				if(this.tackled_players.Contains(player)) {

					continue;
				}
				if(speed_rate < 0.5f) {

					continue;
				}

				if(player.isLocal()) {

					player.control.causeDamage(this.vital.getAttackPower(), -1);
					player.beginBlowOutSide(this.getPosition(), 3.0f*this.getScale(), this.transform.forward);

				}

				this.tackled_players.Add(player);

				finishedCharging();
				break;
			}
		}
	}

	//===========================================================================
	//
	// 퀵 공격.
	//
	//
	public void PlayQuickAttack(GameObject target_player, float attack_power)
	{
		//targetToCharge = target_player;

		changeState(EnemyState.QUICK_ATTACK);
		animator.SetTrigger("QuickAttack");

		//footSmokeRespawnTimer = 0.0f;
		//enableSnortSpawnEffects();
		//createAngryMarkEffect();

       // chargeStartPoisition = transform.position;  //공격시작위치.
       vital.setAttackPower(attack_power);
	}

	protected void		executeQuickAttack()
	{
		// 애니메이터의 이벤트 통지가 오지 않는다? 이런 일이 있을지 모르므로 보험.
		do {

			if(this.animator.IsInTransition(0)) {

				break;
			}
			if(!this.animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle")) {

				break;
			}

			this.NotifyQuickAttack_End();

		} while(false);
	}

	// 애니메이션에서 호출되는 이벤트 퀵 공격이 성공한 순간.
	public void 	NotifyQuickAttack_Impact()
	{
		chrBehaviorLocal	player = PartyControl.get().getLocalPlayer();

		if(Vector3.Distance(player.control.getPosition(), this.getPosition()) < 6.0f*this.getScale()) {

			player.control.causeDamage(vital.getAttackPower(), -1);
			player.beginBlowOut(this.getPosition(), 3.0f*this.getScale());
		}
	}

	// 애니메이션에서 호출되는 이벤트 퀵 공격 모션 끝.
	public void		NotifyQuickAttack_End()
	{
		// 비헤이비어에 알린다(AI힌트).

		chrBehaviorEnemyBoss	behavior = this.behavior as chrBehaviorEnemyBoss;

		if(behavior != null) {

			behavior.NotifyFinishedQuickAttack();
		}

		if(this.state != EnemyState.VANISH) {

			changeState(EnemyState.MAIN);
		}
	}

	//===========================================================================
	//
	// 효과.
	//
	//

	// 발 연기 생성.
	protected void createFootSmoke(float yawDegree, float speed)
	{
		this.createFootSmoke (Quaternion.AngleAxis (yawDegree, Vector3.up) * Vector3.forward * speed);
	}

	// 발 연기 생성.
	protected void createFootSmoke(Vector3 velocity)
	{
		Vector3 effectPosition = transform.position;
		effectPosition.y = floorY;
		createFootSmoke (effectPosition, velocity);
	}

	// 발 연기 생성.
	protected void createFootSmoke(Vector3 worldPosition, Vector3 velocity)
	{
		GameObject go;
		go = EffectRoot.get().createBossFootSmokeEffect (worldPosition);
		go.GetComponent<FootSmokeEffectControl>().velocity = velocity;
	}

	protected void enableSnortSpawnEffects()
	{
		enableSpawnSnortEffects = true;
		snortSpawnTimer = snortSpawnIntervalMax;
	}
	
	// 콧김 생성.
	protected void createSnortEffect()
	{
		if (neckSocket != null && neckEndSocket != null)
		{
			Quaternion effectRot = Quaternion.LookRotation (neckEndSocket.position - neckSocket.position);
			GameObject effect = EffectRoot.getInstance ().createSnortEffect (neckEndSocket.position, effectRot);
			effect.transform.parent = neckEndSocket;
		}
	}

	// 분노 마크 생성.
	protected void createAngryMarkEffect()
	{
		if (headSocket != null)
		{
			GameObject effect = EffectRoot.getInstance ().createAngryMarkEffect(headSocket.position);
			effect.transform.parent = headSocket;
		}
	}

	//===========================================================================
	//
	// 히트.
	//
	//	

    // 벽과 충돌.
	protected void hitWall(Vector3 hitLocation, Vector3 hitNormal)
	{
		if (state == EnemyState.CHARGING)
		{
			finishedCharging();
		}
	}

	
	// ================================================================ //
	// 
	//  휴먼 플레이어가 아니라 AI 플레이어와 컨트롤러의 성능으로서 사용하는 것을 상정한 캐릭터 제어 메소드.
	// 
	//

	// target의 방향으로 향한다. 로크온 공격에서 사용한다.
	protected void updateDesiredDir(GameObject target)
	{
		float turn = 0.0f;

		// target 좌표를 로컬 좌표계로 가져온다.
		Vector3 focal_local_pos = transform.InverseTransformPoint(target.transform.position);
		focal_local_pos.y = 0.0f; // 높이는 보이 않는다.
		
		// 돌아보는 각도를 결정한다.
		if (Vector3.Dot(Vector3.right, focal_local_pos) > 0)
		{
			turn = Vector3.Angle(focal_local_pos, Vector3.forward);
		}
		else
		{
			turn = -Vector3.Angle(focal_local_pos, Vector3.forward);
		}

		// move_dir에 회전 각도를 넣는다.
		desired_dir = this.getDirection() + turn;
	}


	// ================================================================ //
	// 
	// 비헤이비어가 사용하는 커맨드.
	// 단, 네트워크 지원을 위한 스텁으로 되어 있다.
	//
	
	/**
	 targetName이 나타내는 플레이어 캐릭터에 직접 공격(공격)을 한다.
	 @param targetName 돌격 대상 플레이어 캐릭터 이름.
	 @param attackPower 대미지에 걸리는 계수.
	 */
	public void cmdBossDirectAttack(string target_account_name, float attack_power)
	{
		do {

			if(this.state != chrControllerEnemyBase.EnemyState.MAIN) {
	
				break;
			}

			chrBehaviorPlayer charge_target_player = PartyControl.getInstance().getPlayerWithAccountName(target_account_name);

			if(charge_target_player == null) {

				// 발생하지 않아야 하지만....
				Debug.LogError ("Can't find the player to attack directly.");
				break;
			}

			PlayCharge(charge_target_player.gameObject, attack_power);

		} while(false);
	}

	/**
	 점프 공격을 한다.
	 @param attackPower 공격력.
	 @param attackRange 공격 판정이 닿는 범위.
	 */
	public void cmdBossRangeAttack(float power, float attackRange)
	{
		do {

			if(this.state != chrControllerEnemyBase.EnemyState.MAIN) {
	
				break;
			}

			PlayJump(power, attackRange);

		} while(false);
	}


	/**
	 퀵 공격한다.
	 @param targetName 돌격 대상 플레이어 캐릭터 이름.
	 @param attackPower 대미지에 걸리는 계수.
	 */
	public void cmdBossQuickAttack(string target_account_name, float attack_power)
	{
		do {

			if(this.state != chrControllerEnemyBase.EnemyState.MAIN) {
	
				break;
			}

			chrBehaviorPlayer charge_target_player = PartyControl.getInstance().getPlayerWithAccountName(target_account_name);

			if(charge_target_player == null) {
	
				// 발생하지 않아야 하지만....
				Debug.LogError ("Can't find the player to attack directly.");
				break;
			}

			this.PlayQuickAttack(charge_target_player.gameObject, attack_power);

		} while(false);
	}

    /**
     오브젝트 충돌.
     */
	void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.tag == "Wall")
		{
			hitWall(other.contacts[0].point, other.contacts[0].normal);
		}
	}

	void OnCollisionStay(Collision other)
	{
		switch(other.gameObject.tag) {

			case "Player":
			{
				chrBehaviorPlayer	player = other.gameObject.GetComponent<chrBehaviorPlayer>();

				if(player != null) {

					CollisionResult	result = new CollisionResult();
			
					result.object0    = this.gameObject;
					result.object1    = other.gameObject;
					result.is_trigger = false;
	
					this.collision_results.Add(result);
				}
			}
			break;
		}
	}
}
