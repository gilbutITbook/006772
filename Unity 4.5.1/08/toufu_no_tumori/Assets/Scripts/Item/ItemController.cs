using UnityEngine;
using System.Collections;

// 아이템을 가지고 있을 때 캐릭터에게 부여되는 특전.
public class ItemFavor {

	public ItemFavor()
	{
		this.term_word = "";
		this.is_enable_house_move = false;
	}

	public string	term_word = "";			// 어미.
	public bool		is_enable_house_move;	// 이사할 수 있게 됩니다.
};


// 아이템 컨트롤러.
public class ItemController : MonoBehaviour {

	public static float		BALLOON_HEIGHT   = 1.0f;		// 말풍선의 높이.
	public static float		COLLISION_RADIUS = 0.5f;		// 콜리전 구의 반지름.

	// ---------------------------------------------------------------- //

	private GameObject			main_camera = null;	// 카메라.

	public ItemBehaviorBase		behavior = null;	// 비헤이비어.
	public GameObject			model    = null;	// 모델.

	public string		owner_account;					// 이 아이템을 만든 어카운트.
	public string		id = "";						// 유니크 ID.
	protected string	production = "";				// 생산지 밖으로 가져나갈 수 있는 아이템이 나타날 맵 이름.

	public string	picker = "";					// 지금 이 아이템을 가지고 다니는 사람.

	public ChattyBalloon			balloon = null;		// 말풍선.

	// 스테이트.
	// "ing" 가 붙은 상태는 스테이트를 바꾸려고 조정 중입니다.
	//
	// 1.로컬 플레이어가 아이템을 주우려고 했다.
	// 2.[PickingUp] 네트워크 플레이어에게 주워도 되는지 물어본다.
	// 3.[Picked] OK 가 돌아오면 줍는다.
	//

	public enum State
	{
		Growing = 0, 			// 발생 중.
		None, 					// 미획득.
		PickingUp,				// 획득 중.
		Picked,					// 획득.
		Dropping,				// 폐기 중.
		Dropped,				// 폐기.
	}

	public State	state = State.None;	// 아이템의 상태.

	// ---------------------------------------------------------------- //

	public float	timer = 0.0f;				// 아이템이 만들어지고 나서의 시간.
	public float	timer_prev;

	protected struct Billboard {

		public bool		is_enable;				// 빌보드?.
		public float	roll;					// Z축 주위의 회전.
	};
	protected Billboard		billboard;

	protected bool		is_visible = true;		// 아이템의 가시성.
	protected bool		is_pickable = true;		// 주울 수 있는가？(성장 도준엔 주울 수 없다).
	protected bool		is_exportable = false;	// 다른 맵에 가져갈 수 있는가?.

	protected Vector3		initial_position = Vector3.zero;
	protected Quaternion	initial_rotation = Quaternion.identity;

	protected float	collision_radius = COLLISION_RADIUS;

	// ================================================================ //
	// MonoBehaviour から에서 상속.

	void	Start()
	{
		this.main_camera  = GameObject.FindGameObjectWithTag("MainCamera");

		this.balloon = BalloonRoot.get().createBalloon();

		this.timer       = -1.0f;
		this.timer_prev  = -1.0f;

		this.billboard.is_enable = false;
		this.billboard.roll      = 0.0f;

		this.initial_position = this.transform.position;
		this.initial_rotation = this.transform.rotation;

		this.is_visible = true;
	
		this.behavior.start();
	}

	void	Update()
	{
		if(this.isActive()) {

			this.update_entity();
		}
	}

	void	update_entity()
	{
		if(this.timer < 0.0f) {

			this.timer_prev = -1.0f;
			this.timer      =  0.0f;

		} else {

			this.timer_prev = this.timer;
			this.timer += Time.deltaTime;
		}

		// ---------------------------------------------------------------- //
		// 비헤이비어 실행.
		//
		// (마우스 이동(로컬), 네트워크로터 수신한 데이터로 이동(네트워크)).
		//

		this.behavior.execute();

		// 빌보드 일 때는 카메라 쪽으로 향합니다.
		if(this.billboard.is_enable) {

			this.model.transform.localPosition = Vector3.zero;
			this.model.transform.localRotation = Quaternion.identity;

			Vector3		camera_position = this.transform.InverseTransformPoint(this.main_camera.transform.position);

			this.model.transform.Translate(Vector3.up*this.collision_radius);
			this.model.transform.localRotation *= Quaternion.LookRotation(-camera_position, Vector3.up);
			this.model.transform.Translate(-Vector3.up*this.collision_radius);
		}

		// ---------------------------------------------------------------- //
		// 말풍선 위치 / 색상.

		if(this.balloon != null && this.balloon.getText() != "") {

			Vector3		on_screen_position = Camera.main.WorldToScreenPoint(this.transform.position + Vector3.up*BALLOON_HEIGHT);

			this.balloon.setPosition(new Vector2(on_screen_position.x, Screen.height - on_screen_position.y));

			this.balloon.setColor(Color.yellow);
		}
	}

