using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NgramIndex.Tests
{
    public class IndexUtilityTest
    {
        public static string RootDirectory => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase
            , @"..\..");

        /// <summary>
        /// <seealso cref="IndexUtility.SplitNgram"/>をテストします。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ngram"></param>
        /// <param name="expect"></param>
        [TestCaseSource(nameof(SplitNgramTestCaseDatas))]
        public void SplitNgram(string text, int ngram, List<string> expect)
        {
            var result = IndexUtility.SplitNgram(text, ngram);
            CollectionAssert.AreEqual(expect, result);
        }

        /// <summary>
        /// <seealso cref="SplitNgram"/>のテストケースです。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TestCaseData> SplitNgramTestCaseDatas()
        {
            yield return new TestCaseData("今日は雨", 2, new List<string> {"今日", "日は", "は雨"})
                .SetName($"Ngram=2");
            yield return new TestCaseData("今日は雨", 3, new List<string> {"今日は", "日は雨"})
                .SetName($"Ngram=3");
            yield return new TestCaseData("あ", 2, new List<string> {"あ"})
                .SetName($"Ngramの値より小さい長さの文字列の場合");
            yield return new TestCaseData(string.Empty, 2, new List<string>())
                .SetName($"空文字のとき");
        }

        /// <summary>
        /// インデックスデータの保存と読み込みを行い、正しく保存と読み込みが行えることを検証します。
        /// </summary>
        [Test]
        public void SaveAndLoadIndexData()
        {
            var expect = new Dictionary<string, List<int>>
            {
                {"0", new List<int> {1, 2, 4}},
                {"東京", new List<int> {2, 5, 9}},
                {"大阪", new List<int> {4, 8}}
            };
            string saveFilePath =
                Path.Combine(RootDirectory, "TestResult", "IndexUtility", "SaveIndexData", "test.idx");

            var encoding = Encoding.GetEncoding("Shift-JIS");
            IndexUtility.SaveIndexData(saveFilePath, expect, encoding);
            var actual = IndexUtility.LoadIndexData(saveFilePath, encoding);
            CollectionAssert.AreEquivalent(expect, actual);
        }

        /// <summary>
        /// <see cref="IndexUtility.SearchKeywordLines"/>を検証します。
        /// </summary>
        /// <param name="keyword">検索するキーワード</param>
        /// <param name="indexes">インデックス</param>
        /// <param name="expected">期待値</param>
        [TestCaseSource(nameof(SearchKeywordLinesTestCaseDatas))]
        public void SearchKeywordLines(string keyword, Dictionary<string, List<int>> indexes, List<int> expected)
        {
            var actual = IndexUtility.SearchKeywordLines(keyword, indexes).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// <see cref="SearchKeywordLines"/>のテストケースです。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TestCaseData> SearchKeywordLinesTestCaseDatas()
        {
            var index = new Dictionary<string, List<int>>
            {
                {"0", new List<int> {1, 2, 4}},
                {"東京", new List<int> {2, 5, 9}},
                {"大阪", new List<int> {4, 8}}
            };
            yield return new TestCaseData("東", index, new List<int> {2, 5, 9}).SetName("インデックスのNgramより小さい文字長さのとき");
            yield return new TestCaseData("東京", index, new List<int> {2, 5, 9}).SetName("通常の検索");
            yield return new TestCaseData("奈良", index, new List<int>()).SetName("検索結果がなかったとき");
        }

        /// <summary>
        /// <seealso cref="IndexUtility.SplitKeyword"/>をテストします。
        /// </summary>
        /// <returns></returns>
        [TestCaseSource(nameof(SplitKeywordTestCaseDatas))]
        public IList<string> SplitKeyword(string keyword, int ngram)
        {
            return IndexUtility.SplitKeyword(keyword, ngram).ToList();
        }

        /// <summary>
        /// <seealso cref="SplitKeyword"/>のテストケースです。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TestCaseData> SplitKeywordTestCaseDatas()
        {
            yield return new TestCaseData("渋谷", 2).Returns(new List<string> {"渋", "谷"})
                .SetName($"{nameof(SplitKeyword)} 2文字の時");
            yield return new TestCaseData("東京都", 2).Returns(new List<string> {"東京", "京都"})
                .SetName($"{nameof(SplitKeyword)} 3文字の時");
        }
    }
}