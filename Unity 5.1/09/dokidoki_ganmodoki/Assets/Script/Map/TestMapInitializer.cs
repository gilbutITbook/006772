using UnityEngine;
using System.Collections;

public class TestMapInitializer : MapInitializer {

	public int		LevelGeneratorSeed = 5;				//< 제네레이터에 넘겨줄 시드.
	public bool		UseRandomSeedForDebug = false;		//< LevelGeneratorSeed를 사용하지 않고.
	public int		RandomSeedForDebugMin = 0;			//< UseRandomSeedForDebug가 유효한 때 사용하는 랜덤 레인지.
	public int		RandomSeedForDebugMax = 100;		//< UseRandomSeedForDebug가 유효한 때 사용하는 랜덤 레인지.

	public override void initializeMap(GameRoot game_root)
	{
		MapCreator mapCreator = MapCreator.getInstance ();

		Vector3 start_position;

		int seed = 0;

		if (UseRandomSeedForDebug)
		{
			seed = Random.Range(RandomSeedForDebugMin, RandomSeedForDebugMax);
		}
		else
		{
			seed = LevelGeneratorSeed;
		}

		// 맵 자동 생성.

		mapCreator.generateLevel(seed);

		// 로컬 플레이어를 작성. FIXME: 통신 대응.
		PartyControl.getInstance().createLocalPlayer(GlobalParam.getInstance().global_account_id);
		
		// Put items including keys.
		mapCreator.generateItems(PartyControl.get().getLocalPlayer().getAcountID());
		
		// Put lairs in the map.
		//mapCreator.generateLairs();
		
		// FIXME: 이곳은 좌표 쓰기 명령이 되는 건가?
		// 일단 좌표를 직접 써넣는다.
		start_position = mapCreator.getPlayerStartPosition();

		PartyControl.getInstance().getLocalPlayer().transform.position = start_position;
		
		mapCreator.createFloorDoor();
		
		// Activate the start room.
		mapCreator.SetCurrentRoom(mapCreator.getRoomFromPosition(start_position));

		{
			string	local_player_id = PartyControl.get().getLocalPlayer().getAcountID();

			ItemManager.getInstance().createItem("cake00",    local_player_id);
			ItemManager.getInstance().createItem("candy00",   local_player_id);
			ItemManager.getInstance().createItem("ice00",     local_player_id);
			
			ItemManager.getInstance().createItem("key00",     local_player_id);
			ItemManager.getInstance().createItem("key01",     local_player_id);
			ItemManager.getInstance().createItem("key02",     local_player_id);
			ItemManager.getInstance().createItem("key03",     local_player_id);
			//ItemManager.getInstance().createItem("key04",     local_player_id);
			
			ItemManager.getInstance().setPositionToItem("cake00",    start_position + new Vector3(2.0f, 0.0f, 0.0f));
			ItemManager.getInstance().setPositionToItem("candy00",   start_position + new Vector3(4.0f, 0.0f, 0.0f));
			ItemManager.getInstance().setPositionToItem("ice00",     start_position + new Vector3(6.0f, 0.0f, 0.0f));
		
			ItemManager.getInstance().setPositionToItem("key00",     start_position + new Vector3( 2.0f, 0.0f, -2.0f));
			ItemManager.getInstance().setPositionToItem("key01",     start_position + new Vector3( 4.0f, 0.0f, -2.0f));
			ItemManager.getInstance().setPositionToItem("key02",     start_position + new Vector3( 6.0f, 0.0f, -2.0f));
			ItemManager.getInstance().setPositionToItem("key03",     start_position + new Vector3( 8.0f, 0.0f, -2.0f));
			//ItemManager.getInstance().setPositionToItem("key04",     start_position + new Vector3(10.0f, 0.0f, -2.0f));
		}
		/*{
			EnemyRoot.getInstance().createEnemy("Enemy_Kumasan").transform.Translate(new Vector3(0.0f, 0.0f, 5.0f));
			EnemyRoot.getInstance().createEnemy("Enemy_Obake").transform.Translate(new Vector3(-5.0f, 0.0f, 5.0f));
		}*/
	}
}
