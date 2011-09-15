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
using Disibox.Data.Client.Exceptions;
using Disibox.Data.Entities;
using NUnit.Framework;

namespace Disibox.Data.Tests.Client
{
    internal class DeleteUserTests : BaseClientTests
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
        public void DeleteOneCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], UserType.CommonUser);
            DataSource.DeleteUser(CommonUserEmails[0]);

            var commonUserEmails = DataSource.GetCommonUsersEmails();
            Assert.True(commonUserEmails.Count == 0);
        }

        /*=============================================================================
            ArgumentNullException
        =============================================================================*/

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeleteUsingNullEmail()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.DeleteUser(null);
        }

        /*=============================================================================
            CannotDeleteLastAdminException
        =============================================================================*/

        [Test]
        [ExpectedException(typeof(CannotDeleteLastAdminException))]
        public void DeleteDefaultAdmin()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.DeleteUser(DefaultAdminEmail);
        }

        [Test]
        [ExpectedException(typeof(CannotDeleteLastAdminException))]
        public void DeleteTwoAdmins()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(AdminUserEmails[0], AdminUserPwds[0], UserType.AdminUser);
            DataSource.DeleteUser(DefaultAdminEmail);
            DataSource.DeleteUser(AdminUserEmails[0]);
        }

        [Test]
        [ExpectedException(typeof(CannotDeleteLastAdminException))]
        public void DeleteManyAdmins()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            for (var i = 0; i < AdminUserEmails.Count; ++i)
                DataSource.AddUser(AdminUserEmails[i], AdminUserPwds[i], UserType.AdminUser);
            DataSource.DeleteUser(DefaultAdminEmail);
            foreach (var email in AdminUserEmails)
                DataSource.DeleteUser(email);
        }

        /*=============================================================================
            UserNotAdminException
        =============================================================================*/

        [Test]
        [ExpectedException(typeof(UserNotAdminException))]
        public void DeleteOneAdminUserLoggingInAsCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], UserType.CommonUser);
            DataSource.Logout();

            DataSource.Login(CommonUserEmails[0], CommonUserPwds[0]);
            DataSource.AddUser(AdminUserEmails[0], AdminUserPwds[0], UserType.AdminUser);
            DataSource.DeleteUser(AdminUserEmails[0]);
        }

        /*=============================================================================
            UserNotExistingException
        =============================================================================*/

        [Test]
        [ExpectedException(typeof(UserNotExistingException))]
        public void DeleteNotExistingUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.DeleteUser(CommonUserEmails[0]);
        }

        /*=============================================================================
            UserNotLoggedInException
        =============================================================================*/

        [Test]
        [ExpectedException(typeof(UserNotLoggedInException))]
        public void DeleteWithoutLoggingIn()
        {
            DataSource.DeleteUser(DefaultAdminEmail);
        }
    }
}