using System;
using System.Collections.Generic;

namespace BEx.Framework.Base
{
    public class Enviroment
    {
        public String BaseUrl { get; set; }
        public List<String> client_id { get; set; }
        public List<String> scope { get; set; }
        public List<String> grant_type { get; set; }
    }
}
