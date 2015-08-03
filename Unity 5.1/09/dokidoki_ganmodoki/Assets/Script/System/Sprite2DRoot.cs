using UnityEngine;
using System.Collections;
using MathExtension;

// 2D 스프라이트 매니저.
public class Sprite2DRoot : MonoBehaviour {

	// ---------------------------------------------------------------- //

	public Camera			gui_camera;

	public Shader			shader_opaque;
	public Shader			shader_transparent;

	protected GameObject	root_sprite;

	public float			sprite_depth = 10.0f;

	public Vector2			viewport_size;

	public Vector2[]		center_offset;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		this.root_sprite = new GameObject("root sprite");
		this.root_sprite.transform.parent = this.gui_camera.transform;
		this.root_sprite.transform.localPosition = Vector3.forward*this.gui_camera.nearClipPlane*2.0f;
		this.root_sprite.transform.localRotation = Quaternion.identity;

		this.setViewportPixels(new Vector2(Screen.width, Screen.height));
	}

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	void	OnGUI()
	{
	}

	// ================================================================ //

	public float	getAspectConvert()
	{
		float	s = Mathf.Max(1.0f, (this.viewport_size.x/this.viewport_size.y)/((float)Screen.width/(float)Screen.height));

		return(s);
	}

	public void		setViewportPixels(Vector2 size)
	{
		this.viewport_size = size;

		//float	s = Mathf.Max(1.0f, (this.viewport_size.x/this.viewport_size.y)/((float)Screen.width/(float)Screen.height));
		float	s = this.getAspectConvert();

		this.root_sprite.transform.localScale = new Vector3(1.0f/(this.viewport_size.y/2.0f*s), 1.0f/(this.viewport_size.y/2.0f*s), 1.0f);
	}

	// 널 스프라이트를 만든다(스프라이트를 자식으로 만들어 한꺼번에 다루기 위해).
	public Sprite2DControl	createNull()
	{
		GameObject		go = new GameObject();

		go.name = "sprite-2d/null";

		go.layer = LayerMask.NameToLayer("GUI");

		go.transform.parent   = this.root_sprite.transform;
		go.transform.localPosition = new Vector3(0.0f, 0.0f, this.sprite_depth);
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;

		Sprite2DControl	sprite = go.AddComponent<Sprite2DControl>();

		return(sprite);
	}

	// ㅡ프라이트를 만든다.
	public Sprite2DControl	createSprite(Texture texture, int div_count, bool is_transparent)
	{
		GameObject		go = new GameObject();

		go.name = "sprite-2d/";

		if(texture != null) {

			go.name += texture.name;
		}

		go.layer = LayerMask.NameToLayer("GUI");

		go.transform.parent   = this.root_sprite.transform;
		go.transform.localPosition = new Vector3(0.0f, 0.0f, this.sprite_depth);
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;

		MeshRenderer	mesh_render = go.AddComponent<MeshRenderer>();
		MeshFilter		mesh_filter = go.AddComponent<MeshFilter>();

		float	w = 32.0f;
		float	h = 32.0f;

		if(texture != null) {

			w = texture.width;
			h = texture.height;
		}

		// ---------------------------------------------------------------- //
		// 위치.

		Vector3[]	positions = this.calcVertexPositions(w, h, div_count);

		// ---------------------------------------------------------------- //
		// UV.

		Vector2[]	uvs = new Vector2[div_count*div_count];

		float	du = 1.0f/((float)div_count - 1.0f);
		float	dv = 1.0f/((float)div_count - 1.0f);
	
		for(int y = 0;y < div_count;y++) {
			
			for(int x = 0;x < div_count;x++) {
				
				uvs[y*div_count + x] = new Vector2(0.0f + (float)x*du,  0.0f + (float)y*dv);
			}
		}

		// ---------------------------------------------------------------- //
		// 정점 색.
		
		Color[]	colors = new Color[div_count*div_count];

		for(int y = 0;y < div_count;y++) {
			
			for(int x = 0;x < div_count;x++) {
				
				colors[y*div_count + x] = Color.white;
			}
		}

		// ---------------------------------------------------------------- //
		// 인덱스.

		int[]		indices = new int[(div_count - 1)*(div_count - 1)*3*2];
		int			n = 0;

		for(int y = 0;y < div_count - 1;y++) {
			
			for(int x = 0;x < div_count - 1;x++) {
				
				indices[n++] = (y + 1)*div_count + (x + 0);
				indices[n++] = (y + 1)*div_count + (x + 1);
				indices[n++] = (y + 0)*div_count + (x + 1);

				indices[n++] = (y + 0)*div_count + (x + 1);
				indices[n++] = (y + 0)*div_count + (x + 0);
				indices[n++] = (y + 1)*div_count + (x + 0);
			}
		}

		mesh_filter.mesh.vertices  = positions;
		mesh_filter.mesh.uv        = uvs;
		mesh_filter.mesh.colors    = colors;
		mesh_filter.mesh.triangles = indices;
		
		// ---------------------------------------------------------------- //
		// 머티리얼.

		if(is_transparent) {

			mesh_render.material = new Material(this.shader_transparent);

		} else {

			mesh_render.material = new Material(this.shader_opaque);
		}
		mesh_render.material.mainTexture = texture;

		//

		Sprite2DControl	sprite = go.AddComponent<Sprite2DControl>();

		sprite.internalSetSize(new Vector2(w, h));
		sprite.internalSetDivCount(div_count);

		this.sprite_depth -= 0.01f;

		return(sprite);
	}
	public Sprite2DControl	createSprite(Texture texture, bool is_transparent)
	{
		return(this.createSprite(texture, 2, is_transparent));
	}

	// 정점의 위치를 계산한다.
	public Vector3[]	calcVertexPositions(float w, float h, int div_count)
	{
		Vector3[]	positions = new Vector3[div_count*div_count];
	
		float	dx = w/((float)div_count - 1.0f);
		float	dy = h/((float)div_count - 1.0f);
		
		for(int y = 0;y < div_count;y++) {
			
			for(int x = 0;x < div_count;x++) {
				
				positions[y*div_count + x] = new Vector3(-w/2.0f + (float)x*dx,  -h/2.0f + (float)y*dy, 0.0f);
			}
		}

		return(positions);
	}

	// 스프라이트 크기를 설정한다.
	public void		setSizeToSprite(Sprite2DControl sprite, Vector2 size)
	{
		sprite.internalSetSize(size);
		
		Vector3[]	positions = this.calcVertexPositions(sprite.getSize().x, sprite.getSize().y, sprite.getDivCount());

		this.setVertexPositionsToSprite(sprite, positions);
	}

	// 스프라이트에 정점 색을 설정한다.
	public void		setVertexColorToSprite(Sprite2DControl sprite, Color color)
	{
		MeshRenderer	mesh_render = sprite.GetComponent<MeshRenderer>();
		MeshFilter		mesh_filter = sprite.GetComponent<MeshFilter>();

		int		div_count = sprite.getDivCount();

		Color[]		colors = new Color[div_count*div_count];

		foreach(var i in System.Linq.Enumerable.Range(0, div_count*div_count)) {

			colors[i] = color;
		}

		mesh_filter.mesh.colors = colors;
		mesh_render.enabled = false;
		mesh_render.enabled = true;
	}

	// 정점 위치를 설정한다.
	public void		setVertexPositionsToSprite(Sprite2DControl sprite, Vector3[] positions)
	{
		MeshRenderer	mesh_render = sprite.GetComponent<MeshRenderer>();
		MeshFilter		mesh_filter = sprite.GetComponent<MeshFilter>();
	
		mesh_filter.mesh.vertices = positions;
		mesh_render.enabled = false;
		mesh_render.enabled = true;
	}

	// 정점 위치를 획득한다.
	public Vector3[]	getVertexPositionsFromSprite(Sprite2DControl sprite)
	{
		MeshFilter		mesh_filter = sprite.GetComponent<MeshFilter>();

		return(mesh_filter.mesh.vertices);
	}

	// ================================================================ //

	// 스크린 좌표를 스프라이트 좌표계로 변환한다.
	public Vector2	convertScreenPosition(Vector2 screen)
	{
		Vector2	position = new Vector2();

		position.x =   screen.x - Screen.width/2.0f;
		position.y = -(screen.y - Screen.height/2.0f);

		return(position);
	}

	// 마우스 좌표를 스프라이트 좌표계로 변환한다.
	public Vector2	convertMousePosition(Vector2 mouse)
	{
		Vector2	position = new Vector2();
	
		//float	s = Mathf.Max(1.0f, (this.viewport_size.x/this.viewport_size.y)/((float)Screen.width/(float)Screen.height));
		float	s = this.getAspectConvert();

		position.x = (mouse.x - Screen.width/2.0f)*(this.viewport_size.y/Screen.height)*s;
		position.y = (mouse.y - Screen.height/2.0f)*(this.viewport_size.y/Screen.height)*s;
		
		return(position);
	}

	// ================================================================ //
	// 인스턴스.

	private	static Sprite2DRoot	instance = null;

	public static Sprite2DRoot	get()
	{
		if(Sprite2DRoot.instance == null) {

			Sprite2DRoot.instance = GameObject.Find("Sprite2DRoot").GetComponent<Sprite2DRoot>();
		}

		return(Sprite2DRoot.instance);
	}
}
