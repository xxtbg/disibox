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

using System.Collections.Generic;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public class MsgQueue<TMsg> where TMsg : BaseMessage, new()
    {
        private const int PeekCount = 32;

        private readonly CloudQueue _queue;

        public MsgQueue(string queueName, string queueEndpointUri, StorageCredentials credentials)
        {
            _queue = new CloudQueue(queueEndpointUri + "/" + queueName, credentials);
        }

        public TMsg DequeueMessage()
        {
            CloudQueueMessage queueMsg;
            while ((queueMsg = _queue.GetMessage()) == null)
                Thread.Sleep(1000);

            var msg = new TMsg();
            msg.FromString(queueMsg.AsString);
            _queue.DeleteMessage(queueMsg);

            return msg;
        }

        public void EnqueueMessage(TMsg msg)
        {
            // Requirements
            Require.NotNull(msg, "msg");

            var queueMsg = new CloudQueueMessage(msg.ToString());
            _queue.AddMessage(queueMsg);
        }

        public IList<TMsg> PeekMessages()
        {
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

        public void ClearMessages()
        {
            _queue.Clear();
        }
    }
}