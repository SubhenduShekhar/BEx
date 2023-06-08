using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System;
using System.IO;
using System.Text;
using BEx.Framework.Base;
using System.Collections.Generic;

namespace BEx.Framework.Reporter
{
    public sealed class Report 
    {
        private String TestCaseName { get; set; }
        private static Boolean ToReport { get; set; } = true;

        public static Report reportObj = null;
        public static Report reportInstance
        {
            get {
                if (reportObj == null)
                    reportObj = new Report();
                return reportObj;
            }
        }
        public enum Status
        {
            Pass = 1,
            Fail = 2,
            Info = 3,
            Warning = 4,
            Skip = 5,
            Error = 6
        }
        public static AventStack.ExtentReports.ExtentReports Extent { get; set; }
        public ExtentTest Test { get; set; }
        private static FileStream fileStream { get; set; }
        public Report(Boolean ToReport = true)
        {
            Report.ToReport = ToReport;
            Init();
            InitLogger();
        }
        /// <summary>
        /// Inititalizes reports instance with extent reports.
        /// <br/>
        /// Results path and Screenshot path are inititalized
        /// If extent reporter object is null, initiated
        /// </summary>
        public static void Init()
        {
            if (ToReport)
            {
                String ResultFileName = BaseClass.ResultsPath + @"\Report.html";
                BaseClass.ResultsPath = Path.Combine(BaseClass.ResultsPath, "Run_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss"));
                Directory.CreateDirectory(BaseClass.ResultsPath);

                ExtentHtmlReporter htmlReporter = new ExtentHtmlReporter(BaseClass.ResultsPath + @"\Report.html");

                if (Extent == null)
                    Extent = new AventStack.ExtentReports.ExtentReports();
                Extent.AttachReporter(htmlReporter);
            }
        }
        public void InitTest(String TestCaseName)
        {
            this.TestCaseName = TestCaseName;
            if (ToReport)
            {
                Test = Extent.CreateTest(TestCaseName);
            }
            BaseClass.TestSuiteStatus.Add(TestCaseName, Report.Status.Skip);
        }
        public void MarkTestCase(Status status)
        {
            if (BaseClass.TestSuiteStatus[TestCaseName] != Status.Fail)
                BaseClass.TestSuiteStatus[TestCaseName] = status;
        }
        [Obsolete("and will be removed in later releases. Use AddResult() instead")]
        public void Result(string description,string status)
        {
            switch(status.ToLower()){
                case "pass":
                    {
                        Test.Pass(description);
                        break;
                    }
                case "fail":
                    {
                        Test.Fail(description);
                        break;
                    }
                case "info":
                    {
                        Test.Info(description);
                        break;
                    }
                case "skip":
                    {
                        Test.Skip(description);
                        break;
                    }
            }
            Extent.Flush();
        }
        [Obsolete("and will be removed in later releases. Use AddResult() instead")]
        public void Result(string description, string status,bool exittest)
        {
            switch (status.ToLower())
            {
                case "pass":
                    {
                            Test.Pass(description);
                        break;
                    }

                case "fail":
                    {
                        Test.Fail(description);
                        if (exittest)
                        {
                            Reporter.Report.Extent.Flush();
                        }
                        break;
                    }
                case "info":
                    {
                        Test.Info(description);
                        break;
                    }
            }
            Extent.Flush();

        }
        /// <summary>
        /// Adds result line to test case instance of extent report
        /// <br/>
        /// NOTE: Also logs the result in Log.txt file
        /// </summary>
        /// <param name="status">Result status</param>
        /// <param name="Description">Result message</param>
        /// <param name="ExitTest">Exit test</param>
        /// <param name="TestCaseName">Test case name(if required)</param>
        public void AddResult(Status status, String Description, bool ExitTest = false)
        {
            Log(status.ToString() + " : " + Description, status);
            switch (status)
            {
                case Status.Pass:
                    {
                        if(ToReport)
                            Test.Pass(Description);
                        if (BaseClass.TestSuiteStatus[TestCaseName] != Status.Fail)
                            BaseClass.TestSuiteStatus[TestCaseName] = Report.Status.Pass;
                        if (ExitTest && ToReport)
                            Extent.Flush();
                        break;
                    }
                case Status.Fail:
                    {
                        if (ToReport)
                            Test.Fail(Description);
                        BaseClass.TestSuiteStatus[TestCaseName] = Report.Status.Fail;
                        if (ExitTest && ToReport)
                            Extent.Flush();
                        break;
                    }
                case Status.Info:
                    {
                        if (ToReport)
                            Test.Info(Description);
                        if (ExitTest && ToReport)
                            Report.Extent.Flush();
                        break;
                    }
                case Status.Skip:
                    {
                        if (ToReport)
                            Test.Skip(Description);
                        if (ExitTest && ToReport)
                            Extent.Flush();
                        break;
                    }
                case Status.Warning:
                    {
                        if (ToReport)
                            Test.Warning(Description);
                        if (ExitTest && ToReport)
                            Extent.Flush();
                        break;
                    }
                case Status.Error:
                    {
                        if (ToReport)
                            Test.Error(Description);
                        BaseClass.TestSuiteStatus[TestCaseName] = Report.Status.Error;
                        if (ExitTest && ToReport)
                            Extent.Flush();
                        break;
                    }
            }
        }
        public void EndTest(String TestCaseName)
        {
            if(ToReport)
                Extent.Flush();
        }
        public void InitLogger()
        {
            BaseClass.LogPath = Path.Combine(BaseClass.ResultsPath, "Logs-" +
                DateTime.Now.ToString().Replace("/", "-").Replace(":", "-") + ".txt");
        }
        public void Log(String Message, Status status = Status.Info)
        {
            if (ToReport)
            {
                if (File.Exists(BaseClass.LogPath))
                    fileStream = File.Open(BaseClass.LogPath, FileMode.Append);
                else
                    fileStream = File.Create(BaseClass.LogPath);

                Byte[] text = new UTF8Encoding(true).GetBytes(DateTime.Now + " " + Message + "\n");
                if (status == Status.Pass)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if(status == Status.Fail)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if(status == Status.Info)
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if(status == Status.Warning)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if(status == Status.Skip)
                    Console.ForegroundColor = ConsoleColor.Gray;
                else if(status == Status.Error)
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(DateTime.Now + " " + Message);
                Console.ResetColor();
                fileStream.Write(text, 0, text.Length);
                LogFlush();
            }
            else
            {
                if (status == Status.Pass)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (status == Status.Fail)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (status == Status.Info)
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if (status == Status.Warning)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (status == Status.Skip)
                    Console.ForegroundColor = ConsoleColor.Gray;
                else if (status == Status.Error)
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(DateTime.Now + " " + Message);
                Console.ResetColor();
            }
        }
        public void LogFlush()
        {
            fileStream.Close();
        }
        public void SuiteResult()
        {
            Boolean flag = true;
            foreach (String TestCase in BaseClass.TestSuiteStatus.Keys)
            {
                Console.WriteLine(TestCase + " --------------> " + BaseClass.TestSuiteStatus[TestCase].ToString());
                if (BaseClass.TestSuiteStatus[TestCase] == Status.Fail)
                    flag = false;
                else if (BaseClass.TestSuiteStatus[TestCase] == Status.Error)
                    flag = false;
            }
            if (flag)
                Environment.Exit(0);
            else
                Environment.Exit(1);
        }

        public static void SummaryReport()
        {
            Console.WriteLine("Please NOTE, only scripless tests are considered for generating automation summary report");
            Summary summary = new Summary();

            Int32 TotalTestCases = summary.GetAllTestCases();
            List<String> TotalApi = summary.GetTotalApi();

            Console.WriteLine();
            PrintRow(ConsoleColor.Cyan, "Total Test Cases", "Total APIs Automated");
            Console.WriteLine();
            PrintRow(ConsoleColor.Blue, TotalTestCases.ToString(), TotalApi.Count.ToString());
            Console.WriteLine();
        }
        static void PrintRow(ConsoleColor consoleColor, params string[] columns)
        {
            int width = (Console.BufferWidth - columns.Length) / columns.Length;
            string row = "";

            foreach (string column in columns)
            {
                row += AlignCentre(column, width);
            }
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(row);
            Console.ResetColor();
        }

        static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        }
    }
}
