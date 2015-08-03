using System;
using System.Collections.Generic;

public class FieldGenerator
{
	private Random random;
	private int fieldWidth;
	private int fieldHeight;
	private int blockWidth;
	private int blockHeight;
	
	private class Direction
	{
		public const int Left = 1 << 0;
		public const int Right = 1 << 1;
		public const int Top = 1 << 2;
		public const int Bottom = 1 << 3;
	}
	
	public struct Position
	{
		public int fieldHeight;
		public int fieldWidth;
		public int blockHeight;
		public int blockWidth;
	}
	
	/// <summary>
	/// 블록의 최소 구성단위 칩(적당히 이름붙임)의 종류.
	/// </summary>
	public enum ChipType
	{
		/// <summary>
		/// 통행 가능한 플로어.
		/// </summary>
		Floor,
		
		/// <summary>
		/// 통행 불가능한 벽.
		/// </summary>
		Wall,
		
		/// <summary>
		/// 왼쪽 문 열쇠.
		/// </summary>
		LeftKey,
		
		/// <summary>
		/// 오른쪽 문 열쇠.
		/// </summary>
		RightKey,
		
		/// <summary>
		/// 윗문 열쇠.
		/// </summary>
		TopKey,
		
		/// <summary>
		/// 아랫문 열쇠.
		/// </summary>
		BottomKey,
		
		/// <summary>
		/// 문
		/// </summary>
		Door,
		
		/// <summary>
		/// 적의 생성 위치.
		/// </summary>
		Spawn,
		
		/// <summary>
		/// 보스 열쇠.
		/// </summary>
		BossKey,
	}
	
	public FieldGenerator(int seed)
	{
		random = new Random(seed);
	}
	
	public void SetSize( int fieldHeight, int fieldWidth, int blockHeight, int blockWidth )
	{
		if (blockWidth < 5 || blockHeight < 5)
		{
			throw new ArgumentException("block 크기는 5이상");
		}
		
		if (blockWidth % 2 == 0 || blockHeight % 2 == 0)
		{
			throw new ArgumentException("block 크기는 홀수");
		}
		if (fieldHeight <= 1 || fieldWidth <= 1)
		{
			throw new ArgumentException( "field 크기는 2이상" );
		} 
		
		
		this.fieldWidth = fieldWidth;
		this.fieldHeight = fieldHeight;
		this.blockWidth = blockWidth;
		this.blockHeight = blockHeight;
	}
	
	/// <summary>
	/// door를 가진 block을 만든다.
	/// </summary>
	/// <param name="door">문 최대 4방향으로 설치할 수 있다. 방향은 & 연산으로 지정한다</param>
	private void CreateWall(int door)
	{
		if ((door & Direction.Left) != 0)
			Console.WriteLine("left");
		if ((door & Direction.Right) != 0)
			Console.WriteLine("right");
		if ((door & Direction.Top) != 0)
			Console.WriteLine("top");
		if ((door & Direction.Bottom) != 0)
			Console.WriteLine("bottom");
		
	}
	
	private ChipType[,] CreateEmptyBlock(int door, int height, int width)
	{
		var block = new ChipType[height, width];
		
		// 바깥 둘레를 모두 메운다.
		for (int w = 0; w < width; ++w)
		{
			block[0, w] = ChipType.Wall;
			block[height-1,w] = ChipType.Wall;
		}
		for (int h = 1; h < height-1; ++h)
		{
			block[h, 0] = ChipType.Wall;
			block[h, width-1] = ChipType.Wall;
		}
		
		// 홀수 행렬만 메운다.
		// (４타일의 중심. 기둥이 놓인 곳).
		for (int h = 2; h < height-1; h+=2) {
			for (int w = 2; w < width; w+=2) {
				block[h, w] = ChipType.Wall;
			}
		}
		
		return block;
	}
	
