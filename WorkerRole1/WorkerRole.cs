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



namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        //private static CloudQueue queue;
        //private static CloudTable table;
        private static List<string> baseUrlList;
        //private static List<String> robotXmlList;
        //private static CloudQueue htmlQueue;
        //private static List<String> xmlList;
        //private static string baseUrl;
       

        public override void Run()
        {
            Crawl c = new Crawl(); // move inside while loop
            baseUrlList = new List<string>();
            baseUrlList.Add("http://www.cnn.com/");
            // baseUrlList.Add("http://bleacherreport.com/");

            

            while (true)
            {
                Thread.Sleep(1000);

                // go through list of cnn and bleacherreport
                while (baseUrlList.Count > 0)
                {
                    string baseUrl = baseUrlList[0]; //"http://www.cnn.com"
                    baseUrlList.RemoveAt(0); //move to end to be safe
                    string robotsUrl = baseUrl + "/robots.txt";

                    c.parseRobot(robotsUrl);
                    // parse through robots.txt and xmls
                    for (int i = 0; i < c.robotXmlList.Count; i++)
                    {
                        c.parseXML(c.robotXmlList[i]);
                        for (int j = 0; j < c.xmlList.Count; j++)
                        {
                            c.parseXML(c.xmlList[j]);
                        }
                    }
                }

                //once all xmls are parsed, go through html queue and crawl page
                //CloudQueueMessage message = new CloudQueueMessage("");
                CloudQueueMessage message = c.htmlQueue.GetMessage(TimeSpan.FromMinutes(1));
                if (message != null)
                {
                    c.htmlQueue.DeleteMessage(message);
                    c.parseHTML(message.AsString);
                }
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
