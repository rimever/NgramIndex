using System;
using System.IO;
using NgramIndex.Services;

namespace NgramIndex
{
    class Program
    {
        /// <summary>
        /// プログラムのメイン処理
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            var storagePath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "storage");
            var service = new JapanPostSearchService(storagePath);
            switch (args.Length)
            {
                case 0:
                    Console.WriteLine("インデックスの作成を開始します。");
                    service.CreateIndex();
                    Console.WriteLine("インデックスの作成が完了しました。");
                    break;
                case 1:
                    Console.WriteLine("検索を実行します。");
                    foreach (var text in service.SearchRecord(args[0]))
                    {
                        Console.WriteLine(text);
                    }
                    Console.WriteLine("検索結果が出力されました。");
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
    }
}