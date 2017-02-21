using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace WorkerRole1
{
    class Errors: TableEntity
    {
        public string url { get; set; }
        public string message { get; set; }

        public Errors(string url, string message)
        {
            this.url = url;
            this.message = message;
        }
    }
}
