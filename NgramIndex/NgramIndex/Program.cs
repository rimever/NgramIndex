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
        /// インデックスファイルの拡張子
        /// </summary>
        private const string IndexFileExtension = ".idx";

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
            //TODO:処理時刻を計測する
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
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
        /// 未補足の例外が発生したときの処理です。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("想定されない例外が発生しました。");
            Console.WriteLine(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }

        /// <summary>
        /// レコードを検索します。
        /// </summary>
        /// <param name="keyword">検索キーワード</param>
        private static void SearchRecord(string keyword)
        {
            string filePath = GetIndexFilePath();
            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("インデックスがありません。先にインデックスを作成してください。");
                return;
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
            string csvFilePath = GetCsvFilePath(Path.GetFullPath("extract"));

            //TODO: 行の最初の値を定数化
            int line = 1;
            using (var reader = new StreamReader(csvFilePath, FileEncoding))
            {
                string readLine;
                while ((readLine = reader.ReadLine()) != null)
                {
                    if (resultList.Contains(line))
                    {
                        Console.WriteLine(readLine);
                    }

                    line++;
                }
            }

            Console.WriteLine("検索結果が出力されました。");
        }

        /// <summary>
        /// インデックスファイルのパスを取得します。
        /// </summary>
        /// <returns></returns>
        private static string GetIndexFilePath()
        {
            var filePath = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*" + IndexFileExtension,
                SearchOption.AllDirectories).FirstOrDefault();
            return filePath;
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
            string indexFilePath = csvFilePath + IndexFileExtension;
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
            string csvFilePath = GetCsvFilePath(extractDirectory);

            if (csvFilePath == null)
            {
                throw new InvalidOperationException("csvファイルが取得できませんでした。");
            }

            return csvFilePath;
        }

        /// <summary>
        /// csvファイルのパスを取得します。
        /// </summary>
        /// <param name="extractDirectory"></param>
        /// <returns></returns>
        private static string GetCsvFilePath(string extractDirectory)
        {
            return Directory.EnumerateFiles(extractDirectory, "*.csv", SearchOption.AllDirectories)
                .SingleOrDefault();
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