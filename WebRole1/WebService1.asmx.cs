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
        private static CloudQueue crawlQueue;
        private static List<String> list = new List<string>();
        //private static CloudTable table;

        public WebService1()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            stateQueue = queueClient.GetQueueReference("state");
            stateQueue.CreateIfNotExists();
            crawlQueue = queueClient.GetQueueReference("crawl");
            crawlQueue.CreateIfNotExists();
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

        [WebMethod]
        public string ClearIndex()
        {
            CloudQueueMessage message = new CloudQueueMessage("clear");
            stateQueue.AddMessage(message);
            return "done";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AddtoCrawler(string input)
        {
            input = input.ToLower();
            if (input.Contains("cnn"))
            {
                if (list.Contains("http://www.cnn.com/"))
                {
                    return "Duplicate"; 
                }
                else
                {
                    CloudQueueMessage message = new CloudQueueMessage("http://www.cnn.com/");
                    crawlQueue.AddMessage(message);
                    list.Add("http://www.cnn.com/");
                }
            }
            else if (input.Contains("bleacherreport"))
            {
                if (list.Contains("http://bleacherreport.com/"))
                {
                    return "Duplicate";
                }
                else
                {
                    CloudQueueMessage message = new CloudQueueMessage("http://bleacherreport.com/");
                    crawlQueue.AddMessage(message);
                    list.Add("http://bleacherreport.com/");
                }
            } else
            {
                return "Can't";
            }
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Serialize(list);
        }


        [WebMethod]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getTitle(string url)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                   CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference("urls");
                table.CreateIfNotExists();

                string hashed = Webpage.GetHashString(url);
                TableOperation retrieveOperation = TableOperation.Retrieve<Webpage>(hashed, "path");
                TableResult retrievedResult = table.Execute(retrieveOperation);
                if (retrievedResult.Result == null)
                {
                    return "No results found";
                }
                //JavaScriptSerializer jss = new JavaScriptSerializer();
                string result = ((Webpage)retrievedResult.Result).title;
                return result;
                //return jss.Serialize(result);
            }
            catch (Exception e)
            {
                return "Error occured";
            }
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getStats()
        {
            try
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
            catch (Exception e)
            {
                return "Error occured";
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getErrors()
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                   CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference("errors");
                table.CreateIfNotExists();
                var result = table.ExecuteQuery(new TableQuery<Errors>()).ToList();
                JavaScriptSerializer jss = new JavaScriptSerializer();
                return jss.Serialize(result);
            }
            catch (Exception e)
            {
                return "Error occured";
            } 
        }
    }
}
