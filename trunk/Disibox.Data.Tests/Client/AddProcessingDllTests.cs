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
        private const int DefaultDllCount = 1;

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

        /*=============================================================================
            Valid calls
        =============================================================================*/

        [Test]
        public void AddOneDll()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            DataSource.AddProcessingDll(FileNames[0], FileStreams[0]);

            var dllNames = DataSource.GetProcessingDllNames();
            Assert.True(dllNames.Count == DefaultDllCount+1 && dllNames.Contains(FileNames[0]));

            var dll = DataSource.GetProcessingDll(FileNames[0]);
            Assert.True(Shared.StreamsAreEqual(dll, FileStreams[0]));

            DataSource.Logout();
        }

        [Test]
        public void AddManyDlls()
        {
            DataSource.Login(DefaultAdminEmail, DefaultAdminPwd);
            for (var i = 0; i < FileNames.Count; ++i)
                DataSource.AddProcessingDll(FileNames[i], FileStreams[i]);

            var dllNames = DataSource.GetProcessingDllNames();
            Assert.True(dllNames.Count == DefaultDllCount + FileNames.Count);

            for (var i = 0; i < FileNames.Count; ++i)
            {
                Assert.True(dllNames.Contains(FileNames[i]));
                var dll = DataSource.GetProcessingDll(FileNames[i]);
                Assert.True(Shared.StreamsAreEqual(dll, FileStreams[i]));
            }         

            DataSource.Logout();
        }
    }
}
