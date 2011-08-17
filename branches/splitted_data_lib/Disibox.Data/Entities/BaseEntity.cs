using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data.Entities
{
    public abstract class BaseEntity : TableServiceEntity
    {
        protected BaseEntity(string rowKey, string partitionKey)
        {
            RowKey = rowKey;
            PartitionKey = partitionKey;
        }

        /// <summary>
        /// Seems to be required for serialization sake.
        /// </summary>
        [Obsolete]
        protected BaseEntity(string partitionKey)
        {
            RowKey = partitionKey;
            PartitionKey = partitionKey;
        }
    }
}
