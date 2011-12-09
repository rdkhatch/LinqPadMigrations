using System.Linq;
using LinqPadMigrations.Migrators;
using LinqPadMigrations.Support;
using NUnit.Framework;

namespace LinqPadMigrations.Tests
{
    [TestFixture]
    public class Tests : TestBase
    {
        [Test]
        public void should_run_sql()
        {
            PerformTest(TestScripts.SQL_RenameCustomerNamesToBrody,
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
            Assert.Throws<MigrationException>(() => PerformTest(TestScripts.SQL_BadSyntax, null));
        }

        [Test]
        public void linq_should_execute()
        {
            PerformTest(TestScripts.LINQ_UpdateCustomerBOLIDTitlefromOwnertoManager,
                (context) =>
                {
                    var customer = context.Customers.Where(c => c.CustomerID == "BOLID").Single();
                    Assert.AreEqual("Owner", customer.ContactTitle);
                },
                (context) =>
                {
                    var customer = context.Customers.Where(c => c.CustomerID == "BOLID").Single();
                    Assert.AreEqual("Manager", customer.ContactTitle);
                });
        }

        [Test]
        public void should_generate_linq_datacontext()
        {
            var linqMigrator = new SQLCompactLinqMigrator();
            var connectionString = CopyNorthwindDatabaseAndGetConnectionString();

            var generatedCSharpDataContext = linqMigrator.GenerateDataContext_AndGetCSharpCode(connectionString);
            Assert.IsNotNullOrEmpty(generatedCSharpDataContext);
        }

        [Test]
        public void linqpad_files_ending_in_TEST_should_pass_if_returns_true()
        {

        }

        [Test]
        public void should_refresh_data_context_before_executing_linqpad_query()
        {

        }

    }
}