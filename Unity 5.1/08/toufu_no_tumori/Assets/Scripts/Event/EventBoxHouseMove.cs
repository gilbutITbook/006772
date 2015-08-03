using UnityEngine;
using System.Collections;

// 이사 시작 이벤트 박스.
public class EventBoxHouseMove : MonoBehaviour {

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	void	OnTriggerEnter(Collider other)
	{
		do {

			var		player = other.gameObject.GetComponent<chrBehaviorLocal>();

			if(player == null) {

				break;
			}

			if(!GlobalParam.get().is_in_my_home) {

				break;
			}

			player.onEnterHouseMoveEventBox();

		} while(false);
	}

	void	OnTriggerExit(Collider other)
	{
		do {

			var		player = other.gameObject.GetComponent<chrBehaviorLocal>();

			if(player == null) {

				break;
			}

			if(!GlobalParam.get().is_in_my_home) {

				break;
			}

			player.onLeaveHouseMoveEventBox();

		} while(false);
	}
}
