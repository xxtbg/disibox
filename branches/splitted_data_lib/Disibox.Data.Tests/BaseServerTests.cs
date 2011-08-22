using Disibox.Data.Server;
using Disibox.Data.Setup;
using NUnit.Framework;

namespace Disibox.Data.Tests
{
    [TestFixture]
    public abstract class BaseServerTests
    {
        [SetUp]
        protected virtual void SetUp()
        {
            CloudStorageSetup.ResetStorage();
            DataSource = new ServerDataSource();
        }

        [TearDown]
        protected virtual void TearDown()
        {
            DataSource = null;
        }

        protected ServerDataSource DataSource { get; private set; }

        protected static string DefaultAdminEmail
        {
            get { return Setup.Properties.Settings.Default.DefaultAdminEmail; }
        }

        protected static string DefaultAdminPwd
        {
            get { return Setup.Properties.Settings.Default.DefaultAdminPwd; }
        }
    }
}
