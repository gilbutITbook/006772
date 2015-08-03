using UnityEngine;
using System.Collections;

// 아이템의 비헤이비어  꽃용.
public class ItemBehaviorFlower : ItemBehaviorBase {

	private GameObject		flower = null;		// 꽃 : 지면에 자라고 있을 때.
	private GameObject		brooch = null;		// 브로치: 주웠을 때.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 생성 직 후에 호출됩니다.
	public override void	initialize_item()
	{
		this.flower = this.gameObject.transform.FindChild("Flower").gameObject;
		this.brooch = this.gameObject.transform.FindChild("Brooch").gameObject;

		this.flower.renderer.enabled = true;
		this.brooch.renderer.enabled = false;
	}

	// 게임 시작 시에 한 번만 호출됩니다.
	public override void	start()
	{
		this.controll.balloon.setColor(Color.red);
	}

	// 매 프레임 호출됩니다.
	public override void	execute()
	{
	}

	// 픽업됐을 때 호출됩니다.
	public override void	onPicked()
	{
		this.flower.renderer.enabled = false;
		this.brooch.renderer.enabled = true;
	}

	// 다시 생겨났을 때 호출됩니다.
	public override void		onRespawn()
	{
		this.flower.renderer.enabled = true;
		this.brooch.renderer.enabled = false;
	}
}
