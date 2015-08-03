using UnityEngine;
using System.Collections;

public class chrBehaviorPlayer : chrBehaviorBase {

	// 이사 중 정보.
	protected struct StepHouseMove {
		
		public chrBehaviorNPC_House	house;
	};
	protected StepHouseMove		step_house_move;

	// ================================================================ //

	// 이사 시작.
	public virtual void		beginHouseMove(chrBehaviorNPC_House house)
	{
		this.step_house_move.house = house;
	}

	// 이사 종료 시 처리.
	public virtual void		endHouseMove()
	{
		// 집을 자신의 자식이 아니게 한다.
		this.step_house_move.house.transform.parent = null;

		this.controll.game_input.clear();

		// 자신을 표시한다.
		this.controll.setVisible(true);

		// 집의 박스 콜라이더를 유효화하여 물리 운동 시작.
		this.step_house_move.house.GetComponent<BoxCollider>().enabled = true;
		this.step_house_move.house.GetComponent<Rigidbody>().useGravity = true;
		this.step_house_move.house.GetComponent<Rigidbody>().WakeUp();

		// 자신의 박스 콜라이더를 삭제해서 캡슐 콜라이더를.
		// 복귀한다.
		GameObject.DestroyImmediate(this.gameObject.GetComponent<BoxCollider>());
		this.gameObject.GetComponent<CapsuleCollider>().enabled = true;
	}

	// 이사 중？.
	public virtual bool		isNowHouseMoving()
	{
		return(false);
	}

	// 로컬 플레이어？.
	public virtual bool		isLocal()
	{
		return(true);
	}

	// ================================================================ //

	// 걷기 모션 재생.
	public void		playWalkMotion()
	{
		// 걷기 모션.
		this.controll.cmdSetMotion("Take 001", 0);
		
		Sound.ID[]	ids = {Sound.ID.TFT_SE02A, Sound.ID.TFT_SE02B};

		// 발자국 소리 효과.
		SoundManager.get().playSEInterval(ids, 0.5f, this.get_walk_se_slot());
	}
	
	// 멈춤 모션 재생.
	public void		stopWalkMotion()
	{
		// 멈춤 모션.
		this.controll.cmdSetMotion("Take 002", 0);
		
		// 발자국 소리 효과.
		SoundManager.get().stopSEInterval(this.get_walk_se_slot());
	}

	protected Sound.SLOT	get_walk_se_slot()
	{
		Sound.SLOT	slot = Sound.SLOT.SE_WALK0;

		if(this.isLocal()) {

			slot = Sound.SLOT.SE_WALK0;

		} else {

			slot = Sound.SLOT.SE_WALK1;
		}

		return(slot);
	}

	// ================================================================ //

