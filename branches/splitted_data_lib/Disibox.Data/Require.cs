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
using System.Text.RegularExpressions;
using Disibox.Data.Exceptions;

namespace Disibox.Data
{
    public static class Require
    {
        private const string EmailRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                          @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                          @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="argName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void NotNull(object arg, string argName)
        {
            if (arg != null) return;
            throw new ArgumentNullException(argName);
        }

        /// <summary>
        /// Checks if given string is a valid email address.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="argName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Disibox.Data.Exceptions.InvalidEmailException"></exception>
        public static void ValidEmail(string email, string argName)
        {
            // Requirements
            NotNull(email, argName);

            var regex = new Regex(EmailRegex);
            if (regex.IsMatch(email)) return;
            throw new InvalidEmailException(email);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="argName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Disibox.Data.Exceptions.InvalidFileNameException"></exception>
        public static void ValidFileName(string fileName, string argName)
        {
            // Requirements
            NotNull(fileName, argName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pwd"></param>
        /// <param name="argName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Disibox.Data.Exceptions.InvalidPasswordException"></exception>
        public static void ValidPassword(string pwd, string argName)
        {
            // Requirements
            NotNull(pwd, argName);
        }
    }
}
