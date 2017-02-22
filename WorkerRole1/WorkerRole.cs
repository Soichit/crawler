using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Table;
using WebRole1;
using System.Diagnostics;



namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private static List<string> baseUrlList;
        private static CloudTable statsTable;
        private static CloudTable urlsTable;
        private static CloudTable errorsTable;
        private static CloudQueue htmlQueue;
        private static CloudQueue stateQueue;
        private static string status;
        private static Stats stats;
        private static List<string> lastTen;



        public override void Run()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                 CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            statsTable = tableClient.GetTableReference("stats");
            statsTable.CreateIfNotExists();
            //to drop urlsTable and errorsTable
            urlsTable = tableClient.GetTableReference("urls");
            urlsTable.CreateIfNotExistsAsync();
            errorsTable = tableClient.GetTableReference("errors");
            errorsTable.CreateIfNotExistsAsync();
            htmlQueue = queueClient.GetQueueReference("myhtml");
            htmlQueue.CreateIfNotExistsAsync();
            stateQueue = queueClient.GetQueueReference("state");
            stateQueue.CreateIfNotExistsAsync();

            PerformanceCounter theCPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter theMemCounter = new PerformanceCounter("Memory", "Available MBytes");

            Crawl crawler = new Crawl();
            stats = new Stats(theCPUCounter.NextValue(), theMemCounter.NextValue());

            baseUrlList = new List<string>();
            baseUrlList.Add("http://www.cnn.com/");
            
            // baseUrlList.Add("http://bleacherreport.com/");
            lastTen = new List<string>();
            status = "stop";

            while (true)
            {
                Thread.Sleep(1000);
                CloudQueueMessage stateMessage = stateQueue.GetMessage(TimeSpan.FromMinutes(1));
                if (stateMessage != null)
                {
                    stateQueue.DeleteMessage(stateMessage);
                    status = stateMessage.AsString;
                    // restart all data if status changes back to start
                    if (status == "start")
                    {
                        //stats = new Stats(theCPUCounter.NextValue(), theMemCounter.NextValue());
                        //stats.updateState("Loading");
                        //crawler.htmlQueue.Clear();
                        //urlsTable.DeleteIfExists();
                        //urlsTable.CreateIfNotExistsAsync();
                        //errorsTable.DeleteIfExists();
                        //errorsTable.CreateIfNotExistsAsync();
                        //Thread.Sleep(40000);
                    }
                    else //status == "stop"
                    {
                        stats.updateState("Idle");
                    }
                    updateStats();
                }
                if (status == "start")
                {
                    // go through list of cnn and bleacherreport
                    //while (baseUrlList.Count > 0)
                    //{
                    //    stats.updateState("Loading");
                    //    string baseUrl = baseUrlList[0]; //"http://www.cnn.com"
                    //    baseUrlList.RemoveAt(0);

                    //    // parse through robots.txt and xmls
                    //    string robotsUrl = baseUrl + "/robots.txt";
                    //    crawler.parseRobot(robotsUrl);
                    //    for (int i = 0; i < crawler.robotXmlList.Count; i++)
                    //    {
                    //        stats.updatePerformance(theCPUCounter.NextValue(), theMemCounter.NextValue());
                    //        updateStats();
                    //        crawler.parseXML(crawler.robotXmlList[i]);
                    //        for (int j = 0; j < crawler.xmlList.Count; j++)
                    //        {
                    //            crawler.parseXML(crawler.xmlList[j]);
                    //        }
                    //    }
                    //}

                    //once all xmls are parsed, go through html queue and crawl page
                    CloudQueueMessage message = crawler.htmlQueue.GetMessage(TimeSpan.FromMinutes(1));
                    if (message != null)
                    {
                        stats.updateState("Crawling");
                        crawler.htmlQueue.DeleteMessage(message);
                        Boolean addedToTable = crawler.parseHTML(message.AsString);
                        updateLastTen(message.AsString);
                        htmlQueue.FetchAttributes();
                        int queueSize = (int) htmlQueue.ApproximateMessageCount;
                        stats.updateAllStats(theCPUCounter.NextValue(), theMemCounter.NextValue(), queueSize, addedToTable, lastTen);
                        updateStats();
                    }
                }
                

                //Test duplicates
                //Boolean addedToTable = crawler.parseHTML("http://www.cnn.com");
                //updateLastTen("http://www.cnn.com");
                //htmlQueue.FetchAttributes();
                //int queueSize = (int)htmlQueue.ApproximateMessageCount;
                //stats.updateAllStats(theCPUCounter.NextValue(), theMemCounter.NextValue(), queueSize, addedToTable, lastTen);
                //updateStats();
            }
        }

        public void updateLastTen(string url)
        {
            if (lastTen.Count >= 10)
            {
                lastTen.RemoveAt(lastTen.Count - 1);
            }
            lastTen.Insert(0, url);
        }


        private void updateStats()
        {
            try
            {
                TableOperation insertOperation = TableOperation.InsertOrReplace(stats);
                statsTable.ExecuteAsync(insertOperation);
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Error: " + e.Message);
            }
        }


        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
