using UnityEngine;
using System.Collections;

public class BossMapInitializer : MapInitializer
{
	public BossLevelSequence sequencer;

	// 개발용으로 보스를 컴퓨터 입력으로 움직이게 할지 지정.
	// TODO 공개 전에 삭제
	public bool UsesHumanControlForBoss;

	public override void initializeMap(GameRoot game_root)
	{
		MapCreator		map_creator   = MapCreator.get();
		PartyControl	party_control = PartyControl.get();

		map_creator.setRoomNum(1, 1);

		map_creator.floor_root_go = new GameObject("Floor");
		
		// 방 만들기.
		RoomController room = map_creator.createRoomFloor(new Map.RoomIndex(0, 0));

		// 더미 방 만들기.
		map_creator.createVacancy(new Map.RoomIndex(0, -1));

		// 방을 구분하는 벽을 만든다.
		map_creator.createRoomWall();

		// 외벽을 만든다.
		map_creator.createOuterWalls();

		GameRoot.get().createLocalPlayer();
		GameRoot.get().createNetPlayers();

		// 플레이어 위치 설정.

		chrBehaviorLocal	local_player = PartyControl.get().getLocalPlayer();

		Vector3		playerStartPosition = Vector3.zero;

		local_player.transform.position = playerStartPosition + PartyControl.get().getPositionOffset(local_player.control.global_index);
	
		for(int i = 0;i < PartyControl.get().getFriendCount();i++) {

			chrBehaviorPlayer	friend = PartyControl.get().getFriend(i);

			friend.control.cmdSetPositionAnon(playerStartPosition + PartyControl.get().getPositionOffset(friend.control.global_index));
		}

		party_control.setCurrentRoom(room);

		// ボスの作成.

		chrControllerEnemyBase	enemy;

		if(UsesHumanControlForBoss) {

			enemy = CharacterRoot.get().createEnemy("Boss1", "chrControllerEnemyBoss", "chrBehaviorEnemyBoss_Human") as chrControllerEnemyBase;

		} else {

			enemy = CharacterRoot.get().createEnemy("Boss1", "chrControllerEnemyBoss", "chrBehaviorEnemyBoss") as chrControllerEnemyBase;
		}

		enemy.cmdSetPosition(new Vector3 (0.0f, 0.0f, 20.0f));

		// 스테이터스 창.

		Navi.get().createStatusWindows();
	}
}
