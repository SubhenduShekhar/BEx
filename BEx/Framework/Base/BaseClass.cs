using Newtonsoft.Json.Linq;
using BEx.Framework.DataProvider;
using BEx.Framework.Reporter;
using BEx.Framework.Base.Poco;

namespace BEx.Framework.Base
{
    public interface BaseClass
    {
        public String TestName { get; set; }
        public static Report? Reporter { get; set; }
        public Data data { get; set; }
        public string PreviousTest { get; set; }
        public static String? DataFileFormat { get; set; }
        public static String? ResultsPath { get; set; }
        public static String? LogPath { get; set; }
        public static Dictionary<String, Report.Status> TestSuiteStatus { get; set; } = new Dictionary<String, Report.Status>();
        public static Dictionary<String, TestSuiteParams> TestSuite { get; set; } = new Dictionary<String, TestSuiteParams>();
        public static Dictionary<String, Int32> TestCaseStatus { get; set; } = new Dictionary<String, Int32>();
        public static String? TestDataFormat { get; set; }
        public static String? DataDirectory { get; set; }
        public static JObject? Environments { get; set; }
        public static Config? Config { get; set; }
        public static readonly String BaseConfigPath = "Framework\\Config";
        public static CJson.CJson<Config> ConfigCjson { get; set; }
    }
}