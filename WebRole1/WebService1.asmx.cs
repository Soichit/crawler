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
            //add message
            CloudQueueMessage message = new CloudQueueMessage("start");
            stateQueue.AddMessage(message);
            return "done";
        }

        [WebMethod]
        public string StopCrawling()
        {
            //add message
            CloudQueueMessage message = new CloudQueueMessage("stop");
            stateQueue.AddMessage(message);
            return "done";
        }

        public string ClearIndex()
        {
            //add message
            CloudQueueMessage message = new CloudQueueMessage("clear");
            stateQueue.AddMessage(message);
            return "done";
        }



        //[WebMethod]
        //public string readQ()
        //{
        //    // remove message
        //    CloudQueueMessage message2 = stateQueue.GetMessage(TimeSpan.FromMinutes(5));
        //    stateQueue.DeleteMessage(message2);
        //    return "" + message2.AsString;
        //}
    }
}
