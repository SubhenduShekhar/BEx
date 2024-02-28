using System;
using System.Collections.Generic;

namespace BEx.Framework.Base.Poco
{
    public class TestSuiteParams
    {
        public string ToExecute { get; set; }
        public string TestSource { get; set; }
        public List<string> Suite { get; set; }
        public string DataKey { get; set; }
    }
}
