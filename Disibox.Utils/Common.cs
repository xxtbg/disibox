using System;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace Disibox.Utils
{
    /// <summary>
    /// Contains useful and heterogeneous functions used throughout the code.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Returns the corresponding mime type to the file on given path;
        /// if file has an unknown type, "application/octetstream" is returned.
        /// </summary>
        /// <param name="filePath">The path of the file for which the mime type should be found.</param>
        /// <returns>The mime type of the file, or "application/octetstream" if file has an unknown type.</returns>
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

        /// <summary>
        /// Returns whether given streams are equal, that is,
        /// they have the same content, byte by byte.
        /// </summary>
        /// <param name="s1">First stream.</param>
        /// <param name="s2">Second stream.</param>
        /// <returns>True if streams are equal, false otherwise.</returns>
        public static bool StreamsAreEqual(Stream s1, Stream s2)
        {
            if (s1.Length != s2.Length) return false;

            var b1 = StreamToByteArray(s1);
            var b2 = StreamToByteArray(s2);

            for (var i = 0; i < b1.Length; ++i)
                if (b1[i] != b2[i]) return false;

            return true;
        }

        /// <summary>
        /// Converts given stream to a byte array.
        /// </summary>
        /// <param name="input">Stream to convert.</param>
        /// <returns>Converted stream.</returns>
        public static byte[] StreamToByteArray(Stream input)
        {
            var buffer = new byte[input.Length];

            var oldPosition = input.Position;
            input.Seek(0, SeekOrigin.Begin);
            input.Read(buffer, 0, (int) input.Length);
            input.Seek(oldPosition, SeekOrigin.Begin);

            return buffer;
        }

        /// <summary>
        /// Converts given string to a byte array.
        /// </summary>
        /// <param name="input">String to convert.</param>
        /// <returns>Converted string.</returns>
        public static byte[] StringToByteArray(string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        /// <summary>
        /// Converts the given number of bytes into kilobytes
        /// </summary>
        /// <param name="bytes">Number to convert</param>
        /// <returns>number converted into kilobytes</returns>
        public static double ConvertBytesToKilobytes(long bytes)
        {
            return bytes / 1024f;
        }
    }
}
