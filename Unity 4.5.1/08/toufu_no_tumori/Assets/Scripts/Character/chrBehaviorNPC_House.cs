using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 비헤이비어　NPC House 용.
public class chrBehaviorNPC_House : chrBehaviorNPC {

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 생성 직후에 호출됩니다. NPC 용.
	public override void	initialize_npc()
	{
      	rigidbody.constraints = RigidbodyConstraints.FreezeAll;

		this.addPresetText("Ｗｅｌｌｃｏｍｅ！");
		this.addPresetText("이사중");
		this.addPresetText("어서와요");
		this.addPresetText("바이바이~"); 
	}

	// 게임 시작 시에 한 번만 호출됩니다.
	public override void	start()
	{
	}

	// 매 프레임 호출됩니다.
	public override	void	execute()
	{
		GameObject go = GameObject.Find("Network");
		if (go != null && 
		    go.GetComponent<Network>().IsConnected() == false) {
			// 단절 시에 집에 돌아왔을 때는 말풍선을 변경.
			this.controll.cmdDispBalloon(3);
		}
	}

	// 현관문을 엽니다.
	public void		openDoor()
	{
		this.controll.cmdSetMotion("Open", 0);
	}

	// 현관문을 닫습니다.
	public void		closeDoor()
	{
		this.controll.cmdSetMotionRewind("Open", 0);
	}

	// 이사 시작.
	public void		startHouseMove()
	{
		GameObject go = GameObject.Find("Network");
		if (go != null && 
		    go.GetComponent<Network>().IsConnected() == false) {
			// 단절 시에 집에 돌아왔을 때는 말풍선 변경.
			this.controll.cmdDispBalloon(3);
			return;
		}

		this.controll.cmdDispBalloon(1);
	}

	// 이사 끝.
	public void		endHouseMove()
	{
		this.controll.cmdDispBalloon(2);
	}
}