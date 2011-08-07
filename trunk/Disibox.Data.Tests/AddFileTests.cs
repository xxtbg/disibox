using System.Collections.Generic;
using System.IO;
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
        public void AddOneFile()
        {
            DataSource.AddFile(_fileNames[0], _files[0]);
        }
    }
}
