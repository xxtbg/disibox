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
