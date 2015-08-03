using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameObjectExtension;

public class RoomController : MonoBehaviour {

	private List<GameObject> northRoomWalls = new List<GameObject>();
	private List<GameObject> eastRoomWalls  = new List<GameObject>();
	private List<GameObject> southRoomWalls = new List<GameObject>();
	private List<GameObject> westRoomWalls  = new List<GameObject>();

	private List<DoorControl>	doors = new List<DoorControl>();

	public GameObject[,]		floor_objects;		// 플로어 게임 오브젝트.

	protected Map.RoomIndex		index;				// 이 방의 번호.

	// ================================================================ //

	// 인덱스를 설정한다.
	public void		setIndex(Map.RoomIndex index)
	{
		this.index = index;
	}

	// 인덱스를 가져온다.
	public Map.RoomIndex	getIndex()
	{
		return(this.index);
	}

	public void RegisterRoomWall(Map.EWSN dir, GameObject roomWallGO)
	{
		switch (dir)
		{
		case Map.EWSN.NORTH:
			northRoomWalls.Add (roomWallGO);
			break;
		case Map.EWSN.EAST:
			eastRoomWalls.Add (roomWallGO);
			break;
		case Map.EWSN.SOUTH:
			southRoomWalls.Add (roomWallGO);
			break;
		case Map.EWSN.WEST:
			westRoomWalls.Add (roomWallGO);
			break;
		}
	}

	// 방 사이의 벽을 가져온다.
	public List<RoomWallControl>	GetRoomWalls(Map.EWSN dir)
	{
		List<RoomWallControl>	walls = new List<RoomWallControl>();
		List<GameObject>		wall_gos = null;

		switch (dir)
		{
		case Map.EWSN.NORTH:
			wall_gos = northRoomWalls;
			break;
		case Map.EWSN.EAST:
			wall_gos = eastRoomWalls;
			break;
		case Map.EWSN.SOUTH:
			wall_gos = southRoomWalls;
			break;
		case Map.EWSN.WEST:
			wall_gos = westRoomWalls;
			break;
		}

		if(wall_gos != null) {
			foreach(var go in wall_gos) {
				if(go.GetComponent<RoomWallControl>() != null) {
					walls.Add(go.GetComponent<RoomWallControl>());
				}
			}
		}
		return(walls);
	}

	public void RegisterDoor(DoorControl door)
	{
		doors.Add(door);
		door.SetRoom(this);
	}

	// 문의 수를 구한다.
	public int		getDoorCount()
	{
		return(this.doors.Count);
	}

	// 문을 가져온다.
	public DoorControl	getDoor(Map.EWSN dir)
	{
		return(this.doors.Find(x => x.door_dir == dir));
	}

	// index에 해당하는 문을 가져온다.
	public DoorControl	getDoorByIndex(int index)
	{
		return(this.doors[index]);
	}

	// 위치를 가져온다.
	public Vector3	getPosition()
	{
		return(this.gameObject.transform.position);
	}

	public void RegisterKey(string itemName, int keyType)
	{
		foreach (var door in doors) {
			if (door.KeyType == keyType) {
				door.SetKey(itemName);
				break;
			}
		}
	}
	
	// 문(열쇠)의 각 색이 사용되는지 조사한다.
	public List<bool>	checkKeyColorsUsed()
	{
		List<bool>	is_used = new List<bool>();

		for(int i = 0;i < 4;i++) {

			is_used.Add(false);
		}

		for(int i = 0;i < this.doors.Count;i++) {

			int		key_color = this.doors[i].KeyType;

			if(0 <= key_color && key_color < is_used.Count) {

				is_used[key_color] = true;
			}
		}

		return(is_used);
	}

	public int GetUnusedKeyType()
	{
		for (int i = 0; i < 4; ++i) {
			bool alreadyUsed = false;
			foreach (DoorControl door in doors) {
				alreadyUsed |= (door.KeyType == i);
			}
			if (!alreadyUsed) {
				return i;
			}
		}
		Debug.LogError("The room has more doors than 4.");
		return -1;
	}

	public int GetKeyType(Map.EWSN door_dir)
	{
		foreach (var door in doors) {
			if (door.door_dir == door_dir) {
				return door.KeyType;
			}
		}
		Debug.LogError("The room doesn't the door having the " + door_dir + " door.");
		return -1;
	}

