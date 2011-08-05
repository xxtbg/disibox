using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Disibox.Data.Exceptions;
using Microsoft.Win32;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public class DataSource
    {
        private const string ConnectionStringName = "DataConnectionString";

        private const string DataTableName = "data";
        private const string UsersTableName = "users";
        
        private const string FilesBlobName = "files";
        
        private readonly CloudStorageAccount _storageAccount;
        
        private readonly CloudTableClient _tableClient;

        private readonly CloudBlobClient _blobClient;
        private readonly CloudBlobContainer _blobContainer;

        private string _loggedUserId;
        private UserType _loggedUserType;
        private bool _userIsLoggedIn = false;

        //The default constructor initializes the storage account by reading its settings from
        //the configuration and then uses CreateTableIfNotExist method in the CloudTableClient
        //class to create the table used by the application.
        public DataSource()
        {
            string connectionString = "UseDevelopmentStorage=true";//RoleEnvironment.GetConfigurationSettingValue(connectionStringName);

            _storageAccount = CloudStorageAccount.Parse(connectionString);
            
            // Creates users table
            _tableClient = new CloudTableClient(_storageAccount.TableEndpoint.AbsoluteUri, _storageAccount.Credentials);
            _tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
            _tableClient.CreateTableIfNotExist(DataTableName);
            _tableClient.CreateTableIfNotExist(UsersTableName);

            // Creates files blob container
            _blobClient = _storageAccount.CreateCloudBlobClient();
            _blobContainer = _blobClient.GetContainerReference(FilesBlobName);
            _blobContainer.CreateIfNotExist();

            // Set blob container permissions
            var permissions = _blobContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            _blobContainer.SetPermissions(permissions);
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
        /// <param name="userType"></param>
        public void AddUser(string userEmail, string userPwd, UserType userType)
        {
            // Requirements
            RequireLoggedInUser();
            RequireUserType(UserType.AdminUser);

            var user = new User(userEmail, userPwd, userType);
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

            var blobs = _blobContainer.ListBlobs();
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
            var ctx = _tableClient.GetDataServiceContext();
            
            var q = ctx.CreateQuery<User>(User.UserPartitionKey).Where(u => u.Matches(userEmail, userPwd)); 
            if (q.Count() != 1)
                throw new UserNotExistingException();
            var user = q.First();
            
            lock (this)
            {
                _userIsLoggedIn = true;
                _loggedUserId = user.RowKey;
                _loggedUserType = user.Type;
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

        /// <summary>
        /// Uploads the file to blob storage.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contentType"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private string UploadFile(string name, string contentType, Stream content)
        {       
            var uniqueBlobName = FilesBlobName + "/" + name;
            var blob = _blobClient.GetBlockBlobReference(uniqueBlobName);
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
            var ctx = _tableClient.GetDataServiceContext();
            ctx.AddObject(user.PartitionKey, user);
            ctx.SaveChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string GetContentType(string path) 
        {
            var contentType = "application/octetstream";
            var ext = Path.GetExtension(path).ToLower();
            var registryKey = Registry.ClassesRoot.OpenSubKey(ext);
            if (registryKey != null && registryKey.GetValue("Content Type") != null)
                contentType = registryKey.GetValue("Content Type").ToString();
            return contentType;
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
