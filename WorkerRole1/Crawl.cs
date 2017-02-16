using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Table;

namespace WebRole1
{
    public class Crawl
    {

        private static CloudQueue queue;
        private static CloudTable table;

        public Crawl() {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                 CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queue = queueClient.GetQueueReference("myurls");

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("sum");
            //table.DeleteIfExists();  
            table.CreateIfNotExists();
        }


        // Get website content
        // Get all links within content and add to query
        // Add the contents to table
        public void crawlPage(string link)
        {
            // web crawler
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc = web.Load(link);
            Console.WriteLine("1");

            // ParseErrors is an ArrayList containing any errors from the Load statement
            //if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            if (htmlDoc.DocumentNode != null)
            {
                HtmlAgilityPack.HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
                string title = "" + htmlDoc.DocumentNode.SelectSingleNode("//head/title");
                //if (bodyNode != null)
                // insert webpage into table
                Webpage w = new Webpage(link, title, "" + bodyNode);
                TableOperation insertOperation = TableOperation.Insert(w);
                table.Execute(insertOperation);
            }
            HtmlNode[] nodes = htmlDoc.DocumentNode.SelectNodes("//a").ToArray();
            foreach (HtmlNode item in nodes)
            {
                // insert into Queue
                CloudQueueMessage url = new CloudQueueMessage("" + item);
                queue.AddMessage(url);
            }
        }
    }
}