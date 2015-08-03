using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameObjectExtension;

public class MapCreator : MonoBehaviour {
	
	public GameObject[]		floor_prefabs      = null;			// 플로어.
	public GameObject[]		outer_wall_prefabs = null;			// 외벽.
	public GameObject[]		room_wall_prefabs  = null;			// 방을 구분하는 벽.
	public GameObject[]		inner_wall_prefabs = null;			// 방 안의 내벽.
	public GameObject[]		door_prefabs = null;				// 문.
	public GameObject		pillar_prefab = null;				// 방 내벽의 교차 위치에 설치할 기둥.
	
	public static float		BLOCK_SIZE = 10.0f;				// １그리드의 크기.
	public static float		UNIT_SIZE = 1.0f;				// Size of a unit --- player start position and item.

	public static int		ROOM_COLUMNS_NUM	= 3;	// 레벨 안 룸의 가로 수.
	public static int		ROOM_ROWS_NUM		= 3;	// 레벨 안 룸의 세로 수.

	public static int		BLOCK_GRID_COLUMNS_NUM	= 5;	// 룸 안의 그리드(블록)의 가로 칸 수.
	public static int		BLOCK_GRID_ROWS_NUM		= 5;	// 룸 안의 그리드(블록)의 세로 칸 수.

	// Field Generator에 전달할 블록 수.
	// 플로어와 플로어 사이(룸 내벽이 있는 곳)도 1블록으로 센다.
	public static int		BLOCK_COLUMNS_NUM = BLOCK_GRID_COLUMNS_NUM + (BLOCK_GRID_COLUMNS_NUM + 1);
	public static int		BLOCK_ROWS_NUM    = BLOCK_GRID_ROWS_NUM    + (BLOCK_GRID_ROWS_NUM + 1);

	public static float		ROOM_COLUMN_SIZE = BLOCK_SIZE * BLOCK_GRID_COLUMNS_NUM;
	public static float		ROOM_ROW_SIZE    = BLOCK_SIZE * BLOCK_GRID_ROWS_NUM;

	protected int		room_columns_num = ROOM_COLUMNS_NUM;
	protected int		room_rows_num    = ROOM_ROWS_NUM;

	protected int		block_grid_columns_num = BLOCK_GRID_COLUMNS_NUM;
	protected int		block_grid_rows_num    = BLOCK_GRID_ROWS_NUM;

	protected int		block_columns_num = BLOCK_GRID_COLUMNS_NUM + (BLOCK_GRID_COLUMNS_NUM + 1);
	protected int		block_rows_num    = BLOCK_GRID_ROWS_NUM    + (BLOCK_GRID_ROWS_NUM + 1);

	protected float	room_column_size = BLOCK_SIZE*BLOCK_GRID_COLUMNS_NUM;
	protected float	room_row_size    = BLOCK_SIZE*BLOCK_GRID_ROWS_NUM;

	public GameObject		floor_root_go = null;

	protected FieldGenerator	levelGenerator = null;

	protected FieldGenerator.ChipType[,][,]	level_data;

	protected PseudoRandom.Plant	random_plant;				// 난수 생성 오브젝트.

	// 계단　플로어 시작, 목적 지점.
	public struct Stairs {

		public Map.RoomIndex	room_index;
		public Map.BlockIndex	block_index;
	}
	public Stairs	start;
	public Stairs	goal;

	// ---------------------------------------------------------------- //
	// 방.

	private List<RoomController>	rooms = new List<RoomController>();
	private RoomController			currentRoom;
	private DoorControl				bossDoor;
	private string					bossKeyItemName;

	// ---------------------------------------------------------------- //

	// 방0에서 방1로 가는 문.
	public struct DoorData {
		
		public DoorControl.TYPE		type;
		
		public Map.RoomIndex	room0;		// 방0.
		public Map.RoomIndex	room1;		// 방1．

		
		public int	local_position;		// 세로 or가로 위치(그리드 단위).
		// 북-남 문이면 X위치(Z위치는 맨 위 또는 맨 아래).
		// 동-서 문이면 Z 위치(X위치는 가장 오른쪽 또는 가장 왼쪽).
	};

	// ルーム内の内壁のデーター.
	public struct InnerWallData {

		public int		x;				// 플로어의 그리드 번호(왼쪽 아래가 (0,0)).
		public int		z;				// 플로어의 그리드 번호 (왼쪽 아래가 (0,0)).

		public bool		is_horizontal;	
	}

	// 내벽의 끝에 두는 기둥.
	public struct PillarData {
		
		// 왼쪽 아래 그리드.
		// 양초는.
		// (x, x) (x + 1, z) (x, z + 1) (x + 1, z + 1)
		// の４つのグリッドの交点に置かれます.
		public int		x;				// 플로어의 그리드 번호(왼쪽 아래가(0,0)).
		public int		z;				// 플로어의 그리드 번호(왼쪽 아래가(0,0)).
	}

	// 플로어 위에 놓을 것(문,아이템 등).
	public struct BlockInfo {

		public Map.CHIP		chip;
		public int			option0;
	}

	public BlockInfo[,][,]	block_infos;

	// ================================================================ //
	// MonoBehaviour로부터 상속.

	void	Start()
	{
		if(dbwin.root().getWindow("map") == null) {

			this.create_debug_window();
		}
	}
	
	//void	Update()
	//{
	//}

	// ================================================================ //

	public void		setRoomNum(int columns, int rows)
	{
		this.room_columns_num = columns;
		this.room_rows_num    = rows;
	}

	public void		setRoomGridNum(int columns, int rows)
	{
		this.block_grid_columns_num = columns;
		this.block_grid_rows_num    = rows;

		this.block_columns_num = this.block_grid_columns_num + (this.block_grid_columns_num + 1);
		this.block_rows_num    = this.block_grid_rows_num    + (this.block_grid_rows_num + 1);

		this.room_column_size = BLOCK_SIZE*this.block_grid_columns_num;
		this.room_row_size    = BLOCK_SIZE*this.block_grid_rows_num;
	}

	// 방의 수를 가져온다.
	public Map.RoomIndex	getRoomNum()
	{
		return(new Map.RoomIndex(this.room_columns_num, this.room_rows_num));
	}

	// 방 안의 블록 수(가로)를 얻는다.
	public int		getBlockColumnsNum()
	{
		return(this.block_columns_num);
	}

	// 방 안의 블록 수(세로)를 얻는다.
	public int		getBlockRowsNum()
	{
		return(this.block_rows_num);
	}

	// 방 크기(가로)를 얻는다.
	public float	getRoomColumnSize()
	{
		return(BLOCK_SIZE*this.block_grid_columns_num);
	}

	// 방 크기(세로)를 얻는다.
	public float	getRoomRowSize()
	{
		return(BLOCK_SIZE*this.block_grid_rows_num);
	}

	// 방 안의 블록 수(가로)를 얻는다.
	public int		getBlockGridColumnsNum()
	{
		return(this.block_grid_columns_num);
	}

