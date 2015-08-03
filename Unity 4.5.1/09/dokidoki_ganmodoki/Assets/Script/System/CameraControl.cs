using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public chrController		player;

	protected CameraModule		module = null;
	public bool		is_smooth_revert = false;

	protected ipModule.Simple2Points		ip_2point = new ipModule.Simple2Points();

	// ================================================================ //

	public enum STEP {

		NONE = -1,

		NORMAL = 0,			// 
		OUTER_CONTROL,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.step.set_next(STEP.NORMAL);

		this.getModule();

		this.module.setInterest(this.calcGroundLevelInterest());
	}
	
	void	LateUpdate()
	{
		if(this.player != null) {

			this.update();
		}
	}

	private void	update()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.

		switch(this.step.do_transition()) {

			case STEP.NORMAL:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환될 때 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.NORMAL:
				{
					if(this.is_smooth_revert) {

						this.ip_2point.position.start = this.module.getPosture().intererst - this.player.getPosition();
						this.ip_2point.position.goal  = Vector3.zero;
						this.ip_2point.startConstantVelocity(4.0f);
					}
				}
				break;

				case STEP.OUTER_CONTROL:
				{
					this.is_smooth_revert = false;
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.NORMAL:
			{
				this.step_normal_execute();
			}
			break;

		}

		// ---------------------------------------------------------------- //

	}

	protected void	step_normal_execute()
	{
		if(this.is_smooth_revert) {

			Vector3		interest = this.player.getPosition();
	
			this.ip_2point.execute(Time.deltaTime);
	
			interest = this.ip_2point.position.current + this.player.getPosition();
	
			this.module.parallelInterestTo(interest);

		} else {

			this.module.parallelInterestTo(this.player.getPosition());
		}
	}

	public Vector3	calcGroundLevelInterest()
	{
		Vector3	interest = Vector3.zero;
		Plane	plane = new Plane(Vector3.up, Vector3.zero);
		Ray		ray = new Ray(this.transform.position, this.transform.forward);

		float	depth;

		if(plane.Raycast(ray, out depth)) {

			interest = ray.origin + ray.direction*depth;
		}

		return(interest);
	}

	/*public void		parallelMoveByInterst(Vector3 interest)
	{
		Vector3		eye_vector = this.calcGroundLevelInterest() - this.transform.position;

		this.transform.position = interest - eye_vector;
	}*/

	// ================================================================ //

	// 로컬 플레이어 설정.
	public void		setPlayer(chrController player)
	{
		this.player = player;
	}

	// 외부에서의 컨트롤 시작.
	public void		beginOuterControll()
	{
		this.step.set_next(STEP.OUTER_CONTROL);
	}

	// 외부에서의 컨트롤 종료.
	public void		endOuterControll()
	{
		this.step.set_next(STEP.NORMAL);
	}

	// 컨트롤 모듈을 얻는다.
	public CameraModule	getModule()
	{
		if(this.module == null) {
			
			this.module = this.GetComponent<CameraModule>();
		}

		return(this.module);
	}

	// ================================================================ //

	private	static CameraControl	instance = null;

	public static CameraControl	get()
	{
		if(CameraControl.instance == null) {

			CameraControl.instance = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraControl>();
		}

		return(CameraControl.instance);
	}
	public static CameraControl	getInstance()
	{
		return(CameraControl.get());
	}
}
