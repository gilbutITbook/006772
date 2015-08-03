using UnityEngine;
using System.Collections;

// 예/아니오 선택 대화 상자.
public class YesNoAskDialog : MonoBehaviour {

	public enum SELECTION {

		NONE = -1,

		YES = 0,
		NO,

		NUM,
	};
	protected SELECTION	selection = SELECTION.NONE;

	protected enum STEP {

		NONE = -1,

		IDLE = 0,			// 실행 중이 아닙니다.
		DISPATCH,
		SELECTED,			//어느 쪽인지 버튼이 눌렸습니다.
		CLOSE,

		NUM,
	};
	protected Step<STEP>	step = new Step<STEP>(STEP.NONE);

	protected Rect	input_forbidden_area = new Rect((Screen.width - 300)/2.0f, 100, 300, 150);

	public Texture	white_texture;

	protected string	text     = "어느 쪽?";		// 메시지의 텍스트.
	protected string	yes_text = "이쪽 !";		// Yes 버튼의 텍스트.
	protected string	no_text  = "저쪽 !";		// No 버튼의 텍스트.
	protected Rect		text_rect = new Rect(0.0f, 0.0f, 100.0f, 10.0f);

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.step.set_next(STEP.IDLE);
	}
	
	void	Update()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 이행할지 체크합니다.

		switch(this.step.do_transition()) {

			case STEP.DISPATCH:
			{
				if(this.selection != SELECTION.NONE) {

					this.step.set_next(STEP.SELECTED);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태가 전환했을 때의 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.DISPATCH:
				{
					this.selection = SELECTION.NONE;

					CharacterRoot.get().getGameInput().appendForbiddenArea(this.input_forbidden_area);
				}
				break;

				case STEP.CLOSE:
				{
					CharacterRoot.get().getGameInput().removeForbiddenArea(this.input_forbidden_area);
					this.step.set_next(STEP.IDLE);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.IDLE:
			{
			}
			break;
		}
	}

	void	OnGUI()
	{
		switch(this.step.get_current()) {

			case STEP.DISPATCH:
			{
				Color	org_color = GUI.color;

				// 텍스처.
				GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

				GUI.DrawTexture(this.input_forbidden_area, this.white_texture);

				// 텍스트.
				GUI.color = new Color(0.5f, 0.1f, 0.1f, 0.5f);

				GUI.Label(this.text_rect, text);

				GUI.color = org_color;

				if(GUI.Button(new Rect(190, 200, 100, 20), this.yes_text)) {

					this.selection = SELECTION.YES;
				}
				if(GUI.Button(new Rect(350, 200, 100, 20), this.no_text)) {

					this.selection = SELECTION.NO;
				}
			}
			break;
		}
	}

	// ================================================================ //

	// 메시지 텍스트를 설정합니다.
	public void		setText(string text)
	{
		this.text = text;

		float	font_width  = 13.0f;
		float	font_height = 20.0f;

		this.text_rect.width  = font_width*this.text.Length;
		this.text_rect.height = font_height;
		this.text_rect.x = Screen.width/2.0f - this.text_rect.width/2.0f;
		this.text_rect.y = 150.0f;
	}

	// 버튼 텍스트를 설정합니다.
	public void		setButtonText(string yes_text, string no_text)
	{
		this.yes_text = yes_text;
		this.no_text  = no_text;
	}

	public void		dispatch()
	{
		this.step.set_next(STEP.DISPATCH);
	}
	public void		close()
	{
		this.step.set_next(STEP.CLOSE);
	}

	public bool		isSelected()
	{
		bool	is_selected = false;

		if(this.getSelection() != SELECTION.NONE) {

			is_selected = true;
		}

		return(is_selected);
	}

	public SELECTION	getSelection()
	{
		SELECTION	selection = SELECTION.NONE;

		if(this.step.get_current() == STEP.SELECTED) {

			selection = this.selection;
		}

		return(selection);
	}

	// ================================================================ //

	private	static YesNoAskDialog	instance = null;

	public static YesNoAskDialog	get()
	{
		if(YesNoAskDialog.instance == null) {

			YesNoAskDialog.instance = GameObject.Find("GameRoot").GetComponent<YesNoAskDialog>();
		}

		return(YesNoAskDialog.instance);
	}
}
