using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        private static CloudQueue queue;
        private static CloudTable table;

        [WebMethod]
        public string CalculateSumUsingWorkerRole(int a, int b, int c)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                 CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference("myurls");
            queue.CreateIfNotExists();

            //add message
            CloudQueueMessage message = new CloudQueueMessage(Numbers.encode(a, b, c));
            queue.AddMessage(message);
            return "done";
        }


        [WebMethod]
        public List<Numbers> ReadSumFromTableStorage()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
               CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            table = tableClient.GetTableReference("sum");
            TableQuery<Numbers> rangeQuery = new TableQuery<Numbers>();

            List<Numbers> result = table.ExecuteQuery(rangeQuery).ToList();

            return result;
        }

        [WebMethod]
        public int ReadTest()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
               CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            table = tableClient.GetTableReference("sum");
            TableOperation retrieveOperation = TableOperation.Retrieve<Numbers>("7 7 7", "dbc2d7c6-c926-4b9b-93e3-fb25d680eb3c");
            TableResult retrievedResult = table.Execute(retrieveOperation);

            return ((Numbers)retrievedResult.Result).sum;
        }


        [WebMethod]
        public string insertQ()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference("test");
            queue.CreateIfNotExists();

            //add message
            CloudQueueMessage message = new CloudQueueMessage("http://www.cnn.com/index.html");
            CloudQueueMessage message2 = new CloudQueueMessage("2");
            CloudQueueMessage message3 = new CloudQueueMessage("3");
            queue.AddMessage(message);
            queue.AddMessage(message2);
            queue.AddMessage(message3);
            return "done";
        }

        [WebMethod]
        public string readQ()
        {
            // remove message
            CloudQueueMessage message2 = queue.GetMessage(TimeSpan.FromMinutes(5));
            queue.DeleteMessage(message2);
            return "" + message2.AsString;
        }

    }
}
