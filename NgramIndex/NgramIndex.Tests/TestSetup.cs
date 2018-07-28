using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NgramIndex.Tests
{
    public class TestSetup
    {
        public static string RootDirectory => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase
            , @"..\..");

    }
}
