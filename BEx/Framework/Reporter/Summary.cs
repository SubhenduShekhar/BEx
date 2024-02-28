using BEx.Framework.Base;
using BEx.Framework.Base.Poco;
using BEx.Framework.DataProvider;
using BEx.Framework.DataProvider.Formats;
using BEx.Framework.Executor;
using BEx.Framework.Executor.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BEx.Framework.Reporter
{
    public class Summary
    {
        public Int32 GetAllTestCases()
        {
            Int32 count = 0;
            foreach(TestSuiteParams EachKey in BaseClass.TestSuite.Values)
            {
                if (EachKey.TestSource.ToLower().Equals("scriptless"))
                    count++;
            }
            return count;
        }
        private String GetTestScriptPath(String TestCaseName) => Path.Combine(Data.SearchFile(BaseClass.Config?.FrameworkConfig.TestCasePath), TestCaseName + ".json");
        public List<String> GetTotalApi()
        {
            List<String> TestSteps = new List<String>();

            foreach (String TestCases in BaseClass.TestSuite.Keys)
            {
                String testPath = GetTestScriptPath(TestCases);
                if(File.Exists(testPath))
                {
                    try
                    {
                        Dictionary<String, TestCase> testCase = Json.Read(testPath).ToObject<Dictionary<String, TestCase>>();
                        foreach (String EachTestStep in testCase.Keys)
                        {
                            if (!TestSteps.Contains(EachTestStep))
                                TestSteps.Add(EachTestStep);
                        }
                    }
                    catch (Exception e)
                    {
                        List<String> testCase = Json.Read(testPath).ToObject<List<String>>();
                        foreach (String EachTestStep in testCase)
                        {
                            if (!TestSteps.Contains(EachTestStep))
                                TestSteps.Add(EachTestStep);
                        }
                    }
                }
            }
            return TestSteps;
        }
    }
}