	// 방 안의 블록 수(세로)를 얻는다.\
	public int		getBlockGridRowsNum()
	{
		return(this.block_grid_rows_num);
	}

	// 레벨 생성 Kazuhisa Minato.
	public void		generateLevel(int seed)
	{
		if(this.levelGenerator == null) {

			this.random_plant = PseudoRandom.get().createPlant("MapCreator", 100);

			// ---------------------------------------------------------------- //

			// 난수 설정을 생성자에서 한다.
			this.levelGenerator = new FieldGenerator (seed);
			
			// 필드 생성방법.
			// 필드의 w, h는 2이상.
			// 블록의 w, h는 5이상 그리고 홀수라는 제한이 있다..
			// 홀수열의 그리드 폭은 제로로 해석할 필요가 있다.
			this.levelGenerator.SetSize (ROOM_ROWS_NUM, ROOM_COLUMNS_NUM, BLOCK_ROWS_NUM, BLOCK_COLUMNS_NUM);

			// 필드를 생성.
			// 각 요소가 블록인 2차원 배열을 생성한다.
			// 따라서 블록(i,j)에는 return_value[i,j]로 액세스 하고.
			// 블록(i,j)안의 요소(u,v)에는 return_value[i,j][u,v]로 액세스한다.
			var	levelData = levelGenerator.CreateField();


			// FieldGenerator 출력을 정렬한다.
			// ・[z, x]		->	[x, z].
			// ・위가 z = 0	->	아래가 z = 0.

			this.level_data = new FieldGenerator.ChipType[ROOM_COLUMNS_NUM, ROOM_ROWS_NUM][,];

			foreach(var ri in Map.RoomIndex.getRange(ROOM_COLUMNS_NUM, ROOM_ROWS_NUM)) {

				this.level_data[ri.x, ri.z] = new FieldGenerator.ChipType[BLOCK_COLUMNS_NUM, BLOCK_ROWS_NUM];

				foreach(var bi in Map.BlockIndex.getRange(this.block_columns_num, this.block_rows_num)) {

					this.level_data[ri.x, ri.z][bi.x, bi.z] = levelData[ROOM_ROWS_NUM - 1 - ri.z, ri.x][BLOCK_ROWS_NUM - 1 - bi.z, bi.x];
				}
			}

			// 시작, 목표.

			var ep = levelGenerator.GetEndPoints(levelData);

			this.start.room_index.x  = ep[0].fieldWidth;
			this.start.room_index.z  = ROOM_ROWS_NUM - 1 - ep[0].fieldHeight;
			this.start.block_index.x = (ep[0].blockWidth - 1)/2;
			this.start.block_index.z = this.block_grid_rows_num - 1 - (ep[0].blockHeight - 1)/2;

			this.goal.room_index.x  = ep[1].fieldWidth;
			this.goal.room_index.z  = ROOM_ROWS_NUM - 1 - ep[1].fieldHeight;
			this.goal.block_index.x = (ep[1].blockWidth - 1)/2;
			this.goal.block_index.z = this.block_grid_rows_num - 1 - (ep[1].blockHeight - 1)/2;

			//

			// ---------------------------------------------------------------- //

			// 방을 만든다.

			this.floor_root_go = new GameObject("Floor");

			foreach(var room_index in Map.RoomIndex.getRange(ROOM_COLUMNS_NUM, ROOM_ROWS_NUM)) {

				this.createRoom(room_index);
			}

			// 더미인 방.
			// (플로어의 가장 아래 열).
			for(int i = 0;i < ROOM_COLUMNS_NUM;i++) {

				this.createVacancy(new Map.RoomIndex(i, -1));
			}

			// 방 칸막이 생성.
			this.createRoomWall();

			this.createOuterWalls();


			// ---------------------------------------------------------------- //
			// 플로어 위에 둘 것(문, 아이템 등) 정보를 만들어 둔다.

			// 방 이동 문.
	
			this.block_infos = new MapCreator.BlockInfo[ROOM_COLUMNS_NUM, ROOM_ROWS_NUM][,];

			foreach(var ri in Map.RoomIndex.getRange(ROOM_COLUMNS_NUM, ROOM_ROWS_NUM)) {

				this.block_infos[ri.x, ri.z] = new BlockInfo[this.block_grid_columns_num, this.block_grid_rows_num];

				var		block_info_room = this.block_infos[ri.x, ri.z];
				var		level_data_room = this.level_data[ri.x, ri.z];

				foreach(var bi in Map.BlockIndex.getRange(this.block_grid_columns_num, this.block_grid_rows_num)) {

					block_info_room[bi.x, bi.z].chip = Map.CHIP.VACANT;

					if(level_data_room[bi.x*2 + 1 - 1, bi.z*2 + 1] == FieldGenerator.ChipType.Door) {

						block_info_room[bi.x, bi.z].chip    = Map.CHIP.DOOR;
						block_info_room[bi.x, bi.z].option0 = (int)Map.EWSN.WEST;

					} else if(level_data_room[bi.x*2 + 1 + 1, bi.z*2 + 1] == FieldGenerator.ChipType.Door) {

						block_info_room[bi.x, bi.z].chip    = Map.CHIP.DOOR;
						block_info_room[bi.x, bi.z].option0 = (int)Map.EWSN.EAST;

					} else if(level_data_room[bi.x*2 + 1, bi.z*2 + 1 - 1] == FieldGenerator.ChipType.Door) {

						block_info_room[bi.x, bi.z].chip    = Map.CHIP.DOOR;
						block_info_room[bi.x, bi.z].option0 = (int)Map.EWSN.SOUTH;

					} else if(level_data_room[bi.x*2 + 1, bi.z*2 + 1 + 1] == FieldGenerator.ChipType.Door) {

						block_info_room[bi.x, bi.z].chip    = Map.CHIP.DOOR;
						block_info_room[bi.x, bi.z].option0 = (int)Map.EWSN.NORTH;
					}
				}
			}

			// 플로어 이동 문.

			this.block_infos[this.start.room_index.x, this.start.room_index.z][this.start.block_index.x, this.start.block_index.z].chip    = Map.CHIP.STAIRS;
			this.block_infos[this.start.room_index.x, this.start.room_index.z][this.start.block_index.x, this.start.block_index.z].option0 = 0;
			this.block_infos[this.goal.room_index.x,   this.goal.room_index.z][this.goal.block_index.x,   this.goal.block_index.z].chip    = Map.CHIP.STAIRS;
			this.block_infos[this.goal.room_index.x,   this.goal.room_index.z][this.goal.block_index.x,   this.goal.block_index.z].option0 = 1;

			foreach(var ri in Map.RoomIndex.getRange(ROOM_COLUMNS_NUM, ROOM_ROWS_NUM)) {

				var		info_room = this.block_infos[ri.x, ri.z];

				RoomController	room = this.get_room_root_go(ri);

				List<Map.BlockIndex>	reserves = new List<Map.BlockIndex>();

				// ジェネレーター.
				for(int i = 0;i < 3;i++) {

					var		lair_places = this.allocateChipPlacesRoom(ri, Map.CHIP.LAIR, 1);

					if(lair_places.Count == 0) {

						break;
					}

					// 제네레이터가 이웃한 블록에 나오지 않게.
					// 주위 8블록을 예약해 둔다.

					Map.BlockIndex	bi = lair_places[0];

					foreach(var around in bi.getArounds8()) {

						if(this.allocateChipPlaceRoom(ri, around, Map.CHIP.LAIR)) {
	
							reserves.Add(around);
						}
					}
				}

				// 예약한 블록을 반환한다.
				foreach(var reserve in reserves) {

					this.putbackChipPlaceRoom(ri, reserve);
				}

				// 방 이동 열쇠.

				var		key_places = this.allocateChipPlacesRoom(ri, Map.CHIP.KEY, room.getDoorCount());

				for(int i = 0;i < key_places.Count;i++) {

					Map.BlockIndex	place = key_places[i];

					info_room[place.x, place.z].option0 = (int)room.getDoorByIndex(i).KeyType;
				}
			}

			// 플로어 이동 문의 열쇠.

			bool	floor_key_created = false;

			for(int i = 0;i < ROOM_COLUMNS_NUM*ROOM_ROWS_NUM;i++) {

				Map.RoomIndex	ri = new Map.RoomIndex();

				ri.x = this.random_plant.getRandomInt(ROOM_COLUMNS_NUM);
				ri.z = this.random_plant.getRandomInt(ROOM_ROWS_NUM);

				var floor_key_places = this.allocateChipPlacesRoom(ri, Map.CHIP.KEY, 1);

				if(floor_key_places.Count == 0) {

					continue;
				}

				this.block_infos[ri.x, ri.z][floor_key_places[0].x, floor_key_places[0].z].option0 = (int)Item.KEY_COLOR.PURPLE;

				floor_key_created = true;
				break;
			}
			if(!floor_key_created) {

				Debug.LogError("can't create floor key.");
			}

			// ---------------------------------------------------------------- //

		}
	}
#if false
	public int	getPracticableCount(Map.RoomIndex room_index, Map.BlockIndex block_index)
	{
		int		count = 0;

		for(int i = 0;i < (int)Map.EWSN.NUM;i++) {

			if(this.isPracticable(room_index, block_index, (Map.EWSN)i)) {

				count++;
			}
		}

		return(count);
	}
#endif
	// 指定 블록에서 지정 방향으로 진행하는가?.
	public bool	isPracticable(Map.RoomIndex room_index, Map.BlockIndex block_index, Map.EWSN eswn)
	{
		bool	ret = false;
		var		level_data_room = this.level_data[room_index.x, room_index.z];

		switch(eswn) {

			case Map.EWSN.NORTH:
			{
				if(block_index.z < this.block_grid_rows_num - 1) {

					if(level_data_room[block_index.x*2 + 1, block_index.z*2 + 1 + 1] != FieldGenerator.ChipType.Wall) {

						ret = true;
					}
				}
			}
			break;

			case Map.EWSN.SOUTH:
			{
				if(block_index.z > 0) {

					if(level_data_room[block_index.x*2 + 1, block_index.z*2 + 1 - 1] != FieldGenerator.ChipType.Wall) {

						ret = true;
					}
				}
			}
			break;

			case Map.EWSN.EAST:
			{
				if(block_index.x < this.block_grid_columns_num - 1) {

					if(level_data_room[block_index.x*2 + 1 + 1, block_index.z*2 + 1] != FieldGenerator.ChipType.Wall) {

						ret = true;
					}
				}
			}
			break;

			case Map.EWSN.WEST:
			{
				if(block_index.x > 0) {

					if(level_data_room[block_index.x*2 + 1 - 1, block_index.z*2 + 1] != FieldGenerator.ChipType.Wall) {

						ret = true;
					}
				}
			}
			break;
		}

		return(ret);
	}

