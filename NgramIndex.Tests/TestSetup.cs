using System;
using System.IO;

namespace NgramIndex.Tests
{
    public class TestSetup
    {
        public static string RootDirectory => Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase
            , @"..\..");
    }
}