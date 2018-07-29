using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using CsvHelper;
using NgramIndex.Services;
using NgramIndex.Utilities;

namespace NgramIndex.Benchmarks
{
    /// <summary>
    /// <see cref="JapanPostSearchService.CreateIndex"/>をベンチマークします。
    /// </summary>
    public class JapanPostServiceBenchmark
    {
        /// <summary>
        /// ルートディレクトリ
        /// </summary>
        public static string RootDirectory => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase
            , @"..\..");

        [GlobalSetup]
        public void Setup()
        {
            var searchService = new JapanPostSearchService(SearchIndexPath());
            searchService.CreateIndex();
        }

        /// <summary>
        /// <see cref="JapanPostSearchService.CreateIndex"/>をテストします。
        /// </summary>
        [Benchmark]
        public void CreateIndex()
        {
            var service = new JapanPostSearchService(Path.Combine(RootDirectory, "TestResult", "Services",
                "JapanPostSearchService", "CreateIndex"));
            service.CreateIndex();
        }

        /// <summary>
        /// 入力１をテストします。
        /// </summary>
        [Benchmark]
        public void SearchInputCase1()
        {
            var service = new JapanPostSearchService(SearchIndexPath());
            var records = service.SearchRecord("渋谷").ToList();
        }

        /// <summary>
        /// 入力２をテストします。
        /// </summary>
        [Benchmark]
        public void SearchInputCase2()
        {
            var service = new JapanPostSearchService(SearchIndexPath());
            var records = service.SearchRecord("東京都").ToList();
        }

        /// <summary>
        /// インデックス作成→検索１→検索２を通しで行った場合。
        /// </summary>
        [Benchmark]
        public void SearchAll()
        {
            var service = new JapanPostSearchService(Path.Combine(RootDirectory, "TestResult", "Services",
                "JapanPostSearchService", "SearchAll"));
            service.CreateIndex();
            var recordShibuya = service.SearchRecord("渋谷").ToList();
            var recordTokyoto = service.SearchRecord("東京都").ToList();
        }

        /// <summary>
        /// 比較用にインデックスなしで検索してみた場合の結果です。
        /// </summary>
        [Benchmark]
        public void SearchNoIndex()
        {
            var csvFilePath = FileUtility.GetCsvFilePath(SearchIndexPath());
            var records1 = SearchKeywordNoIndex(csvFilePath, "渋谷").ToList();
            var records2 = SearchKeywordNoIndex(csvFilePath, "東京都").ToList();
        }

        /// <summary>
        /// インデックスなしでの検索です。
        /// </summary>
        /// <param name="csvFilePath"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        private static IEnumerable<string> SearchKeywordNoIndex(string csvFilePath, string keyword)
        {
            var splitKeywords = IndexUtility.SplitKeyword(keyword, 2).ToList();
            using (var reader = new CsvReader(new StreamReader(csvFilePath, Encoding.GetEncoding("Shift-JIS"))))
            {
                while (reader.Read())
                {
                    var containCheckList = new bool[splitKeywords.Count()];
                    foreach (var cellData in reader.Context.Record)
                    {
                        foreach (var item in splitKeywords.Select((value, index) => new {value, index}))
                        {
                            if (cellData.Contains(item.value))
                            {
                                containCheckList[item.index] = true;
                            }
                        }
                    }

                    if (containCheckList.All(isContain => isContain))
                    {
                        string text = string.Join(",", reader.Context.Record);
                        yield return text;
                    }
                }
            }
        }

        /// <summary>
        /// 検索処理計測用のインデックスのパス
        /// </summary>
        /// <returns></returns>
        private static string SearchIndexPath()
        {
            return Path.Combine(RootDirectory, "TestResult", "Services", "JapanPostSearchService", "Search");
        }
    }
}