	/// <summary>
	/// Block을 만든다.
	/// </summary>
	/// <param name="emptyBlock"></param>
	/// <returns></returns>
	private void CreateWall(ref ChipType[,] emptyBlock)
	{
		int height = emptyBlock.GetLength(0);
		int width = emptyBlock.GetLength(1);
		
		for (int h = 2; h < height-1; h+=2)
		{
			for (int w = 2; w < width-1; w+=2)
			{
				int randMax = 3;
				var next = random.Next(randMax);
				if (next == 0) // 오른쪽 방향
					emptyBlock[h,w+1] = ChipType.Wall; 
				else if (next == 1) // 왼쪽 방향
					emptyBlock[h,w-1] = ChipType.Wall;
				else if (next == 2) // 아래 방향
					emptyBlock[h+1,w] = ChipType.Wall;
			}
		}
	}
	
	private bool HasTargetDoor(int door, int direction)
	{
		if ((door & direction) == 0)
			return false;
		return true;
	}
	
	private void CreateDoor(ref ChipType[,] block, int door, bool is_room)
	{
		int hd = 0;
		int wd = 0;
		if (((block.GetLength(0)-2) / 2) % 2 == 1)
			hd = 1;
		if (((block.GetLength(1)-2) / 2) % 2 == 1)
			wd = 1;
		
		ChipType doorChip = ChipType.Door;
		
		if ((door & Direction.Left) != 0)
		{
			if(is_room) {

				block[block.GetLength(0)/2-hd, 0] = ChipType.Wall;
				block[block.GetLength(0)/2-hd, 1] = doorChip;

			} else {

				block[block.GetLength(0)/2-hd, 0] = doorChip;
				block[block.GetLength(0)/2-hd, 1] = ChipType.Floor;
			}
		}
		
		if ((door & Direction.Right) != 0)
		{
			if(is_room) {

				block[block.GetLength(0)/2-hd, block.GetLength(1)-1] = ChipType.Wall;
				block[block.GetLength(0)/2-hd, block.GetLength(1)-2] = doorChip;

			} else {

				block[block.GetLength(0)/2-hd, block.GetLength(1)-1] = doorChip;
				block[block.GetLength(0)/2-hd, block.GetLength(1)-2] = ChipType.Floor;
			}
		}
		
		if ((door & Direction.Top) != 0)
		{
			if(is_room) {

				block[0, block.GetLength(1)/2+wd] = ChipType.Wall;
				block[1, block.GetLength(1)/2+wd] = doorChip;

			} else {

				block[0, block.GetLength(1)/2+wd] = doorChip;
				block[1, block.GetLength(1)/2+wd] = ChipType.Floor;
			}
		}
		
		if ((door & Direction.Bottom) != 0)
		{
			if(is_room) {

				block[block.GetLength(0)-1, block.GetLength(1)/2+wd] = ChipType.Wall;
				block[block.GetLength(0)-2, block.GetLength(1)/2+wd] = doorChip;

			} else {

				block[block.GetLength(0)-1, block.GetLength(1)/2+wd] = doorChip;
				block[block.GetLength(0)-2, block.GetLength(1)/2+wd] = ChipType.Floor;
			}
		}
	}
	
	private void CreateKey(ref ChipType[,] block, int door)
	{
		// 플로어 수 카운트
		int nofFloor = 0;
		for (int h = 1; h < block.GetLength(0)-1; ++h)
		{
			for (int w = 1; w < block.GetLength(1)-1; ++w)
			{
				if (block[h, w] == ChipType.Floor)
					++nofFloor;
			}
		}
		
		// 열쇠 설치
		for (int i = 0; i < 4; ++i)
		{
			ChipType key;
			if (i == 0) {
				key = ChipType.RightKey;
				if ((door & Direction.Right) == 0)
					continue;
			}
			else if (i == 1)
			{
				key = ChipType.LeftKey;
				if ((door & Direction.Left) == 0)
					continue;
			}
			else if (i == 2)
			{
				key = ChipType.BottomKey;
				if ((door & Direction.Bottom) == 0)
					continue;
			}
			else
			{
				key = ChipType.TopKey;     
				if ((door & Direction.Top) == 0)
					continue;
			}
			
			double total = 0.0;
			double p = random.NextDouble();
			for (int h = 1; h < block.GetLength(0)-1; ++h)
			{
				for (int w = 1; w < block.GetLength(1)-1; ++w)
				{
					total += 1.0 / nofFloor;
					if (block[h,w] != ChipType.Floor || p > total)
						continue;
					
					block[h, w] = key;
					--nofFloor;
					h = block.GetLength(0);
					break;
				}
			}
		}
	}
	
