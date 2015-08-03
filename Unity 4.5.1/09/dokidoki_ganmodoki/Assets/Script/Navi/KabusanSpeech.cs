using UnityEngine;
using System.Collections;
using MathExtension;
using GameObjectExtension;

// 무기 선택 정보 씬에서의 무 아저씨 자막.
public class KabusanSpeech : MonoBehaviour {

	public Texture[]	font_textures;
	public Texture		balloon_texture;

	// 폰트 종류.
	protected enum FONT {

		NONE = -1,

		KAKKO_L,
		KAKKO_R,
		HATENA,
		TO,
		U,
		FU,
		NE,
		KI,
		TENTEN,
		MAME,
		DAIKON,

		JT,
		UNITY,

		NUM,
	}

	// 폰트 데이터.
	protected struct FontData {

		public FontData(float l_margin, float r_margin)
		{
			this.l_margin = l_margin;
			this.r_margin = r_margin;
		}

		public float	l_margin;		// 좌측 공백.
		public float	r_margin;		// 우측 공백.
	}
	protected FontData[] font_datas;

	// 대사.
	protected FONT[] serif = {

		FONT.KAKKO_L,

		FONT.TO,

		FONT.U,
		FONT.FU,
		FONT.NE,

		FONT.TENTEN,

		FONT.KI,

		FONT.MAME,

		FONT.DAIKON,

		FONT.JT,
		FONT.UNITY,

		FONT.HATENA,
		FONT.KAKKO_R,
	};

	protected KabusanMoji[]		mojis;
	protected Sprite2DControl	balloon_sprite;

	protected float	timer = 0.0f;
	protected int	disp_count = 0;				// 몇 글자까지 표시했는가?.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		Vector2		base_position = new Vector2(0.0f, -50.0f);

		// 받침대.

		this.balloon_sprite = Sprite2DRoot.get().createSprite(this.balloon_texture, true);
		this.balloon_sprite.setVertexAlpha(0.7f);
		this.balloon_sprite.setSize(new Vector2(540.0f, 100.0f));
		this.balloon_sprite.setPosition(base_position);

		// 폰트 데이터 초기화.

		this.font_datas = new FontData[this.font_textures.Length];

		this.font_datas[(int)FONT.KAKKO_L] = new FontData(0.0f, 0.5f);
		this.font_datas[(int)FONT.KAKKO_R] = new FontData(0.5f, 0.0f);
		this.font_datas[(int)FONT.HATENA]  = new FontData(0.0f, 0.0f);
		this.font_datas[(int)FONT.TO]      = new FontData(0.0f, 0.0f);
		this.font_datas[(int)FONT.U]       = new FontData(0.0f, 0.0f);
		this.font_datas[(int)FONT.FU]      = new FontData(0.0f, 0.0f);
		this.font_datas[(int)FONT.NE]      = new FontData(0.0f, 0.0f);
		this.font_datas[(int)FONT.KI]      = new FontData(0.0f, 0.0f);
		this.font_datas[(int)FONT.TENTEN]  = new FontData(0.0f, 0.7f);
		this.font_datas[(int)FONT.MAME]    = new FontData(0.05f, 0.05f);
		this.font_datas[(int)FONT.DAIKON]  = new FontData(0.1f, 0.1f);
		this.font_datas[(int)FONT.JT]      = new FontData(0.1f, 0.1f);
		this.font_datas[(int)FONT.UNITY]   = new FontData(0.1f, 0.05f);

		Vector2	font_size  = Vector2.one*48.0f;

		for(int i = 0;i < this.font_datas.Length;i++) {

			this.font_datas[i].l_margin *= font_size.x;
			this.font_datas[i].r_margin *= font_size.x;
		}

		// 텍스트 전체의 가로폭.

		float		total_serif_width = 0.0f;

		for(int i = 0;i < this.serif.Length;i++) {

			FontData	font_data = this.font_datas[(int)this.serif[i]];
			float		draw_size = font_size.x - (font_data.l_margin + font_data.r_margin);

			total_serif_width += draw_size;
		}
		
		// 스프라이트를 만든다.

		this.mojis = new KabusanMoji[this.serif.Length];

		for(int i = 0;i < this.serif.Length;i++) {

			this.mojis[i] = new KabusanMoji();
			this.mojis[i].font_texture = this.font_textures[(int)this.serif[i]];

			this.mojis[i].font_size    = font_size;
			this.mojis[i].create();
		}

		// 각 스프라이트의 위치.

		Vector2		position = Vector2.zero;

		position.x = -total_serif_width/2.0f;

		for(int i = 0;i < this.serif.Length;i++) {

			FontData	font_data = this.font_datas[(int)this.serif[i]];

			position.x -= font_data.l_margin;

			this.mojis[i].sprite.setPosition(base_position + position + Vector2.right*font_size.x/2.0f);

			position.x += font_size.x - font_data.r_margin;
		}
	}

	void	Start()
	{
	}

	void 	Update()
	{
		this.timer += Time.deltaTime;

		if(this.disp_count < this.mojis.Length) {

			// interval[sec] 마다 한 글자씩 표시한다.

			float	interval   = 0.1f;
			float	begin_time = ((float)this.disp_count + 1.0f)*interval;

			if(begin_time <= this.timer) {
	
				this.mojis[this.disp_count].beginDisp();
				this.disp_count++;
			}
		}

		for(int i = 0;i < this.mojis.Length;i++) {

			this.mojis[i].execute(Time.deltaTime);
		}

	}

	// ================================================================ //

	// 삭제한다.
	public void		destroy()
	{
		this.balloon_sprite.destroy();

		foreach(var moji in this.mojis) {

			moji.destroy();
		}

		this.gameObject.destroy();
	}
}

// 한 글자 스프라이트.
public class KabusanMoji {

	public enum STEP {

		NONE = -1,

		HIDE = 0,		// 비표시.
		APPEAR,			// 표시 시작 중.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ================================================================ //

	public Texture	font_texture;
	public Vector2	font_size = Vector2.one*48.0f;

	public Sprite2DControl	sprite;

	// 변형 제어용 정점.
	protected class ControlVertex {

		public Vector2	position_org;
		public Vector2	position;
		public Vector2	velocity;
	}
	protected ControlVertex[]	vertices;

	protected float		time_scale = 1.0f;

	// ================================================================ //

	public KabusanMoji()
	{
	}

	public void		create()
	{
		// 스프라이트를 만든다.

		this.sprite = Sprite2DRoot.get().createSprite(this.font_texture, 3, true);
		this.sprite.setSize(this.font_size);

		// 변형용 정점을 초기화해 둔다..

		Vector3[]	positions = this.sprite.getVertexPositions();

		this.vertices = new ControlVertex[positions.Length];

		for(int i = 0;i < this.vertices.Length;i++) {

			this.vertices[i] = new ControlVertex();
			this.vertices[i].position     = positions[i];
			this.vertices[i].position_org = this.vertices[i].position;
			this.vertices[i].velocity     = Vector2.zero;
		}

		// 초기 위치를 난수로 변경하고 형태를 왜곡한다.

		for(int i = 0;i < this.vertices.Length;i++) {

			this.vertices[i].position *= Random.Range(0.8f, 1.3f);

			positions[i] = this.vertices[i].position;
		}

		this.sprite.setVertexPositions(positions);
		this.sprite.setVisible(false);

		this.time_scale = Random.Range(0.9f, 1.2f);

		this.step.set_next(STEP.HIDE);
	}

	public void		execute(float delta_time)
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크한다.

		switch(this.step.do_transition()) {

			case STEP.HIDE:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.APPEAR:
				{
					this.sprite.setVisible(true);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.HIDE:
			{
			}
			break;

			case STEP.APPEAR:
			{
				this.exec_step_appear(delta_time);
			}
			break;
		}

		// ---------------------------------------------------------------- //

	}

	public void		exec_step_appear(float delta_time)
	{
		delta_time *= this.time_scale;

		// 정점을 움직인다.

		Vector2[]	positions = new Vector2[this.vertices.Length];

		float	weight = 1.0f/((float)this.vertices.Length - 1.0f);

		for(int i = 0;i < this.vertices.Length;i++) {

			positions[i] = this.vertices[i].position;

			// 자신 이외의 정점과 스프링으로 연결되어있는듯한 느낌.

			for(int j = 0;j < this.vertices.Length;j++) {

				if(i == j) {

					continue;
				}

				// 초기 상태에서 신축한 만큼 역방향으로(원래로 돌아가는 방향).
				// 속도를 더한다.

				float	dist_natural = Vector2.Distance(this.vertices[j].position_org, this.vertices[i].position_org);
				Vector2	dist_vector  = this.vertices[j].position - this.vertices[i].position;
				float	dist_current = dist_vector.magnitude;

				dist_vector.Normalize();
				dist_vector *= (dist_current - dist_natural);

				this.vertices[i].velocity += (dist_vector*1.0f*weight*60.0f*delta_time);			
			}

			positions[i] += this.vertices[i].velocity*delta_time;
			this.vertices[i].velocity *= 0.95f;
		}

		this.sprite.setVertexPositions(positions);

		for(int i = 0;i < this.vertices.Length;i++) {

			this.vertices[i].position = positions[i];
		}
	}


	// ================================================================ //

	// 표시를 시작한다.
	public void		beginDisp()
	{
		this.step.set_next(STEP.APPEAR);
	}


	// 삭제한다.
	public void		destroy()
	{
		this.sprite.destroy();
	}

}

