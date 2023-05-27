using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using BEx.Framework.DataProvider;
using BEx.Framework.Reporter;

namespace BEx.Framework.Base
{
    public interface BaseClass
    {
        public static String FrameworkConfigFilePath { get; set; }
        public String TestName { get; set; }
        public static Report Reporter { get; set; }
        public Data data { get; set; }
        public static JObject FrameworkConfig { get; set; }
        public static string EndPoint { get; set; }
        public string PreviousTest { get; set; }
        public static string DataFilePath { get; set; }
        public static String DataFileFormat { get; set; }
        public static String ResultsPath { get; set; }
        public static String LogPath { get; set; }
        public static Dictionary<String, Report.Status> TestSuiteStatus { get; set; } = new Dictionary<String, Report.Status>();
        public static String TestSuiteConfig { get; set; }
        public static String TestCasePath { get; set; }
        public static Dictionary<String, TestSuite> TestSuite { get; set; } = new Dictionary<String, TestSuite>();
        public static Dictionary<String, Int32> TestCaseStatus { get; set; } = new Dictionary<String, Int32>();
        public static String TestDataFormat { get; set; }
        public static String DataDirectory { get; set; }
        public static Boolean SlackNotify { get; set; }
        public static String WebHookUrl { get; set; }
        public static String Channel { get; set; }
        public static String ProjectName { get; set; }
        public static String Environment { get; set; }
        public static JObject Environments { get; set; }
        public static String DownloadPath { get; set; }
        public static String RunLocation { get; set; }
        public static List<String> Tag { get; set; }
        public static String ApiLibraryPath { get; set; }
        public static String Before { get; set; }
        public static Dictionary<String, TestCase> BeforeTest { get; set; }
    }
}