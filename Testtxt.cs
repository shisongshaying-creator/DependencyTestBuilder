using System;
using System.IO;
using System.Text;

class Program
{
    // 使用する文字集合（小文字・大文字・数字・記号）
    // 必要に応じてこの文字列を書き換えてください
    private const string Charset =
        "abcdefghijklmnopqrstuvwxyz" +          // 小文字
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +          // 大文字
        "0123456789" +                          // 数字
        "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";   // 記号（バックスラッシュは \\ でエスケープ）

    static void Main(string[] args)
    {
        const int length = 5;                         // 文字数
        const string outputPath = "all_5chars.txt";   // 出力ファイル名

        // 上書きモード・UTF-8 でファイルを開く
        using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
        {
            char[] buffer = new char[length];
            GenerateAll(writer, buffer, 0);
        }
    }

    /// <summary>
    /// 全パターンを再帰的に生成して1行ずつ書き込む
    /// </summary>
    private static void GenerateAll(StreamWriter writer, char[] buffer, int index)
    {
        if (index == buffer.Length)
        {
            writer.WriteLine(buffer);
            return;
        }

        foreach (char c in Charset)
        {
            buffer[index] = c;
            GenerateAll(writer, buffer, index + 1);
        }
    }
}