	// ================================================================ //
	// 방을 만든다.

	// 방을 만든다.
	public RoomController	createRoom(Map.RoomIndex room_index)
	{
		Vector3		room_center = Vector3.zero;
		Vector3		position    = Vector3.zero;
		int			n = 0;
		
		// ---------------------------------------------------------------- //
		// 방 관리 오브젝트를 만든다.
	
		RoomController	room = this.get_room_root_go(room_index);

		room_center = this.getRoomCenterPosition(room_index);

		room.transform.position = room_center;
		
		// ---------------------------------------------------------------- //
		// 마루를 만든다.
		
		GameObject	floors = new GameObject("floors");
		
		floors.transform.position = room_center;

		room.floor_objects = new GameObject[this.block_grid_columns_num, this.block_grid_rows_num];

		foreach(var bi in Map.BlockIndex.getRange(this.block_grid_columns_num, this.block_grid_rows_num)) {

			GameObject	prefab = this.floor_prefabs[(bi.x + bi.z)%this.floor_prefabs.Length];
			GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

			position = this.getBlockCenterPosition(bi);
			position += room_center;
			
			go.transform.position = position;
			go.transform.parent   = floors.transform;

			room.floor_objects[bi.x, bi.z] = go;
		}

		// ---------------------------------------------------------------- //
		// 방 이동 문을 만든다.

		List<DoorData>	door_datas = new List<DoorData>();

		//  플로어 우단 방이 아니면…….
		if(room_index.x < ROOM_COLUMNS_NUM - 1) {

			for(int bz = 0; bz < this.block_rows_num; bz++) {
				int bx = this.block_columns_num - 1;

				if (this.level_data[room_index.x, room_index.z][bx, bz] == FieldGenerator.ChipType.Door)
				{
					// 좌:우의 방을 연결하는 문.
					DoorData	data = new DoorData();

					data.type  = DoorControl.TYPE.ROOM;
					data.room0 = room_index;
					data.room1 = room_index.get_next(1, 0);
					data.local_position = bz/2;
					door_datas.Add(data);
				}
			}
		}

		//  플로어 상단 방이 아니라면…….
		if(room_index.z < ROOM_ROWS_NUM - 1) {

			for(int bx = 0; bx < this.block_columns_num; bx++) {
				int bz = this.block_rows_num - 1;
				if (this.level_data[room_index.x, room_index.z][bx, bz] == FieldGenerator.ChipType.Door)
				{
					// 하:상의 방을 연결하는 문.
					DoorData	data = new DoorData();

					data.type  = DoorControl.TYPE.ROOM;
					data.room0 = room_index;				
					data.room1 = room_index.get_next(0, 1);
					data.local_position = bx/2;
					door_datas.Add(data);
				}
			}
		}

		this.create_doors(door_datas);

		// ---------------------------------------------------------------- //
		// 방 내벽을 만든다.

		List<InnerWallData>		wall_datas = new List<InnerWallData>();

		// 우벽 등록.
		for(int bz = 1;bz < this.block_rows_num;bz+=2) {

			for(int bx = 2;bx < this.block_columns_num - 2;bx+=2) {

				// 벽인지 조사한다.
				if(this.level_data[room_index.x, room_index.z][bx, bz] == FieldGenerator.ChipType.Wall) {
					InnerWallData	data = new InnerWallData();
					
					data.x = (bx - 2) / 2;
					data.z = (bz - 1) / 2;
					data.is_horizontal = false; // 세로 벽.
					wall_datas.Add(data);
				}
			}
		}
		// 상벽 등록.
		for(int bx = 1;bx < this.block_columns_num;bx+=2) {

			for(int bz = 2;bz < this.block_rows_num - 2;bz+=2) {

				// 벽인지 조사한다.
				if(this.level_data[room_index.x, room_index.z][bx, bz] == FieldGenerator.ChipType.Wall) {

					InnerWallData	data = new InnerWallData();
					
					data.x = (bx - 1) / 2;
					data.z = (bz - 2) / 2;
					data.is_horizontal = true; // 가로벽.
					wall_datas.Add(data);
				}
			}
		}
		this.create_inner_wall(room_index, wall_datas);

		// ---------------------------------------------------------------- //
		// 기둥을 만든다.
		// 벽이 두 장이상 접해 있는 부분에 만든다.
		
		List<PillarData>	pillar_datas = new List<PillarData>();
		PillarData			pillar_data  = new PillarData();
		
		for(int x = 0;x < BLOCK_GRID_COLUMNS_NUM - 1;x++) {
			
			for(int z = 0;z < BLOCK_GRID_ROWS_NUM - 1;z++) {
				
				n = 0;
				
				// 좌.
				if(wall_datas.Exists(wall => (wall.is_horizontal && wall.x == x && wall.z == z))) {
					
					n++;
				}
				// 우.
				if(wall_datas.Exists(wall => (wall.is_horizontal && wall.x == x + 1 && wall.z == z))) {
					
					n++;
				}
				
				// 하.
				if(wall_datas.Exists(wall => (!wall.is_horizontal && wall.x == x && wall.z == z))) {
					
					n++;
				}
				// 상.
				if(wall_datas.Exists(wall => (!wall.is_horizontal && wall.x == x && wall.z == z + 1))) {
					
					n++;
				}
				
				if(n < 2) {
					
					continue;
				}
				
				pillar_data.x = x;
				pillar_data.z = z;
				
				pillar_datas.Add(pillar_data);
			}
		}
		
		this.create_pillar(room_index, pillar_datas);

		// ---------------------------------------------------------------- //

		floors.transform.parent = room.transform;

		return(room.GetComponent<RoomController>());
	}

