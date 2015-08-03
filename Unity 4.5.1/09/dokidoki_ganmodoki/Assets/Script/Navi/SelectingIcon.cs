using UnityEngine;
using System.Collections;

// 무기 선택 중 아이콘.
public class SelectingIcon : MonoBehaviour {

	// 텍스처.
	public Texture	uun_texture;			// 캐릭터  생각 중.
	public Texture	hai_texture;			// 캐릭터  결정!.

	public Texture	moya_negi_texture;		// 파.
	public Texture	moya_yuzu_texture;		// 유자.
	public Texture	moya_oke_texture;		// 나무통.
	public Texture	moya_kara_texture;		// 텅빔.

	public int		player_index;			// 플레이어의 account_global_index.

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		UUN = 0,		// 생각 중.
		HAI,			// 결정 액션.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	protected Sprite2DControl	root_sprite;	
	protected Sprite2DControl	chr_sprite;		// 캐릭터의 스프라이트.
	
	protected WeaponSelectNavi.Moya		moya;	// 말풍선.

	protected float		timer = 0.0f;

	protected ipModule.Spring	ip_spring = new ipModule.Spring();

	public Vector3	position = Vector3.zero;
	public bool		is_flip = false;

	// ================================================================ //
	// MonoBehaviour에서 상속

	void	Awake()
	{
	}
	void	Start()
	{
	}

	void	Update()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.

		switch(this.step.do_transition()) {

			case STEP.UUN:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환될 때 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 결정! 액션.
				case STEP.HAI:
				{
					this.chr_sprite.setTexture(this.hai_texture);

					this.ip_spring.k      = 750.0f - 400.0f;
					this.ip_spring.reduce = 0.77f;
					this.ip_spring.start(-50.0f);

					this.moya.beginHai();
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 결정! 액션.
			case STEP.HAI:
			{
				this.ip_spring.execute(Time.deltaTime);

				Vector3		chr_position = this.position + Vector3.up*(this.ip_spring.position + 50.0f);

				this.root_sprite.setPosition(chr_position);

				this.moya.setPosition(new Vector3(20.0f, 50.0f, 0.0f));
			}
			break;
		}

		// ---------------------------------------------------------------- //

		if(this.moya != null) {

			this.moya.execute();
		}

		this.timer += Time.deltaTime;
	}

	// ================================================================ //

	// 생성한다.
	public void		create()
	{
		this.root_sprite = Sprite2DRoot.get().createNull();

		// 캐릭터.
		this.chr_sprite = Sprite2DRoot.get().createSprite(this.uun_texture, true);
		this.chr_sprite.setSize(new Vector2(this.uun_texture.width, this.uun_texture.height)/4.0f);
		this.chr_sprite.transform.parent = this.root_sprite.transform;

		// 모락모락.
		this.moya = new WeaponSelectNavi.Moya();
		this.moya.root_sprite    = this.root_sprite;
		this.moya.negi_texture   = this.moya_negi_texture;
		this.moya.yuzu_texture   = this.moya_yuzu_texture;
		this.moya.oke_texture    = this.moya_oke_texture;
		this.moya.kara_texture   = this.moya_kara_texture;
		this.moya.selecting_icon = this;
		this.moya.create();
		this.moya.setPosition(new Vector3(20.0f, 50.0f, 0.0f));

		//

		this.setPosition(Vector3.zero);
	}

	// 위치를 설정한다.
	public void		setPosition(Vector3 position)
	{
		this.position = position;
		this.root_sprite.setPosition(this.position);
	}

	// 표시/비표시를 설정한다.
	public void		setVisible(bool is_visible)
	{
		this.root_sprite.gameObject.SetActive(is_visible);
	}

	// 좌우 반전 여부를 설정한다.
	public void		setFlip(bool is_flip)
	{
		this.is_flip = is_flip;

		if(this.is_flip) {

			this.root_sprite.setScale(new Vector2(-1.0f, 1.0f));

		} else {

			this.root_sprite.setScale(new Vector2( 1.0f, 1.0f));
		}
	}

	// 결정! 액션 시작.
	public void		beginHai()
	{
		this.step.set_next(STEP.HAI);
	}

}

namespace WeaponSelectNavi {

// 머리 위 연기.
public class Moya {

	public enum STEP {

		NONE = -1,

