using BenchmarkDotNet.Running;

namespace NgramIndex.Benchmarks
{
    class Program
    {
        /// <summary>
        /// プログラムのメインエントリです。
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<JapanPostServiceBenchmark>();
            BenchmarkRunner.Run<CsvParserBenchmark>();
        }
    }
}