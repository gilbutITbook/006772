using UnityEngine;
using System.Collections;

// 아이템의 비헤이비어    아줌마고양이용.
public class ItemBehaviorCat : ItemBehaviorBase {

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 생성 직후에 호출됩니다. .
	public override void	initialize_item()
	{
		this.item_favor.is_enable_house_move = true;
	}
}
