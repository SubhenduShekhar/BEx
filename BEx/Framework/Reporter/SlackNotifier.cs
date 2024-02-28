using BEx.Framework.Base;
using BEx.Framework.Executor;
using System;
using System.Net.Http;

namespace BEx.Framework.Reporter
{
    public class SlackNotifier
    {
        private Report report { get; set; }
        private APIBase aPIBase { get; set; }
        public SlackNotifier(Report reporter, APIBase aPIBase)
        {
            this.report = reporter;
            this.aPIBase = aPIBase;
        }
        public void PostMessage()
        {
            if (BaseClass.Config.FrameworkConfig.SlackNotify)
            {
                HttpResponseMessage httpResponseMessage = aPIBase.POST(BaseClass.Config.FrameworkConfig.WebHookUrl, GetMessagePayload());
                if (Convert.ToInt32(httpResponseMessage.StatusCode) == 200)
                    report.Log("Reports posted to slack channel successfully");
                else
                {
                    report.Log("Failed to post reports to slack channel");
                    report.Log(httpResponseMessage.ReasonPhrase);
                }
            }
            else
                report.Log("Slack notification is ignored... continuing");
        }
        private StringContent GetMessagePayload()
        {
            String messagePayload = "{" +
                "\"username\": \"automation-agent\"," +
                "\"channel\" : \"" + BaseClass.Config.FrameworkConfig.Channel + "\"," +
                "\"text\" : \"" + MessageBuilder() + "\"," +
                "\"icon_url\": \"https://content.presentermedia.com/files/animsp/00014000/14495/three_simple_gears_turning_md_wm.gif\"" +
            "}";
            return new StringContent(messagePayload);
        }

        private String MessageBuilder()
        {
            String message = "*" + BaseClass.Config.FrameworkConfig.ProjectName + "* executed successfully.\n" +
                "*Environment* : " + BaseClass.Config.FrameworkConfig.Environment + "\n" + 
                "*Run location* : " + BaseClass.Config.FrameworkConfig.RunLocation + "\n" +
                "Execution summary : \n";
            message += "Total test cases in scope : " + BaseClass.TestSuiteStatus.Keys.Count + "\n";
            foreach (String TestCase in BaseClass.TestSuiteStatus.Keys)
            {
                if(BaseClass.TestSuiteStatus[TestCase] == Report.Status.Pass)
                    message += ":large_green_circle: " + TestCase + " : " + BaseClass.TestSuiteStatus[TestCase].ToString() + "\n";
                if (BaseClass.TestSuiteStatus[TestCase] == Report.Status.Fail)
                    message += ":red_circle: " + TestCase + " : " + BaseClass.TestSuiteStatus[TestCase].ToString() + "\n";
                if (BaseClass.TestSuiteStatus[TestCase] == Report.Status.Skip)
                    message += ":large_yellow_circle: " + TestCase + " : " + BaseClass.TestSuiteStatus[TestCase].ToString() + "\n";

            }
            message += "cc ";
            foreach (String eachTags in BaseClass.Config.FrameworkConfig.Tag)
                message += "<" + eachTags + ">";
            return message;
        }
    }
}
