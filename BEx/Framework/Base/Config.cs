using BEx.Framework.Base.Poco;
using BEx.Framework.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEx.Framework.Base
{
    public class Config
    {
        public FrameworkConfig FrameworkConfig { get; set; }
        public Dictionary<String, TestSuiteParams> TestSuite { get; set; }
    }
}
