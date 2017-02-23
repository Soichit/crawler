using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.Script.Serialization;


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

    [ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        private static CloudQueue stateQueue;
        //private static CloudTable table;

        public WebService1()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            stateQueue = queueClient.GetQueueReference("state");
            stateQueue.CreateIfNotExists();
        }

        [WebMethod]
        public string StartCrawling()
        {
            CloudQueueMessage message = new CloudQueueMessage("start");
            stateQueue.AddMessage(message);
            return "done";
        }

        [WebMethod]
        public string StopCrawling()
        {
            CloudQueueMessage message = new CloudQueueMessage("stop");
            stateQueue.AddMessage(message);
            return "done";
        }

        //[WebMethod]
        //public string ClearIndex()
        //{
        //    CloudQueueMessage message = new CloudQueueMessage("clear");
        //    stateQueue.AddMessage(message);
        //    return "done";
        //}


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getStats()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
               CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("stats");
            table.CreateIfNotExists();
            var result = table.ExecuteQuery(new TableQuery<Stats>()).ToList();

            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Serialize(result);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getErrors()
        {
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            //   CloudConfigurationManager.GetSetting("StorageConnectionString"));
            //CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            //CloudTable table = tableClient.GetTableReference("errors");
            //table.CreateIfNotExists();
            //var result = table.ExecuteQuery(new TableQuery<Errors>()).ToList();

            JavaScriptSerializer jss = new JavaScriptSerializer();
            //return jss.Serialize(result);
            return jss.Serialize("result");
        }

    }
}
