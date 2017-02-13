using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;


namespace WebRole1
{
    public class Webpage : TableEntity
    {
        public string url { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }

    public Webpage(string url, string title, string body) {
        this.PartitionKey = url;
        this.RowKey = "path";
        this.url = url;
        this.title = title;
        this.body = body;
    }

}
