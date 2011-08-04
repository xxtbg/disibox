using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace Disibox.Data
{
    class User : TableServiceEntity
    {
        public const string UserPartitionKey = "users";

        private static readonly HashAlgorithm hashAlg = new MD5Cng();

        private string _email;
        private string _hashedPwd;
        private bool _isAdmin;

        /// <summary>
        /// In addition to the properties required by the data model, every entity in table 
        /// storage has two key properties: the PartitionKey and the RowKey. These properties 
        /// together form the table's primary key and uniquely identify each entity in the table. 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pwd"></param>
        public User(string email, string pwd, bool isAdmin)
        {
            PartitionKey = UserPartitionKey;
            RowKey = Guid.NewGuid().ToString();
            _email = email;
            _hashedPwd = EncryptPwd(pwd);
            _isAdmin = isAdmin;
        }

        /// <summary>
        /// User email address.
        /// </summary>
        public string Email 
        { 
            get { return _email; } 
        }

        /// <summary>
        /// Hashed user password.
        /// </summary>
        public string HashedPassword 
        { 
            get { return _hashedPwd; } 
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAdmin
        {
            get { return _isAdmin; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="userPwd"></param>
        /// <returns></returns>
        public bool Matches(string userEmail, string userPwd)
        {
            return (_email == userEmail) && (_hashedPwd == EncryptPwd(userPwd));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        private string EncryptPwd(string pwd)
        {
            // Converts the input string to a byte array and computes the hash.
            var data = hashAlg.ComputeHash(Encoding.UTF8.GetBytes(pwd));

            // Creates a new Stringbuilder to collect the bytes and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
