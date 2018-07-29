using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NgramIndex.Utilities;

namespace NgramIndex.Services
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

        private readonly string _storageDirectoryPath;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public JapanPostSearchService(string storageDirectoryPath)
        {
            _storageDirectoryPath = storageDirectoryPath;
            Directory.CreateDirectory(storageDirectoryPath);
        }

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
            string filePath = FileUtility.GetIndexFilePath(_storageDirectoryPath);
            if (String.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("インデックスがありません。先にインデックスを作成してください。");
                yield break;
            }

            var keywordSplit = IndexUtility.SplitKeyword(keyword, Ngram).ToList();
            var indices = IndexUtility.LoadIndexData(filePath, FileEncoding, keywordSplit);
            var result = IndexUtility.SearchKeywordLines(keywordSplit.FirstOrDefault(), indices);

            foreach (var item in keywordSplit.Skip(1))
            {
                var next = IndexUtility.SearchKeywordLines(item, indices).ToList();
                result = result.Intersect(next);
            }

            var resultList = result.ToList();

            string csvFilePath = FileUtility.GetCsvFilePath(_storageDirectoryPath);

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
            string zipFilePath = Path.Combine(_storageDirectoryPath, "download.zip");
            FileUtility.DownloadZipFile(zipFilePath, DataSourceUrl);

            string extractDirectory = FileUtility.GetZipExtractDirectory(_storageDirectoryPath);
            string csvFilePath = FileUtility.ExtractCsvFile(zipFilePath, extractDirectory);

            var indexData = IndexUtility.GetIndexData(csvFilePath, Ngram, FileEncoding);
            string indexFilePath = csvFilePath + FileUtility.IndexFileExtension;
            IndexUtility.SaveIndexData(indexFilePath, indexData, FileEncoding);
        }
    }
}