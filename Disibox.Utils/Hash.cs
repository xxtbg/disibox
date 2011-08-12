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
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System.IO;
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

        public static string ComputeMD5(Stream input)
        {
            return ComputeHash(input, MD5Alg);
        }

        public static string ComputeSHA256(string input)
        {
            return ComputeHash(input, SHA256Alg);
        }

        public static string ComputeSHA256(Stream input)
        {
            return ComputeHash(input, SHA256Alg);
        }

        private static string ComputeHash(string input, HashAlgorithm hashAlg)
        {
            // Converts the input string to a byte array and computes the hash.
            var data = hashAlg.ComputeHash(Common.StringToByteArray(input));

            return HashToString(data);
        }

        private static string ComputeHash(Stream input, HashAlgorithm hashAlg)
        {
            var data = hashAlg.ComputeHash(input);

            return HashToString(data);
        }

        private static string HashToString(byte[] data)
        {
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
