using System;
using System.Linq;
using Disibox.Data.Common;
using Disibox.Utils;
using Microsoft.WindowsAzure;

namespace Disibox.Data.Server
{
    public class ServerDataSource
    {
        private readonly MsgQueue<ProcessingMessage> _processingRequests;
        private readonly MsgQueue<ProcessingMessage> _processingCompletions;
        
        private readonly DataContext<User> _usersTableCtx;

        public ServerDataSource()
        {
            var connectionString = Common.Properties.Settings.Default.DataConnectionString;
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var queueEndpointUri = storageAccount.QueueEndpoint.AbsoluteUri;
            var tableEndpointUri = storageAccount.TableEndpoint.AbsoluteUri;
            var credentials = storageAccount.Credentials;

            _processingRequests = new MsgQueue<ProcessingMessage>(queueEndpointUri, credentials);
            _processingCompletions = new MsgQueue<ProcessingMessage>(queueEndpointUri, credentials);

            var usersTableName = Common.Properties.Settings.Default.UsersTableName;
            _usersTableCtx = new DataContext<User>(usersTableName, tableEndpointUri, credentials);
        }

        public bool UserExists(string userEmail, string userPwd)
        {
            var hashedPwd = Hash.ComputeMD5(userPwd);
            var predicate = new Func<User, bool>(u => u.Email == userEmail && u.HashedPassword == hashedPwd);
            var q = _usersTableCtx.Entities.Where(predicate).ToList();
            return (q.Count() == 1);
        }

        public ProcessingMessage DequeueProcessingRequest()
        {
            return _processingRequests.DequeueMessage();
        }

        public void EnqueueProcessingRequest(ProcessingMessage procReq)
        {
            // Requirements
            Require.NotNull(procReq, "procReq");

            _processingRequests.EnqueueMessage(procReq);
        }

        public ProcessingMessage DequeueProcessingCompletion()
        {
            return _processingCompletions.DequeueMessage();
        }

        public void EnqueueProcessingCompletion(ProcessingMessage procCompl)
        {
            // Requirements
            Require.NotNull(procCompl, "procCompl");

            _processingCompletions.EnqueueMessage(procCompl);
        }
    }
}
