using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetContentType(string path)
        {
            var contentType = "application/octetstream";
            
            var ext = Path.GetExtension(path);
            if (ext != null)
            {
                ext = ext.ToLower();
                
                var registryKey = Registry.ClassesRoot.OpenSubKey(ext);
                if (registryKey != null && registryKey.GetValue("Content Type") != null)
                    contentType = registryKey.GetValue("Content Type").ToString();
            }
            
            return contentType;
        }
    }
}
