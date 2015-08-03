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

    /// <summary>
    /// ブロックの最小構成単位チップ(適当に名づけた)の種類
    /// </summary>
    public enum ChipType
    {
        /// <summary>
        /// 通行可能な床
        /// </summary>
        Floor,

        /// <summary>
        /// 通行不可能な壁
        /// </summary>
        Wall,

        /// <summary>
        /// 左扉の鍵
        /// </summary>
        LeftKey,

        /// <summary>
        /// 右扉の鍵
        /// </summary>
        RightKey,

        /// <summary>
        /// 上扉の鍵
        /// </summary>
        TopKey,

        /// <summary>
        /// 下扉の鍵
        /// </summary>
        BottomKey,

        /// <summary>
        /// 扉
        /// </summary>
        Door,
    }

    public FieldGenerator(int seed)
    {
        random = new Random(seed);
    }

    public void SetSize( int fieldHeight, int fieldWidth, int blockHeight, int blockWidth )
    {
        if (blockWidth < 5 || blockHeight < 5)
        {
            throw new ArgumentException("blockサイズは5位上");
        }

        if (blockWidth % 2 == 0 || blockHeight % 2 == 0)
        {
            throw new ArgumentException("blockサイズは奇数");
        }
        if (fieldHeight <= 1 || fieldWidth <= 1)
        {
            throw new ArgumentException( "fieldサイズは2以上" );
        }


        this.fieldWidth = fieldWidth;
        this.fieldHeight = fieldHeight;
        this.blockWidth = blockWidth;
        this.blockHeight = blockHeight;
    }

    /// <summary>
    /// doorを持つblockを作る。
    /// </summary>
    /// <param name="door">扉。最大4方向に設置できる。方向は和演算で指定する</param>
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

        // 外周を全て埋める
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

        // 奇数行列だけ埋める
        for (int h = 2; h < height-1; h+=2) {
            for (int w = 2; w < width; w+=2) {
                block[h, w] = ChipType.Wall;
            }
        }

        return block;
    }

    /// <summary>
    /// Blockを作る
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
                int randMax = 4;
                if (h == 2)
                    randMax = 3;
                var next = random.Next(randMax);
                if (next == 0) // 右方向
                    emptyBlock[h,w+1] = ChipType.Wall; 
                else if (next == 1) // 左方向
                    emptyBlock[h,w-1] = ChipType.Wall;
                else if (next == 2) // 下方向
                    emptyBlock[h+1,w] = ChipType.Wall;
                else if (next == 3) // 上方向
                    emptyBlock[h-1,w] = ChipType.Wall;
            }
        }
    }

    private void CreateDoor(ref ChipType[,] block, int door)
    {
        int hd = 0;
        int wd = 0;
        if (((block.GetLength(0)-2) / 2) % 2 == 1)
            hd = 1;
        if (((block.GetLength(1)-2) / 2) % 2 == 1)
            wd = 1;

        if ((door & Direction.Left) != 0)
        {
            block[block.GetLength(0)/2-hd, 0] = ChipType.Door;
            block[block.GetLength(0)/2-hd, 1] = ChipType.Floor;
        }

        if ((door & Direction.Right) != 0)
        {
            block[block.GetLength(0)/2-hd, block.GetLength(1)-1] = ChipType.Door;
            block[block.GetLength(0)/2-hd, block.GetLength(1)-2] = ChipType.Floor;
        }

        if ((door & Direction.Top) != 0)
        {
            block[0, block.GetLength(1)/2+wd] = ChipType.Door;
            block[1, block.GetLength(1)/2+wd] = ChipType.Floor;
        }

        if ((door & Direction.Bottom) != 0)
        {
            block[block.GetLength(0)-1, block.GetLength(1)/2+wd] = ChipType.Door;
            block[block.GetLength(0)-2, block.GetLength(1)/2+wd] = ChipType.Floor;
        }
    }

    private void CreateKey(ref ChipType[,] block, int door)
    {
        // 床の数カウント
        int nofFloor = 0;
        for (int h = 1; h < block.GetLength(0)-1; ++h)
        {
            for (int w = 1; w < block.GetLength(1)-1; ++w)
            {
                if (block[h, w] == ChipType.Floor)
                    ++nofFloor;
            }
        }

        // 鍵の設置
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
            throw new Exception("サイズをセットしてください");

        // スタートとゴールをランダム選択
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
        CreateWall(ref block);
        CreateDoor(ref block, door);

        // fieldサイズに圧縮
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
                CreateDoor(ref curBlock, blockDoor);
                CreateKey(ref curBlock, blockDoor);
                field[h, w] = curBlock;

            }
        }
        return field;
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
                default:
                    throw new Exception("そんなブロックないよ");
                }
            }
            Console.WriteLine(string.Empty);
        }
    }

    public void DebugPrint(ChipType[,][,] field)
    {
        for (int fh = 0; fh < this.fieldHeight; ++fh)
        {
            for (int bh = 0; bh < this.blockWidth; ++bh)
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
                        default:
                            throw new Exception("そんなブロックないよ");
                        }
                    }
                }
                Console.WriteLine(string.Empty);
            }
        }
    }
}
