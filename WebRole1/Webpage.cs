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


        public Webpage() { }


        public Webpage(string url, string title)
        {
            this.PartitionKey = url;
            this.RowKey = "path";
            this.url = url;
            this.title = title;
        }
    }

}