	public ChipType[,][,] CreateField()
	{
		if (this.blockHeight == 0)
			throw new Exception("크기를 설정해 주세요");
		
		// start과 goal을 랜덤하게 선택
		int start = -1;
		int goal = -1;
		while (start == goal || goal < -1 || start < -1)
		{
			start = random.Next(4);
			goal = random.Next(4);
		}
		
		int door = 0;
		if (start == 0)
			door |= Direction.Top;
		else if (start == 1)
			door |= Direction.Bottom;
		else if (start == 2)
			door |= Direction.Left;
		else if (start == 3)
			door |= Direction.Right;
		
		if (goal == 0)
			door |= Direction.Top;
		else if (goal == 1)
			door |= Direction.Bottom;
		else if (goal == 2)
			door |= Direction.Left;
		else if (goal == 3)
			door |= Direction.Right;
		
		
		ChipType[,][,] field = new ChipType[this.fieldHeight, this.fieldWidth][,];
		
		var block = CreateEmptyBlock(door, fieldHeight * 2 + 1, fieldWidth * 2 + 1);
		
		// 필드 문 설치.
		CreateDoor(ref block, door, false);
		
		// 필드를 구분하는 벽 배치.
		CreateWall(ref block);
		
		//var doorInfo = new int[this.fieldHeight, this.fieldWidth];
		
		
		// 필드 구축.
		for (int h = 0; h < this.fieldHeight; ++h)
		{
			int blockH = 2 * h + 1;
			for (int w = 0; w < this.fieldWidth; ++w)
			{
				int blockW = 2 * w + 1;
				int blockDoor = 0;
				
				if (block[blockH+1, blockW] != ChipType.Wall)
					blockDoor |= Direction.Bottom;
				if (block[blockH-1, blockW] != ChipType.Wall)
					blockDoor |= Direction.Top;
				if (block[blockH, blockW+1] != ChipType.Wall)
					blockDoor |= Direction.Right;
				if (block[blockH, blockW-1] != ChipType.Wall)
					blockDoor |= Direction.Left;
				
				var curBlock = CreateEmptyBlock(blockDoor, this.blockHeight, this.blockWidth);
				field[h, w] = curBlock;
				CreateWall(ref curBlock);
				CreateDoor(ref curBlock, blockDoor, false);
				//CreateKey(ref curBlock, blockDoor);
			}
		}
		
		// 보스 열쇠 배치.
		//SetBossKey(field);
		
		return field;
	}
	
	private void SetBossKey(ChipType[,][,] field)
	{
		var ep = GetEndPoints(field);
		for (int i = 0; i < ep.Length; ++i)
		{
			var block = field[ep[i].fieldHeight, ep[i].fieldWidth]; // Endpoint가 있는 블록.
			
			// endpoint를 여는 열쇠 삭제.
			for (int bh = 0; bh < block.GetLength(0); ++bh)
			{
				for (int bw = 0; bw < block.GetLength(1); ++bw)
				{
					if ((ep[i].blockHeight == 0 && block[bh, bw] == ChipType.TopKey)
					    || (ep[i].blockHeight == block.GetLength(0)-1 && block[bh, bw] == ChipType.BottomKey)
					    || (ep[i].blockWidth == 0 && block[bh, bw] == ChipType.LeftKey)
					    || (ep[i].blockWidth == block.GetLength(1)-1 && block[bh, bw] == ChipType.RightKey))
					{
						block[bh, bw] = ChipType.Floor;
					}
				}
			}
		}
		
		var numFloor = 0;
		for (int fh = 0; fh < this.fieldHeight; ++fh)
		{
			for (int fw = 0; fw < this.fieldWidth; ++fw)
			{
				// 출입구에는 배치하지 않으므로 플로어 카운트를 하지 않는다.
				if ((ep[0].fieldHeight == fh && ep[0].fieldWidth == fw)
				    || (ep[1].fieldHeight == fh && ep[1].fieldWidth == fw))
					continue;
				
				for (int bh = 0; bh < this.blockHeight; ++bh)
					for (int bw = 0; bw < this.blockWidth; ++bw)
						if (field[fh, fw][bh, bw] == ChipType.Floor)
							++numFloor;               
			}
		}
		
		double r = random.NextDouble();
		double total = 0.0;
		
		for (int fh = 0; fh < this.fieldHeight; ++fh)
		{
			for (int fw = 0; fw < this.fieldWidth; ++fw)
			{
				// 출입구에는 배치하지 않으므로 지나감.
				if ((ep[0].fieldHeight == fh && ep[0].fieldWidth == fw)
				    || (ep[1].fieldHeight == fh && ep[1].fieldWidth == fw))
					continue;
				
				for (int bh = 0; bh < this.blockHeight; ++bh)
				{
					for (int bw = 0; bw < this.blockWidth; ++bw)
					{
						if (field[fh, fw][bh, bw] == ChipType.Floor)
						{
							total += 1.0/numFloor;
							if (r < total)
							{
								field[fh, fw][bh, bw] = ChipType.BossKey;
								fh = fw = bh = bw = int.MaxValue-1;
							}
						}
					}
				}
			}
		}
	}
	
