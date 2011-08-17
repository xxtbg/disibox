using System;
using Disibox.Utils;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    public abstract class BaseEntity : TableServiceEntity
    {
        protected BaseEntity(string rowKey, string partitionKey)
        {
            // Requirements
            Require.NotNull(rowKey, "rowKey");
            Require.NotNull(partitionKey, "partitionKey");

            RowKey = rowKey;
            PartitionKey = partitionKey;
        }

        /// <summary>
        /// Seems to be required for serialization sake.
        /// </summary>
        [Obsolete]
        protected BaseEntity(string partitionKey)
        {
            // Requirements
            Require.NotNull(partitionKey, "partitionKey");

            RowKey = partitionKey;
            PartitionKey = partitionKey;
        }
    }
}
