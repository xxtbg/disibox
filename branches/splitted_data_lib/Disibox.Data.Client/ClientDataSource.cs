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
using System.IO;
using System.Linq;
using Disibox.Data.Client.Exceptions;
using Disibox.Data.Entities;
using Disibox.Data.Exceptions;
using Disibox.Utils;
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
            var connectionString = Properties.Settings.Default.DataConnectionString;
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
            return BlobUtils.AddBlob(cloudFileName, fileContentType, fileContent, _filesContainer);
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
                return BlobUtils.DeleteBlob(fileUri, _filesContainer);

            var prefix = _filesContainer.Name + "/" + _loggedUserId;

            if (fileUri.IndexOf(prefix) == -1 )
                throw new DeletingNotOwnedFileException();

            return BlobUtils.DeleteBlob(fileUri, _filesContainer);
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

            return BlobUtils.GetBlob(fileUri, _filesContainer);
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

            var blobs = BlobUtils.GetBlobs(_filesContainer);
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

            var blobs = BlobUtils.GetBlobs(_filesContainer);
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
            return BlobUtils.AddBlob(outputName, outputContentType, outputContent, _outputsContainer);
        }

        public bool DeleteOutput(string outputUri)
        {
            // Requirements
            Require.NotNull(outputUri, "outputUri");
            RequireLoggedInUser();
            RequireAdminUser();

            return BlobUtils.DeleteBlob(outputUri, _outputsContainer);
        }

        public Stream GetOutput(string outputUri)
        {
            // Requirements
            Require.NotNull(outputUri, "outputUri");
            RequireLoggedInUser();
            RequireAdminUser();

            return BlobUtils.GetBlob(outputUri, _outputsContainer);
        }

        private static string GenerateOutputName(string toolName)
        {
            return toolName + Guid.NewGuid();
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
        /// <exception cref="InvalidEmailException"></exception>
        /// <exception cref="InvalidPasswordException"></exception>
        /// <exception cref="LoggedInUserRequiredException">A user must be logged in to use this method.</exception>
        public void AddUser(string userEmail, string userPwd, bool userIsAdmin)
        {
            // Requirements
            Require.ValidEmail(userEmail, "userEmail");
            Require.ValidPassword(userPwd, "userPwd");
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
            Require.ValidEmail(userEmail, "userEmail");
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
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidEmailException"></exception>
        /// <exception cref="InvalidPasswordException"></exception>
        /// <exception cref="UserNotExistingException"></exception>
        public void Login(string userEmail, string userPwd)
        {
            // Requirements
            Require.ValidEmail(userEmail, "userEmail");
            Require.ValidPassword(userPwd, "userPwd");

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

            var filesContainerName = Properties.Settings.Default.FilesContainerName;
            var filesContainerUri = blobEndpointUri + "/" + filesContainerName;
            _filesContainer = new CloudBlobContainer(filesContainerUri, credentials);

            var outputsContainerName = Properties.Settings.Default.OutputsContainerName;
            var outputsContainerUri = blobEndpointUri + "/" + outputsContainerName;
            _outputsContainer = new CloudBlobContainer(outputsContainerUri, credentials);
        }

        private void InitContexts(CloudStorageAccount storageAccount)
        {
            var tableEndpointUri = storageAccount.TableEndpoint.AbsoluteUri;
            var credentials = storageAccount.Credentials;

            var entriesTableName = Properties.Settings.Default.EntriesTableName;
            _entriesTableCtx = new DataContext<Entry>(entriesTableName, tableEndpointUri, credentials);

            var usersTableName = Properties.Settings.Default.UsersTableName;
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
