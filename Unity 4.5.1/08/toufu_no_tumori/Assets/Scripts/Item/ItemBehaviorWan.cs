using UnityEngine;
using System.Collections;

// 아이템의 비헤이비어  개 울음소리용.
public class ItemBehaviorWan : ItemBehaviorBase {

	// ================================================================ //

	public enum STEP {

		NONE = -1,

		READY = 0,			// 비표시로 등장(dispatch)을 기다립니다.
		APPEAR,				// 등장.
		IDLE,
		ATTACHED,			// 줍는 중.

		NUM,
	};

	Step<STEP>		step = new Step<STEP>(STEP.NONE);

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	protected struct StepAppear {

		public Vector3	velocity;
		public Vector3	position;
		public Vector3	direction;

		public bool		is_freezed;
	}

	protected StepAppear	step_appear;

	protected chrBehaviorNPC_Dog	chr_dog = null;			// 개(이 아이템이 발생시키는 개).

	// 생성 직후에 호출됩니다.
	public override void	initialize_item()
	{
		this.item_favor.term_word = "주세요";
	}

	// 게임 시작 시에 한 번만 호출됩니다.
	public override void	start()
	{
		this.chr_dog = CharacterRoot.get().findCharacter<chrBehaviorNPC_Dog>("Dog");

		this.chr_dog.setItemWan(this);

		this.controll.setBillboard(true);
		this.controll.cmdSetVisible(false);

		this.step.set_next(STEP.READY);
	}

	// 매 프레임 호출됩니다.
	public override void	execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 이행할지 체크합니다.


		switch(this.step.do_transition()) {
	
			case STEP.APPEAR:
			{
				if(this.step_appear.is_freezed) {

					this.step.set_next(STEP.IDLE);
				}
			}
			break;

			case STEP.IDLE:
			{
				if(this.step.get_time() > 3.0f) {

					//this.step.set_next(STEP.APPEAR);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환됐을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.READY:
				{
					this.controll.cmdSetPickable(false);
					this.controll.cmdSetVisible(false);
				}
				break;

				case STEP.APPEAR:
				{
					this.controll.cmdSetPickable(false);

					float		h    = 1.0f;
					float		dist = 0.8f;

					this.step_appear.velocity   = this.step_appear.direction;
					this.step_appear.velocity.y = Mathf.Sqrt(Mathf.Abs(2.0f*Physics.gravity.y*h));
				
					float	t = Mathf.Sqrt(2.0f*(2.0f*h)/Mathf.Abs(Physics.gravity.y));

					this.step_appear.velocity.x *= dist/t;
					this.step_appear.velocity.z *= dist/t;
				
					this.step_appear.is_freezed = false;
				}
				break;
	
				case STEP.IDLE:
				{
					this.controll.cmdSetPickable(true);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.APPEAR:
			{

				this.step_appear.velocity += Physics.gravity*Time.deltaTime;
				this.step_appear.position += this.step_appear.velocity*Time.deltaTime;

				if(this.step_appear.position.y < 0.0f) {

					this.step_appear.position.y  = 0.0f;

					this.step_appear.velocity.y *= -0.5f;
					this.step_appear.velocity.x *=  0.5f;
					this.step_appear.velocity.z *=  0.5f;

					if(this.step_appear.velocity.y < Mathf.Abs(Physics.gravity.y*Time.deltaTime)) {

						this.step_appear.is_freezed = true;
					}
				}

				this.controll.transform.position = this.step_appear.position;		

			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

	// 픽업됐을 때 호출됩니다.
	public override void	onPicked()
	{
		this.step.set_next(STEP.ATTACHED);
	}

	// 리스폰했을 때 호출됩니다.
	// (말풍선인 경우는 버려졌을 때).
	public override void		onRespawn()
	{
		//this.controll.cmdSetPickable(false);
		this.step.set_next(STEP.READY);
	}

	// 말풍선이 개한테서 튀어나옵니다.
	public void		beginDispatch(Vector3 position, Vector3 direction)
	{
		this.controll.cmdSetVisible(true);

		this.step_appear.position = position;
		this.step_appear.direction = direction;
		this.step_appear.direction.y = 0.0f;
		this.step_appear.direction.Normalize();

		this.step.set_next(STEP.APPEAR);
	}

	// 출현(dispatch)할 수 있는가?.
	public bool		isEnableDispatch()
	{
		return(this.step.get_current() == STEP.READY);
	}
}
