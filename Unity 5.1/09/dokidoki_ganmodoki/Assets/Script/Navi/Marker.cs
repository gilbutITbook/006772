using UnityEngine;
using System.Collections;
using MathExtension;
using GameObjectExtension;

// 플레이어의 위치를 가리키는 표식.
public class Marker : MonoBehaviour {

	public enum STEP {

		NONE = -1,

		ENTER = 0,		// 화면 밖에서 등장.
		STAY,			// 루프 표시.
		LEAVE,			// 화면 밖으로 퇴장.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	public Texture	karada_texture;
	public Texture	ude_texture;
	public Texture	under_texture;

	protected Sprite2DControl	under_sprite;
	protected Sprite2DControl	karada_sprite;
	protected Sprite2DControl	ude_sprite;

	protected Vector2	offset = new Vector2(-43.0f, -16.0f);			// 팔 스프라이트의 중심 위치.
	protected Vector2	rotation_center = new Vector2(38.0f, 0.0f);		// 팔 스프라이트의 회전 위치.

	protected float	timer = 0.0f;

	protected Vector2	base_position_stay = new Vector2(110.0f,  90.0f);
	protected Vector2	base_position_start = new Vector2(700.0f,  90.0f);

	protected Vector2	base_position;

	protected SimpleSplineObject	enter_curve;
	protected SimpleSpline.Tracer	tracer = new SimpleSpline.Tracer();
	protected ipModule.FCurve		enter_fcurve = new ipModule.FCurve();

	protected SimpleSplineObject	leave_curve;
	protected ipModule.FCurve		leave_fcurve = new ipModule.FCurve();

	protected Vector2	enter_curve_offset;
	protected Vector2	leave_curve_offset;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		this.base_position_stay = new Vector2(110.0f,  90.0f);

		this.enter_curve = this.transform.FindChild("enter_spline").gameObject.GetComponent<SimpleSplineObject>();
		this.enter_curve.createControlVertices();
		this.enter_curve_offset = this.base_position_stay - this.enter_curve.curve.cvs.back().position.xz()*480.0f/2.0f;

		this.leave_curve = this.transform.FindChild("leave_spline").gameObject.GetComponent<SimpleSplineObject>();
		this.leave_curve.createControlVertices();
		this.leave_curve_offset = this.base_position_stay - this.leave_curve.curve.cvs.front().position.xz()*480.0f/2.0f;
	}

	void	Start()
	{
		this.step.set_next(STEP.ENTER);
	}

	void 	Update()
	{
		float	enter_time = 1.5f;
		float	stay_time  = 3.0f;
		float	leave_time = 1.0f;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이동 여부를 확인한다.

		switch(this.step.do_transition()) {

			// 화면 밖에서 등장.
			case STEP.ENTER:
			{
				if(this.step.get_time() > enter_time) {

					this.step.set_next(STEP.STAY);
				}
			}
			break;

			// 루프 표시.
			case STEP.STAY:
			{
				if(this.step.get_time() > stay_time) {

					this.step.set_next(STEP.LEAVE);
				}
			}
			break;

			// 화면 밖으로 퇴장.
			case STEP.LEAVE:
			{
				if(this.step.get_time() > leave_time) {

					this.destroy();
				}
			}
			break;
		}


		// ---------------------------------------------------------------- //
		// 상태가 전환 될 때 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 화면 밖에서 등장.
				case STEP.ENTER:
				{
					this.enter_fcurve.setSlopeAngle(70.0f, 5.0f);
					this.enter_fcurve.setDuration(enter_time);
					this.enter_fcurve.start();

					this.tracer.attach(this.enter_curve.curve);
				}
				break;

				// 화면 밖으로 퇴장.
				case STEP.LEAVE:
				{
					this.leave_fcurve.setSlopeAngle(10.0f, 70.0f);
					this.leave_fcurve.setDuration(leave_time);
					this.leave_fcurve.start();

					this.tracer.attach(this.leave_curve.curve);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 화면 밖에서 등장.
			case STEP.ENTER:
			{
				this.enter_fcurve.execute(Time.deltaTime);

				this.tracer.proceedToDistance(this.enter_fcurve.getValue()*this.tracer.curve.calcTotalDistance());

				this.base_position = this.tracer.getCurrent().position.xz()*480.0f/2.0f + this.enter_curve_offset;
			}
			break;

			// 화면 밖으로 퇴장.
			case STEP.LEAVE:
			{
				this.leave_fcurve.execute(Time.deltaTime);

				this.tracer.proceedToDistance(this.leave_fcurve.getValue()*this.tracer.curve.calcTotalDistance());

				this.base_position = this.tracer.getCurrent().position.xz()*480.0f/2.0f + this.leave_curve_offset;
			}
			break;
		}

		// ---------------------------------------------------------------- //

		this.set_position();

		//

		this.timer += Time.deltaTime;
	}

	// ================================================================ //

	// 생성.
	public void		create()
	{
		// 밑받침 (팔 주위 파란 테두리).
		this.under_sprite = Sprite2DRoot.get().createSprite(this.under_texture, true);
		this.under_sprite.setSize(new Vector2(this.under_texture.width, this.under_texture.height)/4.0f);

		// 몸.
		this.karada_sprite = Sprite2DRoot.get().createSprite(this.karada_texture, true);
		this.karada_sprite.setSize(new Vector2(this.karada_texture.width, this.karada_texture.height)/4.0f);

		// 팔.
		this.ude_sprite = Sprite2DRoot.get().createSprite(this.ude_texture, true);
		this.ude_sprite.setSize(new Vector2(this.ude_texture.width, this.ude_texture.height)/4.0f);

		// 위치를 설정해 둔다.

		this.base_position = this.base_position_start;
		this.set_position();
	}

	// 삭제한다.
	public void		destroy()
	{
		this.under_sprite.destroy();
		this.karada_sprite.destroy();
		this.ude_sprite.destroy();

		this.gameObject.destroy();
	}

	// 위치를 설정한다.
	public void		set_position()
	{
		Vector2	position = this.base_position;

		float	rate = Mathf.Repeat(this.timer, 4.0f);

		rate = Mathf.InverseLerp(0.0f, 4.0f, rate);

		position.y += 16.0f*Mathf.Sin(rate*Mathf.PI*2.0f);

		this.karada_sprite.setPosition(position);

		// 팔의 각도를 구한다.
		// 플레이어가 파를 가리키도록.

		Vector2		player_position = new Vector2(0.0f, 32.0f);

		Vector2		v = player_position - (position + this.offset + this.rotation_center);

		float	angle = Mathf.Atan2(v.y, v.x)*Mathf.Rad2Deg;

		angle = MathUtility.snormDegree(angle + 180.0f);

		this.set_arm_position_angle(position, angle);

	}

	// 팔 스프라이트의 위치와 각도를 구한다.
	protected void	set_arm_position_angle(Vector2 position, float angle)
	{
		// 회전 중심을 어깨 위치로 한다.

		Vector2		shift = this.rotation_center;

		shift -= (Quaternion.AngleAxis(angle, Vector3.forward)*this.rotation_center).xy();

		this.under_sprite.setPosition(position + offset + shift);
		this.ude_sprite.setPosition(position + offset + shift);	

		this.under_sprite.setAngle(angle);
		this.ude_sprite.setAngle(angle);	
	}
}
