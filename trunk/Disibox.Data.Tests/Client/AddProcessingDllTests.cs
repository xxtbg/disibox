using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disibox.Utils;
using NUnit.Framework;

namespace Disibox.Data.Tests.Client
{
    public class AddProcessingDllTests : BaseClientTests
    {
        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            for (var i = 0; i < FileNames.Count; ++i)
                FileNames[i] = FileNames[i] + ".dll";
        }

        [TearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void AddOneDll()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddProcessingDll(FileNames[0], FileStreams[0]);

            var dllNames = DataSource.GetProcessingDllNames();
            Assert.True(dllNames.Count == 1 && dllNames.Contains(FileNames[0]));

            var dll = DataSource.GetProcessingDll(FileNames[0]);
            Assert.True(Shared.StreamsAreEqual(dll, FileStreams[0]));

            DataSource.Logout();
        }
    }
}
