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
            Dictionary<string, List<int>> indexes = new Dictionary<string, List<int>>();
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
                            if (!indexes.ContainsKey(item))
                            {
                                indexes.Add(item, new List<int>());
                            }

                            indexes[item].Add(line);
                        }
                    }

                    line++;
                }
            }

            return indexes;
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
                    //TODO:直接行番号ではなく、行の間隔で保存してみたい。
                    string line = @"""" + item.Key + @""",""" + string.Join(@""",""", item.Value) + @"""";
                    writer.WriteLine(line);
                }

                writer.Flush();
            }
        }

        /// <summary>
        /// インデックスデータを読み込んで、インデックスを取得します。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Dictionary<string, List<int>> LoadIndexData(string filePath, Encoding encoding)
        {
            Dictionary<string, List<int>> indexes = new Dictionary<string, List<int>>();
            using (var reader = new CsvReader(new StreamReader(filePath, encoding)))
            {
                while (reader.Read())
                {
                    string indexKey = string.Empty;
                    List<int> indexLines = new List<int>();
                    foreach (var item in reader.Context.Record.Select((value, index) => new {value, index}))
                    {
                        if (item.index == 0)
                        {
                            indexKey = item.value;
                        }
                        else
                        {
                            if (int.TryParse(item.value, out var convertValue))
                            {
                                indexLines.Add(convertValue);
                            }
                            else
                            {
                                Console.WriteLine($"{item.value}は数値に変換できません。");
                            }
                        }
                    }

                    indexes.Add(indexKey, indexLines);
                }
            }

            return indexes;
        }

        /// <summary>
        /// キーワードを分割します。
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="ngram"></param>
        /// <remarks>
        /// 今回の仕様では渋谷だったら渋,谷
        /// 東京都だったら、東京,京都となっており、1文字、4文字以上の検索条件については想定されていませんので例外とします。
        /// </remarks>
        /// <returns></returns>
        public static IEnumerable<string> SplitKeyword(string keyword, int ngram)
        {
            switch (keyword.Length)
            {
                case 2:
                case 3:
                    for (int i = 0; i < 2; i++)
                    {
                        yield return keyword.Substring(i, keyword.Length - 1);
                    }

                    break;
                default:
                    throw new InvalidOperationException("検索キーワードは2文字か3文字で指定してください。");
            }
        }

        /// <summary>
        /// インデックスを用いて、キーワードにヒットする行を取得します。
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="indexData"></param>
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