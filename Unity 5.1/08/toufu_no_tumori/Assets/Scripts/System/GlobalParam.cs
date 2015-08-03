using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 전체 씬에 걸쳐 사용할 파라미터.
public class GlobalParam : MonoBehaviour {
	
	public	int		global_acount_id	 = 0;			// 글로벌하고 유니크한 어카운트 id..
	public	string	account_name  	 	 = "Toufuya";	// 어카운트 이름(=캐릭터 이름).
	public	bool	is_in_my_home	  	 = true;		// 자신의 정원이 있는가?.
	public	bool	skip_enter_event	 = true;		// 도착 이벤트를 스킵?(게임 시작 시).
	public	bool	is_host				 = false;		// 호스트로서 플레이하고 있는가?.
	public	bool	is_remote_in_my_home = false;		// 리모트 캐릭터가 자신의 정원에 있는가?.
	public	bool	request_move_home	 = false;		// 정원의 이동을 요청했는가?
	public	bool	is_connected		 = false;		// 통신 상대와 접속했는가?.
	public	bool	is_disconnected		 = false;		// 통신 상대와 접속 종료했는가?.

	public	MovingData	local_moving;					// 로컬 캐릭터의 이사 정보.
	public	MovingData	remote_moving;					// 리모트 캐릭터의 이사 정보.

	// 아이템 동기화용 해시 테이블.
	public Dictionary<string, ItemManager.ItemState> item_table = new Dictionary<string, ItemManager.ItemState>();
	

	private static	GlobalParam instance = null;

	public bool		fadein_start = false;

	// ================================================================ //

	public static GlobalParam	getInstance()
	{
		if(instance == null) {

			GameObject	go = new GameObject("GlobalParam");

			instance = go.AddComponent<GlobalParam>();

			DontDestroyOnLoad(go);
		}

		return(instance);
	}
	public static GlobalParam	get()
	{
		return(GlobalParam.getInstance());
	}
}