	public void DebugPrint(ChipType[,] block)
	{
		for (int h = 0; h < block.GetLength(0); ++h)
		{
			for (int w = 0; w < block.GetLength(1); ++w)
			{
				switch (block[h, w]) 
				{
				case ChipType.Wall:
					Console.Write("@");
					break;
				case ChipType.Floor:
					Console.Write(" ");
					break;
				case ChipType.BottomKey:
					Console.Write("b");
					break;
				case ChipType.TopKey:
					Console.Write("t");
					break;
				case ChipType.RightKey:
					Console.Write("r");
					break;
				case ChipType.LeftKey:
					Console.Write("l");
					break;
				case ChipType.Door:
					Console.Write("d");
					break;
				case ChipType.BossKey:
					Console.Write("B");
					break;
				default:
					throw new Exception("그런 블록은 없어요");
				}
			}
			Console.WriteLine(string.Empty);
		}
	}
	
	/// <summary>
	/// 출구와 입구를 획득한다
	/// </summary>
	/// <remarks>출구와 입구에 차이는 없으니 편리한대로 사용하세요</remarks>
	/// <returns>출구와 입구의 장소</returns>
	public Position[] GetEndPoints(ChipType[,][,] field)
	{
		Position[] endpoints = new Position[2];
		endpoints[0].fieldHeight = -1;
		
		// 왼쪽 벽을 조사한다        
		for (int fh = 0; fh < this.fieldHeight; ++fh)
		{
			for (int bh = 0; bh < this.blockHeight; ++bh)
			{
				if (field[fh, 0][bh, 0] == ChipType.Door)
				{
					endpoints[0].fieldHeight = fh;
					endpoints[0].fieldWidth = 0;
					endpoints[0].blockHeight = bh;
					endpoints[0].blockWidth = 0;
					fh = int.MaxValue-1;
					break;
				}
			}
		}
		
		// 위쪽 벽을 조사한다.
		for (int fw = 0; fw < this.fieldWidth; ++fw)
		{
			for (int bw = 0; bw < this.blockWidth; ++bw)
			{
				if (field[0, fw][0, bw] == ChipType.Door)
				{
					int target = endpoints[0].fieldHeight == -1 ? 0 : 1;
					endpoints[target].fieldHeight = 0;
					endpoints[target].fieldWidth = fw;
					endpoints[target].blockHeight = 0;
					endpoints[target].blockWidth = bw;
					if (target == 1)
					{
						return endpoints;
					}
					fw = int.MaxValue-1;
					break;
				}
			}
		}
		
		// 오른쪽 벽을 조사한다.        
		for (int fh = 0; fh < this.fieldHeight; ++fh)
		{
			for (int bh = 0; bh < this.blockHeight; ++bh)
			{
				if (field[fh, this.fieldWidth-1][bh, this.blockWidth-1] == ChipType.Door)
				{
					int target = endpoints[0].fieldHeight == -1 ? 0 : 1;
					endpoints[target].fieldHeight = fh;
					endpoints[target].fieldWidth = this.fieldWidth-1;
					endpoints[target].blockHeight = bh;
					endpoints[target].blockWidth = this.blockWidth-1;
					if (target == 1)
					{
						return endpoints;
					}
					fh = int.MaxValue-1;
					break;
				}
			}
		}
		
		// 아래쪽 벽을 조사한다.
		for (int fw = 0; fw < this.fieldWidth; ++fw)
		{
			for (int bw = 0; bw < this.blockWidth; ++bw)
			{
				if (field[this.fieldHeight-1, fw][this.blockHeight-1, bw] == ChipType.Door)
				{
					int target = endpoints[0].fieldHeight == -1 ? 0 : 1;
					endpoints[target].fieldHeight = this.fieldHeight-1;
					endpoints[target].fieldWidth = fw;
					endpoints[target].blockHeight = this.blockHeight-1;
					endpoints[target].blockWidth = bw;
					if (target == 1)
					{
						return endpoints;
					}
					fw = int.MaxValue-1;
					break;
				}
			}
		}
		return endpoints;
	}
	
