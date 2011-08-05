using System;
using Disibox.Data.Entities;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public static class Setup
    {
        private const string ConnectionStringName = "DataConnectionString";

        private const string FilesBlobName = "files";

        public static void Main(string[] args)
        {
            string connectionString = "UseDevelopmentStorage=true";
                //RoleEnvironment.GetConfigurationSettingValue(connectionStringName);

            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var tableClient = new CloudTableClient(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials)
                                  {
                                      RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1))
                                  };

            InitEntriesTable(tableClient);
            InitUsersTable(tableClient);

            // Creates files blob container
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(FilesBlobName);
            blobContainer.CreateIfNotExist();

            // Set blob container permissions
            var permissions = blobContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            blobContainer.SetPermissions(permissions);
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
            var defaultAdminUser = new User("a0", "admin", "admin", UserType.AdminUser);
            ctx.AddObject(User.UserPartitionKey, defaultAdminUser);
            ctx.SaveChanges();
        }
    }
}