	// 방(더미인 빈 방)을 만든다.
	public RoomController	createVacancy(Map.RoomIndex room_index)
	{
		Vector3		room_center = Vector3.zero;
		Vector3		position    = Vector3.zero;
		int			n = 0;
		
		// ---------------------------------------------------------------- //
		// 방 관리 오브젝트를 만든다.
	
		RoomController	room = this.get_room_root_go(room_index);

		room_center = this.getRoomCenterPosition(room_index);

		room.transform.position = room_center;
		
		// ---------------------------------------------------------------- //
		// 마루를 만든다.
		
		GameObject	floors = new GameObject("floors");
		
		floors.transform.position = room_center;

		for(int z = 0;z < this.block_grid_rows_num;z++) {

			for(int x = 0;x < this.block_grid_columns_num;x++) {

				GameObject	prefab = this.floor_prefabs[n%this.floor_prefabs.Length];
				GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

				position = this.getBlockCenterPosition(new Map.BlockIndex(x, z));

				position += room_center;
				
				go.transform.position = position;
				
				go.transform.parent = floors.transform;
				
				n++;
			}
		}

		floors.transform.parent = room.transform;

		return(room);
	}

	// 방 관리 오브젝트를 가져온다. 아니면 생성한다.
	protected RoomController	get_room_root_go(Map.RoomIndex index)
	{
		string	name = "room" + index.x + index.z;

		GameObject	go = GameObject.Find(name);

		if(go == null) {

			go = new GameObject(name);

			// 관리용 컴포넌트를 추가한다.
			RoomController room = go.AddComponent<RoomController>();

			room.setIndex(index);

			// 리스트에 등록해 둔다.
			this.rooms.Add(room);

			go.transform.parent = this.floor_root_go.transform;
		}

		return(go.GetComponent<RoomController>());
	}

	// 방을 만든다(1룸).
	public void	create_doors(List<DoorData> door_datas)
	{
		DoorControl		door0, door1;
		Map.BlockIndex	block0_index, block1_index;

		int		n = 0;

		foreach(var data in door_datas) {

			// 양쪽 방에서 사용되지 않은 색 중에서 랜덤하게 선택한다.

			RoomController room0 = get_room_root_go(data.room0).GetComponent<RoomController>();
			RoomController room1 = get_room_root_go(data.room1).GetComponent<RoomController>();

			List<bool>	color_used0 = room0.checkKeyColorsUsed();
			List<bool>	color_used1 = room1.checkKeyColorsUsed();

			List<int>	key_colors = new List<int>();

			for(int i = 0;i < color_used0.Count;i++) {

				if(!color_used0[i] && !color_used1[i]) {

					key_colors.Add(i);
				}
			}

			int		key_type = key_colors[this.random_plant.getRandomInt(key_colors.Count)];

			door0 = null;
			door1 = null;

			// 상하의 방을 잇는 문.
			if(data.room0.x == data.room1.x) {

				block0_index = new Map.BlockIndex(data.local_position, BLOCK_GRID_ROWS_NUM - 1);
				block1_index = new Map.BlockIndex(data.local_position, 0);

				// 상(북).
				if(data.room1.z == data.room0.z + 1) {

					door0 = this.create_door(data.type, data.room0, block0_index, Map.EWSN.NORTH, key_type);
					door1 = this.create_door(data.type, data.room1, block1_index, Map.EWSN.SOUTH, key_type);
				}
				// 하(남).
				if(data.room1.z == data.room0.z - 1) {

					door0 = this.create_door(data.type, data.room0, block1_index, Map.EWSN.SOUTH, key_type);
					door1 = this.create_door(data.type, data.room1, block0_index, Map.EWSN.NORTH, key_type);
				}
			}

			// 좌우 방을 잇는 문.
			if(data.room0.z == data.room1.z) {

				block0_index = new Map.BlockIndex(BLOCK_GRID_COLUMNS_NUM - 1, data.local_position);
				block1_index = new Map.BlockIndex(0,                          data.local_position);

				// 동(우).
				if(data.room1.x == data.room0.x + 1) {

					door0 = this.create_door(data.type, data.room0, block0_index, Map.EWSN.EAST, key_type);
					door1 = this.create_door(data.type, data.room1, block1_index, Map.EWSN.WEST, key_type);
				}
				// 서(좌).
				if(data.room1.x == data.room0.x - 1) {

					door0 = this.create_door(data.type, data.room0, block1_index, Map.EWSN.WEST, key_type);
					door1 = this.create_door(data.type, data.room1, block0_index, Map.EWSN.EAST, key_type);
				}
			}

			if(door0 != null && door1 != null) {

				door0.connect_to = door1;
				door1.connect_to = door0;
			}

			n++;
		}
	}

