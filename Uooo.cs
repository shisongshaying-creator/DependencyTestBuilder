using System;
using System.IO;
using System.Text;

class Program
{
    // 使用する文字集合（小文字・大文字・数字・記号）
    private const string Charset =
        "abcdefghijklmnopqrstuvwxyz" +          // 小文字
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +          // 大文字
        "0123456789" +                          // 数字
        "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";   // 記号（バックスラッシュは \\ でエスケープ）

    static void Main(string[] args)
    {
        const int length = 5;                         // 文字数
        const string outputPath = "all_5chars.txt";   // 出力ファイル名

        using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
        {
            GenerateAllIterative(writer, length);
        }
    }

    /// <summary>
    /// 再帰を使わずに、全パターンを生成して1行ずつ書き込む
    /// </summary>
    private static void GenerateAllIterative(StreamWriter writer, int length)
    {
        int charsetLen = Charset.Length;

        // 各「桁」が何番目の文字を指しているかを表す配列
        // 例: [0,0,0,0,0] → 先頭5文字
        int[] indices = new int[length];
        char[] buffer = new char[length];

        while (true)
        {
            // 現在の indices に対応する文字列を作る
            for (int i = 0; i < length; i++)
            {
                buffer[i] = Charset[indices[i]];
            }

            // 1行書き込み
            writer.WriteLine(buffer);

            // 1 文字分カウントアップ（一番後ろの桁から）
            int pos = length - 1;
            while (pos >= 0)
            {
                indices[pos]++;

                // まだ範囲内ならそこで終了（桁上がりなし）
                if (indices[pos] < charsetLen)
                {
                    break;
                }

                // 桁あふれ → 0 に戻して一つ左の桁を繰り上げる
                indices[pos] = 0;
                pos--;
            }

            // 一番左の桁まで繰り上がっても溢れた → 全パターン終了
            if (pos < 0)
            {
                break;
            }
        }
    }
}
