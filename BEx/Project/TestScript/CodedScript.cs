using BEx.Framework.Base;
using BEx.Framework.DataProvider;
using System;
using System.Collections.Generic;
using System.Text;
using BEx.Framework.Reporter;
using BEx.Framework.Executor;

namespace BEx.Project.TestScript
{
    class CodedScript : ITestCaseBase
    {
        public void AfterSuite(Data data)
        {
            Console.WriteLine("After suite");
        }

        public void AfterTest(Data data)
        {
            Console.WriteLine("After test");
        }

        public void BeforeSuite(Data data)
        {
            Console.WriteLine("Before suite");
        }

        public void BeforeTest(Data data)
        {
            Console.WriteLine("Before test");
        }

        public void Test(Data data)
        {
            Console.WriteLine("Test data check for key - Id : " + data.Get("Id"));
            BaseClass.Reporter.AddResult(Report.Status.Pass, "Id : " + data.Get("Id"));
        }
    }
}
