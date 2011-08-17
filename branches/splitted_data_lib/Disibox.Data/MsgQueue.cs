using System.Threading;
using Disibox.Utils;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public class MsgQueue<TMsg> where TMsg : BaseMessage, new()
    {
        private readonly CloudQueue _queue;

        public MsgQueue(string queueUri, StorageCredentials credentials)
        {
            _queue = new CloudQueue(queueUri, credentials);
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
    }
}
