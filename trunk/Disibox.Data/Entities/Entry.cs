using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data.Entities
{
    internal sealed class Entry : TableServiceEntity
    {
        public const string EntryPartitionKey = "entries";

        /// <summary>
        /// In addition to the properties required by the data model, every entity in table 
        /// storage has two key properties: the PartitionKey and the RowKey. These properties 
        /// together form the table's primary key and uniquely identify each entity in the table. 
        /// </summary>
        /// <param name="entryKey"></param>
        /// <param name="entryValue"></param>
        public Entry(string entryKey, string entryValue)
        {
            // TableServiceEntity properties
            PartitionKey = EntryPartitionKey;
            RowKey = entryKey;

            Key = entryKey;
            Value = entryValue;
        }

        public string Key { get; private set; }

        public string Value { get; set; }
    }
}
