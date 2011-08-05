using System.Security.Cryptography;
using System.Text;

namespace Disibox.Data
{
    internal static class Utils
    {
        private static readonly HashAlgorithm HashAlg = new MD5Cng();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static string EncryptPwd(string pwd)
        {
            // Converts the input string to a byte array and computes the hash.
            var data = HashAlg.ComputeHash(Encoding.UTF8.GetBytes(pwd));

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