	// 문을 만든다(하나).
	public DoorControl		create_door(DoorControl.TYPE type, Map.RoomIndex room_index, Map.BlockIndex block_index, Map.EWSN dir, int key_type)
	{
		GameObject	prefab;

		if(type == DoorControl.TYPE.ROOM) {

			prefab = this.door_prefabs[key_type%(this.door_prefabs.Length - 1)];

		} else {

			prefab = this.door_prefabs[this.door_prefabs.Length - 1];
		}

		GameObject		go   = GameObject.Instantiate(prefab) as GameObject;
		
		DoorControl		door = go.AddComponent<DoorControl>();
		
		door.room_index = room_index;
		
		door.type     = type;
		door.KeyType  = key_type;
		door.door_dir = dir;

		//
		
		RoomController	room = this.get_room_root_go(room_index);

		go.transform.parent = room.transform;
		go.transform.localPosition = this.getBlockCenterPosition(block_index);

		room.RegisterDoor(door);
		
		return(door);
	}

	// 방 안의 내벽을 만든다.
	public void		create_inner_wall(Map.RoomIndex room_index, List<InnerWallData> datas)
	{
		Vector3		room_center = Vector3.zero;
		Vector3		position    = Vector3.zero;
		int			n = 0;

		//
		room_center = this.getRoomCenterPosition(room_index);

		// 床

		GameObject	inner_walls = new GameObject("inner walls");

		inner_walls.transform.position = room_center;

		foreach(var data in datas) {

			int		x = data.x;
			int		z = data.z;

			GameObject	prefab = this.inner_wall_prefabs[n%this.inner_wall_prefabs.Length];
			GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

			position = this.getBlockCenterPosition(new Map.BlockIndex(x, z));
			
			if(data.is_horizontal) {
				
				// 가로(상).
				position.z += BLOCK_SIZE/2.0f;
				
			} else {
				
				// 세로(우).
				position.x += BLOCK_SIZE/2.0f;
			}

			go.transform.position = position + room_center;

			// 원래 모델이 세로 방향이므로 가로 방향일 때는 90도 회전한다.
			if(data.is_horizontal) {

				go.transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
			}

			go.transform.parent = inner_walls.transform;

			n++;
		}

		RoomController	room = this.get_room_root_go(room_index);

		inner_walls.transform.parent = room.gameObject.transform;
	}

	// ---------------------------------------------------------------- //
	// 양초 기둥     방 안의 내벽이 교차하는 장소.

	public void		create_pillar(Map.RoomIndex room_index, List<PillarData> datas)
	{
		Vector3		room_center = Vector3.zero;
		Vector3		position    = Vector3.zero;
		int			n = 0;
		
		// 방 중심 좌표를 구한다.
		room_center = this.getRoomCenterPosition(room_index);
		
		// 기둥
		
		GameObject	pillars = new GameObject("pillars");
		
		pillars.transform.position = room_center;
		
		foreach(var data in datas) {
			
			int		x = data.x;
			int		z = data.z;
			
			GameObject	prefab = this.pillar_prefab;
			GameObject	go     = GameObject.Instantiate(prefab) as GameObject;
			
			position = this.getBlockCenterPosition(new Map.BlockIndex(x, z));
			
			position.x += BLOCK_SIZE/2.0f;
			position.z += BLOCK_SIZE/2.0f;
			
			go.transform.position = position + room_center;
			
			go.transform.parent   = pillars.transform;
			
			n++;
		}
		
		RoomController	room = this.get_room_root_go(room_index);
		
		pillars.transform.parent = room.transform;
		
	}
	
	// ---------------------------------------------------------------- //
	// 방 구분 벽을 만든다.

	public void		createRoomWall()
	{
		Vector3		position    = Vector3.zero;

		GameObject	room_walls = new GameObject("room walls");

		int		room_wall_columns_num = this.room_columns_num*this.block_grid_columns_num;
		int		room_wall_rows_num    = (this.room_rows_num + 1)*this.block_grid_rows_num;

		// よこ.

		int		n = 0;

		for(int z = 0;z < this.room_rows_num;z++) {

			for(int x = 0;x < room_wall_columns_num;x++) {

				n++;

				GameObject	prefab = this.room_wall_prefabs[n%this.room_wall_prefabs.Length];
				GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

				position.x = (-((float)room_wall_columns_num/2.0f - 0.5f) + (float)x)*BLOCK_SIZE;
				position.y = 0.0f;
				position.z = (-((float)this.room_rows_num/2.0f - 0.5f) - 1.0f + (float)z)*this.room_row_size + this.room_row_size/2.0f;

				go.transform.position = position;
				go.transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
				go.transform.parent   = room_walls.transform;

				// 방 사이 벽을 방에 등록해 둔다.

				int rx = x/this.block_grid_columns_num;

				RoomController room = get_room_root_go(new Map.RoomIndex(rx, z));

				if(room != null) {

					room.RegisterRoomWall(Map.EWSN.SOUTH, go);
				}

				if(z > 0) {

					room = get_room_root_go(new Map.RoomIndex(rx, z - 1));
	
					if(room != null) {
	
						room.RegisterRoomWall(Map.EWSN.NORTH, go);
					}
				}
			}
		}

		// たて.

		n = 0;

		for(int x = 0;x < this.room_columns_num - 1;x++) {

			for(int z = 0;z < room_wall_rows_num;z++) {

				n++;

				GameObject	prefab = this.room_wall_prefabs[n%this.room_wall_prefabs.Length];
				GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

				position.x = (-((float)this.room_columns_num/2.0f - 0.5f) + (float)x)*this.room_column_size + this.room_column_size/2.0f;
				position.y = 0.0f;
				position.z = (-((float)room_wall_rows_num/2.0f - 0.5f) - this.block_grid_rows_num/2.0f + (float)z)*BLOCK_SIZE;

				go.transform.position = position;
				go.transform.parent   = room_walls.transform;

				int		rz = z/this.block_grid_rows_num;

				RoomController room = get_room_root_go(new Map.RoomIndex(x, rz));
				if (room != null) {
					room.RegisterRoomWall(Map.EWSN.EAST, go);
				}
				room = get_room_root_go(new Map.RoomIndex(x + 1, rz));
				if (room != null) {
					room.RegisterRoomWall(Map.EWSN.WEST, go);
				}
			}
		}

		room_walls.transform.parent = this.floor_root_go.transform;
	}

	// ---------------------------------------------------------------- //
	// 바닥을 만든다.

