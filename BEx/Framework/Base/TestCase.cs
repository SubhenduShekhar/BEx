using System;
using System.Collections.Generic;

namespace BEx.Framework.Base
{
    public class TestCase
    {
        public String ContentType { get; set; }
        public String ToExecute { get; set; }
        public Int32 SerialNo { get; set; }
        public String RequestType { get; set; }
        public Int32 StatusCode { get; set; }
        public String EndPoint { get; set; }
        public List<String> Validate { get; set; }
        public List<String> Expected { get; set; }
        public List<Dictionary<String, String>> Store { get; set; }
        public List<String> Fetch { get; set; }
        public Dictionary<String, String> Payload { get; set; }
        public Dictionary<String, String> Headers { get; set; }
        public Boolean ReportResponse { get; set; }
        public Boolean ShowPayload { get; set; }
        public Dictionary<String, String> Parameters { get; set; }
        public String CustomCode { get; set; }
        public Dictionary<String, String> Loop { get; set; }
        public String DataSource { get; set; }
        public Boolean EnsureFile { get; set; }
        public String DownloadFileAs { get; set; }
    }
}
