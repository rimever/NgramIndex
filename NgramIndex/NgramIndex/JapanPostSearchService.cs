using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NgramIndex.Utilities;

namespace NgramIndex
{
    /// <summary>
    /// 郵便番号の検索を行うためのサービスです。
    /// </summary>
    public class JapanPostSearchService
    {
        /// <summary>
        /// Ngramの値です。
        /// </summary>
        private const int Ngram = 2;

        /// <summary>
        /// データソースとするURL
        /// </summary>
        private const string DataSourceUrl = "http://www.post.japanpost.jp/zipcode/dl/kogaki/zip/ken_all.zip";


        /// <summary>
        /// ここで用いるエンコーディングを指定します。
        /// </summary>
        private static readonly Encoding FileEncoding = Encoding.GetEncoding("Shift-JIS");

        /// <summary>
        /// レコードを検索します。
        /// </summary>
        /// <param name="keyword">検索キーワード</param>
        public IEnumerable<string> SearchRecord(string keyword)
        {
            string filePath = FileUtility.GetIndexFilePath();
            if (String.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("インデックスがありません。先にインデックスを作成してください。");
                yield break;
            }

            var indexData = IndexUtility.LoadIndexData(filePath, FileEncoding);
            var keywordSplit = IndexUtility.SplitKeyword(keyword, Ngram).ToList();
            var result = IndexUtility.SearchKeywordLines(keywordSplit.FirstOrDefault(), indexData);

            foreach (var item in keywordSplit.Skip(1))
            {
                var next = IndexUtility.SearchKeywordLines(item, indexData).ToList();
                result = result.Intersect(next);
            }

            var resultList = result.ToList();

            //TODO:パス管理が煩雑
            string csvFilePath = FileUtility.GetCsvFilePath(Path.GetFullPath("extract"));

            foreach (var text in FileUtility.GetFileLines(resultList, csvFilePath, FileEncoding))
            {
                yield return text;
            }
        }

        /// <summary>
        /// インデックスの作成を行う処理です。
        /// </summary>
        public void CreateIndex()
        {
            string zipFilePath = "download.zip";
            FileUtility.DownloadZipFile(zipFilePath, DataSourceUrl);

            string extractDirectory = Path.GetFullPath("extract");
            string csvFilePath = FileUtility.ExtractCsvFile(zipFilePath, extractDirectory);

            var indexData = IndexUtility.GetIndexData(csvFilePath, Ngram, FileEncoding);
            string indexFilePath = csvFilePath + FileUtility.IndexFileExtension;
            IndexUtility.SaveIndexData(indexFilePath, indexData, FileEncoding);
        }
    }
}