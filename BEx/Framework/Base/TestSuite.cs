using System;
using System.Collections.Generic;

namespace BEx.Framework.Base
{
    public class TestSuite
    {
        public String ToExecute { get; set; }
        public String TestSource { get; set; }
        public List<String> Suite { get; set; }
        public String DataKey { get; set; }
    }
}
