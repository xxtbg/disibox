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
        private const string DllMimeType = "application/x-msdownload";

        private string _loggedUserId;
        private bool _loggedUserIsAdmin;
        private bool _userIsLoggedIn;

        /// <summary>
        /// Creates a data source that can be used client-side.
        /// </summary>
        public ClientDataSource()
        {
            var connectionString = Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var blobEndpointUri = storageAccount.BlobEndpoint.AbsoluteUri;
            var tableEndpointUri = storageAccount.TableEndpoint.AbsoluteUri;
            var credentials = storageAccount.Credentials;

            var filesContainerName = Properties.Settings.Default.FilesContainerName;
            Files = AzureContainer.Connect(filesContainerName, blobEndpointUri, credentials);

            var outputsContainerName = Properties.Settings.Default.OutputsContainerName;
            Outputs = AzureContainer.Connect(outputsContainerName, blobEndpointUri, credentials);

            var procDllsContainerName = Properties.Settings.Default.ProcDllsContainerName;
            ProcDlls = AzureContainer.Connect(procDllsContainerName, blobEndpointUri, credentials);

            var entriesTableName = Properties.Settings.Default.EntriesTableName;
            Entries = AzureTable<Entry>.Connect(entriesTableName, tableEndpointUri, credentials);

            var usersTableName = Properties.Settings.Default.UsersTableName;
            Users = AzureTable<User>.Connect(usersTableName, tableEndpointUri, credentials);
        }

        /*=============================================================================
            Protected properties
        =============================================================================*/

        protected AzureContainer Files { get; private set; }
        
        protected AzureContainer Outputs { get; private set; }

        protected AzureContainer ProcDlls { get; private set; }

        protected AzureTable<Entry> Entries { get; private set; }

        protected AzureTable<User> Users { get; private set; }

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
        /// <exception cref="UserNotLoggedInException">A user must be logged in to use this method.</exception>
        public string AddFile(string fileName, Stream fileContent, bool overwrite = false)
        {
            // Requirements
            Require.ValidFileName(fileName, "fileName");
            RequireLoggedInUser();
            if (!overwrite)
                RequireNotExistingFile(fileName);

            var cloudFileName = _loggedUserId + "/" + fileName;
            var fileContentType = Shared.GetContentType(fileName);
            return Files.AddBlob(cloudFileName, fileContentType, fileContent);
        }

        /// <summary>
        /// Deletes file pointed by given uri.
        /// </summary>
        /// <param name="fileUri">The uri pointing at the file that should be deleted.</param>
        /// <returns>True if file has been really deleted, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Given uri is null.</exception>
        /// <exception cref="FileNotFoundException">Given uri points to a not existing file.</exception>
        /// <exception cref="FileNotOwnedException">A common user is trying to delete another user's file.</exception>
        /// <exception cref="InvalidUriException">Given uri has an invalid format.</exception>
        /// <exception cref="UserNotLoggedInException">A user must be logged in to use this method.</exception>
        public bool DeleteFile(string fileUri)
        {
            // Requirements
            RequireLoggedInUser();
            RequireExistingFileUri(fileUri);

            return Files.DeleteBlob(fileUri);
        }

        /// <summary>
        /// Returns the content of the file pointed by given uri.
        /// </summary>
        /// <param name="fileUri">The uri pointing at the file to download.</param>
        /// <returns>The content of file pointed by given uri.</returns>
        /// <exception cref="ArgumentNullException">Given uri is null.</exception>
        /// <exception cref="FileNotFoundException">Given uri points to a not existing file.</exception>
        /// <exception cref="FileNotOwnedException">A common user is trying to delete another user's file.</exception>
        /// <exception cref="InvalidUriException">Given uri has an invalid format.</exception>
        /// <exception cref="UserNotLoggedInException">A user must be logged in to use this method.</exception>
        public Stream GetFile(string fileUri)
        {
            // Requirements
            RequireLoggedInUser();
            RequireExistingFileUri(fileUri);

            return Files.GetBlobData(fileUri);
        }

        /// <summary>
        /// Returns all metadata associated to user's files. If logged in user is administrator,
        /// metadata associated to all users' files are returned.
        /// </summary>
        /// <returns>A list containing what specified above.</returns>
        /// <exception cref="UserNotLoggedInException">A user must be logged in to use this method.</exception>
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

        /// <summary>
        /// Deletes processing output pointed by given uri.
        /// </summary>
        /// <param name="outputUri">The uri pointing at the processing output that should be deleted.</param>
        /// <exception cref="ArgumentNullException">Given uri is null.</exception>
        /// <exception cref="InvalidUriException">Given uri has an invalid format.</exception>
        /// <exception cref="UserNotLoggedInException"></exception>
        public void DeleteOutput(string outputUri)
        {
            // Requirements
            RequireLoggedInUser();

            Outputs.DeleteBlob(outputUri);
        }

        /// <summary>
        /// Returns the content of the processing output pointed by given uri.
        /// </summary>
        /// <param name="outputUri">The uri pointing at the processing output to download.</param>
        /// <returns>The content of processing output pointed by given uri.</returns>
        /// <exception cref="ArgumentNullException">Given uri is null.</exception>
        /// <exception cref="InvalidUriException">Given uri has an invalid format.</exception>
        /// <exception cref="UserNotLoggedInException"></exception>
        public Stream GetOutput(string outputUri)
        {
            // Requirements
            RequireLoggedInUser();

            return Outputs.GetBlobData(outputUri);
        }

        /*=============================================================================
            Processing DLL handling methods
        =============================================================================*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="dllContent"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        /// <exception cref="UserNotAdminException"></exception>
        /// <exception cref="UserNotLoggedInException"></exception>
        public string AddProcessingDll(string dllName, Stream dllContent, bool overwrite = false)
        {
            // Requirements
            Require.ValidFileName(dllName, "dllName");
            Require.MatchingContentType(dllName, DllMimeType);
            RequireLoggedInUser();
            RequireAdminUser();
            if (!overwrite)
                RequireNotExistingDll(dllName);

            return ProcDlls.AddBlob(dllName, DllMimeType, dllContent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dllName"></param>
        /// <exception cref="UserNotAdminException"></exception>
        /// <exception cref="UserNotLoggedInException"></exception>
        public void DeleteProcessingDll(string dllName)
        {
            // Requirements
            RequireLoggedInUser();
            RequireAdminUser();

            ProcDlls.DeleteBlob(ProcDlls.Uri + "/" + dllName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dllName"></param>
        /// <returns></returns>
        public Stream GetProcessingDll(string dllName)
        {
            // Requirements
            RequireLoggedInUser();
            RequireAdminUser();

            return ProcDlls.GetBlobData(ProcDlls.Uri + "/" + dllName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserNotAdminException"></exception>
        /// <exception cref="UserNotLoggedInException"></exception>
        public IList<string> GetProcessingDllNames()
        {
            // Requirements
            RequireLoggedInUser();
            RequireAdminUser();

            var procDllNames = new List<string>();
            foreach (var procDll in ProcDlls.GetBlobs())
            {
                var procDllUri = procDll.Uri.ToString();
                var lastSlashIndex = procDllUri.LastIndexOf('/');
                var procDllName = procDllUri.Substring(lastSlashIndex + 1); // To avoid '/'
                procDllNames.Add(procDllName);
            }
            return procDllNames;
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
        /// <exception cref="UserNotAdminException">Only administrators can use this method.</exception>
        /// <exception cref="ArgumentNullException">At least one of given email or password is null.</exception>
        /// <exception cref="InvalidEmailException">Given email is not syntactically correct.</exception>
        /// <exception cref="InvalidPasswordException">Given password is shorter than MinPasswordLength.</exception>
        /// <exception cref="UserNotLoggedInException">A user must be logged in to use this method.</exception>
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

            Users.AddEntity(user);
            Users.SaveChanges();
        }

        /// <summary>
        /// Deletes user corresponding to given email address.
        /// </summary>
        /// <param name="userEmail">The email address of the user that should be deleted.</param>
        /// <exception cref="UserNotAdminException">This method can only be used by an administrator.</exception>
        /// <exception cref="ArgumentNullException">Given email address is null.</exception>
        /// <exception cref="UserNotLoggedInException">Must be called by a logged in user.</exception>
        /// <exception cref="UserNotExistingException">There is no user with given email.</exception>
        /// <exception cref="CannotDeleteLastAdminException">The last admin is the target of the delete.</exception>
        public void DeleteUser(string userEmail)
        {
            // Requirements
            Require.ValidEmail(userEmail, "userEmail");
            RequireLoggedInUser();
            RequireAdminUser();
            RequireExistingUser(userEmail);

            var user = Users.Entities.Where(u => u.Email == userEmail).First();
            
            if (user.IsAdmin)
            {
                var adminEmails = GetAdminUsersEmails();
                if (adminEmails.Count == 1 && adminEmails.Contains(userEmail))
                    throw new CannotDeleteLastAdminException();
            }

            Users.DeleteEntity(user);
            Users.SaveChanges();
        }

        /// <summary>
        /// Fetches and returns all administrators emails.
        /// </summary>
        /// <returns>All administrators emails.</returns>
        /// <exception cref="UserNotLoggedInException">A user must be logged in to use this method.</exception>
        /// <exception cref="UserNotAdminException">Only administrators can use this method.</exception>
        public IList<string> GetAdminUsersEmails()
        {
            // Requirements
            RequireLoggedInUser();
            RequireAdminUser();

            var users = Users.Entities.ToList();
            return users.Where(u => u.IsAdmin).Select(u => u.Email).ToList();
        }

        /// <summary>
        /// Fetches and returns all common users emails.
        /// </summary>
        /// <returns>All common users emails.</returns>
        /// <exception cref="UserNotLoggedInException">A user must be logged in to use this method.</exception>
        /// <exception cref="UserNotAdminException">Only administrators can use this method.</exception>
        public IList<string> GetCommonUsersEmails()
        {
            // Requirements
            RequireLoggedInUser();
            RequireAdminUser();

            var users = Users.Entities.ToList();
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
            var q = Users.Entities.Where(predicate).ToList();
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
        /// <exception cref="UserNotAdminException">If logged in user is not administrator.</exception>
        protected virtual void RequireAdminUser()
        {
            if (_loggedUserIsAdmin) return;
            throw new UserNotAdminException();
        }

        /// <summary>
        /// Checks if there is a logged in user.
        /// </summary>
        /// <exception cref="UserNotLoggedInException">No user is currently logged in.</exception>
        protected virtual void RequireLoggedInUser()
        {
            if (_userIsLoggedIn) return;
            throw new UserNotLoggedInException();
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
            RequireNotExistingElement(fileName, GetFileMetadata().Select(f => f.Name));
        }

        private void RequireNotExistingDll(string dllName)
        {
            RequireNotExistingElement(dllName, GetProcessingDllNames());
        }

        /// <summary>
        /// Checks if a user with given email address exists.
        /// </summary>
        /// <param name="userEmail">User email address.</param>
        /// <exception cref="UserNotExistingException">There is no user with given email address.</exception>
        private void RequireExistingUser(string userEmail)
        {
            var matches = Users.Entities.Where(u => u.Email == userEmail).ToList();
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
            var matches = Users.Entities.Where(u => u.Email == userEmail).ToList();
            if (matches.Count == 0) return;
            throw new UserExistingException(userEmail);
        }

        private static void RequireNotExistingElement(string elementId, IEnumerable<string> elementIds)
        {
            var matches = elementIds.Where(e => e == elementId);
            if (matches.Count() == 0) return;
            throw new FileExistingException(elementId);
        }

        /*=============================================================================
            Private methods
        =============================================================================*/

        private string GenerateUserId(bool userIsAdmin)
        {
            var nextUserIdEntry = Entries.Entities.Where(e => e.RowKey == "NextUserId").First();
            var nextUserId = int.Parse(nextUserIdEntry.Value);

            var firstIdChar = (userIsAdmin) ? 'a' : 'u';
            var userId = string.Format("{0}{1}", firstIdChar, nextUserId.ToString("D16"));

            nextUserId += 1;
            nextUserIdEntry.Value = nextUserId.ToString();

            // Next method must be called in order to save the update.
            Entries.UpdateEntity(nextUserIdEntry);
            Entries.SaveChanges();

            return userId;
        }

        private IList<FileMetadata> GetFileMetadataForAdminUser()
        {
            var files = Files.GetBlobs();
            var filesPrefix = Files.Uri + "/";
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
            var files = Files.GetBlobs();
            var filesPrefix = Files.Uri + "/" + _loggedUserId + "/";
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
            var prefix = Files.Uri + "/" + _loggedUserId + "/";

            var index = fileUri.IndexOf(prefix);
            if (index == -1 && !_loggedUserIsAdmin)
                throw new FileNotOwnedException();
            if (index == -1)
                throw new InvalidUriException(fileUri);
            return fileUri.Substring(prefix.Length);
        }

        private string GetUserEmailByUserId(string userId)
        {
            return Users.Entities.Where(u => u.RowKey == userId).First().Email;
        }
    }
}