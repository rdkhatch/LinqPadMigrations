using System.Linq;
using LinqPadMigrations.Support;
using NUnit.Framework;

namespace LinqPadMigrations.Tests
{
    [TestFixture]
    public class SQL_Script_Tests : TestBase
    {
        #region SQL Tests

        [Test]
        public void should_run_sql()
        {
            PerformTest(TestAssets.SQL_RenameCustomerNamesToBrody,
                (context) =>
                {
                    var customer = context.Customers.First();
                    Assert.AreNotEqual("Brody", customer.ContactName);
                },
                (context) =>
                {
                    var customer = context.Customers.First();
                    Assert.AreEqual("Brody", customer.ContactName);
                });
        }

        [Test]
        public void bad_sql_should_throw_exception()
        {
            Assert.Throws<MigrationException>(() => PerformTest(TestAssets.SQL_BadSyntax));
        }

        [Test]
        public void should_support_running_SQL_batches_using_GO_command()
        {
            PerformTest(TestAssets.SQL_BatchCommandsUsingGO,
                (context) =>
                {
                    var customer = context.Customers.First();
                    Assert.AreEqual("Ryan", customer.ContactName);
                });
        }

        #endregion

    }
}