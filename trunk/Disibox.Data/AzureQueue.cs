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
using System.Threading;
using Disibox.Data.Exceptions;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public class AzureQueue<TMsg> : IStorage where TMsg : IMessage, new()
    {
        private const int PeekCount = 32;

        private readonly CloudQueue _queue;

        private AzureQueue(CloudQueue queue)
        {
            _queue = queue;
        }

        public string Name
        {
            get { return _queue.Name; }
        }

        public Uri Uri
        {
            get { return _queue.Uri; }
        }

        public static AzureQueue<TMsg> Connect(string queueName, string queueEndpointUri, StorageCredentials credentials)
        {
            // Requirements
            Require.NotEmpty(queueName, "queueName");
            Require.NotEmpty(queueEndpointUri, "queueEndpointUri");
            Require.NotNull(credentials, "credentials");

            var queue = CreateQueue(queueName, queueEndpointUri, credentials);
            return new AzureQueue<TMsg>(queue);
        }

        public static void Create(string queueName, string queueEndpointUri, StorageCredentials credentials)
        {
            // Requirements
            Require.NotEmpty(queueName, "queueName");
            Require.NotEmpty(queueEndpointUri, "queueEndpointUri");
            Require.NotNull(credentials, "credentials");

            var queue = CreateQueue(queueName, queueEndpointUri, credentials);
            queue.CreateIfNotExist();
        }

        public TMsg DequeueMessage(ReadMode readMode = ReadMode.Blocking)
        {
            // Requirements
            RequireExistingQueue();

            // TODO Very ugly, there should be another way...
            CloudQueueMessage queueMsg;
            if (readMode == ReadMode.Blocking)
                while ((queueMsg = _queue.GetMessage()) == null)
                    Thread.Sleep(1000);
            else // if (readMode == ReadMode.NotBlocking)
                queueMsg = _queue.GetMessage();

            var msg = new TMsg();
            if (queueMsg != null)
                msg.FromString(queueMsg.AsString);
            _queue.DeleteMessage(queueMsg);

            return msg;
        }

        public void EnqueueMessage(TMsg msg)
        {
            // Requirements
            Require.NotNull(msg, "msg");
            RequireExistingQueue();

            var queueMsg = new CloudQueueMessage(msg.ToString());
            _queue.AddMessage(queueMsg);
        }

        public IList<TMsg> PeekMessages()
        {
            // Requirements
            RequireExistingQueue();

            var queueMessages = _queue.PeekMessages(PeekCount);
            var messages = new List<TMsg>();

            foreach (var queueMessage in queueMessages)
            {
                var message = new TMsg();
                message.FromString(queueMessage.AsString);
                messages.Add(message);
            }

            return messages;
        }

        public void Clear()
        {
            // Requirements
            RequireExistingQueue();

            _queue.Clear();
        }

        public void Delete()
        {
            // Requirements
            RequireExistingQueue();

            _queue.Delete();
        }

        public bool Exists()
        {
            return _queue.Exists();
        }

        private static CloudQueue CreateQueue(string queueName, string queueEndpointUri, StorageCredentials credentials)
        {
            return new CloudQueue(queueEndpointUri + "/" + queueName, credentials);
        }

        private void RequireExistingQueue()
        {
            if (Exists()) return;
            throw new QueueNotExistingException(_queue.Name);
        }
    }

    public enum ReadMode
    {
        Blocking, NotBlocking
    }
}