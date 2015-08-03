using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Map {

	// 동서남북.
	public enum EWSN {

		NONE = -1,

		EAST = 0,		// 동(우).
		WEST,			// 서(좌).
		SOUTH,			// 남(후, 하).
		NORTH,			// 북(전, 상).

		NUM,
	};

	public struct eswn {

		public static EWSN	opposite(EWSN eswn)
		{
			switch(eswn) {

				case EWSN.EAST:		eswn = EWSN.WEST;	break;
				case EWSN.WEST:		eswn = EWSN.EAST;	break;
				case EWSN.SOUTH:	eswn = EWSN.NORTH;	break;
				case EWSN.NORTH:	eswn = EWSN.SOUTH;	break;
			}

			return(eswn);
		}

		// 이웃 방향  시계 방향.
		public static EWSN	next_cw(EWSN eswn)
		{
			switch(eswn) {

				case EWSN.EAST:		eswn = EWSN.SOUTH;	break;
				case EWSN.WEST:		eswn = EWSN.NORTH;	break;
				case EWSN.SOUTH:	eswn = EWSN.WEST;	break;
				case EWSN.NORTH:	eswn = EWSN.EAST;	break;
			}

			return(eswn);
		}

		// 이웃 방향 반시계 방향.
		public static EWSN	next_ccw(EWSN eswn)
		{
			switch(eswn) {

				case EWSN.EAST:		eswn = EWSN.NORTH;	break;
				case EWSN.WEST:		eswn = EWSN.SOUTH;	break;
				case EWSN.SOUTH:	eswn = EWSN.EAST;	break;
				case EWSN.NORTH:	eswn = EWSN.WEST;	break;
			}

			return(eswn);
		}
	}


	// 방 번호.
	public struct RoomIndex {

		public int		x;
		public int		z;

		public RoomIndex(int x, int z)
		{
			this.x = x;
			this.z = z;
		}

		public RoomIndex	get_next(int x, int z)
		{
			return(new RoomIndex(this.x + x, this.z + z));
		}

		public static IEnumerable<RoomIndex>	getRange(int x_max, int z_max)
		{
			for(int x = 0;x < x_max;x++) {

				for(int z = 0;z < z_max;z++) {

					yield return(new RoomIndex(x, z));
				}
			}
		}
		public override string	ToString()
		{
			return(this.x.ToString() + "," + this.z.ToString());
		}
	}

	// 블록(플로어 그리드) 번호.
	public struct BlockIndex {

		public int		x;
		public int		z;

		public BlockIndex(int x, int z)
		{
			this.x = x;
			this.z = z;
		}

		public BlockIndex	get_next(int x, int z)
		{
			return(new BlockIndex(this.x + x, this.z + z));
		}

		public BlockIndex	get_next(EWSN eswn)
		{
			BlockIndex	next = this;

			switch(eswn) {

				case EWSN.EAST:		next.x += 1;	break;
				case EWSN.WEST:		next.x -= 1;	break;
				case EWSN.SOUTH:	next.z -= 1;	break;
				case EWSN.NORTH:	next.z += 1;	break;
			}

			return(next);
		}

		public static IEnumerable<BlockIndex>	getRange(int x_max, int z_max)
		{
			for(int x = 0;x < x_max;x++) {

				for(int z = 0;z < z_max;z++) {

					yield return(new BlockIndex(x, z));
				}
			}
		}

		// 주위 4블록의 인덱스.
		public IEnumerable<BlockIndex>	getArounds4()
		{
			yield return(new BlockIndex(x - 1, z));
			yield return(new BlockIndex(x + 1, z));
			yield return(new BlockIndex(x,     z - 1));
			yield return(new BlockIndex(x,     z + 1));
		}

		// 주위 8블록의 인덱스.
		public IEnumerable<BlockIndex>	getArounds8()
		{
			foreach(var around4 in this.getArounds4()) {

				yield return(around4);
			}

			yield return(new BlockIndex(x - 1, z - 1));
			yield return(new BlockIndex(x + 1, z - 1));
			yield return(new BlockIndex(x - 1, z + 1));
			yield return(new BlockIndex(x + 1, z + 1));
		}
		public override string	ToString()
		{
			return(this.x.ToString() + "," + this.z.ToString());
		}

		public static bool isEqual(BlockIndex value0, BlockIndex value1)
		{
			return((value0.x == value1.x) && (value0.z == value1.z));
		}
		/*
		public static bool operator==(BlockIndex value0, BlockIndex value1)
		{
			return((value0.x == value1.x) && (value0.z == value1.z));
		}
		public static bool operator!=(BlockIndex value0, BlockIndex value1)
		{
			return(!(value0 == value1));
		}
		public override bool	Equals(object value0)
		{
			return(((BlockIndex)value0) == this);
		}*/
	}

	// 플로어 위에 둘 것.
	public enum CHIP {

		NONE = -1,

		VACANT = 0,		// 없다.
		STAIRS,			// 계단(시작, 목표).
		DOOR,			// 문.
		KEY,			// 열쇠.
		LAIR,			// 제네레이터.

		ANY,			// 기타.

		NUM,
	}
}

public class MapDef : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
