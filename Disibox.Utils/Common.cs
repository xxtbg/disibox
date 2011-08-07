using System.IO;
using System.Text;
using Microsoft.Win32;

namespace Disibox.Utils
{
    public static class Common
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetContentType(string filePath)
        {
            var contentType = "application/octetstream";
            
            var ext = Path.GetExtension(filePath);
            if (ext != null)
            {
                ext = ext.ToLower();
                
                var registryKey = Registry.ClassesRoot.OpenSubKey(ext);
                if (registryKey != null && registryKey.GetValue("Content Type") != null)
                    contentType = registryKey.GetValue("Content Type").ToString();
            }
            
            return contentType;
        }

        public static byte[] StreamToByteArray(Stream input)
        {
            var buffer = new byte[input.Length];

            //for (var i = 0; i < input.Length; ++i)
            //    buffer[i] = (byte) input.ReadByte();

            input.Read(buffer, 0, (int) input.Length);

            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }
    }
}
