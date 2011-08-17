using System;
using System.Linq;
using Disibox.Data.Common;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data.Setup
{
    public static class StorageSetup
    {
        private static bool _printSteps;

        public static void Main(string[] args)
        {
            SetupStorage(false, true);
        }

        public static void SetupStorage()
        {
            SetupStorage(false);
        }

        /// <summary>
        /// Completely cleans the storage up and sets it up to the initial state.
        /// </summary>
        public static void CleanupStorage()
        {
            SetupStorage(true);
        }

        private static void SetupStorage(bool doCleanup = false, bool printSteps = false)
        {
            _printSteps = printSteps;

            var connectionString = Common.Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            SetupBlobs(storageAccount, doCleanup);
            SetupQueues(storageAccount, doCleanup);
            SetupTables(storageAccount, doCleanup);
        }

        private static void SetupBlobs(CloudStorageAccount storageAccount, bool doCleanup)
        {
            Print("Setting blobs up...");

            var blobClient = storageAccount.CreateCloudBlobClient();

            var filesBlobName = Common.Properties.Settings.Default.FilesContainerName;
            var filesContainer = blobClient.GetContainerReference(filesBlobName);

            var outputsBlobName = Common.Properties.Settings.Default.OutputsContainerName;
            var outputsContainer = blobClient.GetContainerReference(outputsBlobName);

            filesContainer.CreateIfNotExist();
            if (doCleanup)
            {
                filesContainer.Delete();
                filesContainer.Create();
            }

            outputsContainer.CreateIfNotExist();
            if (doCleanup)
            {
                outputsContainer.Delete();
                outputsContainer.Create();
            }

            var permissions = filesContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            filesContainer.SetPermissions(permissions);
            outputsContainer.SetPermissions(permissions);
        }

        private static void SetupQueues(CloudStorageAccount storageAccount, bool doCleanup)
        {
            Print("Setting queues up...");

            var queueClient = storageAccount.CreateCloudQueueClient();

            var processingRequestsName = Common.Properties.Settings.Default.ProcReqQueueName;
            var processingRequests = queueClient.GetQueueReference(processingRequestsName);

            var processingCompletionsName = Common.Properties.Settings.Default.ProcComplQueueName;
            var processingCompletions = queueClient.GetQueueReference(processingCompletionsName);

            processingRequests.CreateIfNotExist();
            if (doCleanup)
            {
                processingRequests.Delete();
                processingRequests.Create();
            }

            processingCompletions.CreateIfNotExist();
            if (doCleanup)
            {
                processingCompletions.Delete();
                processingCompletions.Create();
            }
        }

        private static void SetupTables(CloudStorageAccount storageAccount, bool doCleanup)
        {
            Print("Setting tables up...");

            var tableClient = new CloudTableClient(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));

            SetupEntriesTable(tableClient, doCleanup);
            SetupUsersTable(tableClient, doCleanup);
        }

        private static void SetupEntriesTable(CloudTableClient tableClient, bool doCleanup)
        {
            var entriesTableName = Common.Properties.Settings.Default.EntriesTableName;
            if (doCleanup)
                tableClient.DeleteTableIfExist(entriesTableName);
            tableClient.CreateTableIfNotExist(entriesTableName);

            var tableEndpointUri = tableClient.BaseUri.ToString();
            var credentials = tableClient.Credentials;
            var ctx = new DataContext<Entry>(entriesTableName, tableEndpointUri, credentials);

            var q = ctx.Entities.Where(e => e.RowKey == "NextUserId");
            if (Enumerable.Any(q)) return;

            var nextUserIdEntry = new Entry("NextUserId", 0.ToString());
            ctx.AddEntity(nextUserIdEntry);
            ctx.SaveChanges();
        }

        private static void SetupUsersTable(CloudTableClient tableClient, bool doCleanup)
        {
            var usersTableName = Common.Properties.Settings.Default.UsersTableName;
            if (doCleanup)
                tableClient.DeleteTableIfExist(usersTableName);
            tableClient.CreateTableIfNotExist(usersTableName);

            var tableEndpointUri = tableClient.BaseUri.ToString();
            var credentials = tableClient.Credentials;
            var ctx = new DataContext<User>(usersTableName, tableEndpointUri, credentials);

            var q = ctx.Entities.Where(u => u.RowKey == "a0");
            if (Enumerable.Any(q)) return;

            var defaultAdminEmail = Properties.Settings.Default.DefaultAdminEmail;
            var defaultAdminPwd = Properties.Settings.Default.DefaultAdminPwd;
            var defaultAdminUser = new User("a0", defaultAdminEmail, defaultAdminPwd, true);

            ctx.AddEntity(defaultAdminUser);
            ctx.SaveChanges();
        }

        private static void Print(string msg)
        {
            if (!_printSteps) return;
            Console.WriteLine(msg);
        }
    }
}
