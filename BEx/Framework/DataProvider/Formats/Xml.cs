using BEx.Automation.Framework.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using BEx.Framework.Base;
using BEx.Framework.Reporter;

namespace BEx.Framework.DataProvider.Formats
{
    public class Xml
     {
        private string FileName { get; set; }
        private string FilePath { get; set; }
        private XmlDocument xmlDocument { get; } = new XmlDocument();

        public void SetXMLData(string node, string value)
        {
            FileName = Constant.CurrentTestName + ".xml";
            FilePath = Constant.Projectbasepath + @"\Xmls\" + FileName;
            XmlNode root ;
            if (!File.Exists(FilePath))
            {
                root = xmlDocument.CreateElement("Root");
                xmlDocument.AppendChild(root);
                XmlNode childnode = xmlDocument.CreateElement(node);
                childnode.InnerText = value;
                root.AppendChild(childnode);
            }
            else
            {
                xmlDocument.Load(FilePath);
                root = xmlDocument.DocumentElement;
                if (xmlDocument.SelectNodes("/Root/" + node).Count == 0)
                {
                    XmlNode childnode = xmlDocument.CreateNode(XmlNodeType.Element, node, null);
                    childnode.InnerText = value;
                    root.AppendChild(childnode);
                }
                else
                {
                    xmlDocument.SelectSingleNode("/Root/" + node).InnerText = value;
                }
            }
            xmlDocument.Save(FilePath);
        }

        public string GetXMLData(string node)
        {
            FileName = Constant.CurrentTestName + ".xml";
            FilePath = Constant.Projectbasepath + @"\Xmls\" + FileName;

            string xpath = @"//" + node;
            try
            {
                xmlDocument.Load(FilePath);
                return xmlDocument.SelectSingleNode(xpath).InnerText;
            }
            catch(Exception e)
            {
                return "";
            }
        }

        public static string GetData(string tagname)
        {
            tagname = string.Format("{0}", tagname);
            return Constant.xmlDocument.GetElementsByTagName(tagname)[Constant.CurrentIteration].InnerText;
        }

        public static int GetIterations()
        {
            int iterationCount = (Constant.xmlDocument.GetElementsByTagName("iteration")).Count;
            return iterationCount;
        }

        public static void LoadXml(string path)
        {
           
            Constant.xmlDocument.Load(path);
            Constant.Iterations = Xml.GetIterations();
            Constant.CurrentIteration = 0;
        }

        private void Parse(String FilePath)
        {
            xmlDocument.Load(FilePath);
        }

        public Dictionary<String, String> Read(String FilePath)
        {
            Dictionary<String, String> XmlDict = new Dictionary<String, String>();
            Parse(FilePath);
            XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName("appSettings");
            if (xmlNodeList.Count == 0 || xmlNodeList.Count > 1)
                throw new Exception("Invalid xml format. None or Multiple appSettings detected");
            foreach (XmlNode EachNode in xmlNodeList[0].ChildNodes)
                if(!EachNode.NodeType.ToString().Equals("Comment"))
                    XmlDict.Add(EachNode.Attributes["key"].Value, EachNode.Attributes["value"].Value);
            return XmlDict;
        }
    }
}

