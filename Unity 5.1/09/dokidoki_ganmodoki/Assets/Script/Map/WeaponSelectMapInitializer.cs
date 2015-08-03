using UnityEngine;
using System.Collections;
using GameObjectExtension;

// 무기 선택 맵을 초기화하기 위한 클래스.
public class WeaponSelectMapInitializer : MapInitializer
{
	public Shader	map_shader;

	public override void initializeMap(GameRoot game_root)
	{
		MapCreator		map_creator   = MapCreator.get();
		PartyControl	party_control = PartyControl.get();

		map_creator.setRoomNum(1, 1);

		// Floor 루트 생성.
		map_creator.floor_root_go = new GameObject("Floor");

		// 무기 선택 플로어에서는 방의 블록을 3 x 4로 변경.
		map_creator.setRoomGridNum(3, 4);

		// 방 만들기.
		RoomController room = map_creator.createRoomFloor(new Map.RoomIndex(0, 0));

		// 더미 방 만들기.
		RoomController	vacancy = map_creator.createVacancy(new Map.RoomIndex(0, -1));

		// 방 구분 벽 만들기.
		map_creator.createRoomWall();

		// 외벽 만들기.
		GameObject	outer_walls = map_creator.createOuterWalls();

		// 플로어 이동 도어를 하나만 만든다.
		map_creator.createFloorDoor(new Map.RoomIndex(0, 0), new Map.BlockIndex(1, 3), Map.EWSN.NORTH);

		// ---------------------------------------------------------------- //

		Renderer[]	renderers = outer_walls.GetComponentsInChildren<Renderer>();

		foreach(var render in renderers) {

			render.material.shader = this.map_shader;
		}

		//

		renderers = vacancy.GetComponentsInChildren<Renderer>();

		foreach(var render in renderers) {

			render.material.shader = this.map_shader;
		}

		renderers = room.GetComponentsInChildren<Renderer>();

		foreach(var render in renderers) {

			render.material.shader = this.map_shader;
		}

		// ---------------------------------------------------------------- //
		// 무 아저씨.

		chrController	kabusan = CharacterRoot.get().createNPC("NPC_Kabu_San");

		kabusan.cmdSetPositionAnon(chrBehaviorKabu.getStayPosition());
		kabusan.cmdSetDirectionAnon(chrBehaviorKabu.getStayDirection());

		// ---------------------------------------------------------------- //
		// 로컬 플레이어.

		party_control.createLocalPlayer(GlobalParam.getInstance().global_account_id);

		chrBehaviorLocal	player = PartyControl.get().getLocalPlayer();

		player.control.cmdSetPositionAnon(new Vector3( 0.0f, 0.0f, -9.0f));
		player.changeBulletShooter(SHOT_TYPE.EMPTY);

		// ---------------------------------------------------------------- //
		// 아이템 생성.

		this.generateItems(game_root);

		party_control.setCurrentRoom(room);

		ItemWindow.get().setActive(false);
	}

	// 맵에 아이텝을 배치하는 메소드.
	private void generateItems(GameRoot gameRoot)
	{
		string	local_player_id = PartyControl.get().getLocalPlayer().getAcountID();

		string item_type = "shot_negi";
		string item_name = item_type + "." + local_player_id;
		ItemManager.get().createItem(item_type, item_name, local_player_id);
		ItemManager.get().setPositionToItem(item_name, WeaponSelectMapInitializer.getNegiItemPosition());

		item_type = "shot_yuzu";
		item_name = item_type + "." + local_player_id;
		ItemManager.get().createItem(item_type, item_name, local_player_id);
		ItemManager.get().setPositionToItem(item_name, WeaponSelectMapInitializer.getYuzuItemPosition());
	}

	// 대파 아이템 위치.
	public static Vector3	getNegiItemPosition()
	{
		return(new Vector3( 7.0f, 0.0f,  2.0f));
	}

	// 유자 아이템 위치.
	public static Vector3	getYuzuItemPosition()
	{
		return(new Vector3(-7.0f, 0.0f,  2.0f));
	}
}
