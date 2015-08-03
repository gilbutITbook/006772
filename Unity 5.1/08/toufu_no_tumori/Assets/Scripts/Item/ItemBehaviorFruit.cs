using UnityEngine;
using System.Collections;

// 아이템의 비헤이비어    과일용.
public class ItemBehaviorFruit : ItemBehaviorBase {

	private GameObject		germ  = null;		// 싹.
	private GameObject		glass = null;		// 풀.
	private GameObject		fruit = null;		// 열매.

	// ================================================================ //

	public enum STEP {

		NONE = -1,

		GERM = 0,			// 싹.
		GLASS,				// 풀.
		APPEAR,				// 열매가 등장 중. 풀이 열매가 된다 → 바운드하며 땅에 떨어짐.
		FRUIT,				// 実.

		NUM,
	};
	protected Step<STEP>		step = new Step<STEP>(STEP.NONE);

	protected ipModule.Jump		ip_jump = new ipModule.Jump();

	// ================================================================ //
	// MonoBehaviour에서 상속

	void	Awake()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 생성 직후에 호출됩니다.
	public override void	initialize_item()
	{
		this.germ  = this.gameObject.transform.FindChild("Germ").gameObject;
		this.glass = this.gameObject.transform.FindChild("Glass").gameObject;
		this.fruit = this.gameObject.transform.FindChild("Fruit").gameObject;

		this.germ.SetActive(is_active);
		this.glass.SetActive(false);
		this.fruit.SetActive(false);

		this.controll.cmdSetPickable(false);

		this.ip_jump.setBounciness(new Vector3(0.0f, -0.5f, 0.0f));

		switch(this.transform.parent.name) {

			case "Negi":
			{
				this.addPresetText("뭔가 자랐네요-");
				this.addPresetText("파였군요!");
			}
			break;
		}

		if (is_active) {
			this.step.set_next(STEP.GERM);
		}
	}

	// 게임 시작 시에 한 번만 호출됩니다.
	public override void	start()
	{
		this.controll.setVisible(is_active);

		this.controll.balloon.setColor(Color.green);

		this.controll.setBillboard(false);

		// 성장 중에는 주울 수 없습니다.
		this.controll.cmdSetPickable(false);
	}

	// 매 프레임 호출됩니다.
	public override void	execute()
	{
		float	germ_time = 5.0f;
		float	glass_time = 5.0f;
		
		// ---------------------------------------------------------------- //
		// 다음 상태로 이행할지 체크합니다.

		switch(this.step.do_transition()) {

			case STEP.GERM:
			{
				if(this.step.get_time() > germ_time) {

					this.step.set_next(STEP.GLASS);
				}
			}
			break;

			case STEP.GLASS:
			{
				if(this.step.get_time() > glass_time) {

					this.step.set_next(STEP.APPEAR);
				}
			}
			break;

			case STEP.APPEAR:
			{
				if(this.ip_jump.isDone()) {

					this.step.set_next(STEP.FRUIT);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환되면 초기화합니다.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.GERM:
				{
					// 성장 중엔 주울 수 없습니다.
					this.controll.cmdSetPickable(false);

					this.germ.SetActive(true);
					this.glass.SetActive(false);
					this.fruit.SetActive(false);

					if(this.transform.parent.name == "Negi") {
						ItemManager.ItemState state = ItemManager.get().FindItemState("Negi");
						
						if (state.state != ItemController.State.Picked) {
							this.controll.cmdDispBalloon(0);
						}
					}
				}
				break;

				case STEP.GLASS:
				{
					this.germ.SetActive(false);
					this.glass.SetActive(true);
					this.fruit.SetActive(false);
				}
				break;
	
				case STEP.APPEAR:
				{
					this.germ.SetActive(false);
					this.glass.SetActive(false);
					this.fruit.SetActive(true);

					Vector3		start = this.transform.position;
					Vector3		goal  = this.transform.position;

					this.ip_jump.start(start, goal, 1.0f);

					// 연기 효과.

					// 효과가 열매 모델에 묻히지 않게 카메라 쪽으로 밉니다.

					Vector3		smoke_position = this.transform.position + Vector3.up*0.3f;

					GameObject	main_camera = GameObject.FindGameObjectWithTag("MainCamera");

					Vector3		v = main_camera.transform.position - smoke_position;

					v.Normalize();
					v *= 1.0f;

					smoke_position += v;

					EffectRoot.getInstance().createSmoke01(smoke_position);
				}
				break;

				case STEP.FRUIT:
				{				
					if(this.transform.parent.name == "Negi") {
						ItemManager.ItemState state = ItemManager.get().FindItemState("Negi");
						
						if (state.state != ItemController.State.Picked) {
							this.controll.cmdDispBalloon(1);
						}
					}

					// 성장을 다 했으면 주울 수 있습니다
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
				this.ip_jump.execute(Time.deltaTime);

				this.transform.position = this.ip_jump.position;
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

	// 리스폰했을 때 호출됩니다.
	public override void		onRespawn()
	{
		this.step.set_next(STEP.GERM);
	}

	// 아이템을 성장 상태로 합니다(주울 수 있게 한다).
	public override void		finishGrowing()
	{
		this.step.set_next(STEP.FRUIT);

		this.germ.SetActive(false);
		this.glass.SetActive(false);
		this.fruit.SetActive(true);

		this.controll.cmdSetPickable(true);
	}

	// 아이템 활성화/비활성화 설정.
	public override void		activeItem(bool active)
	{
		this.is_active = active;

		this.germ.SetActive(active);
		this.glass.SetActive(false);
		this.fruit.SetActive(false);
		
		this.controll.cmdSetPickable(false);

		this.controll.setVisible(active);

		this.step.set_next(STEP.GERM);
	}
}
