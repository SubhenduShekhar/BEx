using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace BEx.Framework.DataProvider.Formats
{
    public class Json
    {
        public static dynamic Read(String FilePath, Boolean IsFileContent = false)
        {
            if(!IsFileContent)
            {
                try
                {
                    return JObject.Parse(File.ReadAllText(FilePath));
                }
                catch
                {
                    return JArray.Parse(File.ReadAllText(FilePath));
                }
            }
            else
            {
                try
                {
                    return JObject.Parse(FilePath);
                }
                catch
                {
                    return JArray.Parse(FilePath);
                }
            }
        }
        public static Boolean IsObject(String JsonString, Boolean isFileCotent = true)
        {
            if(isFileCotent)
            {
                try
                {
                    JObject.Parse(JsonString);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                JsonString = File.ReadAllText(JsonString);
                try
                {
                    JObject.Parse(JsonString);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static Boolean IsArray(String JsonString, Boolean isFileCotent = true)
        {
            if(isFileCotent)
            {
                try
                {
                    JArray.Parse(JsonString);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                JsonString = File.ReadAllText(JsonString);
                try
                {
                    JArray.Parse(JsonString);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
