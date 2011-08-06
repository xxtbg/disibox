using System.Security.Cryptography;
using System.Text;

namespace Disibox.Utils
{
    public static class Hash
    {
        private static readonly HashAlgorithm MD5Alg = new MD5Cng();
        private static readonly HashAlgorithm SHA256Alg = new SHA256Cng();

        public static string ComputeMD5(string input)
        {
            return ComputeHash(input, MD5Alg);
        }

        public static string ComputeSHA256(string input)
        {
            return ComputeHash(input, SHA256Alg);
        }

        private static string ComputeHash(string input, HashAlgorithm hashAlg)
        {
            // Converts the input string to a byte array and computes the hash.
            var data = hashAlg.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Creates a new Stringbuilder to collect the bytes and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
