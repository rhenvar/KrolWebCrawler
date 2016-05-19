using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLibrary
{
    public class DiagnosticEntity : TableEntity
    {
        public float CpuUsage { get; set; }
        public float MemUsage { get; set; }
        public string[] LastCrawled { get; set; }
        public string[] ErrorUrls { get; set; }
        public int TotalCrawled { get; set; }
        public int QueueSize { get; set; }
        public int IndexSize { get; set; }
        public DateTime IndexDate { get; set; }
        public Dictionary<string, string> RoleStats{ get; set;}

        public DiagnosticEntity(float cpuUsage, float memUsage, string[] lastCrawled, string[] errorUrls, int totalCrawled, int queueSize, int indexSize, Dictionary<string, string> roleStats)
        {
            this.PartitionKey = Convert.ToString(DateTime.Now.Ticks);
            this.CpuUsage = cpuUsage;
            this.MemUsage = memUsage;
            this.LastCrawled = lastCrawled;
            this.ErrorUrls = ErrorUrls;
            this.TotalCrawled = totalCrawled;
            this.QueueSize = queueSize;
            this.IndexSize = indexSize;
            this.RoleStats = roleStats;
            this.IndexDate = DateTime.Now;
            this.RowKey = "DUMMY KEY";
        }

        public DiagnosticEntity() { }
    }
}
