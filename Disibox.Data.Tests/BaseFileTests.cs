using System.Collections.Generic;
using System.IO;
using Disibox.Utils;
using NUnit.Framework;

namespace Disibox.Data.Tests
{
    public abstract class BaseFileTests : BaseUserTests
    {
        protected const int FileCount = 3;
        protected const int FileNameLength = 5;

        protected readonly IList<string> FileNames = new List<string>();
        protected readonly IList<Stream> Files = new List<Stream>();

        //protected const string CommonUserName = "common";
        //protected const string CommonUserPwd = "common";

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            for (var i = 0; i < FileCount; ++i)
            {
                var currChar = (char)('a' + i);

                var fileName = new string(currChar, FileNameLength);
                FileNames.Add(fileName + ".txt");

                var file = new MemoryStream(Common.StringToByteArray(fileName));
                Files.Add(file);
            }
        }

        [TearDown]
        protected override void TearDown()
        {
            FileNames.Clear();
            Files.Clear();

            base.TearDown();
        }
    }
}
