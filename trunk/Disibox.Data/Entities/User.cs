using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data.Entities
{
    internal sealed class User : TableServiceEntity
    {
        public const string UserPartitionKey = "users";

        /// <summary>
        /// In addition to the properties required by the data model, every entity in table 
        /// storage has two key properties: the PartitionKey and the RowKey. These properties 
        /// together form the table's primary key and uniquely identify each entity in the table. 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userEmail"></param>
        /// <param name="userPwd"></param>
        /// <param name="userType"></param>
        public User(string userId, string userEmail, string userPwd, UserType userType)
        {
            // TableServiceEntity properties
            PartitionKey = UserPartitionKey;
            RowKey = userId;
            
            Id = userId;
            Email = userEmail;
            HashedPassword = Utils.EncryptPwd(userPwd);
            Type = userType;
        }

        public string Id { get; private set; }

        /// <summary>
        /// User email address.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Hashed user password.
        /// </summary>
        public string HashedPassword { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public UserType Type { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="userPwd"></param>
        /// <returns></returns>
        public bool Matches(string userEmail, string userPwd)
        {
            return (Email == userEmail) && (HashedPassword == Utils.EncryptPwd(userPwd));
        }
    }
}