	// 조정을 마친 쿼리 실행.
	protected void		execute_queries()
	{
		foreach(QueryBase query in this.controll.queries) {
			
			if(!query.isDone()) {
				
				continue;
			}
			
			switch(query.getType()) {
				
				case "item.pick":
				{
					QueryItemPick	query_pick = query as QueryItemPick;
					
					if(query.isSuccess()) {
						
						// 아이템을 가지고 있으면 버린다.
						if(this.controll.item_carrier.isCarrying()) {
						Debug.Log("Pick:" + query_pick.target + " Carry:" + this.controll.item_carrier.item.id);
							if (query_pick.target != this.controll.item_carrier.item.id) {
								// 상대방 플레이어에게 드롭한 사실을 알려줘야만 하므로 쿼리를 만든다.
								// 동기화할 필요는 없으므로 드롭은 바로 실행한다.
							Debug.Log ("behavior:cmdItemQueryDrop");

								QueryItemDrop		query_drop = this.controll.cmdItemQueryDrop();

								query_drop.is_drop_done = true;

								this.controll.cmdItemDrop(this.controll.account_name);
							}
						}
						
						this.controll.cmdItemPick(query_pick, this.controll.account_name, query_pick.target);

						if(!query_pick.is_anon) {

							SoundManager.get().playSE(Sound.ID.TFT_SE01);
						}
					}
					
					query.set_expired(true);		
				}
				break;
				
				case "item.drop":
				{
					if(query.isSuccess()) {

						if((query as QueryItemDrop).is_drop_done) {

							// 이미 드롭 완료.
							Debug.Log("[CLIENT CHAR] Item already dropped.");
						} else {
							Debug.Log("[CLIENT CHAR] Item dropped.");

							this.controll.cmdItemDrop(this.controll.account_name);
						}
					}
					
					query.set_expired(true);					
				}
				break;
				
				case "house-move.start":
				{
					do {
						
						if(!query.isSuccess()) {
							
							break;
						}
						
						QueryHouseMoveStart		query_start = query as QueryHouseMoveStart;
						
						chrBehaviorNPC_House	house = CharacterRoot.get().findCharacter<chrBehaviorNPC_House>(query_start.target);
						
						if(house == null) {
							
							break;
						}
						
						var		start_event = EventRoot.get().startEvent<HouseMoveStartEvent>();
						
						start_event.setPrincipal(this);
						start_event.setHouse(house);
						
					} while(false);
					
					query.set_expired(true);
				}
				break;
				
				case "house-move.end":
				{
					do {
						
						if(!query.isSuccess()) {
							
							break;
						}
						
						chrBehaviorNPC_House	house = this.step_house_move.house;
						
						var		end_event = EventRoot.get().startEvent<HouseMoveEndEvent>();
						
						end_event.setPrincipal(this);
						end_event.setHouse(house);
						
					} while(false);
					
					query.set_expired(true);
				}
				break;
				
				case "talk":
				{
					if(query.isSuccess()) {
						
						QueryTalk		query_talk = query as QueryTalk;
						
						this.controll.cmdDispBalloon(query_talk.words);
					}
					query.set_expired(true);
				}
				break;
			}
			
			break;
		}
	}

	// ================================================================ //

	// STEP.HOUSE_MOVE 초기화.
	protected void	initialize_step_house_move_common()
	{
		// 자신의 캡슐 콜라이더를 무효로 하고 집의 박스 콜라이더를.
		// 이식한다.
		this.gameObject.GetComponent<CapsuleCollider>().enabled = false;
		this.gameObject.AddComponent<BoxCollider>();
		this.gameObject.GetComponent<BoxCollider>().size   = this.step_house_move.house.GetComponent<BoxCollider>().size;
		this.gameObject.GetComponent<BoxCollider>().center = this.step_house_move.house.GetComponent<BoxCollider>().center;
	
		// 집의 박스 콜라이더를 무효로 하고 물리 운동을 끈다.
		this.step_house_move.house.GetComponent<BoxCollider>().enabled = false;
		this.step_house_move.house.GetComponent<Rigidbody>().useGravity = false;
		this.step_house_move.house.GetComponent<Rigidbody>().velocity = Vector3.zero;
		this.step_house_move.house.GetComponent<Rigidbody>().Sleep();

		// 캐릭터를 비표시로 한다.
		// 집이 이동하는 거로 보인다.
		// （집을 자식으로 만들기 전에 하지 않으면 집까지 보이지 않는다）.
		//
		this.controll.setVisible(false);

		// 집을 자신의 자식으로 만든다.
		this.transform.position = this.step_house_move.house.transform.position;
		this.transform.rotation = this.step_house_move.house.transform.rotation;

		this.step_house_move.house.transform.parent = this.transform;
	}

	// STEP.HOUSE_MOVE 실행.
	protected void	execute_step_house_move_common()
	{
		this.step_house_move.house.GetComponent<Rigidbody>().velocity        = Vector3.zero;
		this.step_house_move.house.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		this.step_house_move.house.transform.localPosition = Vector3.zero;
		this.step_house_move.house.transform.localRotation = Quaternion.identity;
			
		//this.exec_step_move();
	}
}
