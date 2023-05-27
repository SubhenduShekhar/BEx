using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using BEx.Framework.Base;
using BEx.Framework.DataProvider.Formats;
using BEx.Framework.Reporter;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using System.Net;
using BEx.Framework.DataProvider;

namespace BEx.Framework.Executor.API
{
    public sealed class Scriptless
    {
        APIBase aPIBase { get; set; }
        TestCase TestStep { get; set; }
        String PayloadStructure { get; set; }
        public Scriptless(APIBase aPIBase)
        {
            this.aPIBase = aPIBase;
        }
        public void InitTestStep()
        {
            this.TestStep = aPIBase.data.TestStep;
        }
        public void CheckMandatory()
        {
            if (aPIBase.data.TestCase.Count == 0)
                BaseClass.Reporter.AddResult(Reporter.Report.Status.Info, "This test case as no steps");
            if (TestStep.SerialNo.ToString() == null)
                throw new Exception("SerialNo key cannot be null");
            if(TestStep.RequestType== null)
                throw new Exception("RequestType key cannot be null");
            if (TestStep.StatusCode.ToString() == null)
            {
                BaseClass.Reporter.AddResult(Report.Status.Info, "StatusCode not provided... proceeding with 200-OK");
                aPIBase.data.TestStep.StatusCode = 200;
            }
            if (TestStep.EndPoint == null)
                throw new Exception("EndPoint key cannot be null");
        }
        private void SetHeaders()
        {
            aPIBase.httpClient.DefaultRequestHeaders.Clear();
            if (TestStep.Headers != null)
            {
                if (TestStep.Headers.ContainsKey("from-file"))
                {
                    String FileContent = File.ReadAllText(Path.Combine(BaseClass.DataDirectory, TestStep.Headers["from-file"]));
                    JObject obj = JObject.Parse(FileContent);
                    TestStep.Headers.Clear();
                    List<String> keys = obj.Properties().Select(p => p.Name).ToList();
                    foreach(String eachKey in keys)
                    {
                        String val = obj.SelectToken("$." + eachKey).ToString();
                        if (obj.SelectToken("$." + eachKey).Contains("[Fetch:"))
                        {
                            String key = obj.SelectToken("$." + eachKey).ToString().Split("[Fetch:")[1].Split("]")[0];
                            val = obj.SelectToken("$." + eachKey).ToString().Replace("[Fetch:" + key + "]", Fetch(key));
                            aPIBase.httpClient.DefaultRequestHeaders.Add(eachKey, val);
                        }
                        else
                            aPIBase.httpClient.DefaultRequestHeaders.Add(eachKey, val);
                        TestStep.Headers.Add(eachKey, val);
                    }
                }
                else
                {
                    foreach (String headerKey in TestStep.Headers.Keys.ToList())
                    {
                        if (TestStep.Headers[headerKey].Contains("[Fetch:"))
                        {
                            String key = TestStep.Headers[headerKey].Split("[Fetch:")[1].Split("]")[0];
                            TestStep.Headers[headerKey] = TestStep.Headers[headerKey].Replace("[Fetch:" + key + "]", Fetch(key));
                            aPIBase.httpClient.DefaultRequestHeaders.Add(headerKey, TestStep.Headers[headerKey]);
                        }
                        else if (TestStep.Headers[headerKey].Contains("from-env"))
                        {
                            String[] SplittedVal = TestStep.Headers[headerKey].Split("[");
                            foreach (String EachVal in SplittedVal)
                            {
                                String FromEnvVal = EachVal.Split("]")[0];
                                if (FromEnvVal.Split(":").Count() == 2)
                                {
                                    String key = FromEnvVal.Split(":")[1];
                                    String Val = aPIBase.data.GetEnvironmentValue(BaseClass.Environment + "." + key);
                                    TestStep.Headers[headerKey] = TestStep.Headers[headerKey].Replace("[from-env:" + key + "]", Val);
                                }
                                if (FromEnvVal.Split(":").Count() == 3)
                                {
                                    String Env = FromEnvVal.Split(":")[1];
                                    String key = FromEnvVal.Split(":")[2];
                                    String Val = aPIBase.data.GetEnvironmentValue(Env + "." + key);
                                    TestStep.Headers[headerKey] = TestStep.Headers[headerKey].Replace("[from-env:" + Env + ":" + key + "]", Val);
                                }
                            }
                            aPIBase.httpClient.DefaultRequestHeaders.Add(headerKey, TestStep.Headers[headerKey]);
                        }
                        else
                            aPIBase.httpClient.DefaultRequestHeaders.Add(headerKey, TestStep.Headers[headerKey]);
                    }
                }
            }
            else
                BaseClass.Reporter.Log("No headers set for this call... continuing");
        }
        public Boolean ToExecute()
        {
            try
            {
                if (TestStep.ToExecute.ToLower().Contains("y"))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        public String GetEndPoint()
        {
            if (TestStep.EndPoint.Contains("[Fetch:") ||
                TestStep.EndPoint.Contains("[from-env:") ||
                TestStep.EndPoint.Contains("[from-file:"))
            {
                String[] splittedVal = TestStep.EndPoint.Split("[");
                String key;
                for (Int32 i = 0; i < splittedVal.Length; i++)
                {
                    if (splittedVal[i].StartsWith("from-file"))
                    {
                        key = splittedVal[i].Split(":")[1].Split("]")[0];
                        String FilePath = Path.Combine(BaseClass.DataDirectory, key);
                        String value = File.ReadAllText(FilePath);
                        splittedVal[i] = value;
                        TestStep.EndPoint = TestStep.EndPoint.Replace("[from-file:" + key + "]", value);
                    }
                    if (splittedVal[i].StartsWith("Fetch:"))
                    {
                        key = splittedVal[i].Split("Fetch:")[1].Split("]")[0];
                        splittedVal[i] = Fetch(key);
                        TestStep.EndPoint = TestStep.EndPoint.Replace("[Fetch:" + key + "]", Fetch(key));
                    }
                    if (splittedVal[i].StartsWith("from-env"))
                    {
                        key = splittedVal[i].Split("from-env:")[1].Split("]")[0];
                        String env = BaseClass.Environment;
                        String newKey = key;
                        if (key.Contains(":"))
                        {
                            env = key.Split(":")[0];
                            newKey = key.Split(":")[1];
                        }
                        splittedVal[i] = aPIBase.data.GetEnvironmentValue(env + "." + newKey);
                        TestStep.EndPoint = TestStep.EndPoint.Replace("[from-env:" + key + "]", aPIBase.data.GetEnvironmentValue(env + "." + newKey));
                    }
                }
            }
            if (TestStep.RequestType.Equals("GET") || TestStep.RequestType.Equals("DELETE"))
                TestStep.EndPoint += GetParamtersUrl();
            aPIBase.data.TestStep.EndPoint = TestStep.EndPoint;
            BaseClass.Reporter.Log("Requesting endpoint : " + TestStep.EndPoint);
            return TestStep.EndPoint;
        }
        public void Execute(String RequestType)
        {
            aPIBase.httpClient = new HttpClient();
            if (RequestType.Equals(APIBase.Request.GET.ToString()))
                GET();
            else if (RequestType.Equals(APIBase.Request.POST.ToString()))
                POST();
            else if (RequestType.Equals(APIBase.Request.PUT.ToString()))
                PUT();
            else if (RequestType.Equals(APIBase.Request.UPDATE.ToString()))
                PATCH();
            else if (RequestType.Equals(APIBase.Request.DELETE.ToString()))
                DELETE();
            aPIBase.httpClient.Dispose();
        }
        public void ShowPayload()
        {
            if (TestStep.ShowPayload)
                BaseClass.Reporter.Log(PayloadStructure);
            else
                BaseClass.Reporter.Log("Not showing payload... continuing");
        }
        public void GET()
        {
            if(TestStep.Loop == null && TestStep.DataSource == null)
            {
                SetHeaders();
                GetContentType();
                GetEndPoint();
                if(TestStep.DownloadFileAs == null)
                {
                    aPIBase.httpResponse = aPIBase.httpClient.GetAsync(aPIBase.data.TestStep.EndPoint).Result;
                    if (Convert.ToInt32(aPIBase.httpResponse.StatusCode) == aPIBase.data.TestStep.StatusCode)
                    {
                        aPIBase.Response = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;

                        BaseClass.Reporter.AddResult(Report.Status.Pass, "API executed successfully");
                        ReportResponse();
                        StoreValue();
                        Validate();
                        CustomCode(TestStep.CustomCode);
                    }
                    else
                    {
                        aPIBase.ReportError(aPIBase.httpResponse.StatusCode, aPIBase.httpResponse.ReasonPhrase);
                        BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                    }
                }
                else
                {
                    try
                    {
                        byte[] fileBytes = aPIBase.httpClient.GetByteArrayAsync(aPIBase.data.TestStep.EndPoint).Result;
                        Data.EnsureDirectory(BaseClass.DownloadPath);
                        File.WriteAllBytes(Path.Combine(BaseClass.DownloadPath, TestStep.DownloadFileAs), fileBytes);
                        BaseClass.Reporter.AddResult(Report.Status.Pass, "File downloaded at location : " + Path.Combine(BaseClass.DownloadPath, TestStep.DownloadFileAs));
                        EnsureFile();
                        CustomCode(TestStep.CustomCode);
                    }
                    catch(Exception e)
                    {
                        BaseClass.Reporter.AddResult(Report.Status.Fail, e.Message);
                        BaseClass.Reporter.AddResult(Report.Status.Fail, e.StackTrace);
                    }
                }
            }
            else if(TestStep.DataSource == null)
            {
                Boolean flag = false;
                CheckLoopMandatory();
                Int32 maxCount = Convert.ToInt32(TestStep.Loop["Count"]);
                Int32 count = 0;
                SetHeaders();
                GetContentType();
                GetEndPoint();
                do
                {
                    BaseClass.Reporter.Log("Running iteration : " + (count + 1));
                    aPIBase.httpResponse = aPIBase.httpClient.GetAsync(aPIBase.data.TestStep.EndPoint).Result;
                    aPIBase.Response = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;
                    count++;
                    if (ExecuteExpression(GetCondition()))
                    {
                        flag = true;
                        break;
                    }
                } 
                while (count < maxCount);

                if(!flag)
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, "Failed after " + (count + 1) + " iterations.");
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, "Condition check : " + GetCondition());
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }

                if (Convert.ToInt32(aPIBase.httpResponse.StatusCode) == aPIBase.data.TestStep.StatusCode)
                {
                    BaseClass.Reporter.AddResult(Report.Status.Pass, "API executed successfully");
                    ReportResponse();
                    StoreValue();
                    Validate();
                    CustomCode(TestStep.CustomCode);
                }
                else
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, aPIBase.httpResponse.ReasonPhrase);
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }
            }
        }
        public void POST()
        {
            if (TestStep.Loop == null && TestStep.DataSource == null)
            {
                aPIBase.httpClient = new HttpClient();
                SetHeaders();
                GetContentType();
                GetEndPoint();
                if (TestStep.ContentType.Equals("multipart/form-data"))
                    aPIBase.httpContent = GenerateMultipartFormPayload();
                else if (TestStep.ContentType.Equals("application/x-www-form-urlencoded"))
                    aPIBase.httpContent = GenerateFormUrlEncodedPayload();
                else
                    aPIBase.httpContent = GenerateAppJsonPayload();
                if (TestStep.DownloadFileAs == null)
                {
                    aPIBase.httpResponse = aPIBase.httpClient.PostAsync(aPIBase.data.TestStep.EndPoint, aPIBase.httpContent).Result;

                    ShowPayload();
                    if (Convert.ToInt32(aPIBase.httpResponse.StatusCode) == aPIBase.data.TestStep.StatusCode)
                    {
                        aPIBase.Response = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;
                        ReportResponse();
                        StoreValue();
                        Validate();
                        CustomCode(TestStep.CustomCode);
                    }
                    else
                    {
                        var error = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;
                        aPIBase.ReportError(aPIBase.httpResponse.StatusCode, aPIBase.httpResponse.ReasonPhrase);
                        BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                    }
                }
                else
                {
                    try
                    {
                        aPIBase.httpResponse = aPIBase.httpClient.PostAsync(aPIBase.data.TestStep.EndPoint, aPIBase.httpContent).Result;
                        byte[] bt = aPIBase.httpResponse.Content.ReadAsByteArrayAsync().Result;
                        Data.EnsureDirectory(Path.Combine(BaseClass.DownloadPath));
                        File.WriteAllBytes(Path.Combine(BaseClass.DownloadPath, TestStep.DownloadFileAs), bt);
                        BaseClass.Reporter.AddResult(Report.Status.Pass, "File downloaded at location : " + Path.Combine(BaseClass.DownloadPath, TestStep.DownloadFileAs));
                        EnsureFile();
                        CustomCode(TestStep.CustomCode);
                    }
                    catch (Exception e)
                    {
                        BaseClass.Reporter.AddResult(Report.Status.Fail, e.Message);
                        BaseClass.Reporter.AddResult(Report.Status.Fail, e.StackTrace);
                    }
                }
            }
            else if(TestStep.DataSource == null)
            {
                CheckLoopMandatory();
                Boolean flag = false;
                Int32 maxCount = Convert.ToInt32(TestStep.Loop["Count"]);
                Int32 count = 0;
                aPIBase.httpClient = new HttpClient();
                SetHeaders();
                GetContentType();
                GetEndPoint();
                if (TestStep.ContentType.Equals("multipart/form-data"))
                    aPIBase.httpContent = GenerateMultipartFormPayload();
                else if (TestStep.ContentType.Equals("application/x-www-form-urlencoded"))
                    aPIBase.httpContent = GenerateFormUrlEncodedPayload();
                else
                    aPIBase.httpContent = GenerateAppJsonPayload();
                ShowPayload();
                do
                {
                    aPIBase.httpResponse = aPIBase.httpClient.PostAsync(aPIBase.data.TestStep.EndPoint, aPIBase.httpContent).Result;
                    aPIBase.Response = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;
                    count++;
                    if (ExecuteExpression(GetCondition()))
                    {
                        flag = true;
                        break;
                    }
                } 
                while (count < maxCount && !ExecuteExpression(GetCondition()));

                if (!flag)
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, "Failed after " + (count + 1) + " iterations.");
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, "Condition check : " + GetCondition());
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }

                if (Convert.ToInt32(aPIBase.httpResponse.StatusCode) == aPIBase.data.TestStep.StatusCode)
                {
                    ReportResponse();
                    StoreValue();
                    Validate();
                    CustomCode(TestStep.CustomCode);
                }
                else
                {
                    var error = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, aPIBase.httpResponse.ReasonPhrase);
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }
            }
        }
        public void PUT()
        {
            if (TestStep.Loop == null && TestStep.DataSource == null)
            {
                aPIBase.httpClient = new HttpClient();
                SetHeaders();
                GetContentType();
                GetEndPoint();
                if (TestStep.ContentType.Equals("multipart/form-data"))
                    aPIBase.httpContent = GenerateMultipartFormPayload();
                else if (TestStep.ContentType.Equals("application/x-www-form-urlencoded"))
                    aPIBase.httpContent = GenerateFormUrlEncodedPayload();
                else
                    aPIBase.httpContent = GenerateAppJsonPayload();
                ShowPayload();

                aPIBase.httpResponse = aPIBase.httpClient.PutAsync(aPIBase.data.TestStep.EndPoint, aPIBase.httpContent).Result;

                if (Convert.ToInt32(aPIBase.httpResponse.StatusCode) == aPIBase.data.TestStep.StatusCode)
                {
                    aPIBase.Response = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;
                    ReportResponse();
                    StoreValue();
                    Validate();
                    CustomCode(TestStep.CustomCode);
                }
                else
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, aPIBase.httpResponse.ReasonPhrase);
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }
            }
            else if (TestStep.DataSource == null)
            {
                Boolean flag = false;
                CheckLoopMandatory();
                Int32 maxCount = Convert.ToInt32(TestStep.Loop["Count"]);
                Int32 count = 0;
                SetHeaders();
                GetContentType();
                GetEndPoint();
                if (TestStep.ContentType.Equals("multipart/form-data"))
                    aPIBase.httpContent = GenerateMultipartFormPayload();
                else if (TestStep.ContentType.Equals("application/x-www-form-urlencoded"))
                    aPIBase.httpContent = GenerateFormUrlEncodedPayload();
                else
                    aPIBase.httpContent = GenerateAppJsonPayload();
                ShowPayload();
                do
                {
                    BaseClass.Reporter.Log("Running iteration : " + (count + 1));
                    aPIBase.httpResponse = aPIBase.httpClient.PutAsync(aPIBase.data.TestStep.EndPoint, aPIBase.httpContent).Result;
                    aPIBase.Response = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;
                    count++;
                    if (ExecuteExpression(GetCondition()))
                    {
                        flag = true;
                        break;
                    }
                }
                while (count < maxCount);

                if (!flag)
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, "Failed after " + (count + 1) + " iterations.");
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, "Condition check : " + GetCondition());
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }

                if (Convert.ToInt32(aPIBase.httpResponse.StatusCode) == aPIBase.data.TestStep.StatusCode)
                {
                    BaseClass.Reporter.AddResult(Report.Status.Pass, "API executed successfully");
                    ReportResponse();
                    StoreValue();
                    Validate();
                    CustomCode(TestStep.CustomCode);
                }
                else
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, aPIBase.httpResponse.ReasonPhrase);
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }
            }
        }
        public void PATCH()
        {
            if (TestStep.Loop == null && TestStep.DataSource == null)
            {
                aPIBase.httpClient = new HttpClient();
                SetHeaders();
                GetContentType();
                GetEndPoint();
                if (TestStep.ContentType.Equals("multipart/form-data"))
                    aPIBase.httpContent = GenerateMultipartFormPayload();
                else if (TestStep.ContentType.Equals("application/x-www-form-urlencoded"))
                    aPIBase.httpContent = GenerateFormUrlEncodedPayload();
                else
                    aPIBase.httpContent = GenerateAppJsonPayload();
                aPIBase.httpResponse = aPIBase.httpClient.PatchAsync(aPIBase.data.TestStep.EndPoint, GenerateAppJsonPayload()).Result;

                ShowPayload();
                if (Convert.ToInt32(aPIBase.httpResponse.StatusCode) == aPIBase.data.TestStep.StatusCode)
                {
                    aPIBase.Response = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;
                    ReportResponse();
                    StoreValue();
                    Validate();
                    CustomCode(TestStep.CustomCode);
                }
                else
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, aPIBase.httpResponse.ReasonPhrase);
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }
            }
            else if (TestStep.DataSource == null)
            {
                Boolean flag = false;
                CheckLoopMandatory();
                Int32 maxCount = Convert.ToInt32(TestStep.Loop["Count"]);
                Int32 count = 0;
                SetHeaders();
                GetContentType();
                GetEndPoint();
                if (TestStep.ContentType.Equals("multipart/form-data"))
                    aPIBase.httpContent = GenerateMultipartFormPayload();
                else if (TestStep.ContentType.Equals("application/x-www-form-urlencoded"))
                    aPIBase.httpContent = GenerateFormUrlEncodedPayload();
                else
                    aPIBase.httpContent = GenerateAppJsonPayload();
                ShowPayload();
                do
                {
                    BaseClass.Reporter.Log("Running iteration : " + (count + 1));
                    aPIBase.httpResponse = aPIBase.httpClient.PatchAsync(aPIBase.data.TestStep.EndPoint, GenerateAppJsonPayload()).Result;
                    aPIBase.Response = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;
                    count++;
                    if (ExecuteExpression(GetCondition()))
                    {
                        flag = true;
                        break;
                    }
                }
                while (count < maxCount);

                if (!flag)
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, "Failed after " + (count + 1) + " iterations.");
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, "Condition check : " + GetCondition());
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }

                if (Convert.ToInt32(aPIBase.httpResponse.StatusCode) == aPIBase.data.TestStep.StatusCode)
                {
                    BaseClass.Reporter.AddResult(Report.Status.Pass, "API executed successfully");
                    ReportResponse();
                    StoreValue();
                    Validate();
                    CustomCode(TestStep.CustomCode);
                }
                else
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, aPIBase.httpResponse.ReasonPhrase);
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }
            }
        }
        public void DELETE()
        {
            if (TestStep.Loop == null && TestStep.DataSource == null)
            {
                SetHeaders();
                GetContentType();
                GetEndPoint();
                aPIBase.httpResponse = aPIBase.httpClient.DeleteAsync(aPIBase.data.TestStep.EndPoint).Result;
                if (Convert.ToInt32(aPIBase.httpResponse.StatusCode) == aPIBase.data.TestStep.StatusCode)
                {
                    aPIBase.Response = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;
                    BaseClass.Reporter.AddResult(Report.Status.Pass, "API executed successfully");
                    ReportResponse();
                    StoreValue();
                    Validate();
                    CustomCode(TestStep.CustomCode);
                }
                else
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, aPIBase.httpResponse.ReasonPhrase);
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }
            }
            else if (TestStep.DataSource == null)
            {
                Boolean flag = false;
                CheckLoopMandatory();
                Int32 maxCount = Convert.ToInt32(TestStep.Loop["Count"]);
                Int32 count = 0;
                SetHeaders();
                GetContentType();
                GetEndPoint();
                do
                {
                    BaseClass.Reporter.Log("Running iteration : " + (count + 1));
                    aPIBase.httpResponse = aPIBase.httpClient.DeleteAsync(aPIBase.data.TestStep.EndPoint).Result;
                    aPIBase.Response = aPIBase.httpResponse.Content.ReadAsStringAsync().Result;
                    count++;
                    if (ExecuteExpression(GetCondition()))
                    {
                        flag = true;
                        break;
                    }
                }
                while (count < maxCount);

                if (!flag)
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, "Failed after " + (count + 1) + " iterations.");
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, "Condition check : " + GetCondition());
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }

                if (Convert.ToInt32(aPIBase.httpResponse.StatusCode) == aPIBase.data.TestStep.StatusCode)
                {
                    BaseClass.Reporter.AddResult(Report.Status.Pass, "API executed successfully");
                    ReportResponse();
                    StoreValue();
                    Validate();
                    CustomCode(TestStep.CustomCode);
                }
                else
                {
                    aPIBase.ReportError(aPIBase.httpResponse.StatusCode, aPIBase.httpResponse.ReasonPhrase);
                    BaseClass.TestSuiteStatus[aPIBase.TestName] = Report.Status.Fail;
                }
            }
        }
        public String GetJsonValue(String JPath)
        {
            if (JPath.StartsWith("response:"))
            {
                var a = JsonConvert.SerializeObject(aPIBase.Response);
                if (JPath.Split("response:")[1].StartsWith("root"))
                    JPath = "response:$" + JPath.Split("response:")[1].Split("root")[1];
                try
                {
                    JArray obj = JArray.Parse(aPIBase.Response.ToString());
                    return obj.SelectToken(JPath.Split("response:")[1]).ToString();
                }
                catch
                {
                    JObject obj = Json.Read(aPIBase.Response.ToString(), true);
                    return obj.SelectToken(JPath.Split("response:")[1]).ToString();
                }
            }
            else if (JPath.StartsWith("request:"))
            {
                if (JPath.Split("request:")[1].StartsWith("root"))
                    JPath = "$" + JPath.Split("request:")[1].Split("root")[1];
                if (TestStep.RequestType == "POST" || TestStep.RequestType == "PUT")
                {
                    try
                    {
                        JObject obj = Json.Read(PayloadStructure, true);
                        return obj.SelectToken(JPath).ToString();
                    }
                    catch
                    {
                        JArray arr = JArray.Parse(PayloadStructure);
                        return arr.SelectToken(JPath).ToString();
                    }
                }
                else
                {
                    if (TestStep.EndPoint.Contains(JPath.Split("$.")[1]))
                    {
                        String Parameters = TestStep.EndPoint.Split("?")[1];
                        String Value = Parameters.Split(JPath.Split("$.")[1] + "=")[1];
                        if (Value.Contains("&"))
                            Value = Value.Split("&")[0];
                        return Value;
                    }
                    else
                    {
                        throw new Exception("Cannot find " + JPath + " in GET request with " + TestStep.EndPoint);
                    }
                }
            }
            else if (JPath.StartsWith("raw-response"))
            {
                if (aPIBase.Response.StartsWith("\"") && aPIBase.Response.EndsWith("\""))
                    return aPIBase.Response.Substring(1, aPIBase.Response.Length - 2);
                else
                    return aPIBase.Response;
            }
            else
                throw new KeyNotFoundException("Key not found : " + JPath);
        }
        public void StoreValue()
        {
            if (TestStep.Store != null)
            {
                foreach (Dictionary<String, String> eachStore in TestStep.Store)
                {
                    foreach (String keys in eachStore.Keys)
                    {
                        if (keys.StartsWith("to-file"))
                            ToFile(eachStore[keys]);
                        else
                        {
                            String val = GetJsonValue(eachStore[keys]);
                            try
                            {
                                aPIBase.data.sharedObjects.Add(keys, val);
                            }
                            catch(ArgumentException e)
                            {
                                BaseClass.Reporter.Log("Data with key : " + keys + " already exists. Replacing the value with current value");
                                aPIBase.data.sharedObjects[keys] = val;
                            }
                            BaseClass.Reporter.AddResult(Report.Status.Pass, "Added " + keys + " : " + val + " in framework memory.");
                        }
                    }
                }
            }
            else
                BaseClass.Reporter.Log("Nothing to store... Continuing");
        }
        public dynamic Fetch(String Key)
        {
            try
            {
                return aPIBase.data.sharedObjects[Key];
            }
            catch(KeyNotFoundException e)
            {
                return null;
            }
        }
        public void ReportResponse()
        {
            if (TestStep.ReportResponse)
                BaseClass.Reporter.AddResult(Report.Status.Pass, aPIBase.Response);
            else
                BaseClass.Reporter.Log("Not printing response body in report");
        }
        public void GetContentType()
        {
            if(TestStep.Headers != null)
            {
                Boolean flag = false;
                foreach (String eachHeader in TestStep.Headers.Keys)
                {
                    if (eachHeader.Equals("ContentType"))
                    {
                        TestStep.ContentType = TestStep.Headers[eachHeader];
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    BaseClass.Reporter.Log("Setting default ContentType as application/json");
                    TestStep.ContentType = "application/json";
                }
                aPIBase.data.TestStep.ContentType = TestStep.ContentType;
            }
            else
            {
                BaseClass.Reporter.Log("Setting default ContentType as application/json");
                TestStep.ContentType = "application/json";
            }
        }
        private StringContent GenerateAppJsonPayload()
        {
            String Payload = null;
            if (TestStep.Payload.ContainsKey("from-file"))
            {
                BaseClass.Reporter.Log("Fetching payload from file");
                try
                {
                    Payload = Json.Read(Path.Combine(BaseClass.DataDirectory, TestStep.Payload["from-file"])).ToString();
                }
                catch
                {
                    try
                    {
                        Payload = JArray.Parse(File.ReadAllText(Path.Combine(BaseClass.DataDirectory, TestStep.Payload["from-file"]))).ToString();
                    }
                    catch
                    {
                        Payload = File.ReadAllText(Path.Combine(BaseClass.DataDirectory, TestStep.Payload["from-file"]));
                    }
                }
                while (Payload.Contains("[from-env:")
                        || Payload.Contains("[Fetch:"))
                {
                    if (Payload.Contains("Fetch:"))
                    {
                        String key = Payload.Split("Fetch:")[1].Split("]")[0];
                        Payload = Payload.Replace("[Fetch:" + key + "]", Fetch(key));
                    }
                    if (Payload.Contains("from-env:"))
                    {
                        String key = Payload.Split("from-env:")[1].Split("]")[0];
                        Payload = Payload.Replace("[from-env:" + key + "]", aPIBase.data.GetEnvironmentValue(BaseClass.Environment + "." + key));
                    }
                }
                if (Payload.Contains("[Fetch"))
                {
                    do
                    {
                        String key = Payload.Split("[Fetch:")[1].Split("]")[0];
                        try
                        {
                            Payload = Payload.Replace("[Fetch:" + key + "]", Fetch(key));
                        }
                        catch
                        {
                            BaseClass.Reporter.Log("Key : " + key + " not found in list of stored values");
                        }
                    } while (Payload.Contains("[Fetch:"));
                }
                if(Payload.Contains("[from-env:"))
                {
                    for (int i = 0; i < Payload.Split("[from-env:").Length; i++)
                    {
                        try
                        {
                            String key = Payload.Split("[from-env:")[1].Split("]")[0];
                            String value = aPIBase.data.GetEnvironmentValue(BaseClass.Environment + "." + key);
                            Payload = Payload.Replace("[from-env:" + key + "]", value);
                        }
                        catch { }
                    }
                }
                PayloadStructure = Payload.ToString();
                return new StringContent(Payload, Encoding.UTF8, TestStep.ContentType);
            }
            if(TestStep.Payload.ContainsKey("from-code"))
            {
                BaseClass.Reporter.Log("Fetching payload from code execution");
                try
                {
                    Payload = CustomCode(Path.Combine(BaseClass.DataDirectory, TestStep.Payload["from-code"]));
                    BaseClass.Reporter.Log("Custom code returned payload : \n" + Payload);
                }
                catch
                {
                    BaseClass.Reporter.AddResult(Report.Status.Error, "Custom code returned null value...");
                }
                PayloadStructure = Payload.ToString();
                return new StringContent(Payload, Encoding.UTF8, TestStep.ContentType);
            }
            else
            {
                foreach (String Keys in TestStep.Payload.Keys.ToList())
                {
                    while(TestStep.Payload[Keys].Contains("[from-file:")
                        || TestStep.Payload[Keys].Contains("[from-code:")
                        || TestStep.Payload[Keys].Contains("[from-env:")
                        || TestStep.Payload[Keys].Contains("[Fetch:"))
                    {
                        if (TestStep.Payload[Keys].Contains("[from-file:"))
                        {
                            String key = TestStep.Payload[Keys].Split("[from-file:")[1].Split("]")[0];
                            String fileContent = File.ReadAllText(Path.Combine(BaseClass.DataDirectory, key));
                            try
                            {
                                BaseClass.Reporter.Log("Trying with object parse");
                                TestStep.Payload[Keys] = TestStep.Payload[Keys].Replace(
                                    "[from-file:" + key + "]", JObject.Parse(fileContent).ToString());
                            }
                            catch
                            {
                                BaseClass.Reporter.Log("Failed with object parse");
                                try
                                {
                                    BaseClass.Reporter.Log("Trying with array parse");
                                    var a = JsonConvert.SerializeObject(TestStep.Payload);
                                    TestStep.Payload[Keys] = JArray.Parse(fileContent).ToString();
                                }
                                catch
                                {
                                    BaseClass.Reporter.Log("Failed with object parse");
                                    BaseClass.Reporter.Log("Setting raw content");
                                    TestStep.Payload[Keys] = fileContent;
                                }
                            }
                        }
                        if (TestStep.Payload[Keys].Contains("[from-code:"))
                        {
                            BaseClass.Reporter.Log("Fetching payload from code execution");
                            try
                            {
                                String codePath = TestStep.Payload[Keys].Split("from-code:")[1].Split("]")[0];
                                String result = CustomCode(Path.Combine(BaseClass.DataDirectory, codePath));
                                BaseClass.Reporter.Log("Custom code returned result for parameter : \n" + result);
                                TestStep.Payload[Keys] = TestStep.Payload[Keys].Replace("[from-code:" + codePath + "]", result);
                            }
                            catch
                            {
                                BaseClass.Reporter.AddResult(Report.Status.Error, "Custom code returned null value...");
                            }
                        }
                        if (TestStep.Payload[Keys].Contains("Fetch:"))
                        {
                            String key = TestStep.Payload[Keys].Split("Fetch:")[1].Split("]")[0];
                            TestStep.Payload[Keys] = TestStep.Payload[Keys].Replace("[Fetch:" + key + "]", Fetch(key));
                        }
                        if (TestStep.Payload[Keys].Contains("from-env:"))
                        {
                            String key = TestStep.Payload[Keys].Split("from-env:")[1].Split("]")[0];
                            TestStep.Payload[Keys] = TestStep.Payload[Keys].Replace("[from-env:" + key + "]", aPIBase.data.GetEnvironmentValue(BaseClass.Environment + "." + key));
                        }
                    }
                    if (TestStep.Payload[Keys].Contains("GetDate("))
                    {
                        String format = "";
                        try
                        {
                            format = TestStep.Payload[Keys].Split("GetDate(")[1].Split(")")[0];
                        }
                        catch { }
                        TestStep.Payload[Keys] = TestStep.Payload[Keys].Replace("GetDate(" + format + ")", Utils.GetDate(format));
                    }
                }
            }
            PayloadStructure = GeneratePayloadStructure().ToString();
            return new StringContent(PayloadStructure, Encoding.UTF8, TestStep.ContentType);
        }
        private MultipartFormDataContent GenerateMultipartFormPayload()
        {
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();

            if (TestStep.Payload.ContainsKey("from-file"))
            {
                JObject obj = Json.Read(Path.Combine(BaseClass.DataDirectory, TestStep.Payload["from-file"]));
                List<String> keys = obj.Properties().Select(p => p.Name).ToList();
                foreach(String eachKey in keys)
                {
                    if (obj.SelectToken("$." + eachKey).ToString().Contains("upload-file:"))
                    {
                        String FilePath = Path.Combine(BaseClass.DataDirectory, 
                            obj.SelectToken("$." + eachKey).ToString().Split("upload-file:")[1]);
                        Byte[] FileBytes = File.ReadAllBytes(FilePath);
                        multipartFormDataContent.Add(new ByteArrayContent(FileBytes, 0, FileBytes.Length), eachKey, Path.GetFileName(FilePath));
                    }
                    else if(obj.SelectToken("$." + eachKey).ToString().Contains("[Fetch:"))
                    {
                        String key = obj.SelectToken("$." + eachKey).ToString().Split("[Fetch:")[1].Split("]")[0];
                        multipartFormDataContent.Add(new StringContent(Fetch(key)), eachKey);
                    }
                    else
                        multipartFormDataContent.Add(new StringContent(obj.SelectToken("$." + eachKey).ToString()),
                            eachKey);
                }
            }
            if (TestStep.Payload.ContainsKey("from-code"))
            {
                BaseClass.Reporter.Log("Fetching payload from code execution");
                try
                {
                    String Payload = CustomCode(Path.Combine(BaseClass.DataDirectory, TestStep.Payload["from-code"]));
                    BaseClass.Reporter.Log("Custom code returned payload : \n" + Payload);
                    try
                    {
                        JObject obj = Json.Read(Payload);
                        List<String> keys = obj.Properties().Select(p => p.Name).ToList();
                        foreach (String eachKey in keys)
                        {
                            if (obj.SelectToken("$." + eachKey).ToString().Contains("upload-file:"))
                            {
                                String FilePath = Path.Combine(BaseClass.DataDirectory,
                                    obj.SelectToken("$." + eachKey).ToString().Split("upload-file:")[1]);
                                Byte[] FileBytes = File.ReadAllBytes(FilePath);
                                multipartFormDataContent.Add(new ByteArrayContent(FileBytes, 0, FileBytes.Length), eachKey, Path.GetFileName(FilePath));
                            }
                            else if (obj.SelectToken("$." + eachKey).ToString().Contains("[Fetch:"))
                            {
                                String key = obj.SelectToken("$." + eachKey).ToString().Split("[Fetch:")[1].Split("]")[0];
                                multipartFormDataContent.Add(new StringContent(Fetch(key)), eachKey);
                            }
                            else
                                multipartFormDataContent.Add(new StringContent(obj.SelectToken("$." + eachKey).ToString()),
                                    eachKey);
                        }
                    }
                    catch
                    {
                        BaseClass.Reporter.Log("JArray for multipartformdata() is not yet implemented... contact developer", Report.Status.Error);
                    }
                }
                catch
                {
                    BaseClass.Reporter.AddResult(Report.Status.Error, "Custom code returned null value...");
                }
            }
            else
            {
                foreach (String keys in TestStep.Payload.Keys)
                {
                    if (TestStep.Payload[keys].Contains("Fetch:"))
                    {
                        String[] val = TestStep.Payload[keys].ToString().Split("[Fetch:");
                        String decodedValue = "";
                        for (Int32 i = 0; i < val.Length; i++)
                        {
                            if (val[i].Contains("[Fetch:"))
                                val[i] = val[i].Replace("[Fetch:", "");
                            else if (val[i].Contains("]"))
                                val[i] = val[i].Replace("]", Fetch(val[i].Split("]")[1]));
                            decodedValue += val[i];
                        }
                        multipartFormDataContent.Add(new StringContent(decodedValue), keys);
                    }
                    else if (TestStep.Payload[keys].Contains("upload-file:"))
                    {
                        if (TestStep.Payload[keys].Split("upload-file:")[1].StartsWith("["))
                        {
                            BaseClass.Reporter.Log("Multiple files to be uploaded");
                            String[] FilesArray = TestStep.Payload[keys].Split("upload-file:")[1].Split("[")[1].Split("]")[0].Split(",");
                            foreach(String EachFile in FilesArray)
                            {
                                String FilePath = Path.Combine(BaseClass.DataDirectory, EachFile);
                                Byte[] FileBytes = File.ReadAllBytes(FilePath);
                                multipartFormDataContent.Add(new ByteArrayContent(FileBytes, 0, FileBytes.Length), keys, Path.GetFileName(FilePath));
                            }
                        }
                        else
                        {
                            String FilePath = Path.Combine(BaseClass.DataDirectory,
                                TestStep.Payload[keys].Split("upload-file:")[1]);
                            Byte[] FileBytes = File.ReadAllBytes(FilePath);
                            multipartFormDataContent.Add(new ByteArrayContent(FileBytes, 0, FileBytes.Length), keys, Path.GetFileName(FilePath));
                        }
                    }
                    else if (TestStep.Payload[keys].ToString().Contains("from-env:"))
                    {
                        String[] SplittedVal = TestStep.Payload[keys].ToString().Split("[from-env");
                        String Val = "";
                        foreach (String EachVal in SplittedVal)
                        {
                            if (EachVal.StartsWith(":"))
                            {
                                if(EachVal.Split(":").Count() == 2)
                                {
                                    String key = EachVal.Substring(1).Split("]")[0];
                                    if (key.Contains("["))
                                        key += "]";
                                    String decodedVal = EachVal.Replace(":" + key + "]", aPIBase.data.GetEnvironmentValue(BaseClass.Environment + "." + key));
                                    Val += decodedVal;
                                }
                                else if(EachVal.Split(":").Count() == 3)
                                {
                                    String Env = EachVal.Substring(1).Split(":")[0];
                                    String key = EachVal.Substring(1).Split(":")[1].Split("]")[0];
                                    if (key.Contains("["))
                                        key += "]";
                                    String decodedVal = EachVal.Replace(":" + Env + ":" + key + "]", aPIBase.data.GetEnvironmentValue(Env + "." + key));
                                    Val += decodedVal;
                                }
                            }
                        }
                    }
                    else if (TestStep.Payload[keys].Contains("from-file:"))
                    {
                        foreach(String eachFile in TestStep.Payload[keys].Split("[from-file:"))
                        {
                            if(eachFile != "")
                            {
                                String FilePath = Path.Combine(BaseClass.DataDirectory, eachFile.Split("]")[0]);
                                String FileContent = File.ReadAllText(FilePath);
                                TestStep.Payload[keys] = TestStep.Payload[keys].Replace("[from-file:" + eachFile.Split("]")[0] + "]", FileContent);
                            }
                        }
                    }
                    else if (TestStep.Payload[keys].Contains("from-code:"))
                    {
                        BaseClass.Reporter.Log("Fetching payload from code execution");
                        try
                        {
                            foreach(String eachKey in TestStep.Payload[keys].Split("[from-code:"))
                            {
                                if(eachKey != "")
                                {
                                    String codePath = eachKey.Split("]")[0];
                                    String result = CustomCode(Path.Combine(BaseClass.DataDirectory, codePath));
                                    BaseClass.Reporter.Log("Custom code returned result for parameter : \n" + result);
                                    TestStep.Payload[keys] = TestStep.Payload[keys].Replace("[from-code:" + codePath + "]", result);
                                }
                            }
                        }
                        catch
                        {
                            BaseClass.Reporter.AddResult(Report.Status.Error, "Custom code returned null value...");
                        }
                    }
                    else if (TestStep.Payload[keys].Contains("GetDate("))
                    {
                        String format = "";
                        try
                        {
                            format = TestStep.Payload[keys].Split("GetDate(")[1].Split(")")[0];
                        }
                        catch { }
                        TestStep.Payload[keys] = TestStep.Payload[keys].Replace("GetDate(" + format + ")", Utils.GetDate(format));
                    }
                    else
                        multipartFormDataContent.Add(new StringContent(TestStep.Payload[keys]), keys);
                }
            }
            return multipartFormDataContent;
        }
        private FormUrlEncodedContent GenerateFormUrlEncodedPayload()
        {
            Dictionary<String, String> formData = new Dictionary<String, String>();

            if (TestStep.Payload.ContainsKey("from-file"))
            {
                JObject obj = Json.Read(Path.Combine(BaseClass.DataDirectory, TestStep.Payload["from-file"]));
                List<String> keys = obj.Properties().Select(p => p.Name).ToList();
                foreach (String eachKey in keys)
                {
                    if (obj.SelectToken("$." + eachKey).ToString().Contains("[Fetch:"))
                    {
                        String key = obj.SelectToken("$." + eachKey).ToString().Split("[Fetch:")[1].Split("]")[0];
                        formData.Add(eachKey, Fetch(key));
                    }
                    else
                        formData.Add(eachKey, obj.SelectToken("$." + eachKey).ToString());
                }
            }
            else
            {
                foreach (String keys in TestStep.Payload.Keys)
                {
                    if (TestStep.Payload[keys].Contains("Fetch:"))
                    {
                        String[] val = TestStep.Payload[keys].ToString().Split("[Fetch:");
                        String decodedValue = "";
                        for (Int32 i = 0; i < val.Length; i++)
                        {
                            if (val[i].Contains("[Fetch:"))
                                val[i] = val[i].Replace("[Fetch:", "");
                            else if (val[i].Contains("]"))
                                val[i] = val[i].Replace("]", Fetch(val[i].Split("]")[1]));
                            decodedValue += val[i];
                        }
                        formData.Add(keys, decodedValue);
                    }
                    else if (TestStep.Payload[keys].ToString().Contains("from-env:"))
                    {
                        String[] SplittedVal = TestStep.Payload[keys].ToString().Split("[from-env");
                        String Val = "";
                        foreach (String EachVal in SplittedVal)
                        {
                            if (EachVal.StartsWith(":"))
                            {
                                if(EachVal.Split(":").Count() == 2)
                                {
                                    String key = EachVal.Substring(1).Split("]")[0];
                                    if (key.Contains("["))
                                        key += "]";
                                    String decodedVal = EachVal.Replace(":" + key + "]", aPIBase.data.GetEnvironmentValue(BaseClass.Environment + "." + key));
                                    Val += decodedVal;
                                }
                                else if(EachVal.Split(":").Count() == 3)
                                {
                                    String Env = EachVal.Substring(1).Split(":")[0];
                                    String key = EachVal.Substring(1).Split(":")[1].Split("]")[0];
                                    if (key.Contains("["))
                                        key += "]";
                                    String decodedVal = EachVal.Replace(":" + Env + ":" + key + "]", aPIBase.data.GetEnvironmentValue(Env + "." + key));
                                    Val += decodedVal;
                                }
                            }
                        }
                        formData.Add(keys, Val);
                    }
                    else if (TestStep.Payload[keys].Contains("from-file:"))
                    {
                        foreach (String eachFile in TestStep.Payload[keys].Split("[from-file:"))
                        {
                            if (eachFile != "")
                            {
                                String FilePath = Path.Combine(BaseClass.DataDirectory, eachFile.Split("]")[0]);
                                String FileContent = File.ReadAllText(FilePath);
                                TestStep.Payload[keys] = TestStep.Payload[keys].Replace("[from-file:" + eachFile.Split("]")[0] + "]", FileContent);
                            }
                        }
                    }
                    else if (TestStep.Payload[keys].Contains("from-code:"))
                    {
                        BaseClass.Reporter.Log("Fetching payload from code execution");
                        try
                        {
                            foreach (String eachKey in TestStep.Payload[keys].Split("[from-code:"))
                            {
                                if (eachKey != "")
                                {
                                    String codePath = eachKey.Split("]")[0];
                                    String result = CustomCode(Path.Combine(BaseClass.DataDirectory, codePath));
                                    BaseClass.Reporter.Log("Custom code returned result for parameter : \n" + result);
                                    TestStep.Payload[keys] = TestStep.Payload[keys].Replace("[from-code:" + codePath + "]", result);
                                }
                            }
                        }
                        catch
                        {
                            BaseClass.Reporter.AddResult(Report.Status.Error, "Custom code returned null value...");
                        }
                    }
                    else if (TestStep.Payload[keys].Contains("GetDate("))
                    {
                        String format = "";
                        try
                        {
                            format = TestStep.Payload[keys].Split("GetDate(")[1].Split(")")[0];
                        }
                        catch { }
                        TestStep.Payload[keys] = TestStep.Payload[keys].Replace("GetDate(" + format + ")", Utils.GetDate(format));
                    }
                    else
                        formData.Add(keys, TestStep.Payload[keys]);
                }
                TestStep.Payload = formData;
            }
            return new FormUrlEncodedContent(TestStep.Payload);
        }
        private String GetRealDataFromPath(String RawData)
        {
            String actual = null;
            if (RawData.Contains("[Fetch:"))
            {
                String actualKey = RawData.Split("[Fetch:")[1].Split("]")[0];
                actual = aPIBase.data.sharedObjects[actualKey];
            }
            else if (RawData.Contains("response") || RawData.Contains("request:"))
                actual = GetJsonValue(RawData);
            else
            {
                BaseClass.Reporter.Log("Validating direct data");
                actual = RawData;
            }
            return actual;
        }
        /// <summary>
        /// Implementation for Validate and Expected keys<br/>
        /// Accepted Validate : Fetch:, response:, request, raw data<br/>
        /// Accepted Expected : raw expected data<br/>
        /// </summary>
        public void Validate()
        {
            if (TestStep.Validate != null && TestStep.Expected == null)
                BaseClass.Reporter.AddResult(Report.Status.Warning, "Expected is required if Validate is provided");
            else if (TestStep.Validate == null && TestStep.Expected != null)
                BaseClass.Reporter.AddResult(Report.Status.Warning, "Validate is required if Expected is provided");
            else
            {
                if (TestStep.Validate != null)
                {
                    if (TestStep.Expected.Count == TestStep.Validate.Count)
                    {
                        for (Int32 i = 0; i < TestStep.Expected.Count; i++)
                        {
                            String expected = GetRealDataFromPath(TestStep.Expected[i]);
                            String actual = GetRealDataFromPath(TestStep.Validate[i]);
                            if (expected == actual)
                                BaseClass.Reporter.AddResult(Report.Status.Pass, "Validation passed for " + expected);
                            else
                                BaseClass.Reporter.AddResult(Report.Status.Fail, "Expected : " + expected + ", Actual : " + actual);
                        }
                    }
                    else
                        BaseClass.Reporter.AddResult(Report.Status.Error, "Expected and actual data count mismatch... Please check");
                }
                else
                    BaseClass.Reporter.Log("No fields to validate... continuing");
            }
        }
        /// <summary>
        /// Creates file and extracts data to the file
        /// </summary>
        /// <param name="RawCode">Raw store value from test step</param>
        public void ToFile(String RawCode)
        {
            String jsonPath = RawCode.Split("response:")[1];
            String FileDirectory = Path.Combine(BaseClass.DataDirectory, "Extracts");
            if (!Directory.Exists(FileDirectory))
                Directory.CreateDirectory(FileDirectory);
            FileStream fileStream = File.Create(Path.Combine(FileDirectory, aPIBase.data.TestStepName + "_" + jsonPath + ".txt"));
            String StoreText = GetJsonValue(RawCode);
            fileStream.Write(Encoding.ASCII.GetBytes(StoreText));
            fileStream.Close();
            BaseClass.Reporter.AddResult(Report.Status.Pass, "Extracted data to " + Path.Combine(FileDirectory, aPIBase.data.TestStepName + "_" + jsonPath + ".txt"));
        }
        private JObject GeneratePayloadStructure()
        {
            String payload = "{";
            foreach(String eachData in TestStep.Payload.Keys)
            {
                try
                {
                    var obj = JObject.Parse(TestStep.Payload[eachData]);
                    payload += "\"" + eachData + "\":" + obj.ToString() + ",";
                }
                catch
                {
                    try
                    {
                        var arr = JArray.Parse(TestStep.Payload[eachData]);
                        payload += "\"" + eachData + "\":" + arr.ToString() + ",";
                    }
                    catch
                    {
                        var val = TestStep.Payload[eachData];
                        try
                        {
                            payload += "\"" + eachData + "\":" + Convert.ToInt32(TestStep.Payload[eachData]) + ",";
                        }
                        catch
                        {
                            try
                            {
                                payload += "\"" + eachData + "\":" + Convert.ToBoolean(TestStep.Payload[eachData]) + ",";
                            }
                            catch
                            {
                                try
                                {
                                    payload += "\"" + eachData + "\":" + Convert.ToDouble(TestStep.Payload[eachData]) + ",";
                                }
                                catch
                                {
                                    payload += "\"" + eachData + "\":\"" + TestStep.Payload[eachData] + "\",";
                                }
                            }
                        }
                    }
                }
            }
            payload = payload.Substring(0, payload.Length - 1);
            payload += "}";
            return JObject.Parse(payload);
        }
        private String GetParamtersUrl()
        {
            String ParamUrl = "";
            if(TestStep.Parameters != null)
            {
                if (TestStep.Parameters.Keys.Contains("from-file"))
                {
                    String FilePath = Path.Combine(BaseClass.DataDirectory, TestStep.Parameters["from-file"]);
                    TestStep.Parameters = JsonConvert.DeserializeObject<Dictionary<String, String>>(File.ReadAllText(FilePath));
                }
                ParamUrl = "?";
                foreach(String ParamKey in TestStep.Parameters.Keys.ToList())
                {
                    if(TestStep.Parameters[ParamKey].Contains("RandomName"))
                    {
                        foreach(String EachRandomName in TestStep.Parameters[ParamKey].Split("RandomName("))
                        {
                            if(EachRandomName != "")
                            {
                                Int32 len = Convert.ToInt32(EachRandomName.Split(")")[0]);
                                TestStep.Parameters[ParamKey] = TestStep.Parameters[ParamKey].Replace(
                                    "RandomName(" + len + ")", Utils.RandomName(len));
                            }
                        }
                    }
                    if(TestStep.Parameters[ParamKey].Contains("[from-file:"))
                    {
                        String FileRelPath = TestStep.CustomCode.Split("[from-file:")[1].Split("]")[0];
                        String FilePath = Path.Combine(BaseClass.DataDirectory, FileRelPath);
                        TestStep.Parameters[ParamKey] = TestStep.Parameters[ParamKey].Replace("[from-file:" + FileRelPath + "]", File.ReadAllText(FilePath));
                    }
                    if(TestStep.Parameters[ParamKey].Contains("[from-code:"))
                    {
                        BaseClass.Reporter.Log("Fetching parameter from code execution");
                        try
                        {
                            String codePath = TestStep.Parameters[ParamKey].Split("[from-code:")[1].Split("]")[0];
                            String result = CustomCode(Path.Combine(BaseClass.DataDirectory, codePath));
                            BaseClass.Reporter.Log("Custom code returned result for parameter : \n" + result);
                            TestStep.Parameters[ParamKey] = TestStep.Parameters[ParamKey].Replace("[from-code:" + codePath + "]", result);
                        }
                        catch
                        {
                            BaseClass.Reporter.AddResult(Report.Status.Error, "Custom code returned null value...");
                        }
                    }
                    ParamUrl += ParamKey + "=" + TestStep.Parameters[ParamKey] + "&";
                }
                ParamUrl = ParamUrl.Substring(0, ParamUrl.Length - 1);
            }
            else
                BaseClass.Reporter.Log("No parameters to add... continuing");
            return ParamUrl;
        }
        private String CustomCode(String CodePath)
        {
            if(CodePath != null)
            {
                String code = "";
                try
                {
                    #region Pre-script
                    String precode = "using System;\n" +
                        "using Newtonsoft.Json.Linq;\n" +
                        "String Environment = \"" + BaseClass.Environments.ToString().Replace("\"", "\\\"").Replace("\r\n", "") + "\";\n";
                    if (aPIBase.httpResponse != null)
                        precode += "var httpResponseHeaders = \"" + JsonConvert.SerializeObject(aPIBase.httpResponse.Content.Headers).ToString().Replace("\"", "\\\"").Replace("\r\n", "").Replace("\\/", "/") + "\";\n" +
                        "String StatusCode = \"" + Convert.ToInt32(aPIBase.httpResponse.StatusCode) + "\";\n" +
                        "String httpRequestPayload = " + JsonConvert.SerializeObject(PayloadStructure) + ";\n " +
                        "String httpResponse = " + JsonConvert.SerializeObject(aPIBase.httpResponse.Content.ReadAsStringAsync().Result) + ";\n";//.Content.ReadAsStringAsync().Result.Replace("\"", "\\\"") + "\";\n";
                    if (TestStep.Expected != null)
                    {
                        String Expected = "var Expected = \"[";
                        foreach (String ExpectedValue in TestStep.Expected)
                            Expected += "\\\"" + ExpectedValue + "\\\",";
                        Expected = Expected.Substring(0, Expected.Length - 1);
                        Expected += "]\"";
                        precode += Expected.ToString() + ";\n";
                    }
                    if (TestStep.Validate != null)
                    {
                        String Validate = "var Validate = \"[";
                        foreach (String ValidateValue in TestStep.Validate)
                            Validate +=  "\\\"" + ValidateValue + "\\\",";
                        Validate = Validate.Substring(0, Validate.Length - 1);
                        Validate += "]\"";
                        precode += Validate.ToString() + ";\n";
                    }
                    // Adding sharedobjects availability to file
                    if (aPIBase.data.sharedObjects.Count != 0)
                    {
                        String sharedData = "var storedData = \"{";
                        foreach (String keys in aPIBase.data.sharedObjects.Keys)
                        {
                            try
                            {
                                JObject.Parse(aPIBase.data.sharedObjects[keys]);
                                String serialObj = JsonConvert.SerializeObject(aPIBase.data.sharedObjects[keys]);
                                serialObj = serialObj.Substring(1, serialObj.Length - 2);
                                sharedData += "\\\"" + keys + "\\\":" + serialObj + ",";
                            }
                            catch
                            {
                                try
                                {
                                    JArray.Parse(aPIBase.data.sharedObjects[keys]);
                                    sharedData += "\\\"" + keys + "\\\":" + JsonConvert.SerializeObject(aPIBase.data.sharedObjects[keys]) + ",";
                                }
                                catch
                                {
                                    sharedData += "\\\"" + keys + "\\\":\\\"" + aPIBase.data.sharedObjects[keys] + "\\\",";
                                }
                            }
                        }
                        sharedData = sharedData.Substring(0, sharedData.Length - 1);
                        sharedData += "}\";";
                        precode += sharedData + "\n";
                        precode += "JObject sharedObjects = JObject.Parse(storedData);";
                    }
                    #endregion

                    #region Default libraries
                    var options = ScriptOptions.Default.AddReferences("Newtonsoft.Json", "System").
                                    AddImports("Newtonsoft.Json.Linq", 
                                        "System.Collections.Generic",
                                        "System.IO", 
                                        "System.Text");
                    #endregion

                    code = precode + ReadCode(CodePath);
                    var result = CSharpScript.EvaluateAsync(code, options).Result;
                    
                    if(result != null)
                    {
                        try
                        {
                            Dictionary<String, String> dictResult = (Dictionary<String, String>)result;
                            BaseClass.Reporter.Log("Code returns a disctionary");
                            foreach (String eachResult in dictResult.Keys)
                                aPIBase.data.sharedObjects.Add(eachResult, dictResult[eachResult]);
                            BaseClass.Reporter.AddResult(Report.Status.Pass, "Custom script assertion passed");
                            return null;
                        }
                        catch
                        {
                            BaseClass.Reporter.AddResult(Report.Status.Pass, "Custom script assertion passed");
                            return result.ToString();
                        }
                    }
                    else
                    {
                        BaseClass.Reporter.Log("Code executes and returns null value", Report.Status.Warning);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    BaseClass.Reporter.Log(code, Report.Status.Fail);
                    BaseClass.Reporter.AddResult(Report.Status.Fail, e.Message);
                    return null;
                }
            }
            else
            {
                BaseClass.Reporter.Log("No custom code to execute... continuing");
                return null;
            }
        }
        private String ReadCode(String CodePath)
        {
            if (CodePath.StartsWith("[from-file:"))
            {
                String FilePath = Path.Combine(BaseClass.DataDirectory, CodePath.Split("[from-file:")[1].Split("]")[0]);
                return File.ReadAllText(FilePath);
            }
            else if (File.Exists(CodePath))
                return File.ReadAllText(CodePath);
            else
                throw new Exception("Invalid code format... framewok error");
        }
        private String GetCondition()
        {
            try
            {
                String RawCondition = TestStep.Loop["Condition"];
                Char[] expList = new Char[] { '=', '>', '<', '!', ')', '(' };
                if (RawCondition.Contains("raw-response"))
                {
                    foreach (String eachContent in RawCondition.Split("raw-response"))
                    {
                        if(eachContent != "")
                        {
                            String jsonPath = "";
                            for (int i = 0; i < eachContent.ToCharArray().Length; i++)
                            {
                                Char ch = eachContent.ToCharArray()[i];
                                if (expList.Contains(ch))
                                    break;
                                else
                                    jsonPath += ch;
                            }
                            RawCondition = RawCondition.Replace("raw-response", GetJsonValue("raw-response"));
                        }
                    }
                }
                else if (RawCondition.Contains("response:"))
                {
                    foreach (String eachContent in RawCondition.Split("response:"))
                    {
                        if (eachContent != "")
                        {
                            String jsonPath = "";
                            for (int i = 0; i < eachContent.ToCharArray().Length; i++)
                            {
                                Char ch = eachContent.ToCharArray()[i];
                                if (expList.Contains(ch))
                                    break;
                                else
                                    jsonPath += ch;
                            }
                            RawCondition = RawCondition.Replace("response:" + jsonPath, "\"" + GetJsonValue("response:" + jsonPath) + "\"");
                        }
                    }
                }
                if (RawCondition.Contains("request:"))
                {
                    foreach (String eachContent in RawCondition.Split("request:"))
                    {
                        String jsonPath = "";
                        for (int i = 0; i < eachContent.ToCharArray().Length; i++)
                        {
                            Char ch = eachContent.ToCharArray()[i];
                            if (expList.Contains(ch))
                                break;
                            else
                                jsonPath += ch;
                        }
                        RawCondition = RawCondition.Replace("request:" + jsonPath, GetJsonValue("request:" + jsonPath));
                    }
                }
                if (RawCondition.Contains("[Fetch:"))
                {
                    foreach (String eachContent in RawCondition.Split("[Fetch:"))
                    {
                        String key = "";
                        for (int i = 0; i < eachContent.ToCharArray().Length; i++)
                        {
                            Char ch = eachContent.ToCharArray()[i];
                            if (expList.Contains(ch))
                                break;
                            else
                                key += ch;
                        }
                        RawCondition = RawCondition.Replace("[Fetch:" + key + "]", Fetch(key));
                    }
                }
                if(RawCondition.Contains("this."))
                {
                    foreach (String eachRawCondition in RawCondition.Split("this."))
                    {
                        if(eachRawCondition != "")
                        {
                            String variable = "";
                            for (int i = 0; i < eachRawCondition.ToCharArray().Length; i++)
                            {
                                Char ch = eachRawCondition.ToCharArray()[i];
                                if (expList.Contains(ch))
                                    break;
                                else
                                    variable += ch;
                            }

                            Type t = TestStep.GetType();
                            PropertyInfo[] prop = t.GetProperties();
                            foreach(PropertyInfo eachProp in prop)
                            {
                                if(eachProp.Name == variable)
                                {
                                    RawCondition = RawCondition.Replace("this." + variable, eachProp.GetValue(TestStep).ToString());
                                    break;
                                }
                            }
                        }
                    }
                }
                return RawCondition;
            }
            catch
            {
                return null;
            }
        }
        private Boolean ExecuteExpression(String expression)
        {
            try
            {
                #region Default libraries
                var options = ScriptOptions.Default.AddReferences("Newtonsoft.Json", "System").AddImports("Newtonsoft.Json.Linq", "System.Collections.Generic");
                #endregion

                var result = (Boolean)CSharpScript.EvaluateAsync(expression, options).Result;
                return result;
            }
            catch
            {
                BaseClass.Reporter.Log("Reporting false for ExecuteExpression() in catch()");
                return false;
            }
        }
        private Boolean CheckLoopMandatory()
        {
            List<String> keyList = TestStep.Loop.Keys.ToList();
            if (keyList.Contains("Condition") && keyList.Contains("Count"))
                return true;
            else if(keyList.Contains("Condition") && !keyList.Contains("Count"))
            {
                TestStep.Loop["Count"] = "5";
                return true;
            }
            else
                throw new Exception("'Condition' and 'Count' are mandatory for loop execution");
        }
        private void EnsureFile()
        {
            if (TestStep.EnsureFile)
            {
                try
                {
                    Data.SearchFile(Path.Combine(BaseClass.DownloadPath, TestStep.DownloadFileAs));
                    BaseClass.Reporter.AddResult(Report.Status.Pass, "Validated downloaded file at location : " + Path.Combine(BaseClass.DownloadPath, TestStep.DownloadFileAs));
                }
                catch
                {
                    BaseClass.Reporter.AddResult(Report.Status.Fail, "Failed to validate downloaded file at location : " + Path.Combine(BaseClass.DownloadPath, TestStep.DownloadFileAs));
                }
            }
            else
            {
                BaseClass.Reporter.Log("Not required to validate downloaded file");
            }

        }
    }
}