	// rx와 rz로 지정된 방의 바닥을 깐다.
	public RoomController		createRoomFloor(Map.RoomIndex room_index)
	{
		Vector3		room_center = Vector3.zero;
		Vector3		position    = Vector3.zero;
		int			n = 0;
		
		//
		
		RoomController	room = this.get_room_root_go(room_index);
		
		room_center = this.getRoomCenterPosition(room_index);
		
		room.transform.position = room_center;
		
		// 플로어.
		GameObject	floors = new GameObject("floors");
		
		floors.transform.position = room_center;
		
		for(int z = 0;z < this.block_grid_rows_num;z++) {
			
			for(int x = 0;x < this.block_grid_columns_num;x++) {

				GameObject	prefab = this.floor_prefabs[n%this.floor_prefabs.Length];
				GameObject	go     = GameObject.Instantiate(prefab) as GameObject;
				
				go.isStatic = true;
				
				position = this.getBlockCenterPosition(new Map.BlockIndex(x, z));
				position += room_center;

				go.transform.position = position;
				go.transform.parent = floors.transform;

				n++;
			}
		}
		
		floors.transform.parent = room.transform;

		return room.GetComponent<RoomController>();
	}

	// ---------------------------------------------------------------- //
	// 플로어 외벽을 만든다.

	public GameObject		createOuterWalls()
	{
		Vector3		position    = Vector3.zero;

		// 플로어 남쪽에 더미인 방을 1열 만들기에 그만큼 외벽도 크게 한다.

		GameObject	outer_walls = new GameObject("outer walls");

		int		outer_wall_columns_num = this.room_columns_num*this.block_grid_columns_num;
		int		outer_wall_rows_num    = (this.room_rows_num + 1)*this.block_grid_rows_num;

		// ---------------------------------------------------------------- //

		// 상.
		for(int x = 0;x < outer_wall_columns_num - 2;x++) {

			GameObject	prefab = this.outer_wall_prefabs[0];
			GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

			position.x = (-((float)outer_wall_columns_num/2.0f - 0.5f) + (float)(x + 1))*BLOCK_SIZE;
			position.y = 0.0f;
			position.z = ((float)outer_wall_rows_num/2.0f - this.block_grid_rows_num/2.0f + 0.5f)*BLOCK_SIZE;

			go.transform.position = position;
			go.transform.localRotation = Quaternion.AngleAxis( 90.0f, Vector3.up);
			go.transform.parent   = outer_walls.transform;
		}

		// 하.
		for(int x = 0;x < outer_wall_columns_num - 2;x++) {

			GameObject	prefab = this.outer_wall_prefabs[0];
			GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

			position.x = (-((float)outer_wall_columns_num/2.0f - 0.5f) + (float)(x + 1))*BLOCK_SIZE;
			position.y = 0.0f;
			position.z = -((float)outer_wall_rows_num/2.0f + this.block_grid_rows_num/2.0f + 0.5f)*BLOCK_SIZE;

			go.transform.position = position;
			go.transform.localRotation = Quaternion.AngleAxis(-90.0f, Vector3.up);
			go.transform.parent   = outer_walls.transform;
		}

		// 우.
		for(int z = 0;z < outer_wall_rows_num - 2;z++) {

			GameObject	prefab = this.outer_wall_prefabs[0];
			GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

			position.x =  ((float)outer_wall_columns_num/2.0f + 0.5f)*BLOCK_SIZE;
			position.y = 0.0f;
			position.z = (-((float)outer_wall_rows_num/2.0f + this.block_grid_rows_num/2.0f - 0.5f) + (float)(z + 1))*BLOCK_SIZE;

			go.transform.position = position;
			go.transform.localRotation = Quaternion.AngleAxis( 180.0f, Vector3.up);
			go.transform.parent   = outer_walls.transform;
		}

		// 좌.
		for(int z = 0;z < outer_wall_rows_num - 2;z++) {

			GameObject	prefab = this.outer_wall_prefabs[0];
			GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

			position.x = (-((float)outer_wall_columns_num/2.0f + 0.5f))*BLOCK_SIZE;
			position.y = 0.0f;
			position.z = (-((float)outer_wall_rows_num/2.0f + this.block_grid_rows_num/2.0f - 0.5f) + (float)(z + 1))*BLOCK_SIZE;

			go.transform.position = position;
			go.transform.localRotation = Quaternion.AngleAxis( 0.0f, Vector3.up);
			go.transform.parent   = outer_walls.transform;
		}

		// ---------------------------------------------------------------- //
		// 코너.

		// 우상.
		{
			GameObject	prefab = this.outer_wall_prefabs[1];
			GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

			position.x = ((float)outer_wall_columns_num/2.0f + 0.5f)*BLOCK_SIZE;
			position.y = 0.0f;
			position.z = ((float)outer_wall_rows_num/2.0f - this.block_grid_rows_num/2.0f + 0.5f)*BLOCK_SIZE;

			go.transform.position = position;
			go.transform.localRotation = Quaternion.AngleAxis(-90.0f, Vector3.up);
			go.transform.parent   = outer_walls.transform;

		}
		// 좌상.
		{
			GameObject	prefab = this.outer_wall_prefabs[1];
			GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

			position.x = -((float)outer_wall_columns_num/2.0f + 0.5f)*BLOCK_SIZE;
			position.y = 0.0f;
			position.z =  ((float)outer_wall_rows_num/2.0f - this.block_grid_rows_num/2.0f + 0.5f)*BLOCK_SIZE;

			go.transform.position = position;
			go.transform.localRotation = Quaternion.AngleAxis(180.0f, Vector3.up);
			go.transform.parent   = outer_walls.transform;

		}
		// 우하.
		{
			GameObject	prefab = this.outer_wall_prefabs[1];
			GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

			position.x =  ((float)outer_wall_columns_num/2.0f + 0.5f)*BLOCK_SIZE;
			position.y = 0.0f;
			position.z = -((float)outer_wall_rows_num/2.0f + this.block_grid_rows_num/2.0f + 0.5f)*BLOCK_SIZE;

			go.transform.position = position;
			go.transform.localRotation = Quaternion.AngleAxis( 0.0f, Vector3.up);
			go.transform.parent   = outer_walls.transform;

		}
		// 좌하.
		{
			GameObject	prefab = this.outer_wall_prefabs[1];
			GameObject	go     = GameObject.Instantiate(prefab) as GameObject;

			position.x = -((float)outer_wall_columns_num/2.0f + 0.5f)*BLOCK_SIZE;
			position.y = 0.0f;
			position.z = -((float)outer_wall_rows_num/2.0f + this.block_grid_rows_num/2.0f + 0.5f)*BLOCK_SIZE;

			go.transform.position = position;
			go.transform.localRotation = Quaternion.AngleAxis(90.0f, Vector3.up);
			go.transform.parent   = outer_walls.transform;

		}

		outer_walls.transform.parent = this.floor_root_go.transform;

		return(outer_walls);
	}

