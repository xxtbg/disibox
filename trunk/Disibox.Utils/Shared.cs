//
// Copyright (c) 2011, University of Genoa
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the University of Genoa nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL UNIVERSITY OF GENOA BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace Disibox.Utils
{
    /// <summary>
    /// Contains useful and heterogeneous functions used throughout the code.
    /// </summary>
    public static class Shared
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
        /// Converts given number of bytes into kilobytes.
        /// </summary>
        /// <param name="bytes">Number of bytes to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static double ConvertBytesToKilobytes(long bytes)
        {
            return Math.Round(bytes / 1024f, 2);
        }
    }
}
