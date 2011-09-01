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

using Disibox.Data.Client.Exceptions;
using NUnit.Framework;

namespace Disibox.Data.Tests.Client
{
    public class AddUserTests : BaseUserTests
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
        public void AddManyCommonUsers()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            for (var i = 0; i < CommonUserCount; ++i)
                DataSource.AddUser(CommonUserEmails[i], CommonUserPwds[i], false);

            var commonUsersEmails = DataSource.GetCommonUsersEmails();
            for (var i = 0; i < CommonUserCount; ++i)
                Assert.True(commonUsersEmails.Contains(CommonUserEmails[i]));
        }

        [Test]
        public void AddOneCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);

            var commonUsersEmails = DataSource.GetCommonUsersEmails();
            Assert.True(commonUsersEmails.Contains(CommonUserEmails[0]));
        }

        [Test]
        [ExpectedException(typeof (UserNotLoggedInException))]
        public void AddOneCommonUserWithoutLoggingIn()
        {
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
        }

        [Test]
        [ExpectedException(typeof (UserNotAdminException))]
        public void AddOneCommonUserAsCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
            DataSource.Logout();

            DataSource.Login(CommonUserEmails[0], CommonUserPwds[0]);
            DataSource.AddUser(CommonUserEmails[1], CommonUserPwds[1], false);
        }

        [Test]
        public void AddOneAdminUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(AdminUserEmails[0], AdminUserPwds[0], true);

            var adminUsersEmails = DataSource.GetAdminUsersEmails();
            Assert.True(adminUsersEmails.Contains(AdminUserEmails[0]));
        }

        [Test]
        public void AddManyAdminUsers()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            for (var i = 0; i < AdminUserCount; ++i)
                DataSource.AddUser(AdminUserEmails[i], AdminUserPwds[i], true);

            var adminUsersEmails = DataSource.GetAdminUsersEmails();
            for (var i = 0; i < AdminUserCount; ++i)
                Assert.True(adminUsersEmails.Contains(AdminUserEmails[i]));
        }

        [Test]
        [ExpectedException(typeof (UserNotLoggedInException))]
        public void AddOneAdminUserWithoutLoggingIn()
        {
            DataSource.AddUser(AdminUserEmails[0], AdminUserPwds[0], true);
        }

        [Test]
        [ExpectedException(typeof (UserNotAdminException))]
        public void AddOneAdminUserAsCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
            DataSource.Logout();

            DataSource.Login(CommonUserEmails[0], CommonUserPwds[0]);
            DataSource.AddUser(AdminUserEmails[0], AdminUserPwds[0], true);
        }
    }
}