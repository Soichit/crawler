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
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Diagnostics;


namespace WebRole1
{
    public class Crawl
    {
        // static variables??
        public CloudQueue htmlQueue { get; private set; }
        public List<String> xmlList { get; private set; }
        public List<String> robotXmlList { get; private set; }
        public CloudTable table { get; private set; }
        public string baseUrl { get; private set; }
        public DateTime cutOffDate { get; private set; }
        public HashSet<string> disallows { get; private set; }

        public Crawl() {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                 CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("urlTable");
            table.CreateIfNotExists();

            xmlList = new List<String>();
            robotXmlList = new List<String>();
            htmlQueue = queueClient.GetQueueReference("myhtml");
            htmlQueue.CreateIfNotExists();
            disallows = new HashSet<string>();
            cutOffDate = new DateTime(2016, 12, 1); // 12/1/2016
            baseUrl = "http://www.cnn.com"; //FIX: don't hardcode
        }

        public void startCrawl(string url)
        {
            //parseHTML("http://www.cnn.com/");
            //robotXmlList.Add("http://www.cnn.com/sitemaps/sitemap-index.xml");
        }


        public void parseXML(string url)
        {
            XmlTextReader reader = new XmlTextReader(url);
            string tag = "";
            Boolean dateAllowed = true;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: //tag types
                        tag = reader.Name;
                        break;
                    case XmlNodeType.Text: //text within tags
                        // if vs elseif
                        // for cases where lastmod tag doesn't exist
                        if (tag == "sitemap" || tag == "url")
                        {
                            dateAllowed = true;
                        }
                        if (tag == "lastmod")
                        {
                            string date = reader.Value.Substring(0, 10); //format: 2017-02-17 
                            DateTime dateTime = Convert.ToDateTime(date);
                            int compare = DateTime.Compare(dateTime, cutOffDate);
                            if (compare >= 0)
                            {
                                dateAllowed = true;
                            }
                            else
                            {
                                dateAllowed = false;
                            }
                        }
                        if (tag == "loc")
                        {
                            string link = reader.Value;

                            //check if the date is allowed and robot.txt link is allowed
                            if (dateAllowed && !disallows.Contains(link))
                            {
                                if (link.Substring(link.Length - 4) == ".xml")
                                {
                                    xmlList.Add(link);
                                }
                                else
                                {
                                    CloudQueueMessage htmlLink = new CloudQueueMessage(reader.Value);
                                    //assuming the type is .html or .htm
                                    htmlQueue.AddMessage(htmlLink);
                                }
                            }
                        }
                        break;
                }
            }
        }


        public void parseHTML(string link)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc = web.Load(link); //check if link exists and is an html document

            // ParseErrors is an ArrayList containing any errors from the Load statement
            //if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            if (htmlDoc.DocumentNode != null)
            {
                string title = "";
                var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
                if (titleNode != null)
                {
                    title = titleNode.InnerHtml;
                }
                try
                {
                    // insert webpage into table
                    Webpage page = new Webpage(link, title);
                    TableOperation insertOperation = TableOperation.Insert(page);
                    table.Execute(insertOperation);
                }
                catch (Microsoft.WindowsAzure.Storage.StorageException)
                {
                    Debug.WriteLine("StorageException error at: " + link);
                }
            }
            HtmlNode[] links;
            var linkNodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
            if (linkNodes != null)
            {
                links = linkNodes.ToArray();
            } else
            {
                links = null;
            }
            foreach (HtmlNode item in links)
            {
                // insert into html queue
                string hrefValue = item.GetAttributeValue("href", string.Empty).Trim();
                string correctUrl = "";
                if (hrefValue.Length > 2)
                {
                    if (hrefValue.Substring(0, 2) == "//")
                    {
                        correctUrl = "http://" + hrefValue.Substring(2);
                    }
                    else if (hrefValue.Substring(0, 1) == "/")
                    {
                        correctUrl = baseUrl + hrefValue;
                    }
                    else if (hrefValue.Substring(0, 4) == "http")
                    {
                        correctUrl = hrefValue;
                    } else
                    {
                        correctUrl = "XXX";
                    }
                    //insert into html queue
                    if (!disallows.Contains(correctUrl) && correctUrl.Contains("cnn.com")) // or "bleacherreport.com"
                    {
                        CloudQueueMessage htmlLink = new CloudQueueMessage(correctUrl);
                        htmlQueue.AddMessage(htmlLink);
                    }
                }
            }
        }


        public void parseRobot(string url)
        {
            WebResponse response;
            WebRequest request = WebRequest.Create(url);
            response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            using (reader)
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line.StartsWith("Disallow:"))
                    {
                        string item = line.Substring(10);
                        disallows.Add(baseUrl + item);
                    }
                    else if (line.StartsWith("Sitemap:"))
                    {
                        string item = line.Substring(9);
                        robotXmlList.Add(item);
                    }
                }
            }
            //string output = string.Join("\r\n", disallows.ToArray());
            //string output2 = string.Join("\r\n", xmlList.ToArray());
            //checkSitemap(url, reader);
        }

    }
}