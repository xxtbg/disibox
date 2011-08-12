﻿//
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

using System;
using Disibox.Data.Exceptions;
using Disibox.Utils;
using NUnit.Framework;

namespace Disibox.Data.Tests
{
    public class AddFileTests : BaseFileTests
    {
        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();
        }

        [TearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void AddOneFileAsAdminUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            var fileUri = DataSource.AddFile(FileNames[0], Files[0]);

            var fileNames = DataSource.GetFileNames();
            Assert.True(fileNames.Contains(FileNames[0]));

            var file = DataSource.GetFile(fileUri);
            Assert.True(Common.StreamsAreEqual(file, Files[0]));
        }

        [Test]
        public void AddManyFilesAsAdminUser()
        {
            var uris = new string[FileCount];

            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            for (var i = 0; i < FileCount; ++i)
                uris[i] = DataSource.AddFile(FileNames[i], Files[i]);

            var fileNames = DataSource.GetFileNames();
            for (var i = 0; i < FileCount; ++i)
            {
                Assert.True(fileNames.Contains(FileNames[i]));
                var file = DataSource.GetFile(uris[i]);
                Assert.True(Common.StreamsAreEqual(file, Files[i]));
            }
                
        }

        [Test]
        public void AddOneFileAsCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
            DataSource.Logout();

            DataSource.Login(CommonUserEmails[0], CommonUserPwds[0]);
            var fileUri = DataSource.AddFile(FileNames[0], Files[0]);

            var fileNames = DataSource.GetFileNames();
            Assert.True(fileNames.Contains(FileNames[0]));

            var file = DataSource.GetFile(fileUri);
            Assert.True(Common.StreamsAreEqual(file, Files[0]));
        }

        [Test]
        [ExpectedException(typeof(LoggedInUserRequiredException))]
        public void AddOneFileWithoutLoggingIn()
        {
            DataSource.AddFile(FileNames[0], Files[0]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullFileNameArgument()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddFile(null, Files[0]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullFileContentArgument()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddFile(FileNames[0], null);
        }
    }
}
