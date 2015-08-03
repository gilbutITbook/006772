using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 비헤이비어    NPC Elder 용.
public class chrBehaviorNPC_Elder : chrBehaviorNPC {

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 생성 직후에 호출됩니다.  NPC용.
	public override void	initialize_npc()
	{
		this.addPresetText("내가 촌장이지");
		this.addPresetText("어디서나 볼 수 있는");
		this.addPresetText("촌장이 아닐세");
	}

	// 게임 시작할 때 한 번만 호출됩니다.
	public override void	start()
	{
	}
}