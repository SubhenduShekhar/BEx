using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEx.Framework.Poco
{
    public class FrameworkConfig
    {
        public String DataFilePath { get; set; }
        public String? TestDataFormat { get; set; }
        public String TestSuiteConfig { get; set; }
        public String TestCasePath { get; set; }
        public String Environment { get; set; } 
        public String? ApiLibraryPath { get; set; }
        public String ProjectName { get; set; }
        public Boolean SlackNotify { get; set; }
        public List<String>? Tag { get; set; }
        public String? RunLocation { get; set; }
        public String? DownloadPath { get; set; }
        public String? WebHookUrl { get; set; }
        public String? Channel { get; set; }
    }
}
