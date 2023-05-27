using BEx.Framework.Base;
using BEx.Framework.DataProvider;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace BEx.Framework.Executor.API
{
    public class CLIUpdate
    {
        private static String ReleaseAsset { get; } = "https://api.github.com/repos/MAKnowledgeServices/BATA-Framework/releases/assets/";
        private static String ReleaseList { get; } = "https://api.github.com/repos/MAKnowledgeServices/BATA-Framework/releases";
        private static String AuthCode { get; } = "token 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";

        private static String GetAssetIdByVersion(String VersionTag)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", AuthCode);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClient");

            HttpResponseMessage httpResponseMessage = httpClient.GetAsync(ReleaseList).Result;

            if (Convert.ToInt32(httpResponseMessage.StatusCode) == 200)
            {
                JArray obj = JArray.Parse(httpResponseMessage.Content.ReadAsStringAsync().Result);

                foreach (JToken eachElem in obj)
                {
                    if (eachElem.SelectToken("$.tag_name").ToString().Equals(VersionTag))
                    {
                        foreach (JToken eachAsset in eachElem.SelectToken("$.assets"))
                        {
                            if (eachAsset.SelectToken("$.name").ToString().Equals("BATA.zip"))
                                return eachAsset.SelectToken("$.id").ToString();
                        }
                    }
                }
                return null;
            }
            else
                return null;
        }

        private static void DownloadDependency(Int32 AssetId, String DownloadPath)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", AuthCode);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/octet-stream");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClient");

            Console.WriteLine(DateTime.Now + " Downloading dependency");
            byte[] fileBytes = httpClient.GetByteArrayAsync(ReleaseAsset + AssetId).Result;

            Console.WriteLine(DateTime.Now + " Downloading completed");
            File.WriteAllBytes(Path.Combine(DownloadPath, "bata.zip"), fileBytes);

            //DeleteAllPreviousFiles(DownloadPath);
            //Console.WriteLine(DateTime.Now + " Updating artifacts...");
            //System.IO.Compression.ZipFile.ExtractToDirectory(Path.Combine(DownloadPath, "bata.zip"), DownloadPath);

            //File.Delete(Path.Combine(DownloadPath, "bata.zip"));
            Console.WriteLine(DateTime.Now + " Done");
        }
        public static void Get(String VersionTag, String DownloadPath = null)
        {
            try
            {
                Int32 assetId = Convert.ToInt32(GetAssetIdByVersion(VersionTag));
                if (assetId.ToString() != null)
                    Console.WriteLine(DateTime.Now + " BATA v" + VersionTag + " found");
                else
                {
                    Console.WriteLine(DateTime.Now + " BATA v" + VersionTag + " not found");
                    Environment.Exit(1);
                }
                DownloadDependency(assetId, CheckLibsPath(DownloadPath));
            }
            catch(Exception e)
            {
                Console.WriteLine(DateTime.Now + " Failed to download dependencies : " + e.Message);
                Environment.Exit(1);
            }
        }
        private static String CheckLibsPath(String DownloadPath)
        {
            String libsPath = null;
            if (DownloadPath != null)
                libsPath = Data.SearchFile(DownloadPath);
            else
                libsPath = Data.SearchFile("libs");
            return libsPath;
        }

        private static void DeleteAllPreviousFiles(String DownloadPath)
        {
            foreach(FileInfo file in new DirectoryInfo(DownloadPath).GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in new DirectoryInfo(DownloadPath).GetDirectories())
            {
                dir.Delete();
            }
        }
    }
}