	/// <summary>
	/// 블록에 적의 발생 위치를 하나 랜덤하게 생성한다.
	/// </summary>
	/// <param name="block">대상 블록</param>
	public void CreateSpawnPointAtRandom(ChipType[,] block)
	{
		// 플로어 수를 카운트.
		int candidate = 0; // 스폰 포인트가 될 수 있는 플로어 수.
		for (int bh = 1; bh < block.GetLength(0); ++bh)
		{
			for (int bw = 1; bw < block.GetLength(1); ++bw)
			{
				if (block[bh, bw] == ChipType.Floor)
					++candidate;
			}
		}
		
		// 플로어를 랜덤 선택.
		double r = random.NextDouble();
		double total = 0.0;
		for (int bh = 1; bh < block.GetLength(0); ++bh)
		{
			for (int bw = 1; bw < block.GetLength(1); ++bw)
			{
				if (block[bh, bw] != ChipType.Floor)
					continue;
				total += 1.0/candidate;
				if (r < total)
				{
					block[bh, bw] = ChipType.Spawn;
					return;
				}
			}
		}
	}
	
	/// <summary>
	/// 지정 위치가 모퉁인지 조사한다.
	/// </summary>
	private bool IsCornner(ChipType[,] block, int h, int w)
	{
		// 모퉁이인지 조사한다.
		if (block[h-1, w] == ChipType.Wall)
			if (block[h, w-1] == ChipType.Wall || block[h, w+1] == ChipType.Wall)
				return true;
		
		if (block[h+1, w] == ChipType.Wall)
			if (block[h, w-1] == ChipType.Wall || block[h, w+1] == ChipType.Wall)
				return true;
		
		return false;
	}
	
	/// <summary>
	/// 블록 내의 모퉁이에 적의 발생 위치를 랜덤하게 생성한다.
	/// </summary>
	/// <param name="block">대상 블록</param>
	public void CreateSpawnPointAtCornnerAtRandom(ChipType[,] block)
	{
		// 플로어 수를 카운트.
		int candidate = 0; // 스폰 포인트가 될 수 있는 플로어 수.
		for (int bh = 1; bh < block.GetLength(0); ++bh)
		{
			for (int bw = 1; bw < block.GetLength(1); ++bw)
			{
				if (block[bh, bw] != ChipType.Floor || !IsCornner(block, bh, bw))
					continue;
				
				++candidate;
			}
		}
		
		if (candidate == 0)
			return;
		
		// 플로어를 랜덤 선택.
		double r = random.NextDouble();
		double total = 0.0;
		for (int bh = 1; bh < block.GetLength(0); ++bh)
		{
			for (int bw = 1; bw < block.GetLength(1); ++bw)
			{
				if (block[bh, bw] != ChipType.Floor || !IsCornner(block, bh, bw))
					continue;
				total += 1.0/candidate;
				if (r < total)
				{
					block[bh, bw] = ChipType.Spawn;
					return;
				}
			}
		}
	}
	