	// 플로어 이동 문을 만든다.
	public void		createFloorDoor()
	{
		do {

			if (levelGenerator == null) {
				Debug.LogError("levelGenerator hasn't been initialized.");
				break;
			}

			Map.RoomIndex	room_index  = this.goal.room_index;
			Map.BlockIndex	block_index = this.goal.block_index;
	
			Map.EWSN	door_dir = Map.EWSN.NONE;
	
			if (block_index.x == 0) {
				door_dir = Map.EWSN.WEST;
			}
			else if (block_index.x == (BLOCK_GRID_COLUMNS_NUM - 1)) {
				door_dir = Map.EWSN.EAST;
			}
	
			if (block_index.z == 0) {
				door_dir = Map.EWSN.NORTH;
			}
			else if (block_index.z == (BLOCK_GRID_ROWS_NUM - 1)) {
				door_dir = Map.EWSN.SOUTH;
			}

			if(door_dir == Map.EWSN.NONE) {

				Debug.LogError("illegal floor door position.");
				break;
			}

			this.createFloorDoor(room_index, block_index, door_dir);

		} while(false);
	}

	// 플로어 이동 문을 만든다.
	public void		createFloorDoor(Map.RoomIndex room_index, Map.BlockIndex block_index, Map.EWSN door_dir)
	{
		bossDoor = this.create_door(DoorControl.TYPE.FLOOR, room_index, block_index, door_dir, 4);
	}

	// 방 문의 열쇠(모든 방)와 플로어 문의 열쇠를 만든다.
	public void		generateItems(string account_name)
	{
		// 방 문의 열쇠를 만든다(1룸분, 플로어 열쇠도 포함).

		if(levelGenerator != null) {

			foreach(var ri in Map.RoomIndex.getRange(ROOM_COLUMNS_NUM, ROOM_ROWS_NUM)) {

				this.generateItemsInRoom(ri.x, ri.z, account_name);
			}
		}
	}

	// 문 열쇠를 만든다(방 하나분량, 방 키, 플로어 키).
	private void generateItemsInRoom(int rx, int rz, string account_name)
	{
		ItemManager itemMgr = ItemManager.getInstance();
		if (levelGenerator == null) {
			Debug.LogError("levelGenerator isn't initialized.");
			return;
		}

		Map.RoomIndex	ri = new Map.RoomIndex(rx, rz);

		RoomController	room = get_room_root_go(ri).GetComponent<RoomController>();

		BlockInfo[,]	block_info_room = this.block_infos[ri.x, ri.z];

		foreach(var bi in Map.BlockIndex.getRange(this.block_grid_columns_num, this.block_grid_rows_num)) {

			if(block_info_room[bi.x, bi.z].chip != Map.CHIP.KEY) {

				continue;
			}

			int		key_type = block_info_room[bi.x, bi.z].option0;

			if(key_type == -1) {

				continue;
			}

			// 열쇠 아이템의 인스턴스를 만든다.

			string	type_name = Item.Key.getTypeName((Item.KEY_COLOR)key_type);
			string	item_name = Item.Key.getInstanceName((Item.KEY_COLOR)key_type, ri);

			itemMgr.createItem(type_name, item_name, account_name);

			// 위치를 구한다.

			Vector3 initialPosition = this.getRoomCenterPosition(ri) + this.getBlockCenterPosition(bi);

			// 중심에서 조금 이동한다.
			initialPosition.x += BLOCK_SIZE/4;
			initialPosition.z += BLOCK_SIZE/4;

			itemMgr.setPositionToItem(item_name, initialPosition);

			room.RegisterKey(item_name, key_type);
		}
	}

	// ================================================================ //

	// 열쇠, 문 등을 설치할 장소(블록)을 확보한다.
	public List<Map.BlockIndex>		allocateChipPlacesRoom(Map.RoomIndex room_index, Map.CHIP chip, int count)
	{
		List<Map.BlockIndex>	places = new List<Map.BlockIndex>();

		do {

			if(this.block_infos == null) {

				break;
			}
			BlockInfo[,]	info_room = this.block_infos[room_index.x, room_index.z];
	
			// 비어있는 장소를 센다.
	
			int		vacant_count = 0;
	
			foreach(var bi in Map.BlockIndex.getRange(this.block_grid_columns_num, this.block_grid_rows_num)) {
	
				if(info_room[bi.x, bi.z].chip != Map.CHIP.VACANT) {
	
					continue;
				}
				vacant_count++;
			}

			count = Mathf.Min(count, vacant_count);

			if(count <= 0) {

				break;
			}

			// 0 ～ vacant_count까지의 난수를 count개, 중복되지 않게 선택한다.
	
			List<int>	candidates = new List<int>();
	
			for(int i = 0;i < count;i++) {
	
				int		new_candidate = this.random_plant.getRandomInt(vacant_count - i);
	
				foreach(var candidate in candidates) {
	
					if(new_candidate >= candidate) {
	
						new_candidate++;
					}
				}
				candidates.Add(new_candidate);
			}

			candidates.Sort();

			// candidates[]번째 요소를 places에 복사.

			vacant_count = 0;
	
			foreach(var bi in Map.BlockIndex.getRange(this.block_grid_columns_num, this.block_grid_rows_num)) {
	
				if(info_room[bi.x, bi.z].chip != Map.CHIP.VACANT) {
	
					continue;
				}

				if(vacant_count == candidates[0]) {

					info_room[bi.x, bi.z].chip = chip;

					places.Add(bi);
					candidates.RemoveAt(0);

					if(candidates.Count == 0) {

						break;
					}
				}
				vacant_count++;
			}

		} while(false);

		return(places);
	}

	// 특정 장소 한 곳에 칩을 둔다.
	public bool		allocateChipPlaceRoom(Map.RoomIndex room_index, Map.BlockIndex block_index, Map.CHIP chip)
	{
		bool			ret = false;
		BlockInfo[,]	info_room = this.block_infos[room_index.x, room_index.z];

		do {

			if(block_index.x < 0 || this.block_grid_columns_num <= block_index.x) {

				break;
			}
			if(block_index.z < 0 || this.block_grid_rows_num <= block_index.z) {

				break;
			}

			if(info_room[block_index.x, block_index.z].chip != Map.CHIP.VACANT) {

				break;
			}

			info_room[block_index.x, block_index.z].chip = chip;
			ret = true;

		} while(false);

		return(ret);
	}

	// 특정 장소에서 칩을 제거한다.
	public void		putbackChipPlaceRoom(Map.RoomIndex room_index, Map.BlockIndex block_index)
	{
		BlockInfo[,]	info_room = this.block_infos[room_index.x, room_index.z];

		do {

			if(block_index.x < 0 || this.block_grid_columns_num <= block_index.x) {

				break;
			}
			if(block_index.z < 0 || this.block_grid_rows_num <= block_index.z) {

				break;
			}

			info_room[block_index.x, block_index.z].chip = Map.CHIP.VACANT;

		} while(false);
	}

