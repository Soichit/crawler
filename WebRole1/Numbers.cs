using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;


namespace WebRole1
{
    public class Numbers : TableEntity 
    {

        public int a { get; set; }
        public int b { get; set; }
        public int c { get; set; }
        public int sum { get; set; }

        public Numbers(int a, int b, int c)
        {
            this.PartitionKey = encode(a, b, c);
            this.RowKey = Guid.NewGuid().ToString();
            this.a = a;
            this.b = b;
            this.c = c;
            this.sum = 0;
        }

        public Numbers(){}


        public static string encode(int a, int b, int c)
        {
            return a + " " + b + " " + c;
        }

        public static List<int> decode(string input)
        {
            string[] breakBySpaces = input.Split(' ');
            List<int> result = new List<int>();

            foreach (string s in breakBySpaces)
            {
                result.Add(int.Parse(s));
            }
            return result;
        }

        public void addSum(List<int> input)
        {
            this.sum = 0;
            foreach (int i in input)
            {
                this.sum += i;
            }
        }
    }
}