using System;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using CsvHelper;
using Microsoft.VisualBasic.FileIO;
using NgramIndex.Services;
using NgramIndex.Utilities;

namespace NgramIndex.Benchmarks
{
    /// <summary>
    /// csvのパーサーをベンチマークするクラスです。
    /// </summary>
    /// <remarks>
    /// 参考のためのベンチマークです。
    /// </remarks>
    public class CsvParserBenchmark
    {
        /// <summary>
        /// ルートディレクトリ
        /// </summary>
        public static string RootDirectory => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase
            , @"..\..");

        /// <summary>
        /// 計測用のインデックスのパス
        /// </summary>
        /// <returns></returns>
        private static string CsvLibraryPath()
        {
            return Path.Combine(RootDirectory, "TestResult", "Library", "CsvParser");
        }

        [GlobalSetup]
        public void Setup()
        {
            var searchService = new JapanPostSearchService(CsvLibraryPath());
            searchService.CreateIndex();
        }

        /// <summary>
        /// <see cref="CsvReader"/>の読み込み速度を検証します。
        /// </summary>
        [Benchmark]
        public void LoadCsvHelper()
        {
            var csvFilePath = FileUtility.GetCsvFilePath(CsvLibraryPath());
            using (var reader = new CsvReader(new StreamReader(csvFilePath, Encoding.GetEncoding("Shift-JIS"))))
            {
                while (reader.Read())
                {
                }
            }
        }

        /// <summary>
        /// <see cref="TextFieldParser"/>の読み込み速度を検証します。
        /// </summary>
        [Benchmark]
        public void LoadTextFieldParser()
        {
            var csvFilePath = FileUtility.GetCsvFilePath(CsvLibraryPath());
            using (var parser = new TextFieldParser(csvFilePath, Encoding.GetEncoding("Shift-JIS"))
            {
                TextFieldType = FieldType.Delimited,
                Delimiters = new[] {","},
                HasFieldsEnclosedInQuotes = true
            })
            {
                while (!parser.EndOfData)
                {
                    string[] row = parser.ReadFields();
                }
            }
        }
    }
}