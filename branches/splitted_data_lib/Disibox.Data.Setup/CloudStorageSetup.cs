//
// Copyright (c) 2011, University of Genoa
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the University of Genoa nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL UNIVERSITY OF GENOA BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.Linq;
using Disibox.Data.Common;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data.Setup
{
    public static class CloudStorageSetup
    {
        public static void Main()
        {
            SetupStorage(false, true);
        }

        public static void ResetStorage()
        {
            SetupStorage(true, false);
        }

        private static void SetupStorage(bool doReset, bool printSteps)
        {
            var connectionString = Common.Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            SetupBlobContainers(storageAccount, doReset, printSteps);
            PrintStep("", printSteps);
            SetupProcessingQueues(storageAccount, doReset, printSteps);
            PrintStep("", printSteps);
            SetupEntriesTable(storageAccount, doReset, printSteps);
            PrintStep("", printSteps);
            SetupUsersTable(storageAccount, doReset, printSteps);
        }

        private static void SetupBlobContainers(CloudStorageAccount storageAccount, bool doReset, bool printSteps)
        {
            PrintStep("Creating blob client...", printSteps);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var filesContainerName = Common.Properties.Settings.Default.FilesContainerName;
            SetupBlobContainer(blobClient, filesContainerName, doReset, printSteps);

            var outputsContainerName = Common.Properties.Settings.Default.OutputsContainerName;
            SetupBlobContainer(blobClient, outputsContainerName, doReset, printSteps);
        }

        private static void SetupBlobContainer(CloudBlobClient blobClient, string blobContainerName, bool doReset, bool printSteps)
        {
            PrintStep("Creating " + blobContainerName + " blob container...", printSteps);
            var blobContainer = blobClient.GetContainerReference(blobContainerName);

            blobContainer.CreateIfNotExist();
            if (doReset)
            {
                PrintStep(" * Resetting its content", printSteps);
                blobContainer.Delete();
                blobContainer.Create();
            }

            PrintStep(" * Setting permissions up", printSteps);
            var permissions = blobContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            blobContainer.SetPermissions(permissions);
        }

        private static void SetupProcessingQueues(CloudStorageAccount storageAccount, bool doReset, bool printSteps)
        {
            PrintStep("Creating queue client...", printSteps);
            var queueClient = storageAccount.CreateCloudQueueClient();

            var processingRequestsName = Common.Properties.Settings.Default.ProcReqQueueName;
            SetupProcessingQueue(queueClient, processingRequestsName, doReset, printSteps);

            var processingCompletionsName = Common.Properties.Settings.Default.ProcComplQueueName;
            SetupProcessingQueue(queueClient, processingCompletionsName, doReset, printSteps);
        }

        private static void SetupProcessingQueue(CloudQueueClient queueClient, string processingQueueName, bool doReset, bool printSteps)
        {
            PrintStep("Creating " + processingQueueName + " processing queue...", printSteps);
            var processingQueue = queueClient.GetQueueReference(processingQueueName);

            processingQueue.CreateIfNotExist();
            if (!doReset) return;
            PrintStep(" * Resetting its content", printSteps);
            processingQueue.Delete();
            processingQueue.Create();
        }

        private static void SetupEntriesTable(CloudStorageAccount storageAccount, bool doReset, bool printSteps)
        {
            var tableClient = new CloudTableClient(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));

            var entriesTableName = Common.Properties.Settings.Default.EntriesTableName;
            PrintStep("Creating " + entriesTableName + " table...", printSteps);
            if (doReset)
                tableClient.DeleteTableIfExist(entriesTableName);
            tableClient.CreateTableIfNotExist(entriesTableName);

            PrintStep(" * Adding default entries", printSteps);

            var tableServiceUri = storageAccount.TableEndpoint.AbsoluteUri;
            var credentials = storageAccount.Credentials;
            var entriesTableCtx = new DataContext<Entry>(entriesTableName, tableServiceUri, credentials);

            var q = entriesTableCtx.Entities.Where(e => e.RowKey == "NextUserId").ToList();
            if (q.Any()) return;

            var nextUserIdEntry = new Entry("NextUserId", 0.ToString());
            entriesTableCtx.AddEntity(nextUserIdEntry);
            entriesTableCtx.SaveChanges();
        }

        private static void SetupUsersTable(CloudStorageAccount storageAccount, bool doReset, bool printSteps)
        {
            var tableClient = new CloudTableClient(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));

            var usersTableName = Common.Properties.Settings.Default.UsersTableName;
            PrintStep("Creating " + usersTableName + " table...", printSteps);
            if (doReset)
                tableClient.DeleteTableIfExist(usersTableName);
            tableClient.CreateTableIfNotExist(usersTableName);

            PrintStep(" * Adding default users", printSteps);

            var tableServiceUri = storageAccount.TableEndpoint.AbsoluteUri;
            var credentials = storageAccount.Credentials;
            var usersTableCtx = new DataContext<User>(usersTableName, tableServiceUri, credentials);

            var q = usersTableCtx.Entities.Where(u => u.RowKey == "a0").ToList();
            if (q.Any()) return;

            var defaultAdminEmail = Properties.Settings.Default.DefaultAdminEmail;
            var defaultAdminPwd = Properties.Settings.Default.DefaultAdminPwd;
            var defaultAdminUser = new User("a0", defaultAdminEmail, defaultAdminPwd, true);

            usersTableCtx.AddEntity(defaultAdminUser);
            usersTableCtx.SaveChanges();
        }

        private static void PrintStep(string step, bool printSteps)
        {
            if (!printSteps) return;
            Console.WriteLine(step);
        }
    }
}
