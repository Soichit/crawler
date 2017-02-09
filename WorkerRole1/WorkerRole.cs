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

        private static CloudQueue queue;
        private static CloudTable table;

        public override void Run()
        {
            //Trace.TraceInformation("WorkerRole1 is running");
            while (true)
            {
                Thread.Sleep(10000);
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                 CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                queue = queueClient.GetQueueReference("myurls");

                
                //read and remove msg
                CloudQueueMessage message2 = queue.GetMessage(TimeSpan.FromMinutes(5));
                if (message2 != null)
                {
                    queue.DeleteMessage(message2);
                    List<int> input = Numbers.decode(message2.AsString);
                    Numbers n = new Numbers(input[0], input[1], input[2]);
                    n.addSum(input);
                
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                    table = tableClient.GetTableReference("sum");
                    //table.DeleteIfExists();  
                    table.CreateIfNotExists();

                    TableOperation insertOperation = TableOperation.Insert(n);
                    table.Execute(insertOperation);
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
