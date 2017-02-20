using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text;
using System.Security.Cryptography;


namespace WebRole1
{
    public class Webpage : TableEntity
    {
        public string url { get; set; }
        public string title { get; set; }


        public Webpage() { }


        public Webpage(string url, string title)
        {
            this.PartitionKey = GetHashString(url);
            //this.RowKey = "path";
            this.RowKey = Guid.NewGuid().ToString();
            this.url = url;
            this.title = title;
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();  //or use SHA1.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}