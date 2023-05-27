
using BEx.Framework.DataProvider;

namespace BEx.Framework.Base
{
    public interface ITestCaseBase
    {
        public void BeforeSuite(Data data);
        public void BeforeTest(Data data);
        public void Test(Data data);
        public void AfterTest(Data data);
        public void AfterSuite(Data data);
    }
}
