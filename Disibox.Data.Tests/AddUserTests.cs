using Disibox.Data.Exceptions;
using NUnit.Framework;

namespace Disibox.Data.Tests
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
        public void AddOneCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);

            var commonUsersEmails = DataSource.GetCommonUsersEmails();
            Assert.True(commonUsersEmails.Contains(CommonUserEmails[0]));
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
        [ExpectedException(typeof(LoggedInUserRequiredException))]
        public void AddOneCommonUserWithoutLoggingIn()
        {
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
        }

        [Test]
        [ExpectedException(typeof(AdminUserRequiredException))]
        public void AddOneCommonUserLoggingInAsCommonUser()
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
        [ExpectedException(typeof(LoggedInUserRequiredException))]
        public void AddOneAdminUserWithoutLoggingIn()
        {
            DataSource.AddUser(AdminUserEmails[0], AdminUserPwds[0], true);
        }

        [Test]
        [ExpectedException(typeof(AdminUserRequiredException))]
        public void AddOneAdminUserLoggingInAsCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
            DataSource.Logout();

            DataSource.Login(CommonUserEmails[0], CommonUserPwds[0]);
            DataSource.AddUser(AdminUserEmails[0], AdminUserPwds[0], true);
        }
    }
}
