using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace GrowingAI
{
    // ====== データ構造 ======
    public class MemoryModel
    {
        public int Conversations { get; set; } = 0;
        public List<Utterance> History { get; set; } = new();
        public Dictionary<string,int> Lexicon { get; set; } = new();                 // 語彙頻度
        public Dictionary<string,int> Bigrams { get; set; } = new();                 // バイグラム頻度（日本語は文字、英語は単語）
        public Dictionary<string,int> PositiveWords { get; set; } = new();           // ポジ語出現カウント
        public Dictionary<string,int> NegativeWords { get; set; } = new();           // ネガ語出現カウント
        public Dictionary<string,string> Facts { get; set; } = new();                // 例: user:name -> 太郎
        public List<Preference> Preferences { get; set; } = new();                   // 例: like:ラーメン
        public Dictionary<string,double> DocumentIdf { get; set; } = new();          // 手作りIDF
    }

    public class Utterance
    {
        public DateTime Timestamp { get; set; }
        public string Speaker { get; set; } = "user"; // "user" or "ai"
        public string Text { get; set; } = "";
        public Dictionary<string,double>? Tf { get; set; } // トークンTF
    }

    public class Preference
    {
        public string Type { get; set; } = "like"; // like / dislike
        public string Value { get; set; } = "";
        public int Strength { get; set; } = 1;     // 出現回数で強まる
    }

    // ====== メモリ入出力 ======
    public static class MemoryStore
    {
        private static readonly string PathFile = System.IO.Path.Combine(AppContext.BaseDirectory, "memory.json");
        public static MemoryModel Load()
        {
            try
            {
                if (File.Exists(PathFile))
                {
                    var json = File.ReadAllText(PathFile, Encoding.UTF8);
                    var m = JsonSerializer.Deserialize<MemoryModel>(json);
                    if (m != null) return m;
                }
            }
            catch {}
            return new MemoryModel();
        }

        public static void Save(MemoryModel mem)
        {
            var json = JsonSerializer.Serialize(mem, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PathFile, json, Encoding.UTF8);
        }
    }

    // ====== NLPユーティリティ ======
    public static class Nlp
    {
        static Regex NonWord = new(@"[^A-Za-z0-9]+", RegexOptions.Compiled);
        static Regex CjkRegex = new(@"[\p{IsCJKUnifiedIdeographs}\p{IsHiragana}\p{IsKatakana}ー]", RegexOptions.Compiled);

        public static List<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new();
            var hasCjk = CjkRegex.IsMatch(text);
            if (hasCjk)
            {
                // 日本語など：全角記号や空白を落として文字バイグラム
                var chars = text.Trim()
                    .Where(c => char.IsLetterOrDigit(c) || CjkRegex.IsMatch(c.ToString()))
                    .Select(c => c.ToString())
                    .ToList();
                var bigrams = new List<string>();
                for (int i = 0; i < chars.Count - 1; i++)
                    bigrams.Add(chars[i] + chars[i+1]);
                if (bigrams.Count == 0 && chars.Count > 0) bigrams.Add(chars[0]); // 単文字フォールバック
                return bigrams;
            }
            else
            {
                // 英数：単語ベース
                var lower = text.ToLowerInvariant();
                var tokens = NonWord.Split(lower).Where(t => t.Length > 0).ToList();
                return tokens;
            }
        }

        public static Dictionary<string,double> Tf(List<string> tokens)
        {
            var tf = new Dictionary<string,double>();
            foreach (var t in tokens)
            {
                tf.TryGetValue(t, out var c);
                tf[t] = c + 1;
            }
            // 正規化（L2）
            var norm = Math.Sqrt(tf.Values.Sum(v => v*v));
            if (norm > 0)
            {
                foreach (var k in tf.Keys.ToList())
                    tf[k] /= norm;
            }
            return tf;
        }

        public static double Cosine(Dictionary<string,double> a, Dictionary<string,double> b)
        {
            double s = 0;
            // 片方の小さい方を走査
            var (small, large) = a.Count < b.Count ? (a,b) : (b,a);
            foreach (var kv in small)
                if (large.TryGetValue(kv.Key, out var v)) s += kv.Value * v;
            return s;
        }
    }

    // ====== 成長（学習）部分 ======
    public static class Learner
    {
        static readonly string[] PosWords = { "うれしい","楽しい","最高","好き","good","great","love","awesome","やった","助かる" };
        static readonly string[] NegWords = { "悲しい","つらい","嫌い","最悪","bad","hate","angry","むかつく","困る" };

        public static void IngestUserUtterance(MemoryModel mem, string text)
        {
            var tokens = Nlp.Tokenize(text);
            // 語彙とビグラム更新
            foreach (var t in tokens)
            {
                mem.Lexicon[t] = mem.Lexicon.TryGetValue(t, out var c) ? c+1 : 1;
            }
            for (int i = 0; i < tokens.Count-1; i++)
            {
                var bi = tokens[i] + " " + tokens[i+1];
                mem.Bigrams[bi] = mem.Bigrams.TryGetValue(bi, out var c) ? c+1 : 1;
            }
            // ポジネガ
            if (PosWords.Any(w => text.Contains(w, StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var w in PosWords.Where(w => text.Contains(w, StringComparison.OrdinalIgnoreCase)))
                    mem.PositiveWords[w] = mem.PositiveWords.TryGetValue(w, out var c) ? c+1 : 1;
            }
            if (NegWords.Any(w => text.Contains(w, StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var w in NegWords.Where(w => text.Contains(w, StringComparison.OrdinalIgnoreCase)))
                    mem.NegativeWords[w] = mem.NegativeWords.TryGetValue(w, out var c) ? c+1 : 1;
            }
            // 事実抽出（超・素朴）
            ExtractFacts(mem, text);

            // IDF更新（簡易：書き込み毎に再計算）
            RebuildIdf(mem);
        }

        static void ExtractFacts(MemoryModel mem, string text)
        {
            // 好み
            var likeMatch = Regex.Match(text, @"(.+?)が好き|like\s+(.+)", RegexOptions.IgnoreCase);
            if (likeMatch.Success)
            {
                var v = likeMatch.Groups[1].Success ? likeMatch.Groups[1].Value.Trim() : likeMatch.Groups[2].Value.Trim();
                AddPref(mem, "like", v);
            }
            var dislikeMatch = Regex.Match(text, @"(.+?)が嫌い|dislike\s+(.+)", RegexOptions.IgnoreCase);
            if (dislikeMatch.Success)
            {
                var v = dislikeMatch.Groups[1].Success ? dislikeMatch.Groups[1].Value.Trim() : dislikeMatch.Groups[2].Value.Trim();
                AddPref(mem, "dislike", v);
            }
            // 名前
            var nameMatch = Regex.Match(text, @"(私|ぼく|俺|わたし|name)\s*(は|=)\s*([^\s、。！!]+)", RegexOptions.IgnoreCase);
            if (nameMatch.Success)
            {
                mem.Facts["user:name"] = nameMatch.Groups[3].Value.Trim();
            }
            // 年齢相当（数字があれば控えめに）
            var ageMatch = Regex.Match(text, @"(\d{2})\s*歳", RegexOptions.IgnoreCase);
            if (ageMatch.Success) mem.Facts["user:age"] = ageMatch.Groups[1].Value;
        }

        static void AddPref(MemoryModel mem, string type, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var found = mem.Preferences.FirstOrDefault(p => p.Type==type && p.Value==value);
            if (found == null) mem.Preferences.Add(new Preference{ Type=type, Value=value, Strength=1 });
            else found.Strength++;
        }

        static void RebuildIdf(MemoryModel mem)
        {
            // 厳密なコーパスIDFではなく、履歴から簡易IDF
            var df = new Dictionary<string,int>();
            foreach (var utt in mem.History.Where(h => h.Speaker=="user"))
            {
                var set = new HashSet<string>(Nlp.Tokenize(utt.Text));
                foreach (var t in set)
                    df[t] = df.TryGetValue(t, out var c) ? c+1 : 1;
            }
            int N = Math.Max(1, mem.History.Count(h => h.Speaker=="user"));
            var idf = new Dictionary<string,double>();
            foreach (var kv in df)
            {
                // 平滑化IDF
                idf[kv.Key] = Math.Log((N + 1.0) / (kv.Value + 1.0)) + 1.0;
            }
            mem.DocumentIdf = idf;
        }

        public static Dictionary<string,double> TfIdf(MemoryModel mem, Dictionary<string,double> tf)
        {
            var v = new Dictionary<string,double>();
            double sumSq = 0;
            foreach (var kv in tf)
            {
                var idf = mem.DocumentIdf.TryGetValue(kv.Key, out var d) ? d : 1.0;
                var w = kv.Value * idf;
                v[kv.Key] = w;
                sumSq += w*w;
            }
            // 正規化
            var norm = Math.Sqrt(sumSq);
            if (norm > 0)
            {
                foreach (var k in v.Keys.ToList()) v[k] /= norm;
            }
            return v;
        }
    }

    // ====== 応答生成 ======
    public static class Dialogue
    {
        public static string GenerateReply(MemoryModel mem, string userText)
        {
            var tokens = Nlp.Tokenize(userText);
            var tf = Nlp.Tf(tokens);
            var q = Learner.TfIdf(mem, tf);

            // 1) 類似発話探索（ユーザー過去発言に近い→そのときのAI返答を再利用・改変）
            var (bestSim, bestIdx) = FindMostSimilar(mem, q);
            string reuse = "";
            if (bestIdx >= 0)
            {
                var prevUser = mem.History[bestIdx];
                var prevAi = mem.History.Skip(bestIdx+1).FirstOrDefault(h => h.Speaker=="ai");
                if (prevAi != null) reuse = prevAi.Text;
            }

            // 2) 事実・好みの挿入
            var name = mem.Facts.TryGetValue("user:name", out var n) ? n : null;
            var likes = mem.Preferences.Where(p => p.Type=="like").OrderByDescending(p=>p.Strength).Take(3).Select(p=>p.Value).ToList();
            var dislikes = mem.Preferences.Where(p => p.Type=="dislike").OrderByDescending(p=>p.Strength).Take(2).Select(p=>p.Value).ToList();

            // 3) ポジ・ネガ傾向で相槌
            var mood = EstimateMood(mem);

            // 4) 生成
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(name))
                sb.Append($"[{name}さん向け] ");

            // 似たやりとりが十分あれば、その文脈を踏襲（ただしコピペはしない）
            if (bestSim > 0.55 && !string.IsNullOrEmpty(reuse))
            {
                sb.Append("前に似た話をしていましたね。");
                if (likes.Count>0) sb.Append($"あなたの好み（例: {string.Join(" / ", likes)})も踏まえて、");
                sb.Append("今回の話に合わせて考えると—");
                sb.Append(SoftParaphrase(reuse));
            }
            else
            {
                // 素朴な理解＆返答
                sb.Append(mood switch {
                    >0.3 => "前向きに受け止めました！ ",
                    <-0.3 => "大変でしたね。 ",
                    _ => ""
                });
                sb.Append("なるほど。少しずつわたしの“理解”に取り込みます。");
                if (likes.Count>0) sb.Append($"あなたは「{string.Join(" / ", likes)}」が好きなんですよね。");
                if (dislikes.Count>0) sb.Append($"反対に「{string.Join(" / ", dislikes)}」は避けたいと覚えておきます。");
                sb.Append(" よければ、もう少し詳しく教えてください。");
            }

            // 5) 自己メタ情報（学習の可視化）
            sb.Append(" （学習: 語彙=" + mem.Lexicon.Count + " / 事実=" + mem.Facts.Count + " / 好み=" + mem.Preferences.Count + "）");

            return sb.ToString();
        }

        static (double,int) FindMostSimilar(MemoryModel mem, Dictionary<string,double> q)
        {
            double best = -1; int idx = -1;
            for (int i = 0; i < mem.History.Count; i++)
            {
                var utt = mem.History[i];
                if (utt.Speaker != "user") continue;
                // 過去に保存したTF（なければ計算）
                var tf = utt.Tf ?? Nlp.Tf(Nlp.Tokenize(utt.Text));
                var v = Learner.TfIdf(mem, tf);
                var sim = Nlp.Cosine(q, v);
                if (sim > best) { best = sim; idx = i; }
            }
            return (best, idx);
        }

        static double EstimateMood(MemoryModel mem)
        {
            // ごく簡単に：ポジ/ネガ語の総量差を正規化
            double p = mem.PositiveWords.Values.Sum();
            double n = mem.NegativeWords.Values.Sum();
            if (p+n == 0) return 0;
            return (p - n) / (p + n);
        }

        static string SoftParaphrase(string s)
        {
            // 既存返答を軽く言い換え（装飾のみ）
            if (string.IsNullOrWhiteSpace(s)) return s;
            var tail = s.EndsWith("。") || s.EndsWith(".") ? "" : "。";
            return $"要するに「{s}」という理解です{tail}";
        }
    }

    // ====== エントリポイント ======
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var mem = MemoryStore.Load();
            Console.WriteLine("GrowingAI (no external libs). 終了: /quit");
            Console.WriteLine($"メモリ: {Path.Combine(AppContext.BaseDirectory, "memory.json")}");
            Console.WriteLine();

            while (true)
            {
                Console.Write("You> ");
                var input = Console.ReadLine();
                if (input == null) continue;
                if (input.Trim().Equals("/quit", StringComparison.OrdinalIgnoreCase)) break;

                // 学習
                Learner.IngestUserUtterance(mem, input);
                var userUtt = new Utterance {
                    Timestamp = DateTime.Now,
                    Speaker = "user",
                    Text = input,
                    Tf = Nlp.Tf(Nlp.Tokenize(input))
                };
                mem.History.Add(userUtt);

                // 応答
                var reply = Dialogue.GenerateReply(mem, input);
                Console.WriteLine("AI > " + reply);

                mem.History.Add(new Utterance {
                    Timestamp = DateTime.Now,
                    Speaker = "ai",
                    Text = reply
                });

                mem.Conversations++;
                MemoryStore.Save(mem);
            }

            MemoryStore.Save(mem);
            Console.WriteLine("bye.");
        }
    }
}
