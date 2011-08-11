using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disibox.Data.Exceptions;
using NUnit.Framework;

namespace Disibox.Data.Tests
{
    class DeleteUserTests : BaseUserTests
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
        public void DeleteOneCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
            DataSource.DeleteUser(CommonUserEmails[0]);

            var commonUserEmails = DataSource.GetCommonUsersEmails();
            Assert.True(commonUserEmails.Count == 0);
        }

        [Test]
        [ExpectedException(typeof(AdminUserRequiredException))]
        public void DeleteOneAdminUserLoggingInAsCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
            DataSource.Logout();

            DataSource.Login(CommonUserEmails[0], CommonUserPwds[0]);
            DataSource.AddUser(AdminUserEmails[0], AdminUserPwds[0], true);
            DataSource.DeleteUser(AdminUserEmails[0]);
        }

        [Test]
        [ExpectedException(typeof(CannotDeleteUserException))]
        public void DeleteDefaultAdminUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.DeleteUser(DefaultAdminEmail);
        }
    }
}