	/// <summary>
	/// 지정한 블록 간의 거리( )를 구한다.
	/// </summary>
	/// <param name="from_fh">블록 위치</param>
	/// <param name="from_fw">블록 위치</param>
	/// <param name="to_fh">블록 위치</param>
	/// <param name="to_fw">블록 위치</param>
	/// <returns>거리</returns>
	public int GetDistanceBetweenTwoBlock(ChipType[,][,] field, int from_fh, int from_fw, int to_fh, int to_fw)
	{
		// 일단 미로를 무시한 거리를 반환.
		// return Math.Abs(from_fh - to_fh) + Math.Abs(from_fw - to_fw);
		
		var h = field.GetLength(0);
		var w = field.GetLength(1);
		
		int[,] doorInfo = new int[h, w];
		for (int i = 0; i < h; ++i)
		{
			for (int j = 0; j < w; ++j)
			{
				var d = 0;
				var b = field[i, j];
				var bh = b.GetLength(0);
				var bw = b.GetLength(1);
				var bhc = (bh / 2) % 2 == 0 ? bh/2-1 : bh/2;
				var bwc = (bw / 2) % 2 == 0 ? bw/2+1 : bw/2;
				
				if (b[bhc, 0] == ChipType.Door)
					d |= Direction.Left;
				
				if (b[bhc, bw-1] == ChipType.Door)
					d |= Direction.Right;
				
				if (b[0, bwc] == ChipType.Door)
					d |= Direction.Top;
				
				if (b[bh-1, bwc] == ChipType.Door)
					d |= Direction.Bottom;
				
				doorInfo[i, j] = d;
			}
		}
		
		// DFS
		var distance = int.MaxValue;
		var closed = new bool[h, w];
		var open = new Queue<KeyValuePair<int, int>>();
		var d_buffer = new Queue<int>();
		open.Enqueue(new KeyValuePair<int,int>(from_fh, from_fw));
		d_buffer.Enqueue(0);
		
		Func<int, int, bool> RangeTest = (i, j) => {
			if (i < 0 || j < 0 || i >= h || j >= w)
			return false;
			return true;
		};
		
		while (open.Count != 0)
		{
			var pos = open.Dequeue();
			var i = pos.Key;
			var j = pos.Value;
			var d = d_buffer.Dequeue();
			
			if (closed[i, j])
				continue;
			
			if ((i == to_fh && j == to_fw) && distance > d)
			{
				distance = d;
				continue;
			}
			
			if (HasTargetDoor(doorInfo[i, j], Direction.Top) && RangeTest(i-1, j))
			{
				open.Enqueue(new KeyValuePair<int, int>(i-1, j));
				d_buffer.Enqueue(d+1);
			}
			
			if (HasTargetDoor(doorInfo[i, j], Direction.Bottom) && RangeTest(i+1, j))
			{
				open.Enqueue(new KeyValuePair<int, int>(i+1, j));
				d_buffer.Enqueue(d+1);
			}
			
			if (HasTargetDoor(doorInfo[i, j], Direction.Left) && RangeTest(i, j-1))
			{
				open.Enqueue(new KeyValuePair<int, int>(i, j-1));
				d_buffer.Enqueue(d+1);
			}
			
			if (HasTargetDoor(doorInfo[i, j], Direction.Right) && RangeTest(i, j+1))
			{
				open.Enqueue(new KeyValuePair<int, int>(i, j+1));
				d_buffer.Enqueue(d+1);
			}
			
			closed[i, j] = true;
		}
		
		return distance;
	}
	
	public void DebugPrint(ChipType[,][,] field)
	{
		for (int fh = 0; fh < this.fieldHeight; ++fh)
		{
			for (int bh = 0; bh < this.blockHeight; ++bh)
			{
				for (int fw = 0; fw < this.fieldWidth; ++fw)
				{
					for (int bw = 0; bw < this.blockWidth; ++bw)
					{
						var temp = field[fh, fw][bh, bw];
						switch (temp) 
						{
						case ChipType.Wall:
							Console.Write("@");
							break;
						case ChipType.Floor:
							Console.Write(" ");
							break;
						case ChipType.BottomKey:
							Console.Write("b");
							break;
						case ChipType.TopKey:
							Console.Write("t");
							break;
						case ChipType.RightKey:
							Console.Write("r");
							break;
						case ChipType.LeftKey:
							Console.Write("l");
							break;
						case ChipType.Door:
							Console.Write("d");
							break;
						case ChipType.Spawn:
							Console.Write("s");
							break;
						case ChipType.BossKey:
							Console.Write("B");
							break;
						default:
							throw new Exception("그런 블록은 없어요");
						}
					}
				}
				Console.WriteLine(string.Empty);
			}
		}
	}
}