	// ================================================================ //
	// 비헤이비어가 사용할 커맨드.
	
	// 위치를 설정합니다.
	public void		cmdSetPosition(Vector3 position)
	{
		this.transform.position = position;
	}

	// 방향을 설정합니다.
	public void		cmdSetDirection(float angle)
	{
		this.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
	}

	// ================================================================ //
	// 비헤이비어용 커맨드.

	public void		cmdSetPickable(bool is_pickable)
	{
		this.is_pickable = is_pickable;
	}

	// 프리셋 텍스트를 사용해 말풍선을 표시합니다.
	public void		cmdDispBalloon(int text_id)
	{
		this.balloon.setText(this.behavior.getPresetText(text_id));
	}

	// 말풍선을 지웁니다.
	public void		cmdHideBalloon()
	{
		this.balloon.clearText();
	}

	// 표시/ 비표시합니다.
	public void		cmdSetVisible(bool is_visible)
	{
		this.setVisible(is_visible);
	}
	
	public bool		isActive()
	{
		return this.behavior.is_active;
	}

	// 콜리전을 ON/OFF합니다.
	public void		cmdSetCollidable(bool is_enable)
	{
		this.GetComponent<Collider>().enabled = is_enable;
	}

	// ================================================================ //

	// 표시/비표시합니다.
	public void		setVisible(bool is_visible)
	{
		this.is_visible = is_visible;

		Renderer[]		renderers = this.gameObject.GetComponentsInChildren<Renderer>();

		foreach(var renderer in renderers) {

			renderer.enabled = this.is_visible;
		}

		// 그림자.
		Projector[]		projectors = this.gameObject.GetComponentsInChildren<Projector>();
	
		foreach(var projector in projectors) {

			projector.enabled = this.is_visible;
		}

		// 말풍선.
		this.balloon.setVisible(this.is_visible);

		// 콜리전도 연동시킵니다.
		this.cmdSetCollidable(this.is_visible);
	}

	// 픽업됩니다.
	public void		startPicked()
	{
		if (this.balloon != null) {
			this.balloon.clearText();
		}

		this.behavior.onPicked();
	}

	// 리스폰합니다.
	public void		startRespawn()
	{
		this.transform.position = this.initial_position;
		this.transform.rotation = this.initial_rotation;

		this.timer       = -1.0f;
		this.timer_prev  = -1.0f;

		if (isActive()) {
			this.behavior.onRespawn();
		}
		else {
			this.setVisible(false);
		}
	}

	// 아이템을 성장 상태로 바꿉니다(주울 수 있게 합니다).
	public void		finishGrowing()
	{
		this.behavior.finishGrowing();
	}

	// 다른 맵에 가지고 갈 수 있는가?.
	public bool		isExportable()
	{
		return(this.is_exportable);
	}

	// 다른 맵에 가지고 갈 수 있는지/가지고 갈 수 없는지 설정합니다.
	public void		setExportable(bool is_exportable)
	{
		this.is_exportable = is_exportable;
	}

	// 생산지(스폰한 맵 이름)을 가져옵니다.
	public string		getProduction()
	{
		return(this.production);
	}

	// 생산지(스폰한 맵 이름)을 설정합니다.
	public 	void	setProduction(string production)
	{
		this.production = production;
	}

	// ================================================================ //

	// 픽업할 수 있는가?.
	public bool		isPickable()
	{
		return(is_pickable);
	}

	// 빌보드?를 설정합니다.
	public void		setBillboard(bool is_billboard)
	{
		this.billboard.is_enable = is_billboard;
		this.billboard.roll      = 0.0f;
	}


	// timer가 timer를 지나는 순간이면 true.
	public bool		isPassingTime(float time)
	{
		bool	ret = false;

		if(this.timer_prev < time && time <= this.timer) {

			ret = true;
		}

		return(ret);
	}

	// 콜리전 구의 반지름을 획득합니다.
	public float	getCollisionRadius()
	{
		return(this.collision_radius);
	}
}
