using System;
using System.Configuration;
using Disibox.Data.Entities;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public static class Setup
    {
        // Keys to access settings in app.config
        private const string DataConnectionStringKey = "DataConnectionString";
        private const string DefaultAdminEmailKey = "DefaultAdminName";
        private const string DefaultAdminPwdKey = "DefaultAdminPwd";
        private const string FilesBlobNameKey = "FilesBlobName";
        private const string ProcessingQueueNameKey = "ProcessingQueueName";

        public static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.AppSettings.Get(DataConnectionStringKey);
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            InitBlobs(storageAccount);
            InitQueues(storageAccount);
            InitTables(storageAccount);
        }

        private static void InitBlobs(CloudStorageAccount storageAccount)
        {
            // Creates files blob container
            var blobClient = storageAccount.CreateCloudBlobClient();
            var filesBlobName = ConfigurationManager.AppSettings.Get(FilesBlobNameKey);
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
            var processingQueueName = ConfigurationManager.AppSettings.Get(ProcessingQueueNameKey);
            var queue = queueClient.GetQueueReference(processingQueueName);
            queue.CreateIfNotExist();
        }

        private static void InitTables(CloudStorageAccount storageAccount)
        {
            var tableClient = new CloudTableClient(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials)
            {
                RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1))
            };

            InitEntriesTable(tableClient);
            InitUsersTable(tableClient);
        }

        private static void InitEntriesTable(CloudTableClient tableClient)
        {
            tableClient.CreateTableIfNotExist(Entry.EntryPartitionKey);

            var ctx = tableClient.GetDataServiceContext();
            var nextUserIdEntry = new Entry("NextUserId", 0.ToString());
            ctx.AddObject(Entry.EntryPartitionKey, nextUserIdEntry);
            ctx.SaveChanges();
        }

        private static void InitUsersTable(CloudTableClient tableClient)
        {
            tableClient.CreateTableIfNotExist(User.UserPartitionKey);

            var ctx = tableClient.GetDataServiceContext();
            var defaultAdminEmail = ConfigurationManager.AppSettings.Get(DefaultAdminEmailKey);
            var defaultAdminPwd = ConfigurationManager.AppSettings.Get(DefaultAdminPwdKey);
            var defaultAdminUser = new User("a0", defaultAdminEmail, defaultAdminPwd, UserType.AdminUser);
            ctx.AddObject(User.UserPartitionKey, defaultAdminUser);
            ctx.SaveChanges();
        }
    }
}
