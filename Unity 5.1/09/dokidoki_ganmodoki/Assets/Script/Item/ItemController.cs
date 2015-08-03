using UnityEngine;
using System.Collections;



// 아이템 컨트롤러.
public class ItemController : MonoBehaviour {

	private GameObject			myCamera   = null;	// 카메라.

	public ItemBehaviorBase		behavior = null;	// 비헤이비어.

	public string	type = "";						// 종류("apple"이나 "orange"）.
	public string	owner_account;					// 이 아이템을 만든 계정.
	public string	id = "";						// 고유 ID.

	public string	picker = "";					// 지금 이 아이템을 가지고 다니는 사람.

	// 스테이트.
	// "ing" 가 붙은 상태는 상태를 바꾸기 위해 조정 중.
	//
	// 1.로컬 플레이어가 아이템을 줍고자 했다.
	// 2.[PickingUp] 네트워크 플레이어에게 주어도 되는지 물어본다.
	// 3.[Picked] OK가 돌아오면 줍는다.
	//
	public enum State
	{
		None = 0, 				// 미획득.
		PickingUp,				// 획득 중.
		Picked,					// 횐득.
		Dropping,				// 폐기 중.
		Dropped,				// 폐기.
	};

	public State	state = State.None;	// 아이템 상태.

	// ---------------------------------------------------------------- //

	public float	timer = 0.0f;				// 아이템이 만들어진 후의 시간.
	public float	timer_prev;

	private struct Billboard {

		public bool		is_enable;				// 빌보드?.
		public float	roll;					// Z축 주변 회전.
	};

	private bool		is_pickable = true;		// 주울 수 있는가?(자라는 중엔 주울 수 없다).
	private Billboard	billboard;

	private Vector3		initial_position = Vector3.zero;

	// ================================================================ //
	// MonoBehaviour 에서 상속.

	void	Start()
	{
		this.myCamera  = GameObject.FindGameObjectWithTag("MainCamera");

		this.timer       = -1.0f;
		this.timer_prev  = -1.0f;

		this.billboard.is_enable = false;
		this.billboard.roll      = 0.0f;

		// 3D 모델 아이템이 생길 때까ㅣ는반드시 빌보드에서.
		this.setBillboard(true);

		this.initial_position = this.transform.position;

		this.behavior.start();
	}
	
	void	Update()
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
		//(마우스의 이동(로컬), 네트워크로부터 수신한 데이터로 이동(네트워크).
		//

		if (this.behavior != null) {
			this.behavior.execute();
		}

		// 빌보드일 때는 카메라 쪽으로 향한다.
		if(this.billboard.is_enable) {

			this.transform.rotation = this.myCamera.transform.rotation;
		}
	}

	// ================================================================ //
	// ㅣ비헤이비어용 커맨드.

	public void		cmdSetPickable(bool is_pickable)
	{
		this.is_pickable = is_pickable;
	}

	// ================================================================ //

	// 줍는다.

	public void		startPicked()
	{
		this.behavior.onPicked();
	}

	// 리스폰한다.
	public void		startRespawn()
	{
		this.transform.position = this.initial_position;

		this.timer       = -1.0f;
		this.timer_prev  = -1.0f;

		this.behavior.onRespawn();
	}

	// 삭제한다(주운 후).
	public void		vanish()
	{
		GameObject.Destroy(this.gameObject);
	}

	// ================================================================ //

	// 주울 수 있는가?.
	public bool		isPickable()
	{
		return(is_pickable && state == State.None);
	}

	// 빌보드?를 설정..
	public void		setBillboard(bool is_billboard)
	{
		this.billboard.is_enable = is_billboard;
		this.billboard.roll      = 0.0f;
	}


	// timer가 timer를 지나는 순간이면true.
	public bool		isPassingTime(float time)
	{
		bool	ret = false;

		if(this.timer_prev < time && time <= this.timer) {

			ret = true;
		}

		return(ret);
	}
}
