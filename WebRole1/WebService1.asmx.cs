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
        private static CloudQueue htmlQueue;
        private static CloudTable table;


        [WebMethod]
        public string insertQ()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            htmlQueue = queueClient.GetQueueReference("myhtml");
            htmlQueue.CreateIfNotExists();

            //add message
            CloudQueueMessage message = new CloudQueueMessage("http://www.cnn.com/index.html");
            CloudQueueMessage message2 = new CloudQueueMessage("2");
            CloudQueueMessage message3 = new CloudQueueMessage("3");
            htmlQueue.AddMessage(message);
            htmlQueue.AddMessage(message2);
            htmlQueue.AddMessage(message3);
            return "done";
        }

        [WebMethod]
        public string readQ()
        {
            // remove message
            CloudQueueMessage message2 = htmlQueue.GetMessage(TimeSpan.FromMinutes(5));
            htmlQueue.DeleteMessage(message2);
            return "" + message2.AsString;
        }

    }
}
