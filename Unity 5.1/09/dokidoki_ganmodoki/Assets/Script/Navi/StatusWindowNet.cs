using UnityEngine;
using System.Collections;

// 스테이터스 창   리모트용.
public class StatusWindowNet : MonoBehaviour {

	public Texture		face_icon_texture;
	public Texture		lace_texture;
	public Texture[]	cookie_icon_textures;

	protected Sprite2DControl	face_sprite;
	protected Sprite2DControl	lace_sprite;
	protected Sprite2DControl	cookie_sprite;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
	}

	void	Start()
	{
	}
	
	void 	Update()
	{
	
	}

	// ================================================================ //

	protected float	SCALE = 0.8f;

	public void	create()
	{
		Vector2		root_position = new Vector2(640.0f/2.0f - 100.0f, 0.0f);

		// 아래에 깔리는 레이스.
		this.lace_sprite = Sprite2DRoot.get().createSprite(this.lace_texture, true);
		this.lace_sprite.setSize(new Vector2(96.0f, 96.0f)*SCALE);

		// 쿠키.
		this.cookie_sprite = Sprite2DRoot.get().createSprite(this.cookie_icon_textures[0], true);
		this.cookie_sprite.setSize(new Vector2(60.0f, 60.0f)*SCALE);

		// 얼굴 아이콘.
		this.face_sprite = Sprite2DRoot.get().createSprite(this.face_icon_texture, true);
		this.face_sprite.setSize(new Vector2(60.0f, 60.0f)*SCALE);

		this.setPosition(root_position);
	}

	// 표시 위치를 설정한다.
	public void	setPosition(Vector2 root_position)
	{
		this.lace_sprite.setPosition(root_position   + new Vector2(0.0f, 0.0f));
		this.face_sprite.setPosition(root_position   + new Vector2(20.0f, 20.0f)*SCALE);
		this.cookie_sprite.setPosition(root_position + new Vector2(0.0f, 0.0f));
	}

	// 히트 포인트를 설정한다.
	public void	setHP(float hp)
	{
		// 쿠키.

		int		cookie_sprite_sel = 0;
		bool	is_visible = true;

		if(hp > 80.0f) {

			cookie_sprite_sel = 0;

		} else if(hp > 50.0f) {

			cookie_sprite_sel = 1;

		} else if(hp > 20.0f) {

			cookie_sprite_sel = 2;

		} else if(hp > 0.0f) {

			cookie_sprite_sel = 3;

		} else {

			is_visible = false;
		}

		this.cookie_sprite.setTexture(this.cookie_icon_textures[cookie_sprite_sel]);
		this.cookie_sprite.setVisible(is_visible);
	}
}
