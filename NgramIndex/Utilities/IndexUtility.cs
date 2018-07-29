using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace NgramIndex.Utilities
{
    /// <summary>
    /// インデックスを扱うユーティリティクラスです。
    /// </summary>
    public static class IndexUtility
    {
        /// <summary>
        /// 行数は１からカウントしていきます。
        /// </summary>
        public const int LineStartNumber = 1;

        /// <summary>
        /// 指定文字数のNgramで分割します。
        /// </summary>
        /// <param name="text">分割対象の文字列</param>
        /// <param name="ngram">Ngramの値</param>
        /// <returns></returns>
        public static IEnumerable<string> SplitNgram(string text, int ngram)
        {
            if (ngram <= 0)
            {
                throw new InvalidOperationException($"Ngramの値は1以上を指定してください。");
            }

            if (String.IsNullOrEmpty(text))
            {
                yield break;
            }

            if (text.Length < ngram)
            {
                yield return text;
                yield break;
            }

            for (int i = 0; i < text.Length - (ngram - 1); i++)
            {
                yield return text.Substring(i, ngram);
            }
        }

        /// <summary>
        /// CSVファイルよりインデックスデータを抽出します。
        /// </summary>
        /// <param name="csvFilePath">csvファイルのパス</param>
        /// <param name="ngram">Ngramの値</param>
        /// <param name="encoding">ファイルを処理するエンコーディング</param>
        /// <returns></returns>
        public static Dictionary<string, List<int>> GetIndexData(string csvFilePath, int ngram, Encoding encoding)
        {
            Dictionary<string, List<int>> indeces = new Dictionary<string, List<int>>();
            using (var reader = new CsvReader(new StreamReader(csvFilePath, encoding)))
            {
                int line = LineStartNumber;
                while (reader.Read())
                {
                    foreach (var cellData in reader.Context.Record)
                    {
                        var split = SplitNgram(cellData, ngram);
                        foreach (var item in split.Distinct())
                        {
                            if (!indeces.ContainsKey(item))
                            {
                                indeces.Add(item, new List<int>());
                            }

                            indeces[item].Add(line);
                        }
                    }

                    line++;
                }
            }

            return indeces;
        }

        /// <summary>
        /// インデックスデータを保存します。
        /// </summary>
        /// <remarks>
        /// CsvHelperは自動マッピングに強いため、マッピングなしの書き込みは弱い。そのため、ダブルクォートでラップして書き込む。
        /// </remarks>
        /// <param name="filePath">保存先のファイルパス</param>
        /// <param name="indexData">インデックスデータ</param>
        /// <param name="encoding">エンコーディング</param>
        public static void SaveIndexData(string filePath, Dictionary<string, List<int>> indexData, Encoding encoding)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, encoding))
            {
                foreach (var item in indexData)
                {
                    string line = GetSaveIndexDataLine(item);
                    writer.WriteLine(line);
                }

                writer.Flush();
            }
        }

        /// <summary>
        /// インデックス情報を保存する文字列情報を取得します。
        /// </summary>
        /// <remarks>
        /// 値そのままを保存すると肥大化してしまうので、間隔で保存します。
        /// 9000行,9001行,9007行であれば
        /// 9000,1,6とします。
        /// </remarks>
        /// <param name="keyValuePair">インデックスする単語と行番号のペア</param>
        /// <returns></returns>
        private static string GetSaveIndexDataLine(KeyValuePair<string, List<int>> keyValuePair)
        {
            List<int> storageInts = new List<int>();
            int before = keyValuePair.Value.First();
            storageInts.Add(before);
            foreach (var lineNumber in keyValuePair.Value.Skip(1))
            {
                storageInts.Add(lineNumber - before);
                before = lineNumber;
            }

            return @"""" + keyValuePair.Key + @"""," + string.Join(@",", storageInts);
        }

        /// <summary>
        /// インデックスデータを読み込んで、インデックスを取得します。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="encoding"></param>
        /// <param name="keywords">キーワードを含むインデックスだけを抽出します。</param>
        /// <returns></returns>
        public static Dictionary<string, List<int>> LoadIndexData(string filePath, Encoding encoding,
            List<string> keywords = null)
        {
            Dictionary<string, List<int>> indices = new Dictionary<string, List<int>>();
            using (var reader = new CsvReader(new StreamReader(filePath, encoding)))
            {
                while (reader.Read())
                {
                    var records = reader.Context.Record;
                    if (keywords != null)
                    {
                        if (keywords.All(keyword => !records[0].Contains(keyword)))
                        {
                            continue;
                        }
                    }

                    KeyValuePair<string, List<int>> keyValuePair = LoadIndexDataLine(records);
                    indices.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return indices;
        }

        /// <summary>
        /// インデックスの行データの値からインデックス情報を抽出します。
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        private static KeyValuePair<string, List<int>> LoadIndexDataLine(string[] records)
        {
            string indexKey = string.Empty;
            List<int> indexLines = new List<int>();
            foreach (var item in records.Select((value, index) => new {value, index}))
            {
                if (item.index == 0)
                {
                    indexKey = item.value;
                }
                else
                {
                    if (int.TryParse(item.value, out var convertValue))
                    {
                        int indexLineNumber = convertValue;
                        if (indexLines.Any())
                        {
                            indexLineNumber += indexLines.LastOrDefault();
                        }

                        indexLines.Add(indexLineNumber);
                    }
                    else
                    {
                        Console.WriteLine($"{item.value}は数値に変換できません。");
                    }
                }
            }

            KeyValuePair<string, List<int>> keyValuePair =
                new KeyValuePair<string, List<int>>(indexKey, indexLines);
            return keyValuePair;
        }

        /// <summary>
        /// キーワードを分割します。
        /// </summary>
        /// <param name="keyword">キーワード</param>
        /// <param name="ngram">Ngramの値</param>
        /// <remarks>
        /// 今回の仕様では渋谷だったら渋,谷
        /// 東京都だったら、東京,京都となっており、1文字、4文字以上の検索条件については想定されていません。こちらの方で判断して分割します。
        /// </remarks>
        /// <returns></returns>
        public static IEnumerable<string> SplitKeyword(string keyword, int ngram)
        {
            if (keyword.Length <= ngram)
            {
                for (int i = 0; i < keyword.Length; i++)
                {
                    yield return keyword.Substring(i, 1);
                }
            }
            else
            {
                for (int i = 0; i <= keyword.Length - ngram; i++)
                {
                    yield return keyword.Substring(i, ngram);
                }
            }
        }

        /// <summary>
        /// インデックスを用いて、キーワードにヒットする行を取得します。
        /// </summary>
        /// <param name="keyword">キーワード</param>
        /// <param name="indexData">インデックス情報</param>
        /// <returns></returns>
        public static IEnumerable<int> SearchKeywordLines(string keyword, Dictionary<string, List<int>> indexData)
        {
            foreach (var index in indexData)
            {
                if (index.Key.Contains(keyword))
                {
                    foreach (var lineNumber in index.Value)
                    {
                        yield return lineNumber;
                    }
                }
            }
        }
    }
}