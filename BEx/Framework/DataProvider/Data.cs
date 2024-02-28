using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BEx.Framework.Base;
using BEx.Framework.Base.Poco;
using BEx.Framework.DataProvider.Formats;
using BEx.Framework.Poco;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BEx.Framework.DataProvider
{
    public sealed class Data
    {
        private static Excel excel { get; set; }
        private static Dictionary<String, String> environment { get; set; }
        private Dictionary<String, String> data { get; set; }
        public String TestStepName { get; set; }
        public TestCase TestStep { get; set; }
        public Dictionary<String, TestCase> TestCase { get; set; }
        public Dictionary<String, dynamic> sharedObjects { get; set; }
        public List<String> ExcelHeaders { get; set; }
        private List<Dictionary<String, String>> ExcelData { get; set; }
        public List<Dictionary<String, String>> StepExcelData { get; set; }
        /// <summary>
        /// Call this to init test data with test case name
        /// </summary>
        /// <param name="TestCaseName">Test case name</param>
        public Data(String TestCaseName)
        {
            sharedObjects = new Dictionary<String, Object>();

            if (BaseClass.DataFileFormat == null)
                data = new Dictionary<String, String>();
            if (BaseClass.TestSuite[TestCaseName].TestSource.ToLower().Equals("coded"))
            {
                Console.WriteLine("Initializing data object for coded execution");
                if(BaseClass.DataFileFormat != null)
                {
                    if (BaseClass.DataFileFormat.ToLower().Trim().Equals("xlsx"))
                    {
                        excel = new Excel(TestCaseName);
                        data = excel.GetCurrentTestCaseData();
                        Console.WriteLine("Data provider successfully set to excel");
                    }
                }
            }
            else if (BaseClass.TestSuite[TestCaseName].TestSource.ToLower().Equals("scriptless"))
            {
                Console.WriteLine("Initializing data object for scriptless execution");
                if(BaseClass.TestDataFormat is not null)
                    if (BaseClass.TestSuite[TestCaseName].DataKey != null && 
                        (BaseClass.TestDataFormat.ToLower().Equals("excel") ||
                         BaseClass.TestDataFormat.ToLower().Equals("xlsx")))
                    {
                        excel = new Excel(BaseClass.TestSuite[TestCaseName].DataKey, "Read");
                        ExcelHeaders = excel.GetHeaders();
                        ExcelData = excel.GetData();
                    }
            }
        }
        private static void Init()
        {
            ReadConfiguration();
            ReadEnvironments();
            SetFramework();
            if (BaseClass.Config.FrameworkConfig.TestDataFormat != "")
                BaseClass.DataFileFormat = BaseClass.Config.FrameworkConfig.DataFilePath.Split(".")[BaseClass.Config.FrameworkConfig.DataFilePath.Split(".").Length - 1].ToLower();
        }
        private static void ReadEnvironments()
        {
            String EnvironmentPath = GetFullPath("Framework\\Config\\Environment.json");
            BaseClass.Environments = Json.Read(EnvironmentPath);
        }
        private static void SetFramework()
        {
            FrameworkConfig? FrameworkConfig = BaseClass.Config?.FrameworkConfig;
            if(FrameworkConfig == null)
            {
                Console.WriteLine(DateTime.Now + " Config key : FrameworkConfigFilePath not found.");
                Console.WriteLine("Please provide config file in Framework\\Config\\FrameworkConfig.json");
                Environment.Exit(1);
            }

            if (FrameworkConfig.TestDataFormat == null)
            {
                Console.WriteLine(DateTime.Now + " Config key : TestDataFormat not found.");
                BaseClass.TestDataFormat = "excel";
                Console.WriteLine("Taking default test data format as : excel");
                Console.WriteLine("Else, please provide config file in Framework\\Config\\FrameworkConfig.json");
            }
            
            try
            {
                String RawPath = SearchFile(FrameworkConfig.DataFilePath);
                if (IsDirectory(RawPath))
                {
                    BaseClass.DataDirectory = RawPath;
                    if (FrameworkConfig.TestDataFormat == null)
                        throw new Exception("TestDataFormat key is required if DataFilePath is a directory");

                    if (FrameworkConfig.TestDataFormat.Equals(""))
                    {
                        Console.WriteLine("Test data framework is set for scriptless execution only");
                    }
                    else if (FrameworkConfig.TestDataFormat.Equals("excel") || FrameworkConfig.TestDataFormat.Equals("xlsx"))
                    {
                        String FilePath = GetExcelFileName(RawPath);
                        BaseClass.Config.FrameworkConfig.DataFilePath = FilePath;
                    }
                    else
                        throw new Exception("Framework is not configured for file format " + BaseClass.TestDataFormat);
                }
                else if (IsFile(RawPath))
                {
                    BaseClass.Config.FrameworkConfig.DataFilePath = RawPath;
                    if (FrameworkConfig.TestDataFormat == null)
                        throw new Exception("TestDataFormat key is required if DataFilePath is a directory");

                    if (FrameworkConfig.TestDataFormat.Equals("excel") || FrameworkConfig.TestDataFormat.Equals("xlsx"))
                    {
                        String Path = "";
                        for (Int32 i = 0; i < RawPath.Split("\\").Length - 1; i++)
                            Path += RawPath.Split("\\")[i] + "\\";
                        BaseClass.DataDirectory = Path;
                    }
                    else
                        throw new Exception("Framework is not configured for file format " + FrameworkConfig.TestDataFormat);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now + " Something went wrong with config key : DataFilePath not found.");
                Console.WriteLine("Either DataFilePath or TestDataFormat is not configured properly");

                BaseClass.Config.FrameworkConfig.DataFilePath = SearchFile(Path.Combine(FrameworkConfig.DataFilePath, "RunManager.xlsx"));
                Console.WriteLine("Taking default test data file name as : RunManager.xlsx");
                Console.WriteLine("NOTE: This file name is obsolete and will be removed in next version of framework releases");
            }
            try
            {
                BaseClass.ResultsPath = Directory.GetParent(Directory.GetParent(BaseClass.DataDirectory).FullName).FullName;
                BaseClass.ResultsPath = Directory.GetParent(BaseClass.ResultsPath).FullName;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to initialize results path.");
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            if (FrameworkConfig.Environment == null)
            {
                Console.WriteLine("Failed to initialize environment...");
                Console.WriteLine("Please provide \"Environment\": \"test\"");
                Environment.Exit(1);
            }
            if (FrameworkConfig.TestSuiteConfig == null)
            {
                Console.WriteLine(DateTime.Now + " Config key : TestSuiteConfig not found.");
                Console.WriteLine("Please provide test suite config path");
                Console.WriteLine("For example: \"TestSuiteConfig\": \".\\Framework\\Config\\TestSuite.json");
                Environment.Exit(1);
            }
            if(FrameworkConfig.TestCasePath == null)
            {
                Console.WriteLine(DateTime.Now + " Config key : TestCasePath not found.");
                Console.WriteLine("Please provide test case path");
                Console.WriteLine("For example: \"TestCasePath\": \".\\Project\\TestScript\\");
                Environment.Exit(1);
            }
            if(FrameworkConfig.ProjectName == null)
            {
                Console.WriteLine(DateTime.Now + " Warning - Config key : ProjectName not found.");
                Console.WriteLine("Please provide project name. This is required for reporting purpose.");
                Console.WriteLine("For example: \"ProjectName\": \"YOUR_PROJECT_NAME\"");
                Console.WriteLine();
                Environment.Exit(1);
            }
            if(FrameworkConfig.DownloadPath == null)
            {
                BaseClass.Config.FrameworkConfig.DownloadPath = Path.Combine(BaseClass.DataDirectory, "Extracts");
                Console.WriteLine("Setting download path to : " + BaseClass.Config.FrameworkConfig.DownloadPath);
            }
            if(FrameworkConfig.ApiLibraryPath == null)
            {
                Console.WriteLine("ApiLibraryPath is not provided...");
                Console.WriteLine("Continuing with general scriptless paths");
            }
            if (FrameworkConfig.SlackNotify)
            {
                if (FrameworkConfig.WebHookUrl == null)
                {
                    Console.WriteLine("SlackNotify is true but slack WebHookUrl is not provided");
                    Environment.Exit(1);
                }
                if (FrameworkConfig.Channel == null)
                {
                    Console.WriteLine("SlackNotify is true but slack channel is not provided");
                    Environment.Exit(1);
                }
                if (FrameworkConfig.RunLocation == null)
                {
                    Console.WriteLine("Run location is not provided");
                    BaseClass.Config.FrameworkConfig.RunLocation = "LOCAL";
                    Console.WriteLine("Default RunLocation : LOCAL");
                    Console.WriteLine("continuing...");
                }
                if (FrameworkConfig.Tag == null)
                {
                    Console.WriteLine("Slack message tag is not provided");
                    BaseClass.Config.FrameworkConfig.Tag = new List<String>();
                    Console.WriteLine("continuing...");
                }
            }
            else
                Console.WriteLine("Slack notification is ignored... continuing");
        }
        public static String GetFullPath(String RelativeFilePath)
        {
            if (RelativeFilePath.StartsWith(".\\"))
            {
                RelativeFilePath = RelativeFilePath.Split(".\\")[1];
                return SearchFile(RelativeFilePath);
            }
            else if (RelativeFilePath.StartsWith("\\"))
            {
                RelativeFilePath = RelativeFilePath.Substring(0, 2);
                return SearchFile(RelativeFilePath);
            }
            else if (RelativeFilePath.StartsWith("./"))
            {
                RelativeFilePath = RelativeFilePath.Split("./")[1];
                return SearchFile(RelativeFilePath);
            }
            else if (RelativeFilePath.StartsWith("./"))
            {
                RelativeFilePath = RelativeFilePath.Substring(0, 1);
                return SearchFile(RelativeFilePath);
            }
            else if (RelativeFilePath.Contains(":\\"))
                return RelativeFilePath;
            else
                return SearchFile(RelativeFilePath);
        }
        public static String SearchFile(String RelativeFilePath, Boolean ToReturnType = false)
        {
            String CurDir = Directory.GetCurrentDirectory();
            Int32 count = 0;
            while (count != 5)
            {
                if (IsFile(Path.Combine(CurDir, RelativeFilePath)))
                {
                    if (!ToReturnType)
                        return Path.Combine(CurDir, RelativeFilePath);
                    else
                        return "file:" + Path.Combine(CurDir, RelativeFilePath);
                }
                else if (IsDirectory(Path.Combine(CurDir, RelativeFilePath)))
                {
                    if (!ToReturnType)
                        return Path.Combine(CurDir, RelativeFilePath);
                    else
                        return "directory:" + Path.Combine(CurDir, RelativeFilePath);
                }
                else
                {
                    CurDir = Directory.GetParent(CurDir).FullName;
                    count++;
                }
                if (count == 5)
                    break;
            }
            if (count == 5)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(RelativeFilePath + " directory not found at project root" +
                    ". Please provide the config at the project root path.");
                Console.ResetColor();
                Environment.Exit(1);
                return null;
            }
            else
                return null;
        }
        public static void ReadEnvironmentData()
        {
            if (BaseClass.DataFileFormat == null)
                Init();
            else throw new Exception("Something wrong happened in reading environment data");
        }
        public static String GetEnvironmentData(String EnvKey)
        {
            return environment[EnvKey];
        }
        public void Store(String Key, dynamic Value)
        {
            sharedObjects.Add(Key, Value);
        }
        public dynamic Fetch(String Key)
        {
            return sharedObjects[Key];
        }
        private static Boolean IsDirectory(String FilePath)
        {
            try
            {
                if (Directory.Exists(FilePath))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        private static Boolean IsFile(String FilePath)
        {
            try
            {
                if (File.Exists(FilePath))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        public static Boolean IsEnvironmentPresent()
        {
            return (BaseClass.Environments?.SelectToken(BaseClass.Config.FrameworkConfig.Environment) != null);
        }
        public String GetEnvironmentValue(String Key, Int32 Index = -1)
        {
            if (Index == -1)
                return BaseClass.Environments.SelectToken("$." + Key).ToString();
            else
            {
                JArray arr = JArray.Parse(BaseClass.Environments.SelectToken("$." + Key).ToString());
                return arr[Index].ToString();
            }
        }
        public static void GetSuiteTestCases(String SuiteName = null)
        {
            if (SuiteName != null)
            {
                List<String> TestCaseList = new List<string>();
                foreach (String TestCases in BaseClass.TestSuite.Keys)
                {
                    if (BaseClass.TestSuite[TestCases].Suite.Contains(SuiteName))
                        continue;
                    else
                        BaseClass.TestSuite.Remove(TestCases);
                }
            }
            else
            {
                Console.WriteLine("Executing all test cases in scope");
            }
        }
        public static String GetExcelFileName(String FilePath)
        {
            String[] FileList = Directory.GetFiles(FilePath, "*.xlsx");
            if (FileList.Length == 0)
                throw new Exception("File with extension .xlsx not found in directory... exiting execution.");
            else if (FileList.Length > 1)
            {
                foreach(String eachFile in FileList)
                {
                    if (!eachFile.StartsWith("~"))
                        return eachFile;
                }
                throw new Exception("More than one files found in directory... exiting execution");
            }
            else
                return FileList[0];
        }
        public Boolean IsDataDrive(String TestStepName)
        {
            StepExcelData = new List<Dictionary<String, String>>();
            if (ExcelData != null)
            {
                foreach (Dictionary<String, String> data in ExcelData)
                {
                    if (data["StepName"].Equals(TestStepName))
                        StepExcelData.Add(data);
                }
            }
            if (StepExcelData.Count == 0)
                return false;
            else
                return true;
        }
        public TestCase GetTestStep(Dictionary<String, String> data)
        {
            TestCase testStep = new TestCase();
            foreach (KeyValuePair<string, string> keyValues in data)
            {
                if (keyValues.Key.Equals("ContentType"))
                    testStep.ContentType = keyValues.Value;
                if (keyValues.Key.Equals("ToExecute"))
                    testStep.ToExecute = keyValues.Value;
                if (keyValues.Key.Equals("SerialNo"))
                    testStep.SerialNo = Convert.ToInt32(keyValues.Value);
                if (keyValues.Key.Equals("RequestType"))
                    testStep.RequestType = keyValues.Value;
                if (keyValues.Key.Equals("StatusCode"))
                    testStep.StatusCode = Convert.ToInt32(keyValues.Value);
                if (keyValues.Key.Equals("EndPoint"))
                    testStep.EndPoint = keyValues.Value;
                if (keyValues.Key.Equals("Validate"))
                    testStep.Validate = JsonConvert.DeserializeObject<List<String>>(keyValues.Value);
                if (keyValues.Key.Equals("Expected"))
                    testStep.Expected = JsonConvert.DeserializeObject<List<String>>(keyValues.Value);
                if (keyValues.Key.Equals("Store"))
                    testStep.Store = JsonConvert.DeserializeObject<List<Dictionary<String, String>>>(keyValues.Value);
                if (keyValues.Key.Equals("Fetch"))
                    testStep.Fetch = JsonConvert.DeserializeObject<List<String>>(keyValues.Value);
                if (keyValues.Key.Equals("Payload"))
                    testStep.Payload = JsonConvert.DeserializeObject<Dictionary<String, String>>(keyValues.Value);
                if (keyValues.Key.Equals("Headers"))
                    testStep.Headers = JsonConvert.DeserializeObject<Dictionary<String, String>>(keyValues.Value);
                if (keyValues.Key.Equals("ReportResponse"))
                    testStep.ReportResponse = Convert.ToBoolean(keyValues.Value);
                if (keyValues.Key.Equals("ShowPayload"))
                    testStep.ShowPayload = Convert.ToBoolean(keyValues.Value);
                if (keyValues.Key.Equals("Parameters"))
                    testStep.Parameters = JsonConvert.DeserializeObject<Dictionary<String, String>>(keyValues.Value);
                if (keyValues.Key.Equals("CustomCode"))
                    testStep.CustomCode = keyValues.Value;
                if (keyValues.Key.Equals("Loop"))
                    testStep.Loop = JsonConvert.DeserializeObject<Dictionary<String, String>>(keyValues.Value);
                if (keyValues.Key.Equals("DataSource"))
                    testStep.DataSource = keyValues.Value;
            }
            return testStep;
        }
        public TestCase MergeTestStep(TestCase ExcelTestStep, TestCase RawTestStep)
        {
            if (RawTestStep.ContentType == null)
                RawTestStep.ContentType = ExcelTestStep.ContentType;
            if (RawTestStep.ToExecute == null)
                RawTestStep.ToExecute = ExcelTestStep.ToExecute;
            if (RawTestStep.SerialNo == 0)
                RawTestStep.SerialNo = ExcelTestStep.SerialNo;
            if (RawTestStep.RequestType == null)
                RawTestStep.RequestType = ExcelTestStep.RequestType;
            if (RawTestStep.StatusCode == 0)
                RawTestStep.StatusCode = ExcelTestStep.StatusCode;
            if (RawTestStep.EndPoint == null)
                RawTestStep.EndPoint = ExcelTestStep.EndPoint;
            if (RawTestStep.Validate == null)
                RawTestStep.Validate = ExcelTestStep.Validate;
            if (RawTestStep.Expected == null)
                RawTestStep.Expected = ExcelTestStep.Expected;
            if (RawTestStep.Store == null)
                RawTestStep.Store = ExcelTestStep.Store;
            if (RawTestStep.Fetch == null)
                RawTestStep.Fetch = ExcelTestStep.Fetch;
            if (RawTestStep.Payload ==  null)
                RawTestStep.Payload = ExcelTestStep.Payload;
            if (RawTestStep.Headers == null)
                RawTestStep.Headers = ExcelTestStep.Headers;
            if (RawTestStep.ReportResponse == false)
                RawTestStep.ReportResponse = ExcelTestStep.ReportResponse;
            if (RawTestStep.ShowPayload == false)
                RawTestStep.ShowPayload = ExcelTestStep.ShowPayload;
            if (RawTestStep.Parameters == null)
                RawTestStep.Parameters = ExcelTestStep.Parameters;
            if (RawTestStep.CustomCode == null)
                RawTestStep.CustomCode = ExcelTestStep.CustomCode;
            if (RawTestStep.Loop == null)
                RawTestStep.Loop = ExcelTestStep.Loop;
            if (RawTestStep.DataSource == null)
                RawTestStep.DataSource = ExcelTestStep.DataSource;
            return RawTestStep;
        }
        public static void EnsureDirectory(String path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        private static void ReadConfiguration()
        {
            String FullPath = GetFullPath(BaseClass.BaseConfigPath + "\\Config.cjson");
            BaseClass.ConfigCjson = new CJson.CJson<Config>(new CJson.Path(FullPath));
            BaseClass.Config = BaseClass.ConfigCjson.Deserialize();
        }
    }
}
