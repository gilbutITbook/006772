using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 비헤이비어의 기저 클래스.
public class ItemBehaviorBase : MonoBehaviour {

	public ItemController controll = null;

	public ItemFavor	item_favor = null;						// 특전　아이템을 가지고 있는 캐릭터에 붙는 특수 효과.

	private		List<string>	preset_texts = null;			// 텍스트 프리셋.
	private		bool			is_texts_editable = false;		// preset_texts를 편집할 수 있는가？.

	public bool			is_active = true;						// 활성화/비활성화 설정.

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
	public void	initialize()
	{
		this.item_favor   = new ItemFavor();
		this.preset_texts = new List<string>();

		this.is_texts_editable = true;
		this.initialize_item();
		this.is_texts_editable = false;

		// 대야는 그림자를 조금 움직입니다.
		// (숨어버리므로).
		do {

			if(this.gameObject.name != "Tarai") {

				break;
			}

			var	shadow = this.gameObject.GetComponentInChildren<Projector>();

			if(shadow == null) {

				break;
			}
	
			shadow.transform.localPosition += new Vector3(0.1f, 0.0f, -0.1f);

		} while(false);
	}

	// 프리셋 텍스트를 반환합니다.
	// 프리셋 텍스트 말풍선(NPC의 말풍선)을 표시할 때 호출됩니다.
	public string	getPresetText(int text_id)
	{
		string	text = "";

		if(0 <= text_id && text_id < this.preset_texts.Count) {

			text = this.preset_texts[text_id];
		}

		return(text);
	}

	// ================================================================ //

	// 생성 직후에 호출됩니다. 파생 클래스용..
	public virtual void	initialize_item()
	{
	}

	// 게임 시작 시에 한 번만 호출됩니다.
	public virtual void	start()
	{
	}

	// 매 프레임 호출됩니다.
	public virtual void	execute()
	{
	}

	// 주웠을 때 호출됩니다.
	public virtual void		onPicked()
	{
	}

	// 재생했을 때 호출됩니다.
	public virtual void		onRespawn()
	{
	}

	// 아이템을 성장 상태로 합니다(주울 수 있게 한다).
	public virtual void		finishGrowing()
	{

	}

	// 아이템의 활성화/비활성화 설정.
	public virtual void		activeItem(bool active)
	{
	}

	// ================================================================ //
	// 상속하는 클래스용

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
