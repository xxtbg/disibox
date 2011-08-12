using Disibox.Data.Exceptions;
using NUnit.Framework;

namespace Disibox.Data.Tests
{
    class DeleteFileTests : BaseFileTests
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
        public void DeleteOneCommonUserFileLoggingInAsAdminUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
            DataSource.Logout();

            DataSource.Login(CommonUserEmails[0], CommonUserPwds[0]);
            var fileUri = DataSource.AddFile(FileNames[0], Files[0]);
            DataSource.Logout();

            DataSource.Login(CommonUserEmails[0], CommonUserPwds[0]);
            DataSource.DeleteFile(fileUri);

            var fileNames = DataSource.GetFileNames();
            Assert.True(fileNames.Count == 0);
        }

        [Test]
        [ExpectedException(typeof(DeletingNotOwnedFileException))]
        public void DeleteOneCommonUserFileLoggingInAsOtherCommonUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
            DataSource.AddUser(CommonUserEmails[1], CommonUserPwds[1], false);
            DataSource.Logout();

            DataSource.Login(CommonUserEmails[0], CommonUserPwds[0]);
            var fileUri = DataSource.AddFile(FileNames[0], Files[0]);
            DataSource.Logout();

            DataSource.Login(CommonUserEmails[1], CommonUserPwds[1]);
            DataSource.DeleteFile(fileUri);
        }

        [Test]
        public void DeleteOneCommonUserFileLoggingInAsProprietaryUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddUser(CommonUserEmails[0], CommonUserPwds[0], false);
            DataSource.Logout();

            DataSource.Login(CommonUserEmails[0], CommonUserPwds[0]);
            var fileUri = DataSource.AddFile(FileNames[0], Files[0]);
            DataSource.DeleteFile(fileUri);

            var fileNames = DataSource.GetFileNames();
            Assert.True(fileNames.Count == 0);
        }
    }
}
