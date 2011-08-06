using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Disibox.Data.Entities;
using Disibox.Data.Exceptions;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public class DataSource
    {
        private readonly CloudTableClient _tableClient;

        private readonly CloudBlobClient _blobClient;
        private readonly CloudBlobContainer _blobContainer;
        private readonly string _filesBlobName;

        private readonly CloudQueue _processingQueue;

        private string _loggedUserId = "test_da_togliere";
        private UserType _loggedUserType;
        private bool _userIsLoggedIn;

        /// <summary>
        /// 
        /// </summary>
        public DataSource()
        {
            var connectionString = Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            _processingQueue = InitQueueClient(storageAccount);
            _tableClient = InitTableClient(storageAccount);

            // Creates files blob container
            _blobClient = storageAccount.CreateCloudBlobClient();
            _filesBlobName = Properties.Settings.Default.FilesBlobName;
            _blobContainer = _blobClient.GetContainerReference(_filesBlobName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [Obsolete]
        public string AddFile(string path)
        {
            // Requirements
            RequireLoggedInUser();

            var fileName = Path.GetFileName(path);
            var fileContentType = Utils.GetContentType(path);
            var fileContent = new FileStream(path, FileMode.Open);
            return UploadFile(fileName, fileContentType, fileContent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileContent"></param>
        public void AddFile(string fileName, Stream fileContent)
        {
            // Requirements
            RequireLoggedInUser();

            var fileContentType = Utils.GetContentType(fileName);
            UploadFile(fileName, fileContentType, fileContent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="userPwd"></param>
        /// <param name="userType"></param>
        /// <exception cref="LoggedInUserRequiredException"></exception>
        /// <exception cref="SpecialUserRequiredException"></exception>
        public void AddUser(string userEmail, string userPwd, UserType userType)
        {
            // Requirements
            RequireLoggedInUser();
            RequireUserType(UserType.AdminUser);

            var userId = GenerateUserId(userType);
            var user = new User(userId, userEmail, userPwd, userType);
            UploadUser(user);
        }

        public ProcessingRequest DequeueProcessingRequest()
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
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileAndMime> GetFileNames()
        {
            // Requirements
            RequireLoggedInUser();

            var blobs = _blobContainer.ListBlobs();
//            var names = new List<string>();
            var names = new List<FileAndMime>();
            foreach (var blob in blobs)
            {
                //                names.Add(blob.Uri.ToString());
                var filename = blob.Uri.ToString();
                names.Add(new FileAndMime(filename, Utils.GetContentType(filename)));
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

            var q = ctx.CreateQuery<User>(User.UserPartitionKey).Where(u => u.Matches(userEmail, userPwd));
            if (q.Count() != 1)
                throw new UserNotExistingException();
            var user = q.First();

            lock (this)
            {
                _userIsLoggedIn = true;
                _loggedUserId = user.RowKey;
                _loggedUserType = (user.IsAdmin) ? UserType.AdminUser : UserType.CommonUser;
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

        private string GenerateUserId(UserType userType)
        {
            var ctx = _tableClient.GetDataServiceContext();

            var q = ctx.CreateQuery<Entry>(Entry.EntryPartitionKey).Where(e => e.Name == "NextUserId");
            var nextUserId = int.Parse(q.First().Value);

            var userId = string.Format("{0}{1:16}", char.ToLower(userType.ToString()[0]), nextUserId);

            nextUserId += 1;
            q.First().Value = nextUserId.ToString();

            ctx.SaveChanges();

            return userId;
        }

        private void InitBlobClient(CloudStorageAccount storageAccount)
        {
            
        }

        private static CloudQueue InitQueueClient(CloudStorageAccount storageAccount)
        {
            var queueClient = storageAccount.CreateCloudQueueClient();
            var processingQueueName = Properties.Settings.Default.ProcessingQueueName;
            return queueClient.GetQueueReference(processingQueueName);
        }

        private static CloudTableClient InitTableClient(CloudStorageAccount storageAccount)
        {
            var tableClient = new CloudTableClient(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
            return tableClient;
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
            var uniqueBlobName = _filesBlobName + "/" + _loggedUserId + "/" + name;
            var blob = _blobClient.GetBlockBlobReference(uniqueBlobName);
            blob.Properties.ContentType = contentType;
            blob.UploadFromStream(content);
            return blob.Uri.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        private void UploadUser(TableServiceEntity user)
        {
            var ctx = _tableClient.GetDataServiceContext();
            ctx.AddObject(user.PartitionKey, user);
            ctx.SaveChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="LoggedInUserRequiredException"></exception>
        private void RequireLoggedInUser()
        {
            if (_userIsLoggedIn) return;
            //throw new LoggedInUserRequiredException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userType"></param>
        /// <exception cref="SpecialUserRequiredException"></exception>
        private void RequireUserType(UserType userType)
        {
            if (_loggedUserType == userType) return;
            //throw new SpecialUserRequiredException(userType);
        }
    }
}