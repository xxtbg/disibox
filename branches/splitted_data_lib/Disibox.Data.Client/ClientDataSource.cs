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

namespace Disibox.Data.Client
{
    public class ClientDataSource
    {
        private readonly BlobContainer _filesContainer;
        private readonly BlobContainer _outputsContainer;

        private readonly DataContext<Entry> _entriesTableCtx;
        private readonly DataContext<User> _usersTableCtx;

        private string _loggedUserId;
        private bool _loggedUserIsAdmin;
        private bool _userIsLoggedIn;

        /// <summary>
        /// Creates a client.
        /// </summary>
        public ClientDataSource()
        {
            var connectionString = Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var blobEndpointUri = storageAccount.BlobEndpoint.AbsoluteUri;
            var tableEndpointUri = storageAccount.TableEndpoint.AbsoluteUri;
            var credentials = storageAccount.Credentials;

            var filesContainerName = Properties.Settings.Default.FilesContainerName;
            _filesContainer = new BlobContainer(filesContainerName, blobEndpointUri, credentials);

            var outputsContainerName = Properties.Settings.Default.OutputsContainerName;
            _outputsContainer = new BlobContainer(outputsContainerName, blobEndpointUri, credentials);

            var entriesTableName = Properties.Settings.Default.EntriesTableName;
            _entriesTableCtx = new DataContext<Entry>(entriesTableName, tableEndpointUri, credentials);

            var usersTableName = Properties.Settings.Default.UsersTableName;
            _usersTableCtx = new DataContext<User>(usersTableName, tableEndpointUri, credentials);
        }

        /*=============================================================================
            File handling methods
        =============================================================================*/

