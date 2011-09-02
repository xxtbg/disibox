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

using NUnit.Framework;

namespace Disibox.Data.Tests.Mixed
{
    public class UserExistsTests : BaseMixedTests
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

        /*=============================================================================
            Valid calls
        =============================================================================*/

        [Test]
        public void DefaultAdminExists()
        {
            Assert.True(ServerDataSource.UserExists(DefaultAdminEmail, DefaultAdminPwd));
        }

        [Test]
        public void OneExistingAdminUser()
        {
            ClientDataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            ClientDataSource.AddUser(AdminUserEmails[0], AdminUserPwds[0], true);
            ClientDataSource.Logout();

            Assert.True(ServerDataSource.UserExists(AdminUserEmails[0], AdminUserPwds[0]));
        }

        [Test]
        public void ManyExistingAdminUsers()
        {
            ClientDataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            for (var i = 0; i < AdminUserEmails.Count; ++i)
                ClientDataSource.AddUser(AdminUserEmails[i], AdminUserPwds[i], false);
            ClientDataSource.Logout();

            for (var i = 0; i < AdminUserEmails.Count; ++i)
                Assert.True(ServerDataSource.UserExists(AdminUserEmails[i], AdminUserPwds[i]));
        }

        [Test]
        public void OneExistingCommonUser()
        {
            ClientDataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            ClientDataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
            ClientDataSource.Logout();

            Assert.True(ServerDataSource.UserExists(CommonUserEmails[0], CommonUserPwds[0]));
        }

        [Test]
        public void ManyExistingCommonUsers()
        {
            ClientDataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            for (var i = 0; i < CommonUserEmails.Count; ++i)
                ClientDataSource.AddUser(CommonUserEmails[i], CommonUserPwds[i], false);
            ClientDataSource.Logout();

            for (var i = 0; i < CommonUserEmails.Count; ++i)
                Assert.True(ServerDataSource.UserExists(CommonUserEmails[i], CommonUserPwds[i]));
        }

        [Test]
        public void OneNotExistingUser()
        {
            Assert.False(ServerDataSource.UserExists(AdminUserEmails[0], AdminUserPwds[0]));
        }

        [Test]
        public void ManyNotExistingUsers()
        {
            for (var i = 0; i < AdminUserEmails.Count; ++i)
                Assert.False(ServerDataSource.UserExists(AdminUserEmails[i], AdminUserPwds[i]));
        }
    }
}
