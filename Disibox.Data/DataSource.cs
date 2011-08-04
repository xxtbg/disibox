using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.IO;
using Microsoft.Win32;
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
            var fileName = Path.GetFileName(path);
            var fileContentType = GetContentType(path);
            var fileContent = new FileStream(path, FileMode.Open);
            return UploadFile(fileName, fileContentType, fileContent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pwd"></param>
        public void AddUser(string email, string pwd)
        {

        }

        public IEnumerable<string> GetFileNames()
        {
            var blobs = blobContainer.ListBlobs();
            var names = new List<string>();
            foreach (var blob in blobs)
                names.Add(blob.Uri.ToString());
            return names;
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
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetContentType(string path) {
            var contentType = "application/octetstream";
            var ext = Path.GetExtension(path).ToLower();
            var registryKey = Registry.ClassesRoot.OpenSubKey(ext);
            if (registryKey != null && registryKey.GetValue("Content Type") != null)
                contentType = registryKey.GetValue("Content Type").ToString();
            return contentType;
        }
    }
}
