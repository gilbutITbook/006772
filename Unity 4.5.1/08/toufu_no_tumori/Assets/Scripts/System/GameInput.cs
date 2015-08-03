using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameInput : MonoBehaviour {

	// 클릭한 것.
	public enum POINTEE {

		NONE = -1,

		TERRAIN = 0,		// 지형.
		ITEM,				// 아이템.
		CHARACTER,			// 캐릭터.
		TEXT_FIELD,			// 텍스트 입력 영역.

		NUM,
	}

	public struct Pointing {

		public bool	trigger_on;
		public bool	current;
		public bool	clear_after;		// clear()후　버튼이 떨어지기까지 대기 중.

		public POINTEE	pointee;
		public string	pointee_name;

		public Vector3	position_3d;
	}

	public struct SerifText {

		public bool	trigger_on;
		public bool	current;

		public string	text;
	}

	public Pointing		pointing;
	public SerifText	serif_text;

	public Texture		white_texture;

	// ================================================================ //

	protected Rect	text_field_pos = new Rect(Screen.width - 120, 50, 100, 20);
	protected bool	mouse_lock = false;

	protected	List<Rect>	forbidden_area = new List<Rect>();		// 입력 금지 영역(클릭 등을 잡아내지 않는다）.
	public bool		disp_forbidden_area = false;

	private string		editing_text = "";

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.pointing.trigger_on  = false;
		this.pointing.current     = false;
		this.pointing.clear_after = false;
		this.pointing.pointee     = POINTEE.NONE;

		this.serif_text.trigger_on = false;
		this.serif_text.current    = false;
		this.serif_text.text       = "";
	}
	
	void	Update()
	{
		bool	is_on_invalid_area = false;

		// ---------------------------------------------------------------- //

		if(this.pointing.clear_after) {

			// 클리어 후, 바로 입력되어 버리지 않게 버튼이
			// 떨어질 때까지 기다린다.
			if(!Input.GetMouseButton(0)) {

				this.pointing.clear_after = false;
			}

		} else if(!this.pointing.current) {

			if(Input.GetMouseButton(0)) {

				this.pointing.trigger_on = true;
				this.pointing.current    = true;
			}

		} else {

			this.pointing.trigger_on = false;

			if(!Input.GetMouseButton(0)) {

				this.pointing.current    = false;
			}
		}

		do {

			if(!this.pointing.current) {

				break;
			}

			Vector2		mouse_position_2d = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

			// 디버그창이 클릭되었을 때.
			// (이동하지 않도록).
			//
			if(dbwin.root().isOcuppyRect(mouse_position_2d)) {
	
				is_on_invalid_area = true;
			}

			if(this.forbidden_area.Exists(x => x.Contains(mouse_position_2d))) {

				is_on_invalid_area = true;
			}

			if(this.pointing.trigger_on) {

				// 텍스트 입력 영역이 클릭됐을 때.
				// (이동하지 않도록).
				if(this.text_field_pos.Contains(mouse_position_2d)) {
	
					this.pointing.pointee = POINTEE.NONE;
					break;
	
				}

				// 디버그창이 클릭되었을 때.
				// (이동하지 않도록).
				if(is_on_invalid_area) {
	
					this.pointing.pointee = POINTEE.NONE;
					break;
				}
			}

			// 마우스 커서 위치에 있는 Terrain의 좌표를 구합니다. 
			Vector3		mouse_position = Input.mousePosition;
	
			// 화면의 좌우와 아래로 밀려나왔을 때의 대책.
			mouse_position.x = Mathf.Clamp(mouse_position.x, 0.0f, Screen.width);
			mouse_position.y = Mathf.Clamp(mouse_position.y, 0.0f, Screen.height);
	
			Ray		ray = Camera.main.ScreenPointToRay(mouse_position);
	
			// 레이어 마스크.
	
			int		layer_mask = 0;
	
			layer_mask += 1 << LayerMask.NameToLayer("Terrain");
			layer_mask += 1 << LayerMask.NameToLayer("Clickable");
			layer_mask += 1 << LayerMask.NameToLayer("Player");
	
			RaycastHit 	hit;
	
			if(!Physics.Raycast(ray, out hit, float.PositiveInfinity, layer_mask)) {

				break;
			}

			this.pointing.position_3d = hit.point;

			string	layer_name = LayerMask.LayerToName(hit.transform.gameObject.layer).ToString();

			// '포인팅된 것'은 클릭한 순간만 갱신합니다.
			//  드래그로 아이템을 주울 수 없게 
			if(this.pointing.trigger_on) {

				switch(layer_name) {
	
					case "Player":
					{
						this.pointing.pointee      = POINTEE.CHARACTER;
						this.pointing.pointee_name = hit.transform.gameObject.name;
					}
					break;
	
					case "Terrain":
					{
						this.pointing.pointee = POINTEE.TERRAIN;
					}
					break;

					case "Clickable":
					case "Default":
					{
						switch(hit.transform.gameObject.tag) {

							case "Item":
							{
								this.pointing.pointee      = POINTEE.ITEM;
								this.pointing.pointee_name = hit.transform.gameObject.name;
							}
							break;

							case "Charactor":
							{
								this.pointing.pointee      = POINTEE.CHARACTER;
								this.pointing.pointee_name = hit.transform.gameObject.name;
							}
							break;
						}
					}
					break;
	

					default:
					{
						this.pointing.pointee = POINTEE.NONE;
					}
					break;

				} // switch(layer_name) {

			} else if(this.pointing.current) {

				switch(layer_name) {
	
					case "Player":
					{
						// 지면과의 교점에 둡니다.
						this.pointing.position_3d = ray.origin + ray.direction*Mathf.Abs(ray.origin.y/ray.direction.y);
					}
					break;
				}
			}

		} while(false);
	}

	void	OnGUI()
	{

		if(Event.current.type == EventType.Layout) {

			this.serif_text.trigger_on = false;
		}

		this.editing_text = GUI.TextArea(this.text_field_pos, this.editing_text);

		// 리턴 키가 눌렸으면 확정.
		if(this.editing_text.EndsWith("\n")) {

			this.editing_text = this.editing_text.Remove(this.editing_text.Length - 1);

			this.serif_text.trigger_on = true;
			this.serif_text.text       = this.editing_text;
		}

		// 입력 금지 영역의 디버그 표시.

		if(this.disp_forbidden_area) {

			foreach(var area in this.forbidden_area) {
	
				GUI.color = new Color(1.0f, 0.5f, 0.5f, 0.4f);
				GUI.DrawTexture(area, this.white_texture);
			}
		}
	}

	// ================================================================ //

	// 입력을 클리어합니다.
	public void		clear()
	{
		this.pointing.current      = false;
		this.pointing.clear_after  = true;
		this.pointing.pointee      = POINTEE.NONE;
		this.pointing.pointee_name = "";
	}

	// 입력 금지 영역을 추가합니다.
	public void		appendForbiddenArea(Rect area)
	{
		this.forbidden_area.Add(area);
	}

	// 입력 금지 영역을 삭제합니다.
	public void		removeForbiddenArea(Rect area)
	{
		this.forbidden_area.Remove(area);
	}

}
