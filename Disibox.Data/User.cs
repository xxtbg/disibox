using System;
using Microsoft.WindowsAzure.StorageClient;
using System.Security.Cryptography;
using System.Text;

namespace Disibox.Data
{
    class User : TableServiceEntity
    {
        private const string UserPartitionKey = "users";

        private static const HashAlgorithm hashAlg = new MD5Cng();

        private string _email;
        private string _hashedPwd;

        /// <summary>
        /// In addition to the properties required by the data model, every entity in table 
        /// storage has two key properties: the PartitionKey and the RowKey. These properties 
        /// together form the table's primary key and uniquely identify each entity in the table. 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pwd"></param>
        public User(string email, string pwd)
        {
            PartitionKey = UserPartitionKey;
            RowKey = Guid.NewGuid().ToString();
            _email = email;
            _hashedPwd = EncryptPwd(pwd);
        }

        /// <summary>
        /// User email address.
        /// </summary>
        public string Email { get { return _email; } }

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

        // Verify a hash against a string.
        private bool VerifyPwd(string pwd)
        {
            // Hash the input.
            string hashOfInput = EncryptPwd(pwd);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, _hashedPwd))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
