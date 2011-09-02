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

using System.Collections.Generic;
using System.IO;
using Disibox.Data.Setup;
using Disibox.Utils;
using NUnit.Framework;

namespace Disibox.Data.Tests
{
    [TestFixture]
    public abstract class BaseDataTests
    {
        protected const int AdminUserCount = 5;
        protected const int CommonUserCount = 5;
        protected const int FileCount = 3;

        private const int EmailLength = 7;
        private const int PwdLength = 9;
        private const int FileNameLength = 5;

        protected readonly IList<string> AdminUserEmails = new List<string>();
        protected readonly IList<string> AdminUserPwds = new List<string>();

        protected readonly IList<string> CommonUserEmails = new List<string>();
        protected readonly IList<string> CommonUserPwds = new List<string>();

        protected readonly IList<string> FileNames = new List<string>();
        protected readonly IList<Stream> FileStreams = new List<Stream>();

        [SetUp]
        protected virtual void SetUp()
        {
            CloudStorageSetup.ResetStorage();
            SetUpAdminUsers();
            SetUpCommonUsers();
            SetUpFiles();
        }

        [TearDown]
        protected virtual void TearDown()
        {
            TearDownAdminUsers();
            TearDownCommonUsers();
            TearDownFiles();
            CloudStorageSetup.ResetStorage();
        }

        private void SetUpAdminUsers()
        {
            for (var i = 0; i < AdminUserCount; ++i)
            {
                var currChar = (char)('a' + i);

                var email = new string(currChar, EmailLength);
                AdminUserEmails.Add(email + "_admin@test.pino");

                var pwd = new string(currChar, PwdLength);
                AdminUserPwds.Add(pwd + "_pwd");
            }
        }

        private void TearDownAdminUsers()
        {
            AdminUserEmails.Clear();
            AdminUserPwds.Clear();
        }

        private void SetUpCommonUsers()
        {
            for (var i = 0; i < CommonUserCount; ++i)
            {
                var currChar = (char)('a' + i);

                var email = new string(currChar, EmailLength);
                CommonUserEmails.Add(email + "_common@test.pino");

                var pwd = new string(currChar, EmailLength);
                CommonUserPwds.Add(pwd + "_pwd");
            }
        }

        private void TearDownCommonUsers()
        {
            CommonUserEmails.Clear();
            CommonUserPwds.Clear();
        }

        private void SetUpFiles()
        {
            for (var i = 0; i < FileCount; ++i)
            {
                var currChar = (char)('a' + i);

                var fileName = new string(currChar, FileNameLength);
                FileNames.Add(fileName + ".txt");

                var file = new MemoryStream(Shared.StringToByteArray(fileName));
                FileStreams.Add(file);
            }
        }

        private void TearDownFiles()
        {
            FileNames.Clear();
            FileStreams.Clear();
        }
    }
}
