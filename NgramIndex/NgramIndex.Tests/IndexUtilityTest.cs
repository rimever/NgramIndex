using System.Collections.Generic;
using NUnit.Framework;

namespace NgramIndex.Tests
{
    public class IndexUtilityTest
    {
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
    }
}