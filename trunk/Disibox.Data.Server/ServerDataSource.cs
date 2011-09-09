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
using Disibox.Data.Entities;
using Disibox.Data.Exceptions;
using Disibox.Utils;
using Microsoft.WindowsAzure;

namespace Disibox.Data.Server
{
    public class ServerDataSource
    {
        private readonly AzureContainer _filesContainer;
        private readonly AzureContainer _outputsContainer;

        private readonly AzureQueue<ProcessingMessage> _processingRequests;
        private readonly AzureQueue<ProcessingMessage> _processingCompletions;

        private readonly AzureTable<User> _usersTable;

        /// <summary>
        /// Creates a data source that should be used server-side only.
        /// </summary>
        public ServerDataSource()
        {
            var connectionString = Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var blobEndpointUri = storageAccount.BlobEndpoint.AbsoluteUri;
            var queueEndpointUri = storageAccount.QueueEndpoint.AbsoluteUri;
            var tableEndpointUri = storageAccount.TableEndpoint.AbsoluteUri;
            var credentials = storageAccount.Credentials;

            var filesContainerName = Properties.Settings.Default.FilesContainerName;
            _filesContainer = AzureContainer.Connect(filesContainerName, blobEndpointUri, credentials);

            var outputsContainerName = Properties.Settings.Default.OutputsContainerName;
            _outputsContainer = AzureContainer.Connect(outputsContainerName, blobEndpointUri, credentials);

            var procReqName = Properties.Settings.Default.ProcReqQueueName;
            _processingRequests = AzureQueue<ProcessingMessage>.Connect(procReqName, queueEndpointUri, credentials);

            var procComplName = Properties.Settings.Default.ProcComplQueueName;
            _processingCompletions = AzureQueue<ProcessingMessage>.Connect(procComplName, queueEndpointUri, credentials);

            var usersTableName = Properties.Settings.Default.UsersTableName;
            _usersTable = AzureTable<User>.Connect(usersTableName, tableEndpointUri, credentials);
        }

        /*=============================================================================
            User handling methods
        =============================================================================*/

        /// <summary>
        /// Checks if a user with given credentials exists.
        /// </summary>
        /// <param name="userEmail">User email address.</param>
        /// <param name="userPwd">User password.</param>
        /// <returns>True if a user with given credentials exists, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">At least one argument is null.</exception>
        /// <exception cref="InvalidEmailException">Given email is not syntactically correct.</exception>
        /// <exception cref="InvalidPasswordException">Given password is shorter than MinPasswordLength.</exception>
        public bool UserExists(string userEmail, string userPwd)
        {
            // Requirements
            Require.ValidEmail(userEmail, "userEmail");
            Require.ValidPassword(userPwd, "userPwd");

            var hashedPwd = Hash.ComputeMD5(userPwd);
            var predicate = new Func<User, bool>(u => u.Email == userEmail && u.HashedPassword == hashedPwd);
            var q = _usersTable.Entities.Where(predicate).ToList();
            return (q.Count() == 1);
        }

        /*=============================================================================
            Requests handling methods
        =============================================================================*/

        /// <summary>
        /// Dequeues a request from the queue in blocking mode.
        /// </summary>
        /// <returns>The request at the top of the queue.</returns>
        public ProcessingMessage DequeueProcessingRequest()
        {
            return _processingRequests.DequeueMessage();
        }

        /// <summary>
        /// Enqueues given request.
        /// </summary>
        /// <param name="procReq">The request to enqueue.</param>
        /// <exception cref="ArgumentNullException">Request is null.</exception>
        public void EnqueueProcessingRequest(ProcessingMessage procReq)
        {
            // Requirements
            Require.NotNull(procReq, "procReq");

            _processingRequests.EnqueueMessage(procReq);
        }

        /// <summary>
        /// Peeks a fixed number of requests from the top of the queue.
        /// </summary>
        /// <returns>A fixed number of requests from the top of the queue.</returns>
        public IList<ProcessingMessage> PeekProcessingRequests()
        {
            return _processingRequests.PeekMessages();
        }

        public void ClearProcessingRequests()
        {
            _processingRequests.Clear();
        }

        /*=============================================================================
            Completions handling methods
        =============================================================================*/

        /// <summary>
        /// Dequeues a completion from the queue in blocking mode.
        /// </summary>
        /// <returns>The completion at the top of the queue.</returns>
        public ProcessingMessage DequeueProcessingCompletion()
        {
            return _processingCompletions.DequeueMessage();
        }

        /// <summary>
        /// Enqueues given completion.
        /// </summary>
        /// <param name="procCompl">The completion to enqueue.</param>
        /// <exception cref="ArgumentNullException">Completion is null.</exception>
        public void EnqueueProcessingCompletion(ProcessingMessage procCompl)
        {
            // Requirements
            Require.NotNull(procCompl, "procCompl");

            _processingCompletions.EnqueueMessage(procCompl);
        }

        /// <summary>
        /// Peeks a fixed number of completions from the top of the queue.
        /// </summary>
        /// <returns>A fixed number of completions from the top of the queue.</returns>
        public IList<ProcessingMessage> PeekProcessingCompletions()
        {
            return _processingCompletions.PeekMessages();
        }

        public void ClearProcessingCompletions()
        {
            _processingCompletions.Clear();
        }

        /*=============================================================================
            Blob handling methods
        =============================================================================*/

        /// <summary>
        /// Returns the content of the file pointed by given uri.
        /// </summary>
        /// <param name="fileUri">The uri pointing at the file to download.</param>
        /// <returns>The content of file pointed by given uri.</returns>
        /// <exception cref="ArgumentNullException">Given uri is null.</exception>
        /// <exception cref="InvalidUriException">Given uri has an invalid format.</exception>
        public Stream GetFile(string fileUri)
        {
            return _filesContainer.GetBlob(fileUri);
        }

        /// <summary>
        /// Adds given processing output.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="toolName">The tool that produced the output.</param>
        /// <param name="outputContentType">The content type of the output.</param>
        /// <param name="outputContent">The content of the output.</param>
        /// <returns>The output uri.</returns>
        /// <exception cref="ArgumentNullException">One of the arguments is null.</exception>
        public string AddOutput(string toolName, string outputContentType, Stream outputContent)
        {
            // Requirements
            Require.NotNull(toolName, "toolName");

            var outputName = toolName + Guid.NewGuid();
            return _outputsContainer.AddBlob(outputName, outputContentType, outputContent);
        }
    }
}