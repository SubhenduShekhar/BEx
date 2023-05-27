using System.Xml;

namespace BEx.Automation.Framework.Utilities
{
    [System.Obsolete()]
    public class Constant
    {
        public static string CurrentTestName { get; set; }
        public static string Projectbasepath { get; set; }
        public static int Iterations { get; set; }
        public static int CurrentIteration { get; set; }
        public static XmlDocument xmlDocument { get; set; } = new XmlDocument();
    }
}