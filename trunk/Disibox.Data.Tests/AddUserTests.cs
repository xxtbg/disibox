using System.Collections.Generic;
using Disibox.Data.Exceptions;
using NUnit.Framework;

namespace Disibox.Data.Tests
{
    public class AddUserTests : BaseDataTests
    {
        private const int AdminUserCount = 5;
        private const int CommonUserCount = 5;

        private const int EmailLength = 7;
        private const int PwdLength = 9;

        private readonly IList<string> _adminUserEmails = new List<string>();
        private readonly IList<string> _adminUserPwds = new List<string>();

        private readonly IList<string> _commonUserEmails = new List<string>();
        private readonly IList<string> _commonUserPwds = new List<string>();

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            for (var i = 0; i < AdminUserCount; ++i)
            {
                var currChar = (char)('a' + i);
                
                var email = new string(currChar, EmailLength);
                _adminUserEmails.Add(email + "_admin");

                var pwd = new string(currChar, EmailLength);
                _adminUserPwds.Add(pwd + "_pwd");
            }

            for (var i = 0; i < CommonUserCount; ++i)
            {
                var currChar = (char)('a' + i);

                var email = new string(currChar, EmailLength);
                _commonUserEmails.Add(email + "_common");

                var pwd = new string(currChar, EmailLength);
                _commonUserPwds.Add(pwd + "_pwd");
            }
        }

        [TearDown]
        protected override void TearDown()
        {
            _adminUserEmails.Clear();
            _adminUserPwds.Clear();

            _commonUserEmails.Clear();
            _commonUserPwds.Clear();

            base.TearDown();
        }

        [Test]
        public void AddOneCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(_commonUserEmails[0], _commonUserPwds[0], false);
        }

        [Test]
        public void AddManyCommonUsers()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            for (var i = 0; i < CommonUserCount; ++i)
                DataSource.AddUser(_commonUserEmails[i], _commonUserPwds[i], false);
        }

        [Test]
        [ExpectedException(typeof(LoggedInUserRequiredException))]
        public void AddOneCommonUserWithoutLoggingIn()
        {
            DataSource.AddUser(_commonUserEmails[0], _commonUserPwds[0], false);
        }

        [Test]
        [ExpectedException(typeof(AdminUserRequiredException))]
        public void AddOneCommonUserLoggingInAsCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(_commonUserEmails[0], _commonUserPwds[0], false);
            DataSource.Logout();

            DataSource.Login(_commonUserEmails[0], _commonUserPwds[0]);
            DataSource.AddUser(_commonUserEmails[1], _commonUserPwds[1], false);
        }

        [Test]
        public void AddOneAdminUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(_adminUserEmails[0], _adminUserPwds[0], true);
        }

        [Test]
        public void AddManyAdminUsers()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            for (var i = 0; i < AdminUserCount; ++i)
                DataSource.AddUser(_adminUserEmails[i], _adminUserPwds[i], true);
        }

        [Test]
        [ExpectedException(typeof(LoggedInUserRequiredException))]
        public void AddOneAdminUserWithoutLoggingIn()
        {
            DataSource.AddUser(_adminUserEmails[0], _adminUserPwds[0], true);
        }

        [Test]
        [ExpectedException(typeof(AdminUserRequiredException))]
        public void AddOneAdminUserLoggingInAsCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(_commonUserEmails[0], _commonUserPwds[0], false);
            DataSource.Logout();

            DataSource.Login(_commonUserEmails[0], _commonUserPwds[0]);
            DataSource.AddUser(_adminUserEmails[0], _adminUserPwds[0], true);
        }
    }
}
