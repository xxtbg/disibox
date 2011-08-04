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
        private const string messageTableName = "MessageTable";
        private const string filesBlobName = "files";
        private const string connectionStringName = "DataConnectionString";
        
        private static CloudStorageAccount storageAccount;
        
        private CloudTableClient tableClient;

        private readonly CloudBlobClient blobClient;
        private readonly CloudBlobContainer blobContainer;

        static DataSource()
        {
            
        }

        //The default constructor initializes the storage account by reading its settings from
        //the configuration and then uses CreateTableIfNotExist method in the CloudTableClient
        //class to create the table used by the application.
        public DataSource()
        {
            string connectionString = "UseDevelopmentStorage=true";//RoleEnvironment.GetConfigurationSettingValue(connectionStringName);

            storageAccount = CloudStorageAccount.Parse(connectionString );
            
            /*tableClient = new CloudTableClient(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            tableClient.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
            tableClient.CreateTableIfNotExist(messageTableName);*/

            // create blob container
            blobClient = storageAccount.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference(filesBlobName);
            blobContainer.CreateIfNotExist();

            //set blob container permissions
            var permissions = blobContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            blobContainer.SetPermissions(permissions);
            

            
		
        }

        public string AddFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var fileContentType = GetContentType(filePath);
            var fileContent = new FileStream(filePath, FileMode.Open);
            return UploadFile(fileName, fileContentType, fileContent);
        }

        /// <summary>
        /// Uploads the file to blob storage.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileContentType"></param>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        private string UploadFile(string fileName, string fileContentType, Stream fileContent)
        {       
            string uniqueBlobName = filesBlobName + "/" + fileName;
            CloudBlockBlob blob = blobClient.GetBlockBlobReference(uniqueBlobName);
            blob.Properties.ContentType = fileContentType;
            blob.UploadFromStream(fileContent);
            return blob.Uri.ToString();
        }

        private string GetContentType(string filePath) {
            var contentType = "application/octetstream";
            var ext = Path.GetExtension(filePath).ToLower();
            var registryKey = Registry.ClassesRoot.OpenSubKey(ext);
            if (registryKey != null && registryKey.GetValue("Content Type") != null)
                contentType = registryKey.GetValue("Content Type").ToString();
            return contentType;
        }
    }
}
