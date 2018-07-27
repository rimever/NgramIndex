using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;

namespace NgramIndex
{
    class Program
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
        /// プログラムのメイン処理
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    Console.WriteLine("インデックスの作成を開始します。");
                    CreateIndex();
                    Console.WriteLine("インデックスの作成が完了しました。");
                    break;
                case 1:
                    Console.WriteLine("検索を実行します。");
                    SearchRecord(args[0]);
                    break;
                default:
                    Console.WriteLine("引数を２つ以上指定しないでください。");
                    break;
            }
        }

        /// <summary>
        /// レコードを検索します。
        /// </summary>
        /// <param name="keyword">検索キーワード</param>
        private static void SearchRecord(string keyword)
        {
            //インデックスファイルを読み込む
            //インデックスに従って結果を検出していく。
            throw new NotImplementedException();
        }


        /// <summary>
        /// インデックスの作成を行う処理です。
        /// </summary>
        private static void CreateIndex()
        {
            string zipFilePath = "download.zip";
            DownloadZipFile(zipFilePath);

            string extractDirectory = Path.GetFullPath("extract");
            string csvFilePath = ExtractCsvFile(zipFilePath, extractDirectory);


            var indexData = IndexUtility.GetIndexData(csvFilePath, Ngram, FileEncoding);
            string indexFilePath = csvFilePath + ".idx";
            IndexUtility.SaveIndexData(indexFilePath, indexData, FileEncoding);
        }

        /// <summary>
        /// zipファイルを展開してcsvファイルを取得します。
        /// </summary>
        /// <remarks>
        /// zipファイルを展開したらcsvファイルがあるという前提です。
        /// </remarks>
        /// <param name="zipFilePath"></param>
        /// <param name="extractDirectory"></param>
        /// <returns></returns>
        private static string ExtractCsvFile(string zipFilePath, string extractDirectory)
        {
            Directory.CreateDirectory(extractDirectory);
            if (Directory.Exists(extractDirectory))
            {
                Directory.Delete(extractDirectory, true);
            }

            ZipFile.ExtractToDirectory(zipFilePath, extractDirectory);
            string csvFilePath = Directory.EnumerateFiles(extractDirectory, "*.csv", SearchOption.AllDirectories)
                .SingleOrDefault();

            if (csvFilePath == null)
            {
                throw new InvalidOperationException("csvファイルが取得できませんでした。");
            }

            return csvFilePath;
        }

        /// <summary>
        /// データソースとするZipファイルをダウンロードします。
        /// </summary>
        /// <param name="downloadFilePath">保存先ファイルパス</param>
        private static void DownloadZipFile(string downloadFilePath)
        {
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(DataSourceUrl, downloadFilePath);
            }
        }
    }
}