using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// '준비!' '간식 타임!' 등의 2D 폰트로 외치는 소리 표시.
public class YellDisp : MonoBehaviour {

	public enum STEP {

		NONE = -1,

		APPEAR = 0,		// 캐릭터의 이동에 맞춰 쿠키를 표시.
		FLIP,			// 캐릭터가 턴하면 쿠키가 차례로 문자로 바뀐다.
		STAY,			// 루프 표시.
		FADE_OUT,		// 페이드 아웃.
		FINISH,			// 끝.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	public YELL_WORD		word = YELL_WORD.NONE;
	public Texture			icon_texture;
	public Texture			moji_mae_texture;
	public YELL_FONT[]		yell_words;
	public Sprite2DControl	root_sprite;

	public  const float			POSITION_Y = 128.0f;

	protected Yell.Icon			icon;

	protected List<Yell.Moji>	mojis;

	protected int				flip_count = 0;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
	}

	void	Start()
	{
		this.step.set_next(STEP.APPEAR);
	}

	void	Update()
	{
		float	fade_out_time = 0.5f;

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.

		switch(this.step.do_transition()) {

			// 캐릭터 이동에 맞춰 쿠키 표시.
			case STEP.APPEAR:
			{
				if(this.icon.isFinished()) {

					this.step.set_next(STEP.FLIP);
				}
			}
			break;

			// 캐릭터가  턴하면 쿠키가 차례로 문자로 바뀐다.
			case STEP.FLIP:
			{
				if(!this.mojis.Exists(x => !x.isFinished())) {

					if(this.word == YELL_WORD.CAKE_COUNT || this.word == YELL_WORD.OSIMAI) {
	
						this.step.set_next(STEP.STAY);

					} else {

						this.step.set_next(STEP.FADE_OUT);
					}
				}
			}
			break;

			// 페이트 아웃.
			case STEP.FADE_OUT:
			{
				if(this.step.get_time() > fade_out_time) {

					this.destroy();
					GameObject.Destroy(this.gameObject);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환될 때 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 캐릭터 이동에 맞춰 쿠키 표시.
				case STEP.APPEAR:
				{
					this.icon.reset();
		
					foreach(var moji in this.mojis) {
			
						moji.reset();
					}
				}
				break;

				// 캐릭터가 턴하면 쿠키가 차례로 문자로 바뀐다.
				case STEP.FLIP:
				{
					this.flip_count = 0;
				}
				break;
	
				// 페이드 아웃.
				case STEP.STAY:
				case STEP.FADE_OUT:
				{
					this.icon.beginFadeOut();
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 캐릭터 이동에 맞춰 쿠키 표시.
			case STEP.APPEAR:
			{
				foreach(var moji in this.mojis) {
		
					if(moji.check_start(this.icon.sprite.getPosition().x)) {

						break;
					}
				}
			}
			break;

			// 캐릭터가 턴하면 쿠키가 차례로 문자로 바뀐다.
			case STEP.FLIP:
			{
				float	delay = 0.1f;

				int		n = Mathf.Min(Mathf.FloorToInt(this.step.get_time()/delay), this.mojis.Count);

				if(this.flip_count < n) {

					this.mojis[this.flip_count].startFlip();
					this.flip_count++;
				}
			}
			break;

			// 페이트 아웃.
			case STEP.FADE_OUT:
			{
				float	a = ipCell.get().setInput(this.step.get_time()).ilerp(0.0f, fade_out_time).lerp(1.0f, 0.0f).getCurrent();
	
				foreach(var moji in this.mojis) {

					if(moji.sprite.isVisible()) {

						moji.sprite.setVertexAlpha(a);
					}
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //

		this.icon.execute();

		foreach(var moji in this.mojis) {

			moji.execute();
		}
	}

	// ================================================================ //

	public void		create()
	{
		Navi	navi = Navi.get();

		//

		this.root_sprite = Sprite2DRoot.get().createNull();
		this.root_sprite.setPosition(new Vector3(0.0f, POSITION_Y, 0.0f));

		// 아이콘, 문자 오브젝트 생성.

		this.icon = new Yell.Icon();
		this.icon.create(this.icon_texture);
		this.icon.sprite.transform.parent = this.root_sprite.transform;

		//

		this.mojis = new List<Yell.Moji>();

		for(int i = 0;i < this.yell_words.Length;i++) {

			YellFontData	font_data = navi.getYellFontData(this.yell_words[i]);

			Yell.Moji	moji = new Yell.Moji();

			moji.create(font_data.texture, this.moji_mae_texture);

			if(font_data.is_small) {

				moji.moji_mae_scale *= 0.5f;
			}
			moji.yell = this;
			moji.index = i;
			moji.sprite.transform.parent = this.root_sprite.transform;
			moji.reset();

			if(i%3 == 0) {

				moji.sprite.setVertexColor(new Color(1.0f, 1.0f, 0.5f));

			} else if(i%3 == 1) {

				moji.sprite.setVertexColor(new Color(1.0f, 0.7f, 0.7f));

			} else if(i%3 == 2) {

				moji.sprite.setVertexColor(new Color(0.3f, 1.0f, 1.0f));
			}

			this.mojis.Add(moji);
		}

		this.icon.sprite.setDepth(this.mojis[this.mojis.Count - 1].sprite.getDepth() - 0.1f);

		// 문자 위치.

		float		pitch = 54.0f;
		Vector2		p0 = Vector2.zero;
		Vector2		p1 = Vector2.zero;

		p0.x = (float)this.mojis.Count*pitch/2.0f;
		p0.y = 0.0f;

		p1.x = 0.0f - ((float)this.mojis.Count)*pitch/2.0f - pitch/2.0f;
		p1.y = p0.y;

		this.icon.p0 = p0;
		this.icon.p1 = p1;
		p1.x += pitch;

		p0.x = p1.x;

		for(int i = 0;i < this.mojis.Count;i++) {

			YellFontData	font_data = navi.getYellFontData(this.yell_words[i]);

			this.mojis[i].p0 = p0;
			this.mojis[i].reset();

			if(font_data.is_small) {

				this.mojis[i].p0.x -= pitch*0.25f;
				this.mojis[i].p0.y -= pitch*0.25f;

				p0.x += pitch*0.5f;

			} else {

				p0.x += pitch;
			}
		}

	}

	public void		destroy()
	{
		this.icon.sprite.destroy();

		foreach(var moji in this.mojis) {

			moji.sprite.destroy();
		}

		GameObject.Destroy(this.gameObject);
	}

	// 위치 설정.
	public void		setPosition(Vector3 position)
	{
		this.root_sprite.setPosition(position);
	}

	// index에 해당하는 문자 오브젝트를 가져온다.
	public Yell.Moji	getMoji(int index)
	{
		return(this.mojis[index]);
	}

	// 페이드 아웃 시작.
	public void		beginFadeOut()
	{
		this.step.set_next(STEP.FADE_OUT);
	}

}

namespace Yell {

// 선두 캐릭터.
public class Icon {

	public enum STEP {

		NONE = -1,

		MOVE = 0,		// 이동.
		TURN,			// 빙글 돈다.
		FINISH,			// 끝.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	public Sprite2DControl	sprite;

	public Vector2		p0;					// 시작 지점.
	public Vector2		p1;					// 목표 지점.

	public ipModule.FCurve	fcurve;
	
	protected bool		is_fading  = false;	// 페이드 아웃 중?.
	protected float		fade_timer = 0.0f;
	
	// ================================================================ //

	public void		beginFadeOut()
	{
		if(!this.is_fading) {

			this.fade_timer = 0.0f;
			this.is_fading = true;
		}
	}

	public void		create(Texture texture)
	{
		this.sprite = Sprite2DRoot.get().createSprite(texture, true);
		this.sprite.setSize(Vector2.one*64.0f);
		this.sprite.setVisible(false);

		this.fcurve = new ipModule.FCurve();
		this.fcurve.setSlopeAngle(70.0f, 5.0f);
		this.fcurve.setDuration(0.7f);
		this.fcurve.start();
	}

	// 표시 시작.
	public void		start()
	{
		this.step.set_next(STEP.MOVE);
	}

	// 리셋(재시작) 디버그용.
	public void		reset()
	{
		this.start();
	}

	protected	float	turn_time = 0.2f;

	// 끝?.
	public bool		isFinished()
	{
		bool	ret = false;

		do {

			if(this.step.get_current() != STEP.TURN) {

				break;
			}
			if(this.step.get_time() < turn_time*0.5f) {

				break;
			}

			ret = true;

		} while(false);

		return(ret);
	}

	// 매 프레임 실행.
	public void		execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크.

		switch(this.step.do_transition()) {

			case STEP.MOVE:
			{
				if(this.fcurve.isDone()) {

					this.step.set_next(STEP.TURN);
				}
			}
			break;

			case STEP.TURN:
			{
				if(this.step.get_time() > turn_time) {

					this.step.set_next(STEP.FINISH);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환될 때 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.MOVE:
				{
					this.sprite.setPosition(this.p0);
					this.sprite.setVisible(true);		
					this.fcurve.start();
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.MOVE:
			{
				this.fcurve.execute(Time.deltaTime);
		
				Vector2	icon_pos = Vector2.Lerp(this.p0, this.p1, this.fcurve.getValue());
		
				this.sprite.setPosition(icon_pos);
				this.sprite.setScale(new Vector2( 1.0f, 1.0f));
			}
			break;

			case STEP.TURN:
			{
				float	rate = Mathf.Clamp01(this.step.get_time()/turn_time);

				rate = Mathf.Lerp(0.0f, Mathf.PI, rate);

				float	sx = Mathf.Cos(rate);

				this.sprite.setScale(new Vector2(sx, 1.0f));
			}
			break;
		}

		// ---------------------------------------------------------------- //

		if(this.is_fading) {

			float	fade_out_time = 0.5f;
			float	a = ipCell.get().setInput(this.fade_timer).ilerp(0.0f, fade_out_time).lerp(1.0f, 0.0f).getCurrent();

			this.sprite.setVertexAlpha(a);	

			this.fade_timer += Time.deltaTime;	
		}
	}
}

// 문자.
public class Moji {

	public enum STEP {

		NONE = -1,

		WAIT = 0,		// 표시 시작 대기중(비표시).
		APPEAR,			// 등장.
		FLIP,			// 쿠키가 문자로 변한다.
		FINISH,			// 끝.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	public YellDisp	yell;

	public int		index = -1;

	public Texture	moji_texture;
	public Texture	moji_mae_texture;
	public Vector2	moji_texture_size;

	public Vector2	p0;					// 시작 지점..

	public Sprite2DControl		sprite;

	public ipModule.FCurve		fcurve;
	public ipModule.Spring		spring;

	public float	moji_mae_scale = 0.9f;

	protected bool	is_finished = false;

	// ================================================================ //

	public void		create(Texture moji_texture, Texture moji_mae_texture)
	{
		this.moji_texture      = moji_texture;
		this.moji_mae_texture  = moji_mae_texture;
		this.moji_texture_size = new Vector2(this.moji_texture.width, this.moji_texture.height);

		this.sprite = Sprite2DRoot.get().createSprite(this.moji_mae_texture, true);
		this.sprite.setSize(Vector2.one*64.0f);

		this.fcurve = new ipModule.FCurve();
		this.fcurve.setSlopeAngle(70.0f, 5.0f);
		this.fcurve.setDuration(0.3f);

		this.spring = new ipModule.Spring();
		this.spring.k      = 100.0f;
		this.spring.reduce = 0.90f;
	}

	// 리셋(재시작).
	public void		reset()
	{
		this.sprite.setPosition(this.p0);
		this.sprite.setVisible(false);

		this.sprite.setTexture(this.moji_mae_texture);
		this.step.set_next(STEP.WAIT);
	}

	// 표시를 시작할지 체크한다.
	public bool		check_start(float leader_x)
	{
		bool	trigger_start = false;

		do {

			if(this.step.get_current() != STEP.WAIT) {

				break;
			}
			if(leader_x > this.p0.x) {

				break;
			}

			this.start();
			trigger_start = true;

		} while(false);

		return(trigger_start);
	}

	// 표시 시작.
	public void		start()
	{
		this.step.set_next(STEP.APPEAR);
	}

	// 플립 시작.
	public void		startFlip()
	{
		this.step.set_next(STEP.FLIP);
	}

	// 끝?.
	public bool		isFinished()
	{
		return(this.is_finished);
	}

	// 종료 플래그를 지운다.
	public void		clearFinishedFlag()
	{
		this.is_finished = false;
	}

	// 매 프레임 실행.
	public void		execute()
	{
		float	flip_time = 1.0f;

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다..

		switch(this.step.do_transition()) {

			case STEP.FLIP:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				// 표시 시작 대기 중(비표시).
				case STEP.WAIT:
				{
					this.is_finished = false;
				}
				break;

				// 등장.
				case STEP.APPEAR:
				{
					if(this.moji_mae_texture != null) {

						this.sprite.setVisible(true);
						this.sprite.setPosition(this.p0);
	
						this.fcurve.start();
	
						this.spring.start(0.75f);

					} else {

						this.sprite.setVisible(false);
					}
				}
				break;

				// 쿠키가 문자로 변한다.
				case STEP.FLIP:
				{
					if(this.moji_texture != null) {

						this.sprite.setScale(Vector2.one);
						this.sprite.setTexture(this.moji_texture);
						this.sprite.setSize(this.moji_texture_size);
						this.sprite.setVertexColor(Color.white);

					} else {

						this.sprite.setVisible(false);
					}
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 등장.
			case STEP.APPEAR:
			{
				// 스케일로 순간 부풀어오른다.

				float	scale;

				this.spring.execute(Time.deltaTime);
				
				scale = Mathf.InverseLerp(-1.0f, 1.0f, this.spring.position);
				scale = Mathf.Lerp(0.5f, 1.5f, scale);

				this.sprite.setPosition(this.p0);
				this.sprite.setScale(Vector2.one*scale*this.moji_mae_scale);
			}
			break;

			// 쿠키가 문자로 변한다.
			case STEP.FLIP:
			{
				// 빙글 돌아 문자로 변한다 → 상하로 흔들흔들.

				float	cycle;
				float	s, y;

				switch(this.yell.word) {

					default:
					{
						cycle = 2.0f;
						s     = ipCell.get().setInput(this.step.get_time()).clamp(0.0f, cycle/2.0f).pow(0.2f).remap(36.0f, 0.0f).getCurrent();
	
	
						cycle = 0.6f;	
						y     = ipCell.get().setInput(this.step.get_time()).repeat(cycle).remap(0.0f, 2.0f*Mathf.PI).sin().scale(s).getCurrent();
					}
					break;

					case YELL_WORD.OSIMAI:
					{
						// 흔들흔들 반복.
	
						float	cycle0 = 1.8f;
						float	cycle1 = 0.6f;
	
						float	power, amplitude;
	
						if(this.step.get_time() < cycle0) {
	
							power     = 0.2f;
							amplitude = 36.0f;
	
						} else {
	
							power     = 0.4f;
							amplitude = 24.0f;
						}
	
						s = ipCell.get().setInput(this.step.get_time()).repeat(cycle0).getCurrent();
						s = ipCell.get().setInput(s).lerp(0.0f, cycle0/2.0f).pow(power).lerp(amplitude, 0.0f).getCurrent();
		
						y = ipCell.get().setInput(this.step.get_time()).repeat(cycle1).remap(0.0f, 2.0f*Mathf.PI).sin().scale(s).getCurrent();
					}
					break;

					case YELL_WORD.CAKE_COUNT:
					{
						cycle = 2.0f;

						if(this.step.get_time() < 0.6f) {

							s = ipCell.get().setInput(this.step.get_time()).clamp(0.0f, cycle/2.0f).pow(0.2f).remap(36.0f, 0.0f).getCurrent();

						} else {

							s = ipCell.get().setInput(this.step.get_time()).repeat(0.6f*2.0f).remap(0.0f, 2.0f*Mathf.PI).sin().remap(1.0f, 4.0f).getCurrent();
						}

						cycle = 0.6f;	
						y     = ipCell.get().setInput(this.step.get_time()).repeat(cycle).remap(0.0f, 2.0f*Mathf.PI).sin().scale(s).getCurrent();
					}
					break;
				}

				this.sprite.setPosition(new Vector2(this.p0.x, this.p0.y + y));

				if(this.step.is_acrossing_time(flip_time)) {

					this.is_finished = true;
				}
			}
			break;

		}

		// ---------------------------------------------------------------- //

	}

}

}