        /// <summary>
        /// Adds given file (in the form of a file name and stream carrying its content)
        /// to the user's personal folder. If <paramref name="overwrite"/> is set to true,
        /// if a file with given file name already exists in the folder it will be overwritten.
        /// </summary>
        /// <param name="fileName">The name of the file to add.</param>
        /// <param name="fileContent">The content of the file to add.</param>
        /// <param name="overwrite">If it is true then the file on the cloud will be overwritten with <paramref name="fileName"/></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Both parameters should not be null.</exception>
        /// <exception cref="FileExistingException">If the current user already have a file with the name <paramref name="fileName"/></exception>
        /// <exception cref="InvalidFileNameException"></exception>
        /// <exception cref="LoggedInUserRequiredException">A user must be logged in to use this method.</exception>
        public string AddFile(string fileName, Stream fileContent, bool overwrite = false)
        {
            // Requirements
            Require.ValidFileName(fileName, "fileName");
            Require.NotNull(fileContent, "fileContent");
            RequireLoggedInUser();
            if (!overwrite)
                RequireNotExistingFile(fileName);

            var cloudFileName = _loggedUserId + "/" + fileName;
            var fileContentType = Shared.GetContentType(fileName);
            return _filesContainer.AddBlob(cloudFileName, fileContentType, fileContent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileUri"></param>
        /// <returns>True if file has been really deleted, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FileNotOwnedException">If a common user is trying to delete another user's file.</exception>
        /// <exception cref="InvalidFileUriException"></exception>
        public bool DeleteFile(string fileUri)
        {
            // Requirements
            Require.ValidFileUri(fileUri, "fileUri");
            RequireLoggedInUser();
            RequireExistingFileUri(fileUri);

            return _filesContainer.DeleteBlob(fileUri);
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
            RequireExistingFileUri(fileUri);

            return _filesContainer.GetBlob(fileUri);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IList<FileMetadata> GetFileMetadata()
        {
            // Requirements
            RequireLoggedInUser();

            if (_loggedUserIsAdmin)
                return GetFileMetadataForAdminUser();
            return GetFileMetadataForCommonUser();
        }

        /*=============================================================================
            Output handling methods
        =============================================================================*/

        public bool DeleteOutput(string outputUri)
        {
            // Requirements
            Require.NotNull(outputUri, "outputUri");
            RequireLoggedInUser();
            RequireAdminUser();

            return _outputsContainer.DeleteBlob(outputUri);
        }

        public Stream GetOutput(string outputUri)
        {
            // Requirements
            Require.NotNull(outputUri, "outputUri");
            RequireLoggedInUser();
            RequireAdminUser();

            return _outputsContainer.GetBlob(outputUri);
        }

        /*=============================================================================
            User handling methods
        =============================================================================*/

        /// <summary>
        /// Adds a new user with given credentials.
        /// </summary>
        /// <param name="userEmail">The email address of the new user.</param>
        /// <param name="userPwd">The password for the new user.</param>
        /// <param name="userIsAdmin">Whether new user will be administrator or not.</param>
        /// <exception cref="AdminUserRequiredException">Only administrators can use this method.</exception>
        /// <exception cref="ArgumentNullException">At least one of given email or password is null.</exception>
        /// <exception cref="InvalidEmailException">Given email is not syntactically correct.</exception>
        /// <exception cref="InvalidPasswordException">Given password is shorter than MinPasswordLength.</exception>
        /// <exception cref="LoggedInUserRequiredException">A user must be logged in to use this method.</exception>
        /// <exception cref="UserExistingException">A user with given email address already exists.</exception>
        public void AddUser(string userEmail, string userPwd, bool userIsAdmin)
        {
            // Requirements
            Require.ValidEmail(userEmail, "userEmail");
            Require.ValidPassword(userPwd, "userPwd");
            RequireLoggedInUser();
            RequireAdminUser();
            RequireNotExistingUser(userEmail);

            var userId = GenerateUserId(userIsAdmin);
            var user = new User(userId, userEmail, userPwd, userIsAdmin);

            _usersTableCtx.AddEntity(user);
            _usersTableCtx.SaveChanges();
        }

        /// <summary>
        /// Deletes user corresponding to given email address.
        /// </summary>
        /// <param name="userEmail">The email address of the user that should be deleted.</param>
        /// <exception cref="AdminUserRequiredException">This method can only be used by an administrator.</exception>
        /// <exception cref="ArgumentNullException">Given email address is null.</exception>
        /// <exception cref="LoggedInUserRequiredException">Must be called by a logged in user.</exception>
        /// <exception cref="UserNotExistingException">There is no user with given email.</exception>
        public void DeleteUser(string userEmail)
        {
            // Requirements
            Require.ValidEmail(userEmail, "userEmail");
            RequireLoggedInUser();
            RequireAdminUser();
            RequireExistingUser(userEmail);

            var user = _usersTableCtx.Entities.Where(u => u.Email == userEmail).First();

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
        /// Logs a user in, giving him the ability to use client operations.
        /// </summary>
        /// <param name="userEmail">User email.</param>
        /// <param name="userPwd">User password.</param>
        /// <exception cref="ArgumentNullException">At least one argument is null.</exception>
        /// <exception cref="InvalidEmailException">Given email is not syntactically correct.</exception>
        /// <exception cref="InvalidPasswordException">Given password is shorter than MinPasswordLength.</exception>
        /// <exception cref="UserNotExistingException">A user with credentials does not exist.</exception>
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
        /// Logs current user out. Nothing happens if no user is currently logged in.
        /// </summary>
        public void Logout()
        {
            lock (this)
            {
                _userIsLoggedIn = false;
            }
        }

        /*=============================================================================
            Requirements checking methods
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
        /// Checks if there is a logged in user.
        /// </summary>
        /// <exception cref="LoggedInUserRequiredException">No user is currently logged in.</exception>
        private void RequireLoggedInUser()
        {
            if (_userIsLoggedIn) return;
            throw new LoggedInUserRequiredException();
        }

        private void RequireExistingFile(string fileName)
        {
            var matches = GetFileMetadata().Where(m => m.Name == fileName);
            if (matches.Count() == 1) return;
            throw new FileNotFoundException(fileName);
        }

        private void RequireExistingFileUri(string fileUri)
        {
            var fileName = GetFileNameFromUri(fileUri);
            RequireExistingFile(fileName);
        }

        private void RequireNotExistingFile(string fileName)
        {
            var matches = GetFileMetadata().Where(m => m.Name == fileName);
            if (matches.Count() == 0) return;
            throw new FileExistingException(fileName);
        }

        /// <summary>
        /// Checks if a user with given email address exists.
        /// </summary>
        /// <param name="userEmail">User email address.</param>
        /// <exception cref="UserNotExistingException">There is no user with given email address.</exception>
        private void RequireExistingUser(string userEmail)
        {
            var matches = _usersTableCtx.Entities.Where(u => u.Email == userEmail).ToList();
            if (matches.Count == 1) return;
            throw new UserNotExistingException(userEmail);
        }

        /// <summary>
        /// Checks if no user with given email address exists.
        /// </summary>
        /// <param name="userEmail">User email address.</param>
        /// <exception cref="UserExistingException">There is a user with given email address.</exception>
        private void RequireNotExistingUser(string userEmail)
        {
            var matches = _usersTableCtx.Entities.Where(u => u.Email == userEmail).ToList();
            if (matches.Count == 0) return;
            throw new UserExistingException(userEmail);
        }

        /*=============================================================================
            Private methods
        =============================================================================*/

        private string GenerateUserId(bool userIsAdmin)
        {
            var nextUserIdEntry = _entriesTableCtx.Entities.Where(e => e.RowKey == "NextUserId").First();
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

        private IList<FileMetadata> GetFileMetadataForAdminUser()
        {
            var files = _filesContainer.GetBlobs();
            var filesPrefix = _filesContainer.Uri + "/";
            var fileMetadata = new List<FileMetadata>();

            foreach (var file in files)
            {
                var fileUri = file.Uri.ToString();
                var filePath = fileUri.Substring(filesPrefix.Length);
                var fileOwnerId = filePath.Substring(0, filePath.IndexOf('/'));
                var fileOwner = GetUserEmailByUserId(fileOwnerId);
                var fileName = filePath.Substring(fileOwnerId.Length + 1); // To avoid '/'
                var fileContentType = Shared.GetContentType(fileName);
                var fileSize = Shared.ConvertBytesToKilobytes(file.Properties.Length);
                var metadata = new FileMetadata(fileName, fileContentType, fileUri, fileOwner, fileSize);
                fileMetadata.Add(metadata);
            }

            return fileMetadata;
        }

        private IList<FileMetadata> GetFileMetadataForCommonUser()
        {
            var files = _filesContainer.GetBlobs();
            var filesPrefix = _filesContainer.Uri + "/" + _loggedUserId + "/";
            var fileOwner = GetUserEmailByUserId(_loggedUserId);
            var fileMetadata = new List<FileMetadata>();

            foreach (var file in files)
            {
                var fileUri = file.Uri.ToString();
                if (!fileUri.Contains(filesPrefix)) continue;
                var fileName = fileUri.Substring(filesPrefix.Length);
                var fileContentType = Shared.GetContentType(fileName);
                var fileSize = Shared.ConvertBytesToKilobytes(file.Properties.Length);
                var metadata = new FileMetadata(fileName, fileContentType, fileUri, fileOwner, fileSize);
                fileMetadata.Add(metadata);
            }

            return fileMetadata;
        }

        private string GetFileNameFromUri(string fileUri)
        {
            var prefix = _filesContainer.Uri + "/" + _loggedUserId + "/";

            var index = fileUri.IndexOf(prefix);
            if (index == -1 && !_loggedUserIsAdmin)
                throw new FileNotOwnedException();
            if (index == -1)
                throw new InvalidFileUriException(fileUri);
            return fileUri.Substring(prefix.Length);
        }

        private string GetUserEmailByUserId(string userId)
        {
            return _usersTableCtx.Entities.Where(u => u.RowKey == userId).First().Email;
        }
    }
}