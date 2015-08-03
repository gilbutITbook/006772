using UnityEngine;
using System.Collections;

public class chrBalloon : MonoBehaviour {

	public Texture texture_main    = null;
	public Texture texture_belo    = null;
	public Texture texture_kado_lu = null;
	public Texture texture_kado_ru = null;
	public Texture texture_kado_ld = null;
	public Texture texture_kado_rd = null;

	public	Vector2		position;			// 위치.
	public	string		text  = "";			// 텍스트.
	public	Color		color = Color.red;	// 말풍선 색.

	protected float		timer = 0.0f;
	protected float		lifetime;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		GameRoot	game_root = GameRoot.getInstance();

		this.texture_main    = game_root.texture_main;
		this.texture_belo    = game_root.texture_belo;
		this.texture_kado_lu = game_root.texture_kado_lu;
		this.texture_kado_ru = game_root.texture_kado_ru;
		this.texture_kado_ld = game_root.texture_kado_ld;
		this.texture_kado_rd = game_root.texture_kado_rd;
	}
	
	void	Update()
	{
		if(this.text != "") {

			this.timer += Time.deltaTime;

			if(this.lifetime > 0.0f && this.timer >= this.lifetime) {

				this.text = "";
			}

		} else {

			this.timer = 0.0f;
		}
	}

	public void		setText(string text, float lifetime = -1.0f)
	{
		this.text     = text;
		this.lifetime = lifetime;
	}
	
	// 말풍선을 설정합니다.
	public void		setColor(Color color)
	{
		this.color = color;
	}

	// ================================================================ //

	protected static float	KADO_SIZE = 16.0f;

	void	OnGUI()
	{

		if(this.text != "") {

			Vector2		pos;
	
			pos = this.position;

			// 흔들흔들.

			float	cycle = 4.0f;
			float	t = Mathf.Repeat(this.timer, cycle)/cycle;

			pos.x += 4.0f*Mathf.Sin(t*Mathf.PI*4.0f);
			pos.y += (4.0f*Mathf.Sin(t*Mathf.PI)*4.0f*Mathf.Sin(t*Mathf.PI)) - 100.0f;

			//

			float		font_size   = 13.0f;
			float		font_height = 20.0f;
			Vector2		balloon_size, text_size;

			text_size.x = this.text.Length*font_size;
			text_size.y = font_height;

			balloon_size.x = text_size.x + KADO_SIZE*2.0f;
			balloon_size.y = text_size.y + KADO_SIZE;

			this.disp_balloon(pos, balloon_size, this.color);
	
			Vector2		p;
	
			p.x = pos.x - text_size.x/2.0f;
			p.y = pos.y - text_size.y/2.0f;

			GUI.Label(new Rect(p.x, p.y, text_size.x, text_size.y), this.text);
		}
	}

	// 말풍선을 표시합니다.
	protected void	disp_balloon(Vector2 position, Vector2 size, Color color)
	{
		GUI.color = color;

		float		kado_size = KADO_SIZE;


		Vector2		p, s;

		s.x = size.x - kado_size*2.0f;
		s.y = size.y;

		// 한 가운데.
		p = position - s/2.0f;
		GUI.DrawTexture(new Rect(p.x, p.y, s.x, s.y), this.texture_main);

		// 왼쪽.
		p.x = position.x - s.x/2.0f - kado_size;
		p.y = position.y - s.y/2.0f + kado_size;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, size.y - kado_size*2.0f), this.texture_main);

		// 오른쪽.
		p.x = position.x + s.x/2.0f;
		p.y = position.y - s.y/2.0f + kado_size;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, size.y - kado_size*2.0f), this.texture_main);

		// 왼쪽 위.
		p.x = position.x - s.x/2.0f - kado_size;
		p.y = position.y - s.y/2.0f;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, kado_size), this.texture_kado_lu);

		// 오른쪽 위.
		p.x = position.x + s.x/2.0f;
		p.y = position.y - s.y/2.0f;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, kado_size), this.texture_kado_ru);

		// 왼쪽 아래.
		p.x = position.x - s.x/2.0f - kado_size;
		p.y = position.y + s.y/2.0f - kado_size;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, kado_size), this.texture_kado_ld);

		// 오른쪽 아래.
		p.x = position.x + s.x/2.0f;
		p.y = position.y + s.y/2.0f - kado_size;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, kado_size), this.texture_kado_rd);

		// 혀.
		p.x = position.x - kado_size/2.0f;
		p.y = position.y + s.y/2.0f;
		GUI.DrawTexture(new Rect(p.x, p.y, kado_size, kado_size), this.texture_belo);

		GUI.color = Color.white;
	}


}