	// 특정 방의, 적 제네레이터(Lair, SpawnPoint)을 설치할 블록을 구한다.
	public List<Map.BlockIndex>	getLairPlacesRoom(Map.RoomIndex room_index)
	{
		List<Map.BlockIndex>		places = new List<Map.BlockIndex>();

		BlockInfo[,]	block_info_map = this.block_infos[room_index.x, room_index.z];

		foreach(var bi in Map.BlockIndex.getRange(this.block_grid_columns_num, this.block_grid_rows_num)) {

			BlockInfo	info = block_info_map[bi.x, bi.z];

			if(info.chip != Map.CHIP.LAIR) {

				continue;
			}

			places.Add(bi);
		}

		return(places);
	}

	// 플로어 시작 지점을 가져온다.
	public Vector3	getPlayerStartPosition()
	{
		Map.RoomIndex	ri = this.start.room_index;
		Map.BlockIndex	bi = this.start.block_index;

		Vector3		position = this.getRoomCenterPosition(ri) + this.getBlockCenterPosition(bi);

		return(position);
	}

	// 플로어 목표 지점을 가져온다.
	public Vector3	getBossDoorPosition()
	{
		Map.RoomIndex	ri = this.goal.room_index;
		Map.BlockIndex	bi = this.goal.block_index;

		Vector3		position = this.getRoomCenterPosition(ri) + this.getBlockCenterPosition(bi);

		return(position);
	}

	// 방을 가져온다.
	public RoomController	getRoom(Map.RoomIndex room_index)
	{
		return(this.get_room_root_go(room_index));
	}

	// 월드 좌표에서 방을 가져온다.
	public RoomController	getRoomFromPosition(Vector3 world_position)
	{
		Map.RoomIndex	ri = this.getRoomIndexFromPosition(world_position);

		RoomController	room = this.get_room_root_go(ri);

		if (room == null) {
			Debug.LogError("Invaild world position.");
		}

		return(room);
	}

	// 월드 좌표에서 RoomIndex를 구한다.
	public Map.RoomIndex	getRoomIndexFromPosition(Vector3 world_position)
	{
		Map.RoomIndex	ri;

		world_position.x -= (-(float)ROOM_COLUMNS_NUM/2.0f)*ROOM_COLUMN_SIZE;
		world_position.z -= (-(float)ROOM_ROWS_NUM/2.0f)*ROOM_ROW_SIZE;

		ri.x = Mathf.FloorToInt(world_position.x/ROOM_COLUMN_SIZE);
		ri.z = Mathf.FloorToInt(world_position.z/ROOM_ROW_SIZE);

		return(ri);
	}

	// 월드 좌표로부터 RoomIndex와 BlockIndex를 구한다.
	public void		getBlockIndexFromPosition(out Map.RoomIndex room_index, out Map.BlockIndex block_index, Vector3 world_position)
	{
		room_index = this.getRoomIndexFromPosition(world_position);

		Vector3		room_center = this.getRoomCenterPosition(room_index);

		world_position -= room_center;

		Vector3		room_size = Vector3.zero;

		room_size.x = BLOCK_SIZE*this.block_grid_columns_num;
		room_size.z = BLOCK_SIZE*this.block_grid_rows_num;

		world_position += room_size/2.0f;

		block_index.x = Mathf.FloorToInt(world_position.x/BLOCK_SIZE);
		block_index.z = Mathf.FloorToInt(world_position.z/BLOCK_SIZE);
	}

	// ================================================================ //

	// 방의 중심 좌표를 구한다.
	public Vector3 getRoomCenterPosition(Map.RoomIndex room_index)
	{
		Vector3		room_center = Vector3.zero;

		room_center.x = (-((float)this.room_columns_num/2.0f - 0.5f) + (float)room_index.x)*this.room_column_size;
		room_center.y = 0.0f;
		room_center.z = (-((float)this.room_rows_num/2.0f    - 0.5f) + (float)room_index.z)*this.room_row_size;

		return(room_center);
	}

	// 방의 XZ좌표 사각형을 구한다.
	public Rect		getRoomRect(Map.RoomIndex room_index)
	{
		Vector3	center = this.getRoomCenterPosition(room_index);

		float	w = this.getRoomColumnSize();
		float	h = this.getRoomRowSize();

		Rect	rect = new Rect(center.x - w/2.0f, center.z - h/2.0f, w, h);

		return(rect);
	}

	// 블록(플로어 그리드)의 중심 좌표를 구한다.
	public Vector3	getBlockCenterPosition(Map.BlockIndex block_index)
	{
		Vector3		block_center = Vector3.zero;
		
		block_center.x = (-(float)this.block_grid_columns_num/2.0f + 0.5f + (float)block_index.x)*BLOCK_SIZE;
		block_center.y = 0.0f;
		block_center.z = (-(float)this.block_grid_rows_num/2.0f    + 0.5f + (float)block_index.z)*BLOCK_SIZE;
		
		return(block_center);
	}

	// 플로어의 높이를 구한다.
	public float	getFloorHeight()
	{
		return(0.0f);
	}

	// ================================================================ //
	// 방 관련.

	public void SetCurrentRoom(RoomController newCurrentRoom)
	{
		//Debug.Log ("Party " + currentRoom + " ---> " + newCurrentRoom);
		if (currentRoom != null) {
			currentRoom.NotifyPartyLeave();
		}
		currentRoom = newCurrentRoom;
		currentRoom.NotifyPartyEnter();
	}
	
	public RoomController	GetCurrentRoom()
	{
		return this.currentRoom;
	}

	public RoomController FindRoomByDoor(DoorControl door)
	{
		return door.GetRoom();
	}

	public void		UnlockBossDoor()
	{
		if (bossDoor != null) {
			bossDoor.Unlock();
			bossDoor.beginWaitEnter();
		}
		else {
			//Debug.LogError("The current level doesn't have the boss door, but someone would like to unlock it.");
		}
	}
	
	// ================================================================ //

	protected void	create_debug_window()
	{
		var		window = dbwin.root().createWindow("map");

		window.createButton("door")
			.setOnPress(() => 
			{
				var		current_room = PartyControl.get().getCurrentRoom();

				if(current_room != null) {

					for(int i = 0;i < (int)Map.EWSN.NUM;i++) {

						var		door = current_room.getDoor((Map.EWSN)i);

						if(door == null) {

							continue;
						}

						if(door.IsUnlocked()) {

							door.dbLock();
							door.connect_to.dbLock();

						} else {

							door.Unlock();
							door.connect_to.Unlock();
						}
					}
				}
			});

		window.createButton("점화")
			.setOnPress(() => 
			{
				RoomController		current_room = PartyControl.get().getCurrentRoom();

				current_room.igniteCandles();
			});

		window.createButton("소화")
			.setOnPress(() => 
			{
				RoomController		current_room = PartyControl.get().getCurrentRoom();

				current_room.puffOutCandles();
			});
	}

	// ================================================================ //
	// 인스턴스.

	private	static MapCreator	instance = null;

	public static MapCreator	getInstance()
	{
		if(MapCreator.instance == null) {

			MapCreator.instance = GameObject.Find("GameRoot").GetComponent<MapCreator>();
		}

		return(MapCreator.instance);
	}
	public static MapCreator	get()
	{
		return(MapCreator.getInstance());
	}
}