		UUN = 0,		// 생각 중.
		HAI,			// 결정! 액션.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	public SelectingIcon	selecting_icon;

	// 텍스처.
	public Texture		negi_texture;		// 파 텍스처.
	public Texture		yuzu_texture;		// 유자 텍스처.
	public Texture		oke_texture;		// 나무통 텍스처.
	public Texture		kara_texture;		// 텅빈 텍스처.

	public    Sprite2DControl	root_sprite;
	protected Sprite2DControl	negi_sprite;
	protected Sprite2DControl	yuzu_sprite;
	protected Sprite2DControl[]	mini_moya_sprite;	// 꼬리.

	protected Vector3	moya_position;
	protected Vector3	moya_position_center;		// 움직임의 중심.
	protected Vector3	moya_root_position;			// 꼬리가 캐릭터에 붙어 있는 위치 .

	protected float			timer = 0.0f;

	protected const float	MOYA_CYCLE = 2.3f;			// [sec] 주기.

	// '결정!'액션 중의 정보.
	protected struct StepHai {

		public Vector3	moya_offset;
	}
	protected StepHai	step_hai;

	// ================================================================ //

	public void		create()
	{
		// 꼬리.

		this.mini_moya_sprite = new Sprite2DControl[2];

		this.mini_moya_sprite[0] =  Sprite2DRoot.get().createSprite(this.kara_texture, true);
		this.mini_moya_sprite[0].setSize(new Vector2(this.kara_texture.width, this.kara_texture.height)*0.05f);
		this.mini_moya_sprite[0].transform.parent = this.root_sprite.transform;

		this.mini_moya_sprite[1] =  Sprite2DRoot.get().createSprite(this.kara_texture, true);
		this.mini_moya_sprite[1].setSize(new Vector2(this.kara_texture.width, this.kara_texture.height)*0.1f);
		this.mini_moya_sprite[1].transform.parent = this.root_sprite.transform;

		// 파/유자.
		this.negi_sprite = Sprite2DRoot.get().createSprite(this.negi_texture, true);
		this.negi_sprite.setSize(new Vector2(this.negi_texture.width, this.negi_texture.height)/4.0f);
		this.negi_sprite.transform.parent = this.root_sprite.transform;

		this.yuzu_sprite = Sprite2DRoot.get().createSprite(this.yuzu_texture, true);
		this.yuzu_sprite.setSize(new Vector2(this.yuzu_texture.width, this.yuzu_texture.height)/4.0f);
		this.yuzu_sprite.transform.parent = this.root_sprite.transform;

		//

		this.step.set_next(STEP.UUN);
	}

	public void		destroy()
	{
		foreach(var sprite in mini_moya_sprite) {

			sprite.destroy();
		}

		this.negi_sprite.destroy();
		this.yuzu_sprite.destroy();
	}

	public void		execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.

		switch(this.step.do_transition()) {

			case STEP.UUN:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환될 때 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				// '결정!' 액션.
				case STEP.HAI:
				{
					foreach(var sprite in mini_moya_sprite) {

						sprite.setVisible(false);
					}

					this.step_hai.moya_offset = this.moya_position - this.moya_root_position;
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.UUN:
			{
				this.exec_step_uun();
			}
			break;

			// '결정' 액션.
			case STEP.HAI:
			{
				this.exec_step_hai();
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

	// 위치를 설정한다.
	public void		setPosition(Vector3 position)
	{
		this.moya_root_position   = position;
		this.moya_position_center = this.moya_root_position + new Vector3(32.0f, 60.0f, 0.0f);
	}

	// 결정한 후의 액션을 시작한다.
	public void		beginHai()
	{
		this.step.set_next(STEP.HAI);
	}

	// ---------------------------------------------------------------- //

	protected void		exec_step_uun()
	{
		// 파 / 유자 포지션

		this.moya_position = this.calc_moya_position(this.timer);
		this.negi_sprite.setPosition(this.moya_position);
		this.yuzu_sprite.setPosition(this.moya_position);

		// 파/유자 교대로 표시.

		float	cycle = 4.0f;
		float	time0 = 1.5f;
		float	time1 = 2.0f;
		float	time2 = 3.5f;
		float	time3 = 4.0f;

		float	rate = Mathf.Repeat(this.timer, cycle);

		float	alpha = 1.0f;

		if(rate < time0) {

			alpha = 1.0f;

		} else if(rate < time1) {

			alpha = Mathf.InverseLerp(time0, time1, rate);
			alpha = Mathf.Cos(alpha*Mathf.PI/2.0f);

		} else if(rate < time2) {

			alpha = 0.0f;

		} else {

			alpha = Mathf.InverseLerp(time2, time3, rate);
			alpha = Mathf.Sin(alpha*Mathf.PI/2.0f);
		}

		this.yuzu_sprite.setVertexAlpha(1.0f - alpha);

		// 작은 연기 2개.
		// 조금 느리게 움직이는 것처럼 보이게 한다.

		Vector3		p;

		p = Vector3.Lerp(this.moya_root_position, this.calc_moya_position(this.timer - MOYA_CYCLE*0.2f), 0.5f);
		this.mini_moya_sprite[1].setPosition(p);

		p = Vector3.Lerp(this.moya_root_position, this.calc_moya_position(this.timer - MOYA_CYCLE*0.4f), 0.3f);
		this.mini_moya_sprite[0].setPosition(p);

		//

		this.timer += Time.deltaTime;
	}

	// 가장 위의 연기 위치를 구한다.
	protected Vector3	calc_moya_position(float time)
	{
		Vector3		position = this.moya_position_center;
		float		rate;

		rate = Mathf.Repeat(time, MOYA_CYCLE)/MOYA_CYCLE;
		rate = Mathf.Lerp((Mathf.Sin((rate - 0.5f)*Mathf.PI) + 1.0f)/2.0f, rate, 0.7f);
		position.x += Mathf.Sin(rate*Mathf.PI*2.0f)*16.0f;

		rate = Mathf.Repeat(time, MOYA_CYCLE*2.0f)/(MOYA_CYCLE*2.0f);

		if(rate < 0.5f) {

			rate *= 2.0f;
			rate = Mathf.Lerp((Mathf.Sin((rate - 0.5f)*Mathf.PI) + 1.0f)/2.0f, rate, 0.7f);
			rate = rate*0.5f;

		} else {

			rate = (rate - 0.5f)*2.0f;
			rate = Mathf.Lerp((Mathf.Sin((rate - 0.5f)*Mathf.PI) + 1.0f)/2.0f, rate, 0.7f);
			rate = 0.5f + rate*0.5f;
		}

		position.y += Mathf.Sin(rate*Mathf.PI*2.0f)*8.0f;

		return(position);
	}

	// ---------------------------------------------------------------- //

	// 『決めた！』アクションの実行.
	protected void		exec_step_hai()
	{
		float	FLIP_START    = 0.2f;
		float	FLIP_DURATION = 0.2f;

		Vector3		p0 = this.moya_root_position + this.step_hai.moya_offset;
		Vector3		p1 = this.moya_root_position + Vector3.up*40.0f;

		float	rate, scale;

		rate = Mathf.Min(this.step.get_time(), FLIP_START)/FLIP_START;
		rate = Mathf.Sin(rate*Mathf.PI/2.0f);

		this.moya_position = Vector3.Lerp(p0, p1, rate);

		this.negi_sprite.setPosition(this.moya_position);
		this.yuzu_sprite.setPosition(this.moya_position);

		if(this.step.get_time() > FLIP_START) {

			rate = Mathf.Min(this.step.get_time() - FLIP_START, FLIP_DURATION)/FLIP_DURATION;

			scale = Mathf.Sin(rate*Mathf.PI/2.0f);
			scale = Mathf.Cos(scale*Mathf.PI);
			scale = Mathf.Abs(scale);

			this.negi_sprite.setScale(new Vector2(scale, 1.0f));
			this.yuzu_sprite.setScale(new Vector2(scale, 1.0f));

			// 반 바퀴 돈 시점에서 텍스처를 교체한다.
			if(this.step.is_acrossing_time(FLIP_START + FLIP_DURATION*0.5f)) {

				this.negi_sprite.setTexture(this.oke_texture);
				this.yuzu_sprite.setVisible(false);

			}
			if(this.step.get_time() >= FLIP_START + FLIP_DURATION*0.5f) {

				if(this.selecting_icon.is_flip) {

					this.negi_sprite.setScale(new Vector2(-scale, 1.0f));
				}
			}
		}
	}

}

}

