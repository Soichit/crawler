using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WebRole1
{
    public class Stats: TableEntity
        
    {
        // take away private set
        public string state { get; set; }
        public string lastTen { get; set; }
        public int queueSize { get; set; }
        public int tableSize { get; set; }
        public int URLcounter { get; set; }
        public int CPUCounter { get; set; }
        public int MemCounter { get; set; }
        //errors table

        public Stats() { }


        public Stats(float CPUCounter, float MemCounter) {
            this.PartitionKey = "PartionKey";
            this.RowKey = "RowKey";
            //this.RowKey = "path";
            this.state = "Idle";
            this.lastTen = "";
            this.queueSize = 0;
            this.tableSize = 0;
            this.URLcounter = 0;
            this.CPUCounter = (int) CPUCounter;
            this.MemCounter = (int) MemCounter;
        }

        public void updateAllStats(float CPUCounter, float MemCounter, int queueSize, Boolean noErrors, List<string> list)
        {     
            this.CPUCounter = (int) CPUCounter;
            this.MemCounter = (int) MemCounter;
            this.queueSize = queueSize;
            this.URLcounter++;
            //if there were no errors, increment table size
            if (noErrors)
            {
                this.tableSize++;
            }
            updateLastTen(list);
        }

        public void updateState(string state)
        {
            this.state = state;
        }

        public void updateLastTen(List<string> list)
        {
            this.lastTen = "";
            foreach (string s in list)
            {
                this.lastTen += s + "; ";
            }
        }

        public void updatePerformance(float CPUCounter, float MemCounter)
        {
            this.CPUCounter = (int) CPUCounter;
            this.MemCounter = (int)MemCounter;
        }
    }
}
