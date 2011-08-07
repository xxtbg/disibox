using NUnit.Framework;

namespace Disibox.Data.Tests
{
    [TestFixture]
    public abstract class BaseDataTests
    {
        [SetUp]
        protected virtual void SetUp()
        {
            DataSource = new DataSource();
            DataSource.Clear();
        }

        [TearDown]
        protected virtual void TearDown()
        {
            DataSource = null;
        }

        protected DataSource DataSource { get; private set; }

        protected string DefaultAdminEmail
        {
            get { return Properties.Settings.Default.DefaultAdminEmail; }
        }

        protected string DefaultAdminPwd
        {
            get { return Properties.Settings.Default.DefaultAdminPwd; }
        }
    }
}
