using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace NgramIndex
{
    /// <summary>
    /// インデックスを扱うユーティリティクラスです。
    /// </summary>
    public static class IndexUtility
    {
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
                int line = 1;
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
                    string line = @"""" + item.Key + @""",""" + string.Join(@""",""", item.Value) + @"""";
                    writer.WriteLine(line);
                }

                writer.Flush();
            }
        }
    }
}