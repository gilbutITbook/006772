using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 비헤이비어　NPC용.
public class chrBehaviorNPC : chrBehaviorBase {

	private		List<string>	preset_texts = null;
	private		bool			is_texts_editable = false;	// preset_texts를 편집할 수 있는가?.

	protected float			timer = 0.0f;

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 생성 직후에 호출됩니다.
	public override sealed void	initialize()
	{
		this.preset_texts = new List<string>();

		this.is_texts_editable = true;
		this.initialize_npc();
		this.is_texts_editable = false;
		this.controll.setPlayer(false);
	}

	// 생성 직후에 호출되는 NPC용.
	public virtual void	initialize_npc()
	{
	}

	// 게임 시작 시에 한 번만 호출됩니다.
	public override void	start()
	{
		this.controll.balloon.setColor(Color.red);
	}

	// 매 프레임 호출됩니다.
	public override	void	execute()
	{
		this.timer += Time.deltaTime;

		int		text_id = (int)Mathf.Repeat(this.timer, (float)this.preset_texts.Count);

		this.controll.cmdDispBalloon(text_id);
	}

	// 정형문을 반환합니다.
	// 정형문 말풍선(NPC 말풍선)을 표시할 때 호출됩니다.
	public override sealed string	getPresetText(int text_id)
	{
		string	text = "";

		if(0 <= text_id && text_id < this.preset_texts.Count) {

			text = this.preset_texts[text_id];
		}

		return(text);
	}

	// ================================================================ //
	// 상속받는 클래스용

	protected void	addPresetText(string text)
	{
		if(this.is_texts_editable) {

			this.preset_texts.Add(text);

		} else {

			// initialize() 메소드 이외에서는 텍스트를 추가할 수 없습니다.
			Debug.LogError("addPresetText() can use only in initialize_npc().");
		}
	}
}