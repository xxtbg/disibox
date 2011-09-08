using System;

namespace Disibox.Data.Exceptions
{
    public class QueueNotExistingException : Exception
    {
        public QueueNotExistingException(string queueName) : base(queueName)
        {
            // Empty
        }
    }
}
