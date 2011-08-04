using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Win32;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public class DataSource
    {
        private const string usersTableName = "users";
        private const string filesBlobName = "files";
        private const string connectionStringName = "DataConnectionString";
        
        private static CloudStorageAccount storageAccount;
        
        private CloudTableClient tableClient;

        private readonly CloudBlobClient blobClient;
        private readonly CloudBlobContainer blobContainer;

        private string _loggedUserId = null;
        private bool _userIsAdmin = false;
        private bool _userIsLoggedIn = false;

        //The default constructor initializes the storage account by reading its settings from
        //the configuration and then uses CreateTableIfNotExist method in the CloudTableClient
        //class to create the table used by the application.
        public DataSource()
        {
            string connectionString = "UseDevelopmentStorage=true";//RoleEnvironment.GetConfigurationSettingValue(connectionStringName);

            storageAccount = CloudStorageAccount.Parse(connectionString);
            
            // Creates users table
            tableClient = new CloudTableClient(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
            tableClient.CreateTableIfNotExist(usersTableName);

            // Creates files blob container
            blobClient = storageAccount.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference(filesBlobName);
            blobContainer.CreateIfNotExist();

            // Set blob container permissions
            var permissions = blobContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            blobContainer.SetPermissions(permissions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string AddFile(string path)
        {
            // Requirements
            RequireLoggedInUser();

            var fileName = Path.GetFileName(path);
            var fileContentType = GetContentType(path);
            var fileContent = new FileStream(path, FileMode.Open);
            return UploadFile(fileName, fileContentType, fileContent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="userPwd"></param>
        /// <param name="userIsAdmin"></param>
        public void AddUser(string userEmail, string userPwd, bool userIsAdmin)
        {
            // Requirements
            RequireLoggedInUser();
            RequireAdminUser();

            var user = new User(userEmail, userPwd, userIsAdmin);
            UploadUser(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileAndMime> GetFileNames()
        {
            // Requirements
            RequireLoggedInUser();

            var blobs = blobContainer.ListBlobs();
//            var names = new List<string>();
            var names = new List<FileAndMime>();
            foreach (var blob in blobs) {
                //                names.Add(blob.Uri.ToString());
                var filename = blob.Uri.ToString();
                names.Add(new FileAndMime(filename, GetContentType(filename)));
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
            var ctx = tableClient.GetDataServiceContext();
            
            var q = ctx.CreateQuery<User>(User.UserPartitionKey).Where(u => u.Matches(userEmail, userPwd)); 
            if (q.Count() != 1)
                throw new UserNotExistingException();
            var user = q.First();
            
            lock (this)
            {
                _loggedUserId = user.RowKey;
                _userIsAdmin = user.IsAdmin;
                _userIsLoggedIn = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Logout()
        {
            lock (this)
            {
                _loggedUserId = null;
                _userIsAdmin = false;
                _userIsLoggedIn = false;
            }
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
            var uniqueBlobName = filesBlobName + "/" + name;
            var blob = blobClient.GetBlockBlobReference(uniqueBlobName);
            blob.Properties.ContentType = contentType;
            blob.UploadFromStream(content);
            return blob.Uri.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        private void UploadUser(User user)
        {
            var ctx = tableClient.GetDataServiceContext();
            ctx.AddObject(user.PartitionKey, user);
            ctx.SaveChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetContentType(string path) 
        {
            var contentType = "application/octetstream";
            var ext = Path.GetExtension(path).ToLower();
            var registryKey = Registry.ClassesRoot.OpenSubKey(ext);
            if (registryKey != null && registryKey.GetValue("Content Type") != null)
                contentType = registryKey.GetValue("Content Type").ToString();
            return contentType;
        }

        private void RequireAdminUser()
        {
            return; // Da fare...
        }

        private void RequireLoggedInUser()
        {
            return; // Da fare...
        }
    }
}
