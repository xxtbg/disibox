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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Disibox.Data.Entities;
using Disibox.Data.Exceptions;
using Disibox.Utils;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public class DataSource
    {
        private static CloudBlobContainer _filesContainer;
        private static CloudBlobContainer _outputsContainer;

        private static CloudQueue _processingRequests;
        private static CloudQueue _processingCompletions;

        private static DataContext<Entry> _entriesTableCtx;
        private static DataContext<User> _usersTableCtx;

        private static CloudTableClient _tableClient;

        private string _loggedUserId;
        private bool _loggedUserIsAdmin;
        private bool _userIsLoggedIn;

        /// <summary>
        /// 
        /// </summary>
        public DataSource()
        {
            Setup(false);
        }

        public static void Main()
        {
            Setup(true);
        }

        /*=============================================================================
            Processing queues methods
        =============================================================================*/

        public ProcessingMessage DequeueProcessingRequest()
        {
            // Requirements
            RequireLoggedInUser();
            RequireAdminUser();

            return DequeueProcessingMessage(_processingRequests);
        }

        public void EnqueueProcessingRequest(ProcessingMessage procReq)
        {
            // Requirements
            RequireNotNull(procReq, "procReq");
            RequireLoggedInUser();
            RequireAdminUser();

            EnqueueProcessingMessage(procReq, _processingRequests);
        }

        public ProcessingMessage DequeueProcessingCompletion()
        {
            // Requirements
            RequireLoggedInUser();
            RequireAdminUser();

            return DequeueProcessingMessage(_processingCompletions);
        }

        public void EnqueueProcessingCompletion(ProcessingMessage procCompl)
        {
            // Requirements
            RequireNotNull(procCompl, "procCompl");
            RequireLoggedInUser();
            RequireAdminUser();

            EnqueueProcessingMessage(procCompl, _processingCompletions);
        }

        private static ProcessingMessage DequeueProcessingMessage(CloudQueue procQueue)
        {
            CloudQueueMessage dequeuedMsg;
            while ((dequeuedMsg = procQueue.GetMessage()) == null)
                Thread.Sleep(1000);

            var procMsg = ProcessingMessage.FromString(dequeuedMsg.AsString);
            procQueue.DeleteMessage(dequeuedMsg);

            return procMsg;
        }

        private static void EnqueueProcessingMessage(ProcessingMessage procMsg, CloudQueue procQueue)
        {
            var msg = new CloudQueueMessage(procMsg.ToString());

            lock (procQueue)
            {
                procQueue.AddMessage(msg);
                Monitor.PulseAll(procQueue);
            }
        }

        /*=============================================================================
            File and output handling methods
        =============================================================================*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileContent"></param>
        /// <param name="overwrite">If it is true then the file on the cloud will be overwritten with <paramref name="fileName"/></param>
        /// <returns></returns>
        /// <exception cref="FileAlreadyExistingException">If the current user already have a file with the name <paramref name="fileName"/></exception>
        public string AddFile(string fileName, Stream fileContent, bool overwrite = false)
        {
            if (overwrite)
                return AddFile(fileName, fileContent);

            var fileToAdd = _loggedUserId + "/" + fileName;
            var filesOfUser = GetFileMetadata();

            foreach (var fileAndMime in filesOfUser)
            {
                string tempFileName = fileAndMime.Name;
                if (!_loggedUserIsAdmin)
                    tempFileName = _loggedUserId + "/" + tempFileName;

                if (tempFileName.Equals(fileToAdd))
                    throw new FileAlreadyExistingException();
            }

            return AddFile(fileName, fileContent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileContent"></param>
        /// <exception cref="ArgumentNullException">Both parameters should not be null.</exception>
        /// <exception cref="LoggedInUserRequiredException">A user must be logged in to use this method.</exception>
        public string AddFile(string fileName, Stream fileContent)
        {
            // Requirements
            RequireNotNull(fileName, "fileName");
            RequireNotNull(fileContent, "fileContent");
            RequireLoggedInUser();

            var cloudFileName = GenerateFileName(_loggedUserId, fileName);
            var fileContentType = Common.GetContentType(fileName);
            return UploadBlob(cloudFileName, fileContentType, fileContent, _filesContainer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileUri"></param>
        /// <returns>True if file has been really deleted, false otherwise.</returns>
        /// <exception cref="DeletingNotOwnedFileException">If a common user is trying to delete another user's file.</exception>
        public bool DeleteFile(string fileUri)
        {
            // Requirements
            RequireNotNull(fileUri, "fileUri");
            RequireLoggedInUser();
            
            // Administrators can delete every file.
            if (_loggedUserIsAdmin)
                return DeleteBlob(fileUri, _filesContainer);

            var prefix = _filesContainer.Name + "/" + _loggedUserId;

            if (fileUri.IndexOf(prefix) == -1 )
                throw new DeletingNotOwnedFileException();

            return DeleteBlob(fileUri, _filesContainer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileUri"></param>
        /// <returns></returns>
        public Stream GetFile(string fileUri)
        {
            // Requirements
            RequireNotNull(fileUri, "fileUri");
            RequireLoggedInUser();

            return DownloadBlob(fileUri, _filesContainer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="LoggedInUserRequiredException"></exception>
        /// <returns></returns>
        public IList<FileMetadata> GetFileMetadata()
        {
            // Requirements
            RequireLoggedInUser();

            var blobs = ListBlobs(_filesContainer);
            var prefix = _filesContainer.Name + "/";
            var prefixLength = prefix.Length;

            if (_loggedUserIsAdmin)
                prefixLength--;

            if (!_loggedUserIsAdmin)
                return (from blob in blobs
                        select (CloudBlob) blob into file
                        let uri = file.Uri.ToString()
                        let size = Common.ConvertBytesToKilobytes(file.Properties.Length)
                        let controlUserFiles = prefix + "" + _loggedUserId
                        let prefixStart = uri.IndexOf(controlUserFiles)
                        let fileName = uri.Substring(prefixStart + prefixLength + _loggedUserId.Length + 1)
                        where uri.IndexOf(controlUserFiles) != -1
                        select new FileMetadata(fileName, Common.GetContentType(fileName), uri, size)).ToList();

            return (from blob in blobs
                    select (CloudBlob) blob into file
                    let uri = file.Uri.ToString()
                    let size = Common.ConvertBytesToKilobytes(file.Properties.Length)
                    let prefixStart = uri.IndexOf(prefix)
                    let fileName = uri.Substring(prefixStart + prefixLength + 1)
                    select new FileMetadata(fileName, Common.GetContentType(fileName), uri, size)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="LoggedInUserRequiredException"></exception>
        /// <returns></returns>
        public IList<string> GetFileNames()
        {
            // Requirements
            RequireLoggedInUser();

            var blobs = ListBlobs(_filesContainer);
            var names = new List<string>();

            var prefix = _filesContainer.Name + "/" + _loggedUserId;
            var prefixLength = prefix.Length;

            foreach (var blob in blobs)
            {
                var uri = blob.Uri.ToString();
                var prefixStart = uri.IndexOf(prefix);
                var fileName = uri.Substring(prefixStart + prefixLength + 1);
                names.Add(fileName);
            }

            return names;
        }

        private static string GenerateFileName(string userId, string fileName)
        {
            return userId + "/" + fileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="toolName"></param>
        /// <param name="outputContentType"></param>
        /// <param name="outputContent"></param>
        /// <returns></returns>
        public string AddOutput(string toolName, string outputContentType, Stream outputContent)
        {
            // Requirements
            RequireNotNull(toolName, "toolName");
            RequireNotNull(outputContentType, "outputContentType");
            RequireNotNull(outputContent, "outputContent");
            RequireLoggedInUser();
            RequireAdminUser();

            var outputName = GenerateOutputName(toolName);
            return UploadBlob(outputName, outputContentType, outputContent, _outputsContainer);
        }

        public bool DeleteOutput(string outputUri)
        {
            // Requirements
            RequireNotNull(outputUri, "outputUri");
            RequireLoggedInUser();
            RequireAdminUser();

            return DeleteBlob(outputUri, _outputsContainer);
        }

        public Stream GetOutput(string outputUri)
        {
            // Requirements
            RequireNotNull(outputUri, "outputUri");
            RequireLoggedInUser();
            RequireAdminUser();

            return DownloadBlob(outputUri, _outputsContainer);
        }

        private static string GenerateOutputName(string toolName)
        {
            return toolName + Guid.NewGuid();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blobUri"></param>
        /// <param name="blobContainer"></param>
        /// <returns></returns>
        private static Stream DownloadBlob(string blobUri, CloudBlobContainer blobContainer)
        {
            var blob = blobContainer.GetBlockBlobReference(blobUri);
            return blob.OpenRead();
        }

        /// <summary>
        /// Uploads given stream to blob storage.
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="blobContentType"></param>
        /// <param name="blobContent"></param>
        /// <param name="blobContainer"></param>
        /// <returns></returns>
        private static string UploadBlob(string blobName, string blobContentType, Stream blobContent, CloudBlobContainer blobContainer)
        {
            blobContent.Seek(0, SeekOrigin.Begin);
            var blob = blobContainer.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = blobContentType;
            blob.UploadFromStream(blobContent);
            return blob.Uri.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blobUri"></param>
        /// <param name="blobContainer"></param>
        /// <returns></returns>
        private static bool DeleteBlob(string blobUri, CloudBlobContainer blobContainer)
        {
            var blob = blobContainer.GetBlobReference(blobUri);
            return blob.DeleteIfExists();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blobContainer"></param>
        /// <returns></returns>
        private static IEnumerable<IListBlobItem> ListBlobs(CloudBlobContainer blobContainer)
        {
            var options = new BlobRequestOptions {UseFlatBlobListing = true};
            return  blobContainer.ListBlobs(options);
        }

        /*=============================================================================
            User handling methods
        =============================================================================*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="userPwd"></param>
        /// <param name="userIsAdmin"></param>
        /// <exception cref="AdminUserRequiredException">Only administrators can use this method.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="LoggedInUserRequiredException">A user must be logged in to use this method.</exception>
        public void AddUser(string userEmail, string userPwd, bool userIsAdmin)
        {
            // Requirements
            RequireNotNull(userEmail, "userEmail");
            RequireNotNull(userPwd, "userPwd");
            RequireLoggedInUser();
            RequireAdminUser();

            var userId = GenerateUserId(userIsAdmin);
            var user = new User(userId, userEmail, userPwd, userIsAdmin);
            
            var ctx = _tableClient.GetDataServiceContext();
            ctx.AddObject(user.PartitionKey, user);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Deletes user corresponding to given email address.
        /// </summary>
        /// <param name="userEmail">The email address of the user that should be deleted.</param>
        /// <exception cref="CannotDeleteUserException"></exception>
        /// <exception cref="UserNotExistingException"></exception>
        /// <exception cref="AdminUserRequiredException"></exception>
        /// <exception cref="LoggedInUserRequiredException"></exception>
        public void DeleteUser(string userEmail)
        {
            // Requirements
            RequireNotNull(userEmail, "userEmail");
            RequireLoggedInUser();
            RequireAdminUser();

            if (userEmail == Properties.Settings.Default.DefaultAdminEmail)
                throw new CannotDeleteUserException();
            
            // Added a call to ToList() to avoid an error on Count() call.
            var q = _usersTableCtx.Entities.Where(u => u.Email == userEmail).ToList();
            if (q.Count() == 0)
                throw new UserNotExistingException(userEmail);
            var user = q.First();

            _usersTableCtx.DeleteEntity(user);
            _usersTableCtx.SaveChanges();
        }

        /// <summary>
        /// Completely clears the storage and sets it up to the initial state.
        /// </summary>
        public static void Clear()
        {
            // There seems to be no way to check if a container really exists...
            _filesContainer.CreateIfNotExist();
            _filesContainer.Delete();
            _outputsContainer.CreateIfNotExist();
            _outputsContainer.Delete();

            // Same problem for the queues...
            _processingRequests.CreateIfNotExist();
            _processingRequests.Delete();
            _processingCompletions.CreateIfNotExist();
            _processingCompletions.Delete();

            _tableClient.DeleteTableIfExist(Entry.EntryPartitionKey);
            _tableClient.DeleteTableIfExist(User.UserPartitionKey);

            Setup(true);
        }

        /// <summary>
        /// Fetches and returns all administrators emails.
        /// </summary>
        /// <returns>All administrators emails.</returns>
        /// <exception cref="LoggedInUserRequiredException">A user must be logged in to use this method.</exception>
        /// <exception cref="AdminUserRequiredException">Only administrators can use this method.</exception>
        public IList<string> GetAdminUsersEmails()
        {
            // Requirements
            RequireLoggedInUser();
            RequireAdminUser();

            var users = _usersTableCtx.Entities.ToList();
            return users.Where(u => u.IsAdmin).Select(u => u.Email).ToList();
        }

        /// <summary>
        /// Fetches and returns all common users emails.
        /// </summary>
        /// <returns>All common users emails.</returns>
        /// <exception cref="LoggedInUserRequiredException">A user must be logged in to use this method.</exception>
        /// <exception cref="AdminUserRequiredException">Only administrators can use this method.</exception>
        public IList<string> GetCommonUsersEmails()
        {
            // Requirements
            RequireLoggedInUser();
            RequireAdminUser();

            var users = _usersTableCtx.Entities.ToList();
            return users.Where(u => !u.IsAdmin).Select(u => u.Email).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="userPwd"></param>
        /// <exception cref="UserNotExistingException"></exception>
        public void Login(string userEmail, string userPwd)
        {
            var hashedPwd = Hash.ComputeMD5(userPwd);
            var predicate = new Func<User, bool>(u => u.Email == userEmail && u.HashedPassword == hashedPwd);
            var q = _usersTableCtx.Entities.Where(predicate);
            if (q.Count() != 1)
                throw new UserNotExistingException(userEmail);
            var user = q.First();

            lock (this)
            {
                _userIsLoggedIn = true;
                _loggedUserId = user.RowKey;
                _loggedUserIsAdmin = user.IsAdmin;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Logout()
        {
            lock (this)
            {
                _userIsLoggedIn = false;
            }
        }

        private static string GenerateUserId(bool userIsAdmin)
        {
            var q = _entriesTableCtx.Entities.Where(e => e.RowKey == "NextUserId");
            var nextUserIdEntry = q.First();
            var nextUserId = int.Parse(nextUserIdEntry.Value);

            var firstIdChar = (userIsAdmin) ? 'a' : 'u';
            var userId = string.Format("{0}{1}", firstIdChar, nextUserId.ToString("D16"));

            nextUserId += 1;
            nextUserIdEntry.Value = nextUserId.ToString();
            
            // Next method must be called in order to save the update.
            _entriesTableCtx.UpdateEntity(nextUserIdEntry);
            _entriesTableCtx.SaveChanges();

            return userId;
        }

        private static void InitBlobs(CloudStorageAccount storageAccount, bool doInitialSetup)
        {
            var blobClient = storageAccount.CreateCloudBlobClient();
            
            var filesBlobName = Properties.Settings.Default.FilesBlobName;
            _filesContainer = blobClient.GetContainerReference(filesBlobName);

            var outputsBlobName = Properties.Settings.Default.OutputsBlobName;
            _outputsContainer = blobClient.GetContainerReference(outputsBlobName);

            // Next instructions are dedicated to initial setup.
            if (!doInitialSetup) return;

            _filesContainer.CreateIfNotExist();
            _outputsContainer.CreateIfNotExist();

            var permissions = _filesContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            _filesContainer.SetPermissions(permissions);
            _outputsContainer.SetPermissions(permissions);
        }

        private static void InitQueues(CloudStorageAccount storageAccount, bool doInitialSetup)
        {
            var queueClient = storageAccount.CreateCloudQueueClient();
            
            var processingRequestsName = Properties.Settings.Default.ProcessingRequestsName;
            _processingRequests = queueClient.GetQueueReference(processingRequestsName);

            var processingCompletionsName = Properties.Settings.Default.ProcessingCompletionsName;
            _processingCompletions = queueClient.GetQueueReference(processingCompletionsName);

            // Next instructions are dedicated to initial setup.)
            if (!doInitialSetup) return;

            _processingRequests.CreateIfNotExist();
            _processingCompletions.CreateIfNotExist();
        }

        private static void InitTables(CloudStorageAccount storageAccount, bool doInitialSetup)
        {
            _tableClient = new CloudTableClient(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            _tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));

            // Next instructions are dedicated to initial setup.
            if (!doInitialSetup) return;

            InitEntriesTable(storageAccount.Credentials);
            InitUsersTable(storageAccount.Credentials);
        }

        private static void InitEntriesTable(StorageCredentials credentials)
        {
            _tableClient.CreateTableIfNotExist(Entry.EntryPartitionKey);

            _entriesTableCtx = new DataContext<Entry>(Entry.EntryPartitionKey, _tableClient.BaseUri.ToString(), credentials);

            var q = _entriesTableCtx.Entities.Where(e => e.RowKey == "NextUserId");
            if (Enumerable.Any(q)) return;

            var nextUserIdEntry = new Entry("NextUserId", 0.ToString());
            _entriesTableCtx.AddEntity(nextUserIdEntry);
            _entriesTableCtx.SaveChanges();
        }

        private static void InitUsersTable(StorageCredentials credentials)
        {
            _tableClient.CreateTableIfNotExist(User.UserPartitionKey);

            _usersTableCtx = new DataContext<User>(User.UserPartitionKey, _tableClient.BaseUri.ToString(), credentials);

            var q = _usersTableCtx.Entities.Where(u => u.RowKey == "a0");
            if (Enumerable.Any(q)) return;

            var defaultAdminEmail = Properties.Settings.Default.DefaultAdminEmail;
            var defaultAdminPwd = Properties.Settings.Default.DefaultAdminPwd;
            var defaultAdminUser = new User("a0", defaultAdminEmail, defaultAdminPwd, true);

            _usersTableCtx.AddEntity(defaultAdminUser);
            _usersTableCtx.SaveChanges();
        }

        private static void Setup(bool createIfNotExist)
        {
            var connectionString = Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            InitBlobs(storageAccount, createIfNotExist);
            InitQueues(storageAccount, createIfNotExist);
            InitTables(storageAccount, createIfNotExist);
        }

        /*=============================================================================
            Requirement checking methods
        =============================================================================*/

        /// <summary>
        /// Checks if currently logged in user is administrator;
        /// if he's not, an appropriate exception is thrown.
        /// </summary>
        /// <exception cref="AdminUserRequiredException">If logged in user is not administrator.</exception>
        private void RequireAdminUser()
        {
            if (_loggedUserIsAdmin) return;
            throw new AdminUserRequiredException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="LoggedInUserRequiredException"></exception>
        private void RequireLoggedInUser()
        {
            if (_userIsLoggedIn) return;
            throw new LoggedInUserRequiredException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="paramName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void RequireNotNull(object obj, string paramName)
        {
            if (obj != null) return;
            throw new ArgumentNullException(paramName);
        }
    }
}