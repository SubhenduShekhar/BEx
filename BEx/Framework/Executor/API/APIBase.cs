using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using BEx.Framework.Base;
using BEx.Framework.DataProvider;
using BEx.Framework.DataProvider.Formats;
using BEx.Framework.Reporter;
using BEx.Framework.Base.Poco;

namespace BEx.Framework.Executor
{
    public class APIBase : BaseClass
    {
        public String TestName { get; set; }
        public String PreviousTest { get; set; }
        public Data data { get; set; }
        public Int32 SerialNo { get; set; } = -1;
        public Int32 DataSerialNo { get; set; } = 1;
        public HttpClient httpClient { get; set; }
        public HttpResponseMessage httpResponse { get; set; }
        public HttpContent httpContent { get; set; }
        public enum Request
        {
            GET = 1,
            POST = 2,
            UPDATE = 3,
            PUT = 4,
            DELETE = 5
        }
        public String Response { get; set; }
        public APIBase(String TestName, Data data)
        {
            this.TestName = TestName;
            this.data = data;
        }
        public static void InitSuite(String[] TestCaseList = null)
        {
            ReadSuite(TestCaseList);
            InitTestCaseStatus();
        }
        public void InitTest(String TestCaseName, Boolean InitTestCaseReport=true)
        {
            ReadAllTestCaseSteps(TestCaseName);
            if(InitTestCaseReport)
                MarkExecutingTestCase(TestCaseName);
        }
        public void InitTestStep(String StepName, TestCase testStep = null, Boolean ToMergeTestSteps = true)
        {
            if(testStep == null)
            {
                this.data.TestStepName = StepName;
                this.data.TestStep = this.data.TestCase[StepName];
            }
            else
            {
                if (ToMergeTestSteps)
                    this.data.TestStep = this.data.MergeTestStep(testStep, this.data.TestCase[StepName]);
                else
                    this.data.TestStep = testStep;
            }
        }
        public void ReportError(System.Net.HttpStatusCode statusCode, String Message)
        {
            BaseClass.Reporter.AddResult(Report.Status.Error, "Response ended with status code : " + Convert.ToInt32(statusCode));
            BaseClass.Reporter.Log(Message);
        }
        private void MarkExecutingTestCase(String TestCaseName)
        {
            BaseClass.TestCaseStatus[TestCaseName] = 1;
        }
        /// <summary>
        /// Reads test suites from file path and desrializes data to objects
        /// </summary>
        private static void ReadSuite(String[] TestCaseList = null)
        {
            String SuiteFullPath = Data.GetFullPath(BaseClass.Config.FrameworkConfig.TestSuiteConfig);
            JObject obj = Json.Read(SuiteFullPath);
            BaseClass.TestSuite = obj.ToObject<Dictionary<String, TestSuiteParams>>();
            if (TestCaseList != null)
            {
                foreach(String EachTestCases in BaseClass.TestSuite.Keys)
                {
                    if (TestCaseList.Contains(EachTestCases))
                        continue;
                    else
                        BaseClass.TestSuite.Remove(EachTestCases);
                }
            }
        }
        private static void InitTestCaseStatus()
        {
            foreach(String testSuite in BaseClass.TestSuite.Keys)
                BaseClass.TestCaseStatus.Add(testSuite, 0);
        }
        private void ReadAllTestCaseSteps(String TestCaseName)
        {
            String TestsFullPath = Data.GetFullPath(BaseClass.Config.FrameworkConfig.TestCasePath);
            dynamic obj;
            this.data.TestCase = new Dictionary<String, TestCase>();
            try
            {
                obj = Json.Read(Path.Combine(TestsFullPath, TestCaseName + ".json"));
                TestCase testCase = null;

                if (Json.IsObject(Path.Combine(TestsFullPath, TestCaseName + ".json"), false))
                {
                    JObject jsonObject = (JObject)obj;
;                   List<String> testSteps = jsonObject.Properties().Select(p => p.Name).ToList();
                    foreach (String eachStep in testSteps)
                    {
                        String stepContent = obj.SelectToken("$." + eachStep).ToString();
                        testCase = JsonSerializer.Deserialize<TestCase>(stepContent);
                        this.data.TestCase.Add(eachStep, testCase);
                    }
                }
                else
                {
                    List<String> testSteps = ((JArray)obj).ToObject<List<String>>();
                    foreach(String eachStep in testSteps)
                    {
                        String stepContent = File.ReadAllText(Path.Combine(BaseClass.DataDirectory, "..", BaseClass.Config.FrameworkConfig.ApiLibraryPath, eachStep + ".json"));
                        testCase = JsonSerializer.Deserialize<TestCase>(stepContent);
                        this.data.TestCase.Add(eachStep, testCase);
                    }
                }
            }
            catch(FileNotFoundException)
            {
                throw new Exception("TestCase : " + TestCaseName + " not found at path");
            }
        }
        public HttpResponseMessage POST(String EndPoint, StringContent Payload)
        {
            httpClient = new HttpClient();
            httpResponse = httpClient.PostAsync(EndPoint, Payload).Result;
            return httpResponse;
        }
    }
}
