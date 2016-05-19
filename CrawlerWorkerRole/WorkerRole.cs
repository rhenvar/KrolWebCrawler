using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.Xml;
using CloudLibrary;
using HtmlAgilityPack;
using System.Linq;
using System;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace CrawlerWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        public static ConcurrentSet<string> visitedUrls = new ConcurrentSet<string>();
        public static ConcurrentFixedQueue<string> lastTen = new ConcurrentFixedQueue<string>(10);
        public static ConcurrentFixedQueue<string> lastTenError = new ConcurrentFixedQueue<string>(10);

        public static int totalIndex = 0;
        public static int queueSize = 0;

        public static List<string> forbiddenUrls = new List<string>();
        public readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public EventWaitHandle EventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private readonly List<Thread> threads = new List<Thread>();
        private readonly List<ThreadWorker> workers = new List<ThreadWorker>();

        private PerformanceCounter theCPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private PerformanceCounter theMemCounter = new PerformanceCounter("Memory", "Available MBytes");

        public static CloudTableClient tableClient = AccountManager.storageAccount.CreateCloudTableClient();
        public static CloudQueueClient queueClient = AccountManager.storageAccount.CreateCloudQueueClient();

        public static CloudQueue htmlQueue = queueClient.GetQueueReference("htmlqueue");
        public static CloudQueue forbiddenQueue = queueClient.GetQueueReference("forbiddenqueue");
        public static CloudQueue errorQueue = queueClient.GetQueueReference("errorqueue");
        public static CloudQueue xmlQueue = queueClient.GetQueueReference("xmlqueue");
        public static CloudQueue stopQueue = queueClient.GetQueueReference("stopqueue");
        public static CloudQueue resumeQueue = queueClient.GetQueueReference("resumequeue");

        public static CloudTable urlTable = tableClient.GetTableReference("urltable");
        public static CloudTable diagnosticTable = tableClient.GetTableReference("diagnostictable");

        public override void Run()
        {
            Trace.TraceInformation("CrawlerWorkerRole is running");
            try
            {
                stopQueue.CreateIfNotExists();
                htmlQueue.CreateIfNotExists();
                forbiddenQueue.CreateIfNotExists();
                errorQueue.CreateIfNotExists();
                urlTable.CreateIfNotExists();
                diagnosticTable.CreateIfNotExists();
                resumeQueue.CreateIfNotExists();

                foreach (var worker in workers)
                {
                    threads.Add(new Thread(worker.RunInternal));
                }
                foreach (var thread in threads)
                {
                    thread.Start();
                }

                while (true)
                {
                    Dictionary<string, string> roleStatus = new Dictionary<string, string>();

                    ParseForbiddenUrls();

                    CloudQueueMessage stopMessage = stopQueue.GetMessage();
                    if (stopMessage != null)
                    {
                        stopQueue.DeleteMessage(stopMessage);
                        for (int i = 0; i < threads.Count; i++)
                        {
                            threads[i].Suspend();
                            workers[i].Status = "idle";
                        }
                    }

                    CloudQueueMessage resumeMessage = resumeQueue.GetMessage();
                    if (resumeMessage != null)
                    {
                        resumeQueue.DeleteMessage(resumeMessage);
                        for (int i = 0; i < threads.Count; i++)
                        {
                            if (threads[i].ThreadState == System.Threading.ThreadState.Suspended)
                            {
                                threads[i].Resume();
                                workers[i].Status = "crawling";
                            }
                        }
                    }

                    CloudQueueMessage xmlMessage = xmlQueue.GetMessage();
                    if (xmlMessage != null)
                    {
                        roleStatus["XmlWorker"] = "loading";
                        xmlQueue.DeleteMessage(xmlMessage);
                        ParseXmlUrl(xmlMessage.AsString);
                    }
                    else
                    {
                        roleStatus["XmlWorker"] = "idle";
                    }
                    for (var i = 0; i < threads.Count; i++)
                    {
                        roleStatus["HtmlWorker" + i] = workers[i].Status;
                        if (threads[i].IsAlive)
                        {
                            continue;
                        }
                        threads[i] = new Thread(workers[i].RunInternal);
                        threads[i].Start();
                    }

                    int totalCrawled = visitedUrls.Size;
                    float cpuUsage = theCPUCounter.NextValue();
                    float memUsage = theMemCounter.NextValue();
                    
                    DiagnosticEntity diagnosticEntity = new DiagnosticEntity(cpuUsage, memUsage, lastTen.ToArray(), lastTenError.ToArray(), visitedUrls.Size, queueSize, totalIndex, roleStatus);
                    InsertDiagnostics(diagnosticEntity);

                    Thread.Sleep(100);
                }
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        private void InsertDiagnostics(DiagnosticEntity entity)
        {
            TableOperation insert = TableOperation.Insert(entity);
            diagnosticTable.Execute(insert);
        }

        private void ParseForbiddenUrls()
        {
            CloudQueueMessage forbiddenMessage = forbiddenQueue.GetMessage();
            while (forbiddenMessage != null)
            {
                forbiddenQueue.DeleteMessage(forbiddenMessage);
                forbiddenUrls.Add(forbiddenMessage.AsString);
                forbiddenMessage = forbiddenQueue.GetMessage();
            }
        }

        public override bool OnStart()
        {
            // ADD 3 OF SAME THREADS WITH SEQUENTIAL CODE MUCH EASIER
            ServicePointManager.DefaultConnectionLimit = 12;

            xmlQueue.CreateIfNotExists();
            workers.Add(new PageWorker());
            workers.Add(new PageWorker());
            workers.Add(new PageWorker());
            bool result = base.OnStart();
            return result;
        }

        // On stop method
        // called when role is to be shut down
        // good place to clean up, only have 30 sec tho
        public override void OnStop()
        {
            Trace.TraceInformation("CrawlerWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("CrawlerWorkerRole has stopped");
        }

        private void ParseXmlUrl(string xmlUrl)
        {
            if (visitedUrls.Contains(xmlUrl))
            {
                return;
            }
            visitedUrls.Add(xmlUrl);
            XmlDocument xmlDoc = new XmlDocument();
            using (XmlTextReader tr = new XmlTextReader(xmlUrl))
            {
                tr.Namespaces = false;
                xmlDoc.Load(tr);
            }
            XmlNodeList urls = xmlDoc.SelectNodes("//loc");
            foreach (XmlNode urlNode in urls)
            {
                string url = urlNode.InnerText;
                if (!visitedUrls.Contains(url))
                {
                    CloudQueueMessage message = new CloudQueueMessage(url);
                    if (UriValidator.IsValidXml(url))
                    {
                        xmlQueue.AddMessage(message);
                    }
                    else if (UriValidator.IsValidHtml(url))
                    {
                        htmlQueue.AddMessage(message);
                        Interlocked.Increment(ref queueSize);
                    }
                }
            }
        }

        internal class PageWorker : ThreadWorker
        {
            public override void Run()
            {
                while (true)
                {
                    CloudQueueMessage message = htmlQueue.GetMessage();
                    if (message != null)
                    {
                        this.Status = "crawling";
                        htmlQueue.DeleteMessage(message);
                        ParseHtml(message.AsString);
                    }
                    else
                    {
                        this.Status = "idle";
                    }
                    Thread.Sleep(10);
                }
            }

            private void ParseHtml(string htmlUrl)
            {
                if (visitedUrls.Contains(htmlUrl) || IsForbidden(htmlUrl))
                {
                    return;
                }
                visitedUrls.Add(htmlUrl);
                HtmlDocument htmlDoc = new HtmlDocument();
                try
                {
                    using (var client = new WebClient())
                    {
                        htmlDoc.LoadHtml(client.DownloadString(htmlUrl));
                    }
                    if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                    {
                        throw new WebException();
                    }

                    lastTen.Enqueue(htmlUrl);
                    DateTime date;
                    var pageDate = htmlDoc.DocumentNode.Descendants("meta").Where(m => m.Attributes["content"] != null
                    && m.GetAttributeValue("name", "").Equals("pubdate", StringComparison.InvariantCultureIgnoreCase)
                    || m.GetAttributeValue("name", "").Equals("og:pubdate", StringComparison.InvariantCultureIgnoreCase)).Select(m => m.Attributes["content"].Value).ToList();
                    if (pageDate.Count == 0 || pageDate == null)
                    {
                        date = DateTime.Today;
                    }
                    else
                    {
                        date = DateTime.Parse(pageDate[0]);
                    }

                    string title;
                    var pageTitle = htmlDoc.DocumentNode.Descendants("title").Where(t => t != null).Select(t => t.InnerHtml).ToList();
                    if (pageTitle.Count == 0 || pageTitle == null)
                    {
                        title = "Page Title Not Found";
                    }
                    else
                    {
                        title = pageTitle[0];
                    }


                    InsertToTable(new WebPageEntity(htmlUrl, date, title));

                    var links = htmlDoc.DocumentNode.Descendants("a").ToList().Where(a => a.Attributes["href"] != null && a.Attributes["href"].Value != "/").Select(a => a.Attributes["href"]).ToList();
                    if (links != null && links.Count > 0)
                    {
                        foreach (HtmlAttribute pageLinkAttribute in links)
                        {
                            string pageLink = pageLinkAttribute.Value;

                            string foundUrl;
                            if (UriValidator.IsAbsoluteUrl(pageLink))
                            {
                                foundUrl = pageLink;
                            }
                            else
                            {
                                var baseUrl = new Uri(htmlUrl);
                                var url = new Uri(baseUrl, pageLink);
                                foundUrl = url.AbsoluteUri;
                            }
                            if (UriValidator.IsValidHtml(foundUrl))
                            {
                                CloudQueueMessage newHtmlPage = new CloudQueueMessage(foundUrl);
                                htmlQueue.AddMessage(newHtmlPage);
                                Interlocked.Increment(ref WorkerRole.queueSize);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    lastTenError.Enqueue(htmlUrl);
                }
            }
            private void InsertToTable(WebPageEntity webPage)
            {
                TableOperation insert = TableOperation.Insert(webPage);
                urlTable.Execute(insert);
                Interlocked.Increment(ref WorkerRole.totalIndex);
            }
        }
    }
}
