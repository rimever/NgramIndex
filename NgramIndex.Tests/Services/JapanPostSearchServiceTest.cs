using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var service = new JapanPostSearchService(Path.Combine(TestSetup.RootDirectory, "TestResult", "Services", "JapanPostSearchService", "CreateIndex"));
            service.CreateIndex();
        }
        /// <summary>
        /// 仕様１をテストします。
        /// </summary>
        [Test]
        public void SearchInputCase1()
        {
            var service = new JapanPostSearchService(Path.Combine(TestSetup.RootDirectory, "TestResult", "Services","JapanPostSearchService","SearchInputCase1"));
            service.CreateIndex();
            foreach (var record in service.SearchRecord("渋谷"))
            {
                Assert.IsTrue(record.Contains("渋"));
                Assert.IsTrue(record.Contains("谷"));
            }
        }
        /// <summary>
        /// 仕様２をテストします。
        /// </summary>
        [Test]
        public void SearchInputCase2()
        {
            var service = new JapanPostSearchService(Path.Combine(TestSetup.RootDirectory, "TestResult", "Services", "JapanPostSearchService", "SearchInputCase2"));
            service.CreateIndex();
            foreach (var record in service.SearchRecord("東京都"))
            {
                Assert.IsTrue(record.Contains("東京"));
                Assert.IsTrue(record.Contains("京都"));
            }

        }
    }
}
