using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Disibox.Data.Client.Exceptions;
using Disibox.Data.Common;
using Disibox.Utils;
using Disibox.Utils.Exceptions;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data.Client
{
    public class ClientDataSource
    {
        private CloudBlobContainer _filesContainer;
        private CloudBlobContainer _outputsContainer;

        private DataContext<Entry> _entriesTableCtx;
        private DataContext<User> _usersTableCtx;

        private string _loggedUserId;
        private bool _loggedUserIsAdmin;
        private bool _userIsLoggedIn;

        /// <summary>
        /// 
        /// </summary>
        public ClientDataSource()
        {
            var connectionString = Common.Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            InitContainers(storageAccount);
            InitContexts(storageAccount);
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
                var tempFileName = fileAndMime.Name;
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
        /// <exception cref="InvalidFileNameException"></exception>
        /// <exception cref="LoggedInUserRequiredException">A user must be logged in to use this method.</exception>
        public string AddFile(string fileName, Stream fileContent)
        {
            // Requirements
            Require.ValidFileName(fileName, "fileName");
            Require.NotNull(fileContent, "fileContent");
            RequireLoggedInUser();

            var cloudFileName = GenerateFileName(_loggedUserId, fileName);
            var fileContentType = Shared.GetContentType(fileName);
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
            Require.NotNull(fileUri, "fileUri");
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
            Require.NotNull(fileUri, "fileUri");
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
                        let size = Shared.ConvertBytesToKilobytes(file.Properties.Length)
                        let controlUserFiles = prefix + "" + _loggedUserId
                        let prefixStart = uri.IndexOf(controlUserFiles)
                        let fileName = uri.Substring(prefixStart + prefixLength + _loggedUserId.Length + 1)
                        where uri.IndexOf(controlUserFiles) != -1
                        select new FileMetadata(fileName, Shared.GetContentType(fileName), uri, size)).ToList();

            return (from blob in blobs
                    select (CloudBlob) blob into file
                    let uri = file.Uri.ToString()
                    let size = Shared.ConvertBytesToKilobytes(file.Properties.Length)
                    let prefixStart = uri.IndexOf(prefix)
                    let fileName = uri.Substring(prefixStart + prefixLength + 1)
                    select new FileMetadata(fileName, Shared.GetContentType(fileName), uri, size)).ToList();
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
            Require.NotNull(toolName, "toolName");
            Require.NotNull(outputContentType, "outputContentType");
            Require.NotNull(outputContent, "outputContent");
            RequireLoggedInUser();
            RequireAdminUser();

            var outputName = GenerateOutputName(toolName);
            return UploadBlob(outputName, outputContentType, outputContent, _outputsContainer);
        }

        public bool DeleteOutput(string outputUri)
        {
            // Requirements
            Require.NotNull(outputUri, "outputUri");
            RequireLoggedInUser();
            RequireAdminUser();

            return DeleteBlob(outputUri, _outputsContainer);
        }

        public Stream GetOutput(string outputUri)
        {
            // Requirements
            Require.NotNull(outputUri, "outputUri");
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
            Require.ValidEmail(userEmail, "userEmail");
            Require.NotNull(userPwd, "userPwd");
            RequireLoggedInUser();
            RequireAdminUser();

            var userId = GenerateUserId(userIsAdmin);
            var user = new User(userId, userEmail, userPwd, userIsAdmin);
            
            _usersTableCtx.AddEntity(user);
            _usersTableCtx.SaveChanges();
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
            Require.NotNull(userEmail, "userEmail");
            RequireLoggedInUser();
            RequireAdminUser();
            
            // Added a call to ToList() to avoid an error on Count() call.
            var q = _usersTableCtx.Entities.Where(u => u.Email == userEmail).ToList();
            if (q.Count() == 0)
                throw new UserNotExistingException(userEmail);
            var user = q.First();

            _usersTableCtx.DeleteEntity(user);
            _usersTableCtx.SaveChanges();
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
            var q = _usersTableCtx.Entities.Where(predicate).ToList();
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

        private string GenerateUserId(bool userIsAdmin)
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

        /*=============================================================================
            Init methods
        =============================================================================*/

        private void InitContainers(CloudStorageAccount storageAccount)
        {
            var blobEndpointUri = storageAccount.BlobEndpoint.AbsoluteUri;
            var credentials = storageAccount.Credentials;

            var filesContainerName = Common.Properties.Settings.Default.FilesContainerName;
            var filesContainerUri = blobEndpointUri + "/" + filesContainerName;
            _filesContainer = new CloudBlobContainer(filesContainerUri, credentials);

            var outputsContainerName = Common.Properties.Settings.Default.OutputsContainerName;
            var outputsContainerUri = blobEndpointUri + "/" + outputsContainerName;
            _outputsContainer = new CloudBlobContainer(outputsContainerUri, credentials);
        }

        private void InitContexts(CloudStorageAccount storageAccount)
        {
            var tableEndpointUri = storageAccount.TableEndpoint.AbsoluteUri;
            var credentials = storageAccount.Credentials;

            var entriesTableName = Common.Properties.Settings.Default.EntriesTableName;
            _entriesTableCtx = new DataContext<Entry>(entriesTableName, tableEndpointUri, credentials);

            var usersTableName = Common.Properties.Settings.Default.UsersTableName;
            _usersTableCtx = new DataContext<User>(usersTableName, tableEndpointUri, credentials);
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
    }
}
