using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    sealed class Datum : TableServiceEntity
    {
        public static readonly string DatumPartitionKey = "data";

        /// <summary>
        /// In addition to the properties required by the data model, every entity in table 
        /// storage has two key properties: the PartitionKey and the RowKey. These properties 
        /// together form the table's primary key and uniquely identify each entity in the table. 
        /// </summary>
        /// <param name="datumKey"></param>
        /// <param name="datumValue"></param>
        public Datum(string datumKey, string datumValue)
        {
            PartitionKey = DatumPartitionKey;
            RowKey = datumKey;
            Key = datumKey;
            Value = datumValue;
        }

        public string Key { get; private set; }

        public string Value { get; private set; }
    }
}
