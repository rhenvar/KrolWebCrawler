using CloudLibrary;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Services;

namespace CrawlerWebRole
{
    /// <summary>
    /// Summary description for admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Admin : System.Web.Services.WebService
    {
        private static CloudTable urls = AccountManager.tableClient.GetTableReference("urltable");
        private static CloudTable diagnostics = AccountManager.tableClient.GetTableReference("diagnostictable");

        // should only be reading data from azure table here
        // don't invoke worker or insert urls
        [WebMethod]
        public void StartCrawling()
        {
            CloudQueue resumeQueue = AccountManager.queueClient.GetQueueReference("resumequeue");
            resumeQueue.AddMessage(new CloudQueueMessage(""));
        }

        [WebMethod]
        public bool ParseRobots()
        {
            using (var client = new WebClient())
            {
                byte[] cnnData = client.DownloadData("http://www.cnn.com/robots.txt");
                ParseRobots(cnnData);
                CloudQueue xmlQueue = AccountManager.queueClient.GetQueueReference("xmlqueue");
                xmlQueue.CreateIfNotExists();
                xmlQueue.AddMessage(new CloudQueueMessage("http://bleacherreport.com/sitemap/nba.xml"));
            }
            return true;
        }

        [WebMethod]
        public void StopCrawling()
        {
            CloudQueue stopQueue = AccountManager.queueClient.GetQueueReference("stopqueue");
            CloudQueueMessage stopMessage = new CloudQueueMessage("");
            stopQueue.AddMessage(stopMessage);
        }
        
        [WebMethod]
        public bool ClearIndex()
        {
            StopCrawling();
            Thread.Sleep(1000);
            CloudQueue queue = CloudLibrary.AccountManager.queueClient.GetQueueReference("myurls");
            queue.FetchAttributes();
            CloudQueueMessage message = queue.GetMessage();
            while (message != null)
            {

            }

            return true;
        }
        
        [WebMethod]
        public string GetPageTitle(string url)
        {
            string key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url));
            TableQuery<WebPageEntity> query = new TableQuery<WebPageEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key));
            foreach (WebPageEntity page in urls.ExecuteQuery(query))
            {
                return page.Title;
            }
            return "No such page found!";
        }

        [WebMethod]
        public DiagnosticEntity GetData()
        {
            long ticks = DateTime.Now.Ticks;
            string query = TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, ticks.ToString()), TableOperators.And, TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, (ticks - 10000).ToString()));
            TableQuery<DiagnosticEntity> diagnosticQuery = new TableQuery<DiagnosticEntity>().Where(query);
            foreach (DiagnosticEntity dE in diagnostics.ExecuteQuery(diagnosticQuery))
            {
                return dE;
            }
            return null;
        }

        private void ParseRobots(byte[] data)
        {
            // NEED TO PARSE DISALLOW FIRST SO CRAWLING DOESN'T START WITHOUT KNOWING 
            // WHAT IS BLACKLISTED
            CloudQueue forbiddenQueue = CloudLibrary.AccountManager.queueClient.GetQueueReference("forbiddenqueue");
            forbiddenQueue.CreateIfNotExists();

            using (StreamReader reader = new StreamReader(new MemoryStream(data)))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Contains("Disallow"))
                    {
                        string[] testLine = line.Split(' ');

                        string disallowExtension = testLine[1];
                        forbiddenQueue.AddMessage(new CloudQueueMessage("cnn.com" + disallowExtension));
                    }
                }
            }

            CloudQueue xmlQueue = CloudLibrary.AccountManager.queueClient.GetQueueReference("xmlqueue");
            xmlQueue.CreateIfNotExists();
            using (StreamReader reader = new StreamReader(new MemoryStream(data)))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Contains("Sitemap"))
                    {
                        string[] testLine = line.Split(' ');

                        if (testLine[1].Contains(".xml"))
                        {
                            xmlQueue.AddMessage(new CloudQueueMessage(testLine[1]));
                        }
                    }
                }
            }
        }
    }
}