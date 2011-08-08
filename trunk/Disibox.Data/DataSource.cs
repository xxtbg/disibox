using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.IO;
using Disibox.Data.Entities;
using Disibox.Data.Exceptions;
using Disibox.Utils;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public class DataSource
    {
        private static CloudBlobClient _blobClient;
        private static CloudBlobContainer _blobContainer;
        private static string _filesBlobName;

        private static CloudQueue _processingQueue;

        private static CloudTableClient _tableClient;

        private string _loggedUserId = "test_da_togliere";
        private bool _loggedUserIsAdmin;
        private bool _userIsLoggedIn;

        /// <summary>
        /// 
        /// </summary>
        public DataSource()
        {
            var connectionString = Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            // We do not have to setup everything up every time,
            // that's why we pass "false" as parameter to next three calls.
            InitBlobs(storageAccount, false);
            InitQueues(storageAccount, false);
            InitTables(storageAccount, false);
        }

        public static void Main()
        {
            Setup();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileContent"></param>
        /// /// <exception cref="ArgumentNullException">Both parameters should not be null.</exception>
        /// /// <exception cref="LoggedInUserRequiredException">A user must be logged in to use this method.</exception>
        public string AddFile(string fileName, Stream fileContent)
        {
            // Requirements
            RequireNotNull(fileName, "fileName");
            RequireNotNull(fileContent, "fileContent");
            RequireLoggedInUser();

            var fileContentType = Common.GetContentType(fileName);
            return UploadFile(fileName, fileContentType, fileContent);
        }

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
            UploadUser(user);
        }

        /// <summary>
        /// Completely clears the storage and sets it up to the initial state.
        /// </summary>
        public static void Clear()
        {
            _blobContainer.Delete();
            _processingQueue.Delete();
            _tableClient.DeleteTableIfExist(Entry.EntryPartitionKey);
            _tableClient.DeleteTableIfExist(User.UserPartitionKey);

            Setup();
        }

        public static ProcessingRequest DequeueProcessingRequest()
        {
            var msg = _processingQueue.GetMessage();
            if (msg == null) return null;

            var procReq = ProcessingRequest.FromString(msg.AsString);
            _processingQueue.DeleteMessage(msg);

            return procReq;
        }

        public void EnqueueProcessingRequest(ProcessingRequest procReq)
        {
            // Requirements
            RequireLoggedInUser();

            var msg = new CloudQueueMessage(procReq.ToString());
            _processingQueue.AddMessage(msg);
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

            var ctx = _tableClient.GetDataServiceContext();
            var adminUsers = GetTable<User>(ctx, User.UserPartitionKey).Where(u => u.IsAdmin).ToList();
            return adminUsers.Select(u => u.Email).ToList();
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

            var ctx = _tableClient.GetDataServiceContext();
            var commonUsers = GetTable<User>(ctx, User.UserPartitionKey).Where(u => !u.IsAdmin).ToList();
            return commonUsers.Select(u => u.Email).ToList();
        }

        public Stream GetFile(string fileUri)
        {
            // Requirements
            RequireLoggedInUser();

            var blob = _blobContainer.GetBlobReference(fileUri);
            return blob.OpenRead();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IList<FileAndMime> GetFilesMetadata()
        {
            // Requirements
            RequireLoggedInUser();

            var options = new BlobRequestOptions();
            options.UseFlatBlobListing = true;

            var blobs = _blobContainer.ListBlobs(options);

            var names = new List<FileAndMime>();

            var prefix = _filesBlobName + "/";
            if (!_loggedUserIsAdmin) 
                prefix += _loggedUserId;
            var prefixLength = prefix.Length;

            if (_loggedUserIsAdmin)
                prefixLength--;

            foreach (var blob in blobs)
            {
                var uri = blob.Uri.ToString();
                var prefixStart = uri.IndexOf(prefix);
                var fileName = uri.Substring(prefixStart + prefixLength + 1);
                names.Add(new FileAndMime(fileName, Common.GetContentType(fileName), uri));
            }

            return names;
        }

        public IList<string> GetFilesNames()
        {
            // Requirements
            RequireLoggedInUser();

            var options = new BlobRequestOptions();
            options.UseFlatBlobListing = true;
            //options.BlobListingDetails = BlobListingDetails.All;

            var blobs = _blobContainer.ListBlobs(options);
            var names = new List<string>();

            var prefix = _filesBlobName + "/" + _loggedUserId;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="userPwd"></param>
        /// <exception cref="UserNotExistingException"></exception>
        public void Login(string userEmail, string userPwd)
        {
            var ctx = _tableClient.GetDataServiceContext();

            var hashedPwd = Hash.ComputeMD5(userPwd);
            var predicate = new Func<User, bool>(u => u.Email == userEmail && u.HashedPassword == hashedPwd);
            var q = GetTable<User>(ctx, User.UserPartitionKey).Where(predicate);
            if (q.Count() != 1)
                throw new UserNotExistingException();
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

        private static string GenerateFileName(string userId, string fileName)
        {
            return _filesBlobName + "/" + userId + "/" + fileName;
        }

        private static string GenerateUserId(bool userIsAdmin)
        {
            var ctx = _tableClient.GetDataServiceContext();

            var q = GetTable<Entry>(ctx, Entry.EntryPartitionKey).Where(e => e.RowKey == "NextUserId");
            var nextUserIdEntry = q.First();
            var nextUserId = int.Parse(nextUserIdEntry.Value);

            var firstIdChar = (userIsAdmin) ? 'a' : 'u';
            var userId = string.Format("{0}{1}", firstIdChar, nextUserId.ToString("D16"));

            nextUserId += 1;
            nextUserIdEntry.Value = nextUserId.ToString();
            
            // Next method must be called in order to save the update.
            ctx.UpdateObject(nextUserIdEntry);
            ctx.SaveChanges();

            return userId;
        }

        private static IQueryable<T> GetTable<T>(DataServiceContext ctx, string tableName) where T : TableServiceEntity
        {
            return ctx.CreateQuery<T>(tableName).Where(e => e.PartitionKey == tableName);
        }

        private static void InitBlobs(CloudStorageAccount storageAccount, bool doInitialSetup)
        {
            _blobClient = storageAccount.CreateCloudBlobClient();
            _filesBlobName = Properties.Settings.Default.FilesBlobName;
            _blobContainer = _blobClient.GetContainerReference(_filesBlobName);

            // Next instructions are dedicated to initial setup.
            if (!doInitialSetup) return;

            _blobContainer.CreateIfNotExist();

            var permissions = _blobContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            _blobContainer.SetPermissions(permissions);
        }

        private static void InitQueues(CloudStorageAccount storageAccount, bool doInitialSetup)
        {
            var queueClient = storageAccount.CreateCloudQueueClient();
            var processingQueueName = Properties.Settings.Default.ProcessingQueueName;
            _processingQueue = queueClient.GetQueueReference(processingQueueName);

            // Next instructions are dedicated to initial setup.
            if (!doInitialSetup) return;

            _processingQueue.CreateIfNotExist();
        }

        private static void InitTables(CloudStorageAccount storageAccount, bool doInitialSetup)
        {
            _tableClient = new CloudTableClient(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            _tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));

            // Next instructions are dedicated to initial setup.
            if (!doInitialSetup) return;

            InitEntriesTable();
            InitUsersTable();
        }

        private static void InitEntriesTable()
        {
            _tableClient.CreateTableIfNotExist(Entry.EntryPartitionKey);

            var ctx = _tableClient.GetDataServiceContext();

            var q = GetTable<Entry>(ctx, Entry.EntryPartitionKey).Where(e => e.RowKey == "NextUserId");
            if (Enumerable.Any(q)) return;

            var nextUserIdEntry = new Entry("NextUserId", 0.ToString());
            ctx.AddObject(Entry.EntryPartitionKey, nextUserIdEntry);
            ctx.SaveChanges();
        }

        private static void InitUsersTable()
        {
            _tableClient.CreateTableIfNotExist(User.UserPartitionKey);

            var ctx = _tableClient.GetDataServiceContext();

            var q = GetTable<User>(ctx, User.UserPartitionKey).Where(u => u.RowKey == "a0");
            if (Enumerable.Any(q)) return;

            var defaultAdminEmail = Properties.Settings.Default.DefaultAdminEmail;
            var defaultAdminPwd = Properties.Settings.Default.DefaultAdminPwd;
            var defaultAdminUser = new User("a0", defaultAdminEmail, defaultAdminPwd, true);

            ctx.AddObject(User.UserPartitionKey, defaultAdminUser);
            ctx.SaveChanges();
        }

        private static void Setup()
        {
            var connectionString = Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            InitBlobs(storageAccount, true);
            InitQueues(storageAccount, true);
            InitTables(storageAccount, true);
        }

        /// <summary>
        /// Uploads the file to blob storage.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contentType"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private string UploadFile(string name, string contentType, Stream content)
        {
            var uniqueBlobName = GenerateFileName(_loggedUserId, name);
            var blob = _blobClient.GetBlockBlobReference(uniqueBlobName);
            blob.Properties.ContentType = contentType;
            blob.UploadFromStream(content);
            return blob.Uri.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        private static void UploadUser(TableServiceEntity user)
        {
            var ctx = _tableClient.GetDataServiceContext();
            ctx.AddObject(user.PartitionKey, user);
            ctx.SaveChanges();
        }

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
        private void RequireNotNull(object obj, string paramName)
        {
            if (obj != null) return;
            throw new ArgumentNullException(paramName);
        }
    }
}