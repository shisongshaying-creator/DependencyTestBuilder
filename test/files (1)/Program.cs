using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CSharpLearner
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string learnedContext = "";
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== C# ソースコード学習・生成ツール ===\n");
            
            while (true)
            {
                Console.WriteLine("\n--- メニュー ---");
                Console.WriteLine("1. ソースファイル/フォルダを読み込んで学習");
                Console.WriteLine("2. 学習内容を表示");
                Console.WriteLine("3. コード生成の指示を出す");
                Console.WriteLine("4. 学習内容をクリア");
                Console.WriteLine("5. 終了");
                Console.Write("\n選択してください (1-5): ");
                
                string choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        await LearnFromSource();
                        break;
                    case "2":
                        DisplayLearnedContent();
                        break;
                    case "3":
                        await GenerateCode();
                        break;
                    case "4":
                        ClearLearning();
                        break;
                    case "5":
                        Console.WriteLine("終了します。");
                        return;
                    default:
                        Console.WriteLine("無効な選択です。");
                        break;
                }
            }
        }
        
        static async Task LearnFromSource()
        {
            Console.Write("\nファイルパスまたはフォルダパスを入力してください: ");
            string path = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("パスが入力されていません。");
                return;
            }
            
            try
            {
                StringBuilder sourceCode = new StringBuilder();
                
                if (File.Exists(path))
                {
                    // 単一ファイルの場合
                    if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        string content = File.ReadAllText(path);
                        sourceCode.AppendLine($"// ファイル: {Path.GetFileName(path)}");
                        sourceCode.AppendLine(content);
                        sourceCode.AppendLine("\n");
                    }
                    else
                    {
                        Console.WriteLine("C#ソースファイル (.cs) を指定してください。");
                        return;
                    }
                }
                else if (Directory.Exists(path))
                {
                    // フォルダの場合
                    var csFiles = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
                    
                    if (csFiles.Length == 0)
                    {
                        Console.WriteLine("指定されたフォルダにC#ファイルが見つかりませんでした。");
                        return;
                    }
                    
                    Console.WriteLine($"\n{csFiles.Length}個のC#ファイルを読み込んでいます...");
                    
                    foreach (var file in csFiles)
                    {
                        string content = File.ReadAllText(file);
                        sourceCode.AppendLine($"// ファイル: {Path.GetRelativePath(path, file)}");
                        sourceCode.AppendLine(content);
                        sourceCode.AppendLine("\n");
                    }
                }
                else
                {
                    Console.WriteLine("指定されたパスが見つかりません。");
                    return;
                }
                
                Console.WriteLine("学習中...");
                
                // Claude APIで学習
                await AnalyzeSourceCode(sourceCode.ToString());
                
                Console.WriteLine("✓ 学習が完了しました！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"エラー: {ex.Message}");
            }
        }
        
        static async Task AnalyzeSourceCode(string sourceCode)
        {
            string prompt = $@"以下のC#ソースコードを分析し、コーディングスタイル、パターン、構造、使用されている技術やライブラリについて学習してください。

{sourceCode}

このコードベースの特徴を簡潔にまとめてください。後でこの情報を使って、同じスタイルや構造でコードを生成します。";

            var response = await CallClaudeAPI(prompt);
            
            // 学習内容を保存
            learnedContext = $"=== 学習したソースコード ===\n{sourceCode}\n\n=== 分析結果 ===\n{response}";
        }
        
        static void DisplayLearnedContent()
        {
            if (string.IsNullOrEmpty(learnedContext))
            {
                Console.WriteLine("\nまだ学習が行われていません。");
                return;
            }
            
            Console.WriteLine("\n" + new string('=', 50));
            
            // 分析結果のみを表示（ソースコード全体は長いので省略）
            var lines = learnedContext.Split('\n');
            bool showingAnalysis = false;
            
            foreach (var line in lines)
            {
                if (line.Contains("=== 分析結果 ==="))
                {
                    showingAnalysis = true;
                }
                
                if (showingAnalysis)
                {
                    Console.WriteLine(line);
                }
            }
            
            Console.WriteLine(new string('=', 50));
        }
        
        static async Task GenerateCode()
        {
            if (string.IsNullOrEmpty(learnedContext))
            {
                Console.WriteLine("\nまず学習を行ってください（メニュー1を選択）。");
                return;
            }
            
            Console.WriteLine("\n生成したいコードの指示を入力してください:");
            Console.WriteLine("（例: ユーザー管理クラスを作成して、CRUD操作のメソッドを含めてください）");
            Console.Write("\n指示: ");
            
            string instruction = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(instruction))
            {
                Console.WriteLine("指示が入力されていません。");
                return;
            }
            
            Console.WriteLine("\nコードを生成中...");
            
            string prompt = $@"以前分析したC#コードベースの情報:

{learnedContext}

上記のコードベースのスタイル、パターン、構造に従って、以下の指示に基づいてC#コードを生成してください:

指示: {instruction}

完全で実行可能なC#コードを生成してください。コメントも適切に含めてください。";

            var generatedCode = await CallClaudeAPI(prompt);
            
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("生成されたコード:");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine(generatedCode);
            Console.WriteLine(new string('=', 50));
            
            // ファイルに保存するか確認
            Console.Write("\nこのコードをファイルに保存しますか？ (y/n): ");
            string save = Console.ReadLine();
            
            if (save?.ToLower() == "y")
            {
                Console.Write("保存先のファイル名を入力してください (例: GeneratedCode.cs): ");
                string filename = Console.ReadLine();
                
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    // .cs拡張子がなければ追加
                    if (!filename.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        filename += ".cs";
                    }
                    
                    File.WriteAllText(filename, generatedCode);
                    Console.WriteLine($"✓ {filename} に保存しました。");
                }
            }
        }
        
        static void ClearLearning()
        {
            learnedContext = "";
            Console.WriteLine("\n学習内容をクリアしました。");
        }
        
        static async Task<string> CallClaudeAPI(string prompt)
        {
            var requestBody = new
            {
                model = "claude-sonnet-4-20250514",
                max_tokens = 4000,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };
            
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);
            var responseString = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API呼び出しエラー: {responseString}");
            }
            
            var jsonDoc = JsonDocument.Parse(responseString);
            var contentArray = jsonDoc.RootElement.GetProperty("content");
            
            if (contentArray.GetArrayLength() > 0)
            {
                return contentArray[0].GetProperty("text").GetString();
            }
            
            return "応答を取得できませんでした。";
        }
    }
}
