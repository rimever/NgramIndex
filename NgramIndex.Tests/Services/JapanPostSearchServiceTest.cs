using System.IO;
using System.Linq;
using NgramIndex.Services;
using NUnit.Framework;

namespace NgramIndex.Tests.Services
{
    /// <summary>
    /// <seealso cref="JapanPostSearchService"/>をテストします。
    /// </summary>
    [TestFixture]
    public class JapanPostSearchServiceTest
    {
        /// <summary>
        /// <see cref="JapanPostSearchService.CreateIndex"/>をテストします。
        /// </summary>
        [Test]
        public void CreateIndex()
        {
            var service = new JapanPostSearchService(Path.Combine(TestSetup.RootDirectory, "TestResult", "Services",
                "JapanPostSearchService", "CreateIndex"));
            service.CreateIndex();
        }

        /// <summary>
        /// 入力１をテストします。
        /// </summary>
        [Test]
        public void SearchInputCase1()
        {
            var service = new JapanPostSearchService(Path.Combine(TestSetup.RootDirectory, "TestResult", "Services",
                "JapanPostSearchService", "SearchInputCase1"));
            service.CreateIndex();
            var records = service.SearchRecord("渋谷").ToList();
            Assert.IsTrue(records.Any());
            foreach (var record in records)
            {
                Assert.IsTrue(record.Contains("渋"));
                Assert.IsTrue(record.Contains("谷"));
            }
        }

        /// <summary>
        /// 入力２をテストします。
        /// </summary>
        [Test]
        public void SearchInputCase2()
        {
            var service = new JapanPostSearchService(Path.Combine(TestSetup.RootDirectory, "TestResult", "Services",
                "JapanPostSearchService", "SearchInputCase2"));
            service.CreateIndex();
            var records = service.SearchRecord("東京都").ToList();
            Assert.IsTrue(records.Any());
            foreach (var record in records)
            {
                Assert.IsTrue(record.Contains("東京"));
                Assert.IsTrue(record.Contains("京都"));
            }
        }

        /// <summary>
        /// 結果のない検索
        /// </summary>
        [Test]
        public void SearchInputIllegalCase()
        {
            var service = new JapanPostSearchService(Path.Combine(TestSetup.RootDirectory, "TestResult", "Services",
                "JapanPostSearchService", "CreateIndex"));
            service.CreateIndex();
            var records = service.SearchRecord("ダミー").ToList();
            Assert.IsFalse(records.Any());
        }
    }
}