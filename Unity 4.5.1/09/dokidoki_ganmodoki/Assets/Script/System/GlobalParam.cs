using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 씬을 넘어 사용하고 싶은 파라미터.
public class GlobalParam : MonoBehaviour {
	
	public	int			global_account_id	= 0;			// 글로벌 고유 어카운트 id.

	public	bool		is_host				= false;		// 호스트로서 플레이하고 있는가?.

	// 아이템 동기화용 해시 테이블.
	public Dictionary<string, ItemManager.ItemState> item_table = new Dictionary<string, ItemManager.ItemState>();
	
	// 초기 장비 보존.
	public SHOT_TYPE[]	shot_type = new SHOT_TYPE[NetConfig.PLAYER_MAX];

	private static		GlobalParam instance = null;

	public int			floor_number = 0;

	public bool			fadein_start = false;

	public bool[]		db_is_connected = new bool[NetConfig.PLAYER_MAX];

	// 통신에서 사용하는 난수 시드 값.
	public int			seed = 0;

	// ================================================================ //
	
	public void		create()
	{
		for(int i = 0;i < this.db_is_connected.Length;i++) {

			this.db_is_connected[i] = false;
		}

		for(int i = 0;i < shot_type.Length;i++) {
			this.shot_type[i] = SHOT_TYPE.NEGI;
		}
	}

	// ================================================================ //

	public static GlobalParam	get()
	{
		if(instance == null) {

			GameObject	go = new GameObject("GlobalParam");

			instance = go.AddComponent<GlobalParam>();
			instance.create();

			DontDestroyOnLoad(go);
		}

		return(instance);
	}
	public static GlobalParam	getInstance()
	{
		return(GlobalParam.get());
	}

	// FIXME : floor_number을 더해서 다름 레벨을 결정하고 로드한다. 개발용으로 여기에 써둠..
	public void GoToNextLevel()
	{
		floor_number++;

		switch (floor_number)
		{
		case 1:
			Application.LoadLevel("WeaponSelectScene");
			break;
		case 2:
			Application.LoadLevel("GameScene");
			break;
		case 3:
			Application.LoadLevel("BossCene");
			break;
		}
	}
}
