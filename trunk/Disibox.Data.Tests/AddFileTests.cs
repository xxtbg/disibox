using System;
using System.Collections.Generic;
using System.IO;
using Disibox.Data.Exceptions;
using Disibox.Utils;
using NUnit.Framework;

namespace Disibox.Data.Tests
{
    public class AddFileTests : BaseDataTests
    {
        private const int FileCount = 3;
        private const int FileNameLength = 5;

        private readonly IList<string> _fileNames = new List<string>();
        private readonly IList<Stream> _files = new List<Stream>();

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            for (var i = 0; i < FileCount; ++i)
            {
                var currChar = (char)('a' + i);
                
                var fileName = new string(currChar, FileNameLength);
                _fileNames.Add(fileName + ".txt");
                
                var file = new MemoryStream(Common.StringToByteArray(fileName));
                _files.Add(file);
            }
        }

        [TearDown]
        protected override void TearDown()
        {
            _fileNames.Clear();
            _files.Clear();

            base.TearDown();
        }

        [Test]
        public void AddOneFileAsAdminUser()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            var fileUri = DataSource.AddFile(_fileNames[0], _files[0]);

            var fileNames = DataSource.GetFilesNames();
            Assert.True(fileNames.Contains(_fileNames[0]));

            var file = DataSource.GetFile(fileUri);
            Assert.True(Common.StreamsAreEqual(file, _files[0]));
        }

        [Test]
        public void AddManyFilesAsAdminUser()
        {
            var uris = new string[FileCount];

            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            for (var i = 0; i < FileCount; ++i)
                uris[i] = DataSource.AddFile(_fileNames[i], _files[i]);

            var fileNames = DataSource.GetFilesNames();
            for (var i = 0; i < FileCount; ++i)
            {
                Assert.True(fileNames.Contains(_fileNames[i]));
                var file = DataSource.GetFile(uris[i]);
                Assert.True(Common.StreamsAreEqual(file, _files[i]));
            }
                
        }

        [Test]
        [ExpectedException(typeof(LoggedInUserRequiredException))]
        public void AddOneFileWithoutLoggingIn()
        {
            DataSource.AddFile(_fileNames[0], _files[0]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullFileNameArgument()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddFile(null, _files[0]);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullFileContentArgument()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddFile(_fileNames[0], null);
        }
    }
}
