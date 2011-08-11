using System.Collections.Generic;
using NUnit.Framework;

namespace Disibox.Data.Tests
{
    public class BaseUserTests : BaseDataTests
    {
        protected const int AdminUserCount = 5;
        protected const int CommonUserCount = 5;

        protected const int EmailLength = 7;
        protected const int PwdLength = 9;

        protected readonly IList<string> AdminUserEmails = new List<string>();
        protected readonly IList<string> AdminUserPwds = new List<string>();

        protected readonly IList<string> CommonUserEmails = new List<string>();
        protected readonly IList<string> CommonUserPwds = new List<string>();

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            for (var i = 0; i < AdminUserCount; ++i)
            {
                var currChar = (char)('a' + i);

                var email = new string(currChar, EmailLength);
                AdminUserEmails.Add(email + "_admin");

                var pwd = new string(currChar, PwdLength);
                AdminUserPwds.Add(pwd + "_pwd");
            }

            for (var i = 0; i < CommonUserCount; ++i)
            {
                var currChar = (char)('a' + i);

                var email = new string(currChar, EmailLength);
                CommonUserEmails.Add(email + "_common");

                var pwd = new string(currChar, EmailLength);
                CommonUserPwds.Add(pwd + "_pwd");
            }
        }

        [TearDown]
        protected override void TearDown()
        {
            AdminUserEmails.Clear();
            AdminUserPwds.Clear();

            CommonUserEmails.Clear();
            CommonUserPwds.Clear();

            base.TearDown();
        }
    }
}
