using System;
using System.Linq;
using Disibox.Data.Entities;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public static class Setup
    {
        public static void Main(string[] args)
        {
            var connectionString = Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            InitBlobs(storageAccount);
            InitQueues(storageAccount);
            InitTables(storageAccount);
        }

        private static void InitBlobs(CloudStorageAccount storageAccount)
        {
            // Creates files blob container
            var blobClient = storageAccount.CreateCloudBlobClient();
            var filesBlobName = Properties.Settings.Default.FilesBlobName;
            var blobContainer = blobClient.GetContainerReference(filesBlobName);
            blobContainer.CreateIfNotExist();

            // Set blob container permissions
            var permissions = blobContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            blobContainer.SetPermissions(permissions);
        }

        private static void InitQueues(CloudStorageAccount storageAccount)
        {
            var queueClient = storageAccount.CreateCloudQueueClient();
            var processingQueueName = Properties.Settings.Default.ProcessingQueueName;
            var processingQueue = queueClient.GetQueueReference(processingQueueName);
            processingQueue.CreateIfNotExist();
        }

        private static void InitTables(CloudStorageAccount storageAccount)
        {
            var tableClient = new CloudTableClient(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));

            InitEntriesTable(tableClient);
            InitUsersTable(tableClient);
        }

        private static void InitEntriesTable(CloudTableClient tableClient)
        {
            tableClient.CreateTableIfNotExist(Entry.EntryPartitionKey);

            /*var ctx = tableClient.GetDataServiceContext();

            var q = ctx.CreateQuery<Entry>(Entry.EntryPartitionKey).Where(e => e.Name == "NextUserId");
            if (q.Count() != 0) return;

            var nextUserIdEntry = new Entry("NextUserId", 0.ToString());
            ctx.AddObject(Entry.EntryPartitionKey, nextUserIdEntry);
            ctx.SaveChanges();*/
        }

        private static void InitUsersTable(CloudTableClient tableClient)
        {
            tableClient.CreateTableIfNotExist(User.UserPartitionKey);

            var ctx = tableClient.GetDataServiceContext();

            var q = ctx.CreateQuery<User>(User.UserPartitionKey).Where(e => e.Id == "a0");
            if (q.Count() != 0) return;

            var defaultAdminEmail = Properties.Settings.Default.DefaultAdminEmail;
            var defaultAdminPwd = Properties.Settings.Default.DefaultAdminPwd;
            var defaultAdminUser = new User("a0", defaultAdminEmail, defaultAdminPwd, UserType.AdminUser);

            ctx.AddObject(User.UserPartitionKey, defaultAdminUser);
            ctx.SaveChanges();
        }
    }
}
