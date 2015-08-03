using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 비헤이비어 기저 클래스.
public class ItemBehaviorBase : MonoBehaviour {

	public enum STEP {

		NONE = -1,

		IDLE = 0,
		SPAWN,			// 쓰러진 적에게서 나타남.
		FALL,			// 위에서 떨어져 내림(무기 선택 시 열쇠).
		BUFFET,			// 무제한 제공 케이크.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	public ItemController	controll = null;

	public Item.Favor		item_favor = null;					// 특전　아이템을 가진 캐릭터에 붙는 특수효과.

	private		List<string>	preset_texts = null;			// 프리셋 텍스트.
	private		bool			is_texts_editable = false;		// preset_texts를 편집할 수 있는가?

	protected ipModule.Jump		ip_jump = new ipModule.Jump();

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //


	// 생성 직후에 호출.
	public void	initialize()
	{
		this.item_favor   = new Item.Favor();
		this.preset_texts = new List<string>();

		this.is_texts_editable = true;
		this.initialize_item();
		this.is_texts_editable = false;

		switch(this.controll.type) {

			case "candy00":
			{
				this.item_favor.category = Item.CATEGORY.CANDY;
			}
			break;

			case "key00":
			case "key01":
			case "key02":
			case "key03":
			{
				this.item_favor.category = Item.CATEGORY.KEY;
				break;
			}

			case "key04":
			{
				this.item_favor.category = Item.CATEGORY.FLOOR_KEY;
				break;
			}

			case "ice00":
			{
				this.item_favor.category = Item.CATEGORY.SODA_ICE;
				this.item_favor.option0 = (object)false;
			}
			break;

			case "cake00":
			{
				this.item_favor.category = Item.CATEGORY.FOOD;
			}
			break;

			case "shot_negi":
			case "shot_yuzu":
			{
				this.item_favor.category = Item.CATEGORY.WEAPON;
			}
			break;


			case "dagger00":
			{
				this.item_favor.category = Item.CATEGORY.WEAPON;
			}
			break;
			case "arrow00":
			{
				this.item_favor.category = Item.CATEGORY.WEAPON;
			}
			break;
			case "bomb00":
			{
				this.item_favor.category = Item.CATEGORY.ETC;
			}
			break;
		}
	}

	// 아이템 효과의 옵션 파라미터(아이스 당첨 등) 설정
	public void	setFavorOption(object option0)
	{
		this.item_favor.option0 = option0;
	}

	// 프리셋을 반환.
	// 프리셋의 말풍선(NPC의 말풍선)을 표시할 때 호출.
	public string	getPresetText(int text_id)
	{
		string	text = "";

		if(0 <= text_id && text_id < this.preset_texts.Count) {

			text = this.preset_texts[text_id];
		}

		return(text);
	}

	// ================================================================ //

	// 생성 직후 호출되는 파생 클래스용.
	public virtual void	initialize_item()
	{
	}

	// 게임 시작 시에 한 번만 호출.
	public virtual void	start()
	{
		if(this.controll.type == "shot_negi" || this.controll.type == "shot_yuzu") {

			this.controll.setBillboard(false);
		}

		if(this.step.get_next() == STEP.NONE) {

			this.step.set_next(STEP.IDLE);
		}
	}

	public Vector3	buffet_goal = Vector3.zero;
	public float	buffet_height = 1.0f;

	// 매 프레임 호출.
	public virtual void	execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크.


		switch(this.step.do_transition()) {

			case STEP.IDLE:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.SPAWN:
				{
					this.ip_jump.setBounciness(new Vector3(0.0f, -0.5f, 0.0f));

					Vector3		start = this.transform.position;
					Vector3		goal  = this.transform.position;

					this.ip_jump.start(start, goal, 1.0f);

					EffectRoot.get().createSmokeMiddle(this.transform.position);
				}
				break;

				case STEP.FALL:
				{
					this.ip_jump.setBounciness(new Vector3(0.0f, -0.5f, 0.0f));

					Vector3		start = this.transform.position + Vector3.up*10.0f;
					Vector3		goal  = this.transform.position;

					this.ip_jump.start(start, goal, 1.0f);
				}
				break;

				case STEP.BUFFET:
				{
					this.ip_jump.setBounciness(new Vector3(0.0f, -0.5f, 0.0f));

					Vector3		start = this.transform.position;
					Vector3		goal  = this.buffet_goal;

					this.ip_jump.start(start, goal, this.buffet_height);

					//EffectRoot.get().createSmokeMiddle(this.transform.position);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.FALL:
			case STEP.SPAWN:
			{
				this.ip_jump.execute(Time.deltaTime);

				this.transform.position = this.ip_jump.position;
			}
			break;

			case STEP.BUFFET:
			{
				this.ip_jump.execute(Time.deltaTime);

				if(!this.controll.isPickable()) {

					if(this.ip_jump.velocity.y <= 0.0f) {

						this.controll.cmdSetPickable(true);
					}
				}

				this.transform.position = this.ip_jump.position;
			}
			break;
		}

		// ---------------------------------------------------------------- //
	}

	public void		beginFall()
	{
		this.step.set_next(STEP.FALL);
	}

	public void		beginSpawn()
	{
		this.step.set_next(STEP.SPAWN);
	}

	public void		beginBuffet()
	{
		this.controll.cmdSetPickable(false);
		this.step.set_next(STEP.BUFFET);
	}

	// 주웠을 때 호출.
	public virtual void		onPicked()
	{
	}

	// 리스폰했을 때 호출.
	public virtual void		onRespawn()
	{
	}

	// ================================================================ //
	// 상속할 클래스용

	protected void	addPresetText(string text)
	{
		if(this.is_texts_editable) {

			this.preset_texts.Add(text);

		} else {

			// initialize() 메소드 외에서는 텍스츠를 추가할 수 없다.
			Debug.LogError("addPresetText() can use only in initialize_npc().");
		}
	}
}