	public void OnConsumedKey(string key_type)
	{
		//Debug.Log ("consumed key --- " + type);

		DoorControl		door = null;

		do {

			// 열쇠에 대응하는 문을 찾는다.

			Item.KEY_COLOR	key_color = Item.Key.getColorFromTypeName(key_type);

			if(key_color == Item.KEY_COLOR.NONE) {

				break;
			}

			door = this.doors.Find(x => x.KeyType == (int)key_color);

			if(door == null) {

				break;
			}

			// 언락.
			door.Unlock();

			// 연결된 문도 언락.
			if(door.connect_to != null) {

				door.connect_to.Unlock();
			}

		} while(false);
	}

	// 파티가 방에 들어갔을 때(현재 방이 되었을 때) 호출된다.
	public void NotifyPartyEnter()
	{
		// 남쪽 방 벽을 반투명으로 되돌린다.
		foreach(var wall in southRoomWalls) {

			wall.GetComponent<RoomWallControl>().FadeOut();
		}

		// 문의 슬립을 해제한다.
		foreach(var door in this.doors) {

			door.endSleep();
		}

		// 양초에 불을 켠다(이펙트).
		this.igniteCandles();
	}

	// 파티가 방을 나왔을 때(현재 방이 아니게 되었을 때)에 호출된다.
	public void NotifyPartyLeave()
	{
		// 남쪽 방 벽을 불투명하게 되돌린다.
		foreach(var wall in southRoomWalls) {

			wall.GetComponent<RoomWallControl>().FadeIn();
		}

		// 문을 슬립한다.
		foreach(var door in this.doors) {

			door.beginSleep();
		}

		this.puffOutCandles();
	}

	// 양초에 불을 붙인다(효과).
	public void		igniteCandles()
	{
		do {
			// 이미 불이 붙어 있을 때는(효과를 묶는 GameObject가 있다).
			// 아무것도 하지 않는다.
			if(this.transform.FindChild("fires") != null) {

				break;
			}

			// 기둥을 묶는 GameObject를 찾는다.

			Transform childPillars = this.transform.FindChild("pillars");

			if(childPillars == null) {

				break;
			}

			GameObject	pillars_root = this.transform.FindChild("pillars").gameObject;

			// 이펙트를 만든다.
	
			GameObject	fire_root = new GameObject("fires");
	
			fire_root.transform.parent = this.gameObject.transform;
	
			float		height = 3.0f;

			// 모든 자식(모든 기둥)에 효과를 적용한다.
			for(int i = 0;i < pillars_root.transform.childCount;i++) {
	
				GameObject	pillar = pillars_root.transform.GetChild(i).gameObject;
	
				GameObject	effect = EffectRoot.get().createCandleFireEffect(pillar.transform.position + Vector3.up*height);
	
				effect.transform.parent = fire_root.transform;
			}

		} while(false);
	}

	// 양초의 불을 끈다.
	public void		puffOutCandles()
	{
		do {

			Transform	child = this.transform.FindChild("fires");

			if(child == null) {

				break;
			}

			GameObject.Destroy(child.gameObject);

		} while(false);
	}

	// 현재 방(파티가 있는 방)인가?
	public bool	isCurrent()
	{
		return(PartyControl.get().getCurrentRoom() == this);
	}

	// ================================================================ //
	// 디버그용.

	// 플로어 디버그용 플레인을 표시한다.
	public void		dbSetFloorColor(Map.BlockIndex bi, Color color)
	{
		GameObject	debug_go = this.floor_objects[bi.x, bi.z].findChildGameObject("Debug");

		debug_go.SetActive(true);

		debug_go.GetComponent<MeshRenderer>().material.color = color;
	}

	// 플로어 디버그용 플레인을 비표시한다.
	public void		dbHideFloorColor(Map.BlockIndex bi)
	{
		GameObject	debug_go = this.floor_objects[bi.x, bi.z].findChildGameObject("Debug");

		debug_go.SetActive(false);
	}

	public void		dbArrowSetVisible(Map.BlockIndex bi, Map.EWSN ewsn, bool is_visible)
	{
		string		arrow_name = "";

		switch(ewsn) {

			case Map.EWSN.EAST:		arrow_name = "DebugE";	break;
			case Map.EWSN.WEST:		arrow_name = "DebugW";	break;
			case Map.EWSN.SOUTH:	arrow_name = "DebugS";	break;
			case Map.EWSN.NORTH:	arrow_name = "DebugN";	break;
		}

		if(arrow_name != "") {

			GameObject	arrow_go = this.floor_objects[bi.x, bi.z].findChildGameObject(arrow_name);

			arrow_go.SetActive(is_visible);
		}
	}

	public void		dbArrowHideAll(Map.BlockIndex bi)
	{
		for(int i = 0;i < (int)Map.EWSN.NUM;i++) {

			this.dbArrowSetVisible(bi, (Map.EWSN)i, false);
		}
	}
}