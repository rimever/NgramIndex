using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;

namespace NgramIndex.Utilities
{
    public static class FileUtility
    {
        /// <summary>
        /// インデックスファイルの拡張子
        /// </summary>
        public const string IndexFileExtension = ".idx";

        /// <summary>
        /// 指定行のファイルを取得します。
        /// </summary>
        /// <param name="lineNumbers"></param>
        /// <param name="filePath"></param>
        /// <param name="encoding"></param>
        public static IEnumerable<string> GetFileLines(List<int> lineNumbers, string filePath, Encoding encoding)
        {
            int line = IndexUtility.LineStartNumber;
            using (var reader = new StreamReader(filePath, encoding))
            {
                string readLine;
                while ((readLine = reader.ReadLine()) != null)
                {
                    if (lineNumbers.Contains(line))
                    {
                        yield return readLine;
                    }

                    line++;
                }
            }
        }

        /// <summary>
        /// インデックスファイルのパスを取得します。
        /// </summary>
        /// <param name="storageDirectory"></param>
        /// <returns></returns>
        public static string GetIndexFilePath(string storageDirectory)
        {
            var filePath = Directory.EnumerateFiles(storageDirectory, "*" + IndexFileExtension,
                SearchOption.AllDirectories).FirstOrDefault();
            return filePath;
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
        public static string ExtractCsvFile(string zipFilePath, string extractDirectory)
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
        /// <param name="storageDirectory"></param>
        /// <returns></returns>
        public static string GetCsvFilePath(string storageDirectory)
        {
            return Directory.EnumerateFiles(storageDirectory, "*.csv", SearchOption.AllDirectories)
                .SingleOrDefault();
        }

        /// <summary>
        /// zipの解凍先フォルダを取得します。
        /// </summary>
        /// <param name="storageDirectory"></param>
        /// <returns></returns>
        public static string GetZipExtractDirectory(string storageDirectory)
        {
            return Path.Combine(storageDirectory, "extract");
        }

        /// <summary>
        /// データソースとするZipファイルをダウンロードします。
        /// </summary>
        /// <param name="downloadFilePath">保存先ファイルパス</param>
        /// <param name="dataSourceUrl"></param>
        public static void DownloadZipFile(string downloadFilePath, string dataSourceUrl)
        {
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(dataSourceUrl, downloadFilePath);
            }
        }
    }
}