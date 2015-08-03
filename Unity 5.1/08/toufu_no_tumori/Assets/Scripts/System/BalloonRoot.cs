using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 말풍선.
public class ChattyBalloon {

	public ChattyBalloon(BalloonRoot root)
	{
		this.root     = root;
		this.priority = 0;
	}

	protected BalloonRoot	root = null;

	protected static float	KADO_SIZE = 16.0f;

	protected bool		is_visible = true;

	protected string	text = "";
	protected int		priority;			// 그리기 우선 순위(작은 쪽이 앞).
	protected Vector2	position;
	protected Color		color = Color.red;	// ふきだしのいろ.

	public Vector2		balloon_size, text_size;

	public Vector2		draw_pos;
	public Rect			draw_rect;

	protected float		timer = 0.0f;

	// ================================================================ //

	// 매 프레임 실행 처리.
	public void		execute()
	{
		if(this.is_visible && this.text != "") {

			this.draw_pos = this.position;

			// 흔들흔들.

			float	cycle = 4.0f;
			float	t = Mathf.Repeat(this.timer, cycle)/cycle;

			this.draw_pos.x += 4.0f*Mathf.Sin(t*Mathf.PI*4.0f);
			this.draw_pos.y += (4.0f*Mathf.Sin(t*Mathf.PI)*4.0f*Mathf.Sin(t*Mathf.PI));

			//

			this.draw_rect.x = this.draw_pos.x - this.text_size.x/2.0f;
			this.draw_rect.y = this.draw_pos.y - this.text_size.y/2.0f;
			this.draw_rect.width  = this.text_size.x;
			this.draw_rect.height = this.text_size.y;


			//

			this.timer += Time.deltaTime;

		} else {

			this.timer = 0.0f;
		}
	}

	// 그리기.
	public void		draw()
	{
		if(this.is_visible && this.text != "") {

			this.disp_balloon(this.draw_pos, this.balloon_size, this.color);

			GUI.color = Color.white;	
			GUI.Label(this.draw_rect, this.text);
		}
	}

	// 말풍선(문자이외)를 표시합니다.
	protected void		disp_balloon(Vector2 position, Vector2 size, Color color)
	{
		GUI.color = color;

		float		kado_size = KADO_SIZE;
		Vector2		p, s;

		s.x = size.x - kado_size*2.0f;
		s.y = size.y;

		// 한 가운데.
		p = position - s/2.0f;
		GUI.DrawTexture(new Rect(p.x, p.y, s.x, s.y), this.root.texture_main);

		// 좌.
		p.x = position.x - s.x/2.0f - kado_size;
		p.y = position.y - s.y/2.0f + kado_size;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, size.y - kado_size*2.0f), this.root.texture_main);

		// 우.
		p.x = position.x + s.x/2.0f;
		p.y = position.y - s.y/2.0f + kado_size;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, size.y - kado_size*2.0f), this.root.texture_main);

		// 좌상.
		p.x = position.x - s.x/2.0f - kado_size;
		p.y = position.y - s.y/2.0f;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, kado_size), this.root.texture_kado_lu);

		// 우상.
		p.x = position.x + s.x/2.0f;
		p.y = position.y - s.y/2.0f;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, kado_size), this.root.texture_kado_ru);

		// 좌하.
		p.x = position.x - s.x/2.0f - kado_size;
		p.y = position.y + s.y/2.0f - kado_size;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, kado_size), this.root.texture_kado_ld);

		// 우하.
		p.x = position.x + s.x/2.0f;
		p.y = position.y + s.y/2.0f - kado_size;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, kado_size), this.root.texture_kado_rd);

		// 혀.
		p.x = position.x - kado_size/2.0f;
		p.y = position.y + s.y/2.0f;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, kado_size), this.root.texture_belo);
	}

	// ================================================================ //

	// 표시/비표시
	public void		setVisible(bool is_visible)
	{
		this.is_visible = is_visible;
	}

	// 텍스트 설정.
	public void		setText(string text)
	{
		this.text = text;

		if(this.text != "") {

			float		font_size   = 13.0f;
			float		font_height = 20.0f;

			this.text_size.x = this.text.Length*font_size;
			this.text_size.y = font_height;

			this.balloon_size.x = text_size.x + KADO_SIZE*2.0f;
			this.balloon_size.y = text_size.y + KADO_SIZE;
		}
	}

	// 텍스트를 클리어합니다(비표시로 합니다).
	public void		clearText()
	{
		this.text = "";
	}

	// 텍스트를 가져옵니다.
	public string	getText()
	{
		return(this.text);
	}

	// 위치를 설정합니다.
	public void		setPosition(Vector2 position)
	{
		this.position = position;
	}

	// 말풍선의 색을 설정합니다.
	public void		setColor(Color color)
	{
		this.color = color;
	}

	// 그리기 우선 순위를 설정합니다.
	public void		setPriority(int priority)
	{
		this.priority = priority;
		this.root.setSortRequired();
	}

	// 그리기 우선 선위를 가져옵니다.
	public int		getPriority()
	{
		return(this.priority);
	}

	// ================================================================ //


};

// 말풍선 관리 클래스.
public class BalloonRoot : MonoBehaviour {

	public Texture texture_main    = null;
	public Texture texture_belo    = null;
	public Texture texture_kado_lu = null;
	public Texture texture_kado_ru = null;
	public Texture texture_kado_ld = null;
	public Texture texture_kado_rd = null;

	public List<ChattyBalloon>	balloons = new List<ChattyBalloon>();

	protected bool		is_sort_required = false;

	public void		setSortRequired()
	{
		this.is_sort_required = true;
	}

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.is_sort_required = true;
	}
	
	void	Update()
	{
		// 그리는 순서로 정렬.
		if(this.is_sort_required) {

			// 우선 순위 수치가 작은 게 나중에 그려지게
			// 우선 순위 수치가 큰 순서로 정렬합니다.
			this.balloons.Sort((x, y) => -(x.getPriority() - y.getPriority()));

			this.is_sort_required = false;
		}

		foreach(var balloon in this.balloons) {

			balloon.execute();
		}
	}

	void	OnGUI()
	{
		Color	color_org = GUI.color;

		foreach(var balloon in this.balloons) {

			balloon.draw();
		}

		GUI.color = color_org;
	}

	// 말풍선을 만듭니다.
	public ChattyBalloon	createBalloon()
	{
		ChattyBalloon	balloon = new ChattyBalloon(this);

		balloon.setPriority(0);

		this.balloons.Add(balloon);

		return(balloon);
	}

	// ================================================================ //
	// 인스턴스.

	private	static BalloonRoot	instance = null;

	public static BalloonRoot	get()
	{
		if(BalloonRoot.instance == null) {

			BalloonRoot.instance = GameObject.Find("BalloonRoot").GetComponent<BalloonRoot>();
		}

		return(BalloonRoot.instance);
	}
}
