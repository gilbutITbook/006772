using UnityEngine;
using System.Collections;

public class chrControllerEnemyBase : chrController {

	// FIXME 스테이트 문법이 없는 언어이므로 여기에 공배수적인 스테이트 리스트를 만들 수 밖에 없나?.
	public enum EnemyState
	{
		MAIN,
		VANISH,

		JUMP_PREACTION,	// 날아올라가기 전 예비 동작.
		JUMPING,		// 점프 중(애니메이션 표시 중).

		CHARGE_PREACTION,
		CHARGING,
		CHARGE_FINISHED,

		QUICK_ATTACK,
	};
	
	public EnemyState state = EnemyState.MAIN;

	protected float localStateTimer;

	public bool			damage_trigger = false;

	//========================================================================
	// キャラクターメトリクス
	public float		life = 5.0f;		// 라이프.
	public float		maxSpeed = 2.0f;	// 속도.

	protected float		floorY = 0.48f;		// 피봇의 높이.

	//========================================================================
	// ローカルな移動で処理するためのパラメータ.
	public float		move_dir = 0.0f;					// 이동 방향.
	public float		velocity = 0.0f;					// 이동 속도.
	public float		acceleration = 0.0f;				// 이동 가속도.
	
	protected bool		isPaused = false;					// 연출적인 이유로 일시정지가 명령되어 있는가?

	//========================================================================
	protected RoomController	room;							//< reference to room.

	//===================================================================
	// 애니메이션 관련
	protected Animator animator;

	//========================================================================
	// 일시적으로 maxSpeed를 높일 때.
	protected virtual float getMaxSpeedModifier()
	{
		return 1.0f;
	}

	// 적 제어용 템플릿 메소드.
	// 착지를 검출했을 때 호출됩니다(또는 호출합니다).
	protected virtual void landed()
	{
	}
	
	// 라이프 등의 기본 속성의 변경 등.
	override protected void _awake()
	{
		base._awake();
		
		animator = GetComponent<Animator>();
	}

	override protected void _start()
	{
		base._start();
		
		// RigidBody의 Y축을 프리즈.
		GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;
	}
	
	override protected void execute()
	{
		if (isPaused) {
			return;
		}
	
		localStateTimer += Time.deltaTime;

		// FIXME: 프레임레이트가 떨어지면 대미지 처리량이 변해버리지만...
		if (state != EnemyState.VANISH) {
			if(this.damage_trigger) {
				this.life -= 1.0f;
				if(this.life <= 0.0f) {
					goToVanishState(true);
				}
				else {
					playDamage();
				}
			}
		}

		switch (state) {
		case EnemyState.MAIN:
			exec_step_move();
			break;

		case EnemyState.VANISH:
			exec_step_vanish();
			break;
		default:
			break;
		}
		
		this.damage_trigger = false;

		updateAnimator();
	}

	protected virtual void exec_step_vanish()
	{
		if (this.damage_effect.isVacant())
		{
			if (room != null) {
			}

			(this.behavior as chrBehaviorEnemy).onDelete();

			EnemyRoot.getInstance().deleteEnemy(this);
		}
	}

	protected void changeState(EnemyState newState)
	{
        Debug.Log(newState);
        Debug.Log(state);
		if (state != newState)
		{
			state = newState;
			localStateTimer = 0.0f;
		}
	}

	public virtual void		causeDamage()
	{
		// 테이크 대미지가 유효한 슽이트인지 체크해서 처리.
		if (this.state != EnemyState.VANISH)
		{
			this.damage_trigger = true;
		}
	}
	
	public virtual void		playDamage()
	{
		this.damage_effect.startDamage();
		if (animator != null)
		{
			animator.SetTrigger("Damage");
		}
		SoundManager.getInstance().playSE(Sound.ID.DDG_SE_ENEMY01);
	}
	
	public virtual void		causeVanish(bool is_local = true)
	{
		goToVanishState(is_local);
	}
	
	protected virtual void goToVanishState(bool is_local)
	{
		// 이동을 허용할 스테이트인지 체크해서 상태 전환 실행.
		if (this.state != EnemyState.VANISH)
		{
			this.behavior.onVanished();

			playDying();

			this.state = EnemyState.VANISH;
			this.localStateTimer = 0.0f;
			this.damage_effect.startVanish();

			// 충돌을 제외.
			this.GetComponent<Collider>().enabled = false;
			this.GetComponent<Rigidbody>().Sleep();

			// 씬 읽어오기 등으로 시작 시각이 어긋다버리는 경우가 있으므로.
			// 만약을 위해 보스의 사망 통지를 송신합니다.
			if (is_local) {
				EnemyRoot.get().RequestBossDead(this.behavior.name);
			}
		}
	}

	// 사망 연출.
	protected virtual void playDying()
	{
		SoundManager.getInstance().playSE(Sound.ID.DDG_SE_ENEMY02);
	}

	// STEP.MOVE 실행.
	// 이동... FIXME  We have to call this from FixedUpdate().
	protected void	exec_step_move()
	{
		// ---------------------------------------------------------------- //
		// 이동(위치좌표 보간).
		
		Vector3		position  = this.getPosition();
		float		cur_dir   = this.getDirection();

		velocity += acceleration * Time.deltaTime;
		velocity = Mathf.Clamp(velocity, 0.0f, maxSpeed * getMaxSpeedModifier());
		float		speed_per_frame = velocity * Time.deltaTime;
		
		Vector3		move_vector = Quaternion.AngleAxis(this.move_dir, Vector3.up)*Vector3.forward;

		if (speed_per_frame > 0.0f) {
			position += move_vector*speed_per_frame;
		
			// 방향 보간.
		
			float	dir_diff = this.move_dir - cur_dir;
			
			if(dir_diff > 180.0f) {
				
				dir_diff = dir_diff - 360.0f;
				
			} else if(dir_diff < -180.0f) {
				
				dir_diff = dir_diff + 360.0f;
			}
			
			dir_diff *= 0.1f;
			
			if(Mathf.Abs(dir_diff) < 1.0f) {
				
				cur_dir = this.move_dir;
				
			} else {
				
				cur_dir += dir_diff;
			}
			
			// FIXME 동적인 플로어 컨택트로 할 것.
			position.y = floorY;
		}

		this.cmdSetPosition(position);
		this.cmdSetDirection(cur_dir);
	}

	protected virtual void updateAnimator()
	{
		if (animator != null)
		{
			animator.SetFloat("Motion_Speed", velocity / maxSpeed);
		}
	}
	
	//======================================================================
	//
	// 외부에서 호출되는 메소드.
	//

	// 적과 방을 연결합니다.	
	public void SetRoom(RoomController a_room)
	{
		this.room = a_room;
	}
	
	/// <summary>
	/// 캐릭터 제어의 일시 정지를 설정합니다.
	/// </summary>
	/// <param name="newPause">참일 때 포즈가 작동하고, 거짓일 때 포즈가 해제됩니다</param>
	public void SetPause(bool newPause)
	{
		isPaused = newPause;
		if (animator != null)
		{
			animator.speed = isPaused ? 0.0f : 1.0f;
		}
	}	
}
