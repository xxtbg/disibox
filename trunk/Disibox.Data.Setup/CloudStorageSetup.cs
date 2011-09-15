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
using System.IO;
using System.Linq;
using Disibox.Data.Entities;
using Disibox.Data.Server;
using Disibox.Data.Setup.Properties;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data.Setup
{
    public static class CloudStorageSetup
    {
        private static bool _doReset;
        private static bool _printSteps;

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
            var connectionString = Data.Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var blobEndpointUri = storageAccount.BlobEndpoint.ToString();
            var queueEndpointUri = storageAccount.QueueEndpoint.ToString();
            var tableEndpointUri = storageAccount.TableEndpoint.ToString();
            var credentials = storageAccount.Credentials;

            _doReset = doReset;
            _printSteps = printSteps;

            SetupContainers(blobEndpointUri, credentials);
            PrintStep("");
            SetupProcessingQueues(queueEndpointUri, credentials);
            PrintStep("");
            SetupEntriesTable(tableEndpointUri, credentials);
            PrintStep("");
            SetupUsersTable(tableEndpointUri, credentials);
        }

        private static void SetupContainers(string blobEndpointUri, StorageCredentials credentials)
        {
            PrintStep("Creating blob containers...");

            var filesContainerName = Data.Properties.Settings.Default.FilesContainerName;
            SetupBlobContainer(filesContainerName, blobEndpointUri, credentials);

            var outputsContainerName = Data.Properties.Settings.Default.OutputsContainerName;
            SetupBlobContainer(outputsContainerName, blobEndpointUri, credentials);
            
            SetupProcDllsContainer(blobEndpointUri, credentials);
        }

        private static void SetupProcDllsContainer(string blobEndpointUri, StorageCredentials credentials)
        {
            var containerName = Data.Properties.Settings.Default.ProcDllsContainerName;
            var container = SetupBlobContainer(containerName, blobEndpointUri, credentials);

            PrintStep(" * Adding default DLL");
            using (var defaultDll = new MemoryStream(Resources.DefaultTools))
                container.AddBlob("DefaultTools.dll", "application/x-msdownload", defaultDll);
        }

        private static AzureContainer SetupBlobContainer(string containerName, string blobEndpointUri, StorageCredentials credentials)
        {
            PrintStep("Creating " + containerName + " blob container...");
            var container = AzureContainer.Create(containerName, blobEndpointUri, credentials);
            if (_doReset)
            {
                PrintStep(" * Resetting its content");
                container.Clear();
            }

            PrintStep(" * Setting permissions up");
            container.Permissions.PublicAccess = BlobContainerPublicAccessType.Off;

            return container;
        }

        private static void SetupProcessingQueues(string queueEndpointUri, StorageCredentials credentials)
        {
            PrintStep("Creating queue client...");

            var processingRequestsName = Data.Properties.Settings.Default.ProcReqQueueName;
            SetupProcessingQueue(processingRequestsName, queueEndpointUri, credentials);

            var processingCompletionsName = Data.Properties.Settings.Default.ProcComplQueueName;
            SetupProcessingQueue(processingCompletionsName, queueEndpointUri, credentials);
        }

        private static void SetupProcessingQueue(string queueName, string queueEndpointUri, StorageCredentials credentials)
        {
            PrintStep("Creating " + queueName + " processing queue...");
            var queue = AzureQueue<ProcessingMessage>.Create(queueName, queueEndpointUri, credentials);

            if (!_doReset) return;
            PrintStep(" * Resetting its content");
            queue.Clear();
        }

        private static void SetupEntriesTable(string tableEndpointUri, StorageCredentials credentials)
        {
            PrintStep("Creating entries table...");
            var entriesTable = AzureTable<Entry>.Create(tableEndpointUri, credentials);
            if (_doReset)
                entriesTable.Clear();

            PrintStep(" * Adding default entries");

            var q = entriesTable.Entities.Where(e => e.RowKey == "NextUserId").ToList();
            if (q.Any()) return;

            var nextUserIdEntry = new Entry("NextUserId", 0.ToString());
            entriesTable.AddEntity(nextUserIdEntry);
            entriesTable.SaveChanges();
        }

        private static void SetupUsersTable(string tableEndpointUri, StorageCredentials credentials)
        {
            PrintStep("Creating users table...");
            var usersTable = AzureTable<User>.Create(tableEndpointUri, credentials);
            if (_doReset)
                usersTable.Clear();

            PrintStep(" * Adding default users");

            var q = usersTable.Entities.Where(u => u.RowKey == "a0").ToList();
            if (q.Any()) return;

            var defaultAdminEmail = Settings.Default.DefaultAdminEmail;
            var defaultAdminPwd = Settings.Default.DefaultAdminPwd;
            var defaultAdminUser = new User("a0", defaultAdminEmail, defaultAdminPwd, UserType.AdminUser);

            usersTable.AddEntity(defaultAdminUser);
            usersTable.SaveChanges();
        }

        private static void PrintStep(string step)
        {
            if (!_printSteps) return;
            Console.WriteLine(step);
        }
    }
}
