using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLibrary
{
    public static class AccountManager
    {
        public static CloudStorageAccount storageAccount { get; private set; }
        public static CloudQueueClient queueClient { get; private set; }
        public static CloudTableClient tableClient { get; private set; }
        private static string connString = "DefaultEndpointsProtocol=https;AccountName=krolcloudservicestorage;AccountKey=kxqrhi5PjnIzniLNN4Spe5l4AmLZyutTgYmOkg5e0hWwBYOIYK9KgqDYnHzrqXvEoAHT0dpsQLN3wIk1gUeHEg==";

        static AccountManager()
        {
            try
            {
                storageAccount = CloudStorageAccount.Parse(connString);
                queueClient = storageAccount.CreateCloudQueueClient();
                tableClient = storageAccount.CreateCloudTableClient();
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}