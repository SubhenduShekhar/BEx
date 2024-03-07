using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BEx.Framework.Base;
using BEx.Framework.DataProvider;
using BEx.Framework.Executor;
using BEx.Framework.Executor.API;
using BEx.Framework.Reporter;
using Newtonsoft.Json;

namespace BEx.Framework
{
    public sealed class Driver
    {
        private static String[]? TestCaseList { get; set; }
        private static Boolean ToReport { get; set; } = true;
        private static String? Suite { get; set; }
        /// <summary>
        /// Execute with commandline arguments
        /// </summary>
        /// <param name="args">Commandline arguments</param>
        private static void ExecuteTest(String[] args)
        {
            foreach (String EachArgs in args)
            {
                if (EachArgs.StartsWith("--tests"))
                {
                    if(Suite == null)
                    {
                        String CommaSepTCs = EachArgs.Split("--tests=")[1];
                        if (CommaSepTCs == "")
                            throw new Exception("Missing test cases in argumets... \nSyntax: --tests=Comma,Seperated,TestCases");
                        TestCaseList = CommaSepTCs.Split(",");
                    }
                    else
                        throw new Exception("--tests and --suite cannot be used simultaneously... exiting execution with -1");
                }
                if (EachArgs.StartsWith("--suite"))
                {
                    if (TestCaseList != null)
                        throw new Exception("--tests and --suite cannot be used simultaneously... exiting execution with -1");
                    else
                        Suite = EachArgs.Split("--suite=")[1];
                }
                if (EachArgs.StartsWith("--toreport"))
                {
                    if (EachArgs.Split("--toreport=")[1].ToLower().Equals("true"))
                        ToReport = true;
                    else
                    {
                        Console.WriteLine("Executing tests without reporting");
                        ToReport = false;
                    }
                }
            }
            ExecuteTestsInList(TestCaseList);
        }
        /// <summary>
        /// Executes tests provied in commandline arguments
        /// </summary>
        /// <param name="TestCaseList">Commandline argument tests</param>
        private static void ExecuteTestsInList(String[] TestCaseList)
        {
            #region Framework setup
            Data.ReadEnvironmentData();
            // --toreport
            BaseClass.Reporter = new Report(ToReport);
            APIBase.InitSuite(TestCaseList);

            SlackNotifier slackNotifier = null;
            APIBase aPIBase = null;
            Scriptless scriptless = null;
            Data data = null;
            // --suite
            if (TestCaseList == null)
                Data.GetSuiteTestCases(Suite);
            BaseClass.Reporter.Log("There are " + BaseClass.TestSuite.Keys.Count + " test cases in scope");
            #endregion

            if (Data.IsEnvironmentPresent())
            {
                foreach (String testCases in BaseClass.TestSuite.Keys)
                {
                    try
                    {
                        BaseClass.Reporter.Log("Initializing framework for test case : " + testCases);
                        aPIBase = ExecuteTests(testCases, data, aPIBase, scriptless);
                    }
                    catch (Exception e)
                    {
                        BaseClass.Reporter.Log(e.Message);
                        BaseClass.Reporter.Log(e.StackTrace);
                        BaseClass.Reporter.AddResult(Report.Status.Fail, e.Message);
                        BaseClass.Reporter.EndTest(testCases);
                    }
                    BaseClass.Reporter.Log("Finished executing test case : " + testCases);
                }
            }
            else
            {
                BaseClass.Reporter.Log("Environment \"" + BaseClass.Config?.FrameworkConfig.Environment + "\" not present in Environment.json file... Please check");
                Environment.Exit(1);
            }

            #region Framework teardown
            if (aPIBase != null)
            {
                slackNotifier = new SlackNotifier(BaseClass.Reporter, aPIBase);
                slackNotifier.PostMessage();
            }
            BaseClass.Reporter.SuiteResult();
            #endregion
        }
        private static void NoArgsExecute()
        {
            #region Framework setup
            Data.ReadEnvironmentData();
            BaseClass.Reporter = new Report();
            APIBase.InitSuite();
            SlackNotifier slackNotifier = null;
            APIBase aPIBase = null;
            Scriptless scriptless = null;
            Data data = null;
            BaseClass.Reporter.Log("There are " + BaseClass.TestSuite.Keys.Count + " test cases in scope");
            #endregion

            if (Data.IsEnvironmentPresent())
            {
                foreach (String testCases in BaseClass.TestSuite.Keys)
                {
                    try
                    {
                        BaseClass.Reporter.Log("Initializing framework for test case : " + testCases);
                        aPIBase = ExecuteTests(testCases, data, aPIBase, scriptless);
                    }
                    catch (Exception e)
                    {
                        BaseClass.Reporter.Log(e.Message);
                        BaseClass.Reporter.Log(e.StackTrace);
                        BaseClass.Reporter.AddResult(Report.Status.Fail, e.Message);
                        BaseClass.Reporter.EndTest(testCases);
                    }
                    BaseClass.Reporter.Log("Finished executing test case : " + testCases);
                }
            }
            else
            {
                BaseClass.Reporter.Log("Environment \"" + BaseClass.Config.FrameworkConfig.Environment + "\" not present in Environment.json file... Please check");
                Environment.Exit(1);
            }

            #region Framework teardown
            if (aPIBase != null)
            {
                slackNotifier = new SlackNotifier(BaseClass.Reporter, aPIBase);
                slackNotifier.PostMessage();
            }
            BaseClass.Reporter.SuiteResult();
            #endregion
        }
        private static APIBase ExecuteTests(String testCases,
            Data data,
            APIBase aPIBase, Scriptless scriptless)
        {
            #region Scriptless
            data = new Data(testCases);
            BaseClass.Reporter?.InitTest(testCases);
            aPIBase = new APIBase(testCases, data);
            aPIBase.InitTest(testCases);
            scriptless = new Scriptless(aPIBase);

            try
            {
                if (BaseClass.TestSuite[testCases].ToExecute.ToLower().Contains("y"))
                {
                    BaseClass.Reporter?.Log("Executing test case : " + testCases);
                    BaseClass.Reporter?.Log("This test case contains " + aPIBase.data.TestCase.Keys.Count + " test steps");

                    aPIBase.SerialNo = 1;
                    foreach (String TestStepName in aPIBase.data.TestCase.Keys)
                    {
                        BaseClass.Reporter?.AddResult(Report.Status.Info, "Executing test step : " + TestStepName);
                        if (data.IsDataDrive(TestStepName))
                        {
                            BaseClass.Reporter?.Log("This is a data driven test case with " + data.StepExcelData.Count + " iterations");
                            Int32 count = 1;
                            foreach (Dictionary<String, String> eachData in data.StepExcelData)
                            {
                                try
                                {
                                    BaseClass.Reporter?.AddResult(Report.Status.Info, "Executing data driven test for iteration : " + count);
                                    TestCase testStep = data.GetTestStep(eachData);
                                    ExecuteTestStep(TestStepName, data, aPIBase, scriptless, testStep);
                                    BaseClass.Reporter?.AddResult(Report.Status.Pass, "Iteration : " + count + " passed");
                                }
                                catch (Exception e)
                                {
                                    BaseClass.Reporter?.AddResult(Report.Status.Fail, "Iteration : " + count + " failed");
                                }
                                count++;
                            }
                        }
                        else
                        {
                            ExecuteTestStep(TestStepName, data, aPIBase, scriptless);
                        }
                        aPIBase.SerialNo++;
                    }
                    BaseClass.Reporter?.EndTest(testCases);
                    BaseClass.Reporter?.MarkTestCase(Report.Status.Pass);
                }
                else
                    BaseClass.Reporter?.AddResult(Report.Status.Skip, "Skipping test case : " + testCases);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                BaseClass.Reporter?.AddResult(Report.Status.Fail, e.Message);
                BaseClass.Reporter?.MarkTestCase(Report.Status.Fail);
            }
            #endregion
            return aPIBase;
        }
        public static void Main(String[] args)
        {
            if (args.Length == 0)
                NoArgsExecute();
            else
            {
                if (args[0] == "--version")
                    VersionInfo();
                else if(args[0].Contains("--update"))
                {
                    if(args.Length == 1)
                        CLIUpdate.Get(args[0].Split("--update@")[1]);
                    else
                        CLIUpdate.Get(args[0].Split("--update@")[1], args[1].Split("--path=")[1]);
                }
                else if(args[0].Equals("--summary"))
                {
                    Data.ReadEnvironmentData();
                    APIBase.InitSuite();
                    Report.SummaryReport();
                }
                else
                    ExecuteTest(args);
            }
        }
        private static void ExecuteTestStep(String TestStepName, Data data, APIBase aPIBase, Scriptless scriptless, TestCase testStep = null, Boolean TestStepFromExcel = true)
        {
            if(TestStepFromExcel)
                aPIBase.InitTestStep(TestStepName, testStep);
            else
                aPIBase.InitTestStep(TestStepName, testStep, false);
            scriptless.InitTestStep();
            scriptless.CheckMandatory();

            if (aPIBase.data.TestStep.SerialNo == aPIBase.SerialNo)
            {
                if (scriptless.ToExecute())
                {
                    scriptless.Execute(aPIBase.data.TestStep.RequestType);
                    BaseClass.Reporter.AddResult(Report.Status.Info, "Done Executing : " + TestStepName);
                }
                else
                {
                    BaseClass.Reporter.AddResult(Report.Status.Skip, "Skipping test step : " + TestStepName);
                }
            }
            else
            {
                BaseClass.Reporter.AddResult(Report.Status.Error, "Test steps not in sequence for " + TestStepName);
                BaseClass.Reporter.MarkTestCase(Report.Status.Fail);
                throw new Exception("Test steps not in sequence for " + TestStepName);
            }
        }
        private static void VersionInfo()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Backend Explorer [Version 1.0.0]");
            Console.WriteLine("(c) All rights reserved");
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}
