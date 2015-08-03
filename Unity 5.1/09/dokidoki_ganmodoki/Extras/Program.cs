using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageGenerator
{
    class Program
    {

        static void Main(string[] args)
        {
            Example();
        }

        static void Example()
        {
            // 乱数の設定をコンストラクタで行います。
            FieldGenerator gen = new FieldGenerator(5);

            // フィールドの生成方法
            // フィールドのw, hは2以上
            // ブロックのw, hは5以上かつ奇数という制限があります
            int fieldHeight = 3;
            int fieldWidth = 3;
            int blockHeight = 5;
            int blockWidth = 5;
            gen.SetSize(fieldHeight, fieldWidth, blockHeight, blockWidth);

            // フィールドを生成します
            // 各要素がブロックである2次元配列を生成します
            // したがって、ブロック(i,j)にはreturn_value[i,j]でアクセスし、
            // ブロック(i,j)内の要素(u,v)にはreturn_value[i,j][u,v]でアクセスします
            var field = gen.CreateField();
            gen.DebugPrint(field);

            // 生成したフィールドの1つの要素が何を示すかはFieldGenerator.ChipTypeで調べます。
            if (FieldGenerator.ChipType.Wall == field[0, 0][0, 0])
                Console.WriteLine("壁です");
        }
    }
}
