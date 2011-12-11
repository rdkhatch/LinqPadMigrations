using System.Linq;
using LinqPadMigrations.ScriptCompiler;
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
        public void should_generate_linq_datacontext_with_connection_string_as_default_constructor()
        {
            var generator = new LinqToSQLDataContextGenerator();
            var connectionString = CopyNorthwindDatabaseAndGetConnectionString();

            // Pull .SDF filename out of the connection string  (required for SQL Compact)
            var extractedSQLCompactFilename = connectionString.Replace("Data Source", "").Replace("=", "");
            var sqlMetalConnectionString = string.Format("\"{0}\"", extractedSQLCompactFilename);

            // Generate DataContext
            var generatedDataContext = generator.GenerateDataContext_AndGetCSharpCode(connectionString, sqlMetalConnectionString);

            // Connection String should now be embedded into DataContext default constructor
            var defaultConstructorSignature = "public TypedDataContext() : this(@\"" + connectionString + "\")";
            Assert.IsTrue(generatedDataContext.Contains(defaultConstructorSignature));
        }

        [Test]
        // Passes - This has wide code-coverage
        public void linq_QueryKind_Program_should_execute_()
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
        public void linq_QueryKind_Expression_Should_PASS_when_returns_zero_results()
        {
            PerformTest(TestScripts.LinqPadQueryExpression_Passing, null);
        }

        [Test]
        public void linq_QueryKind_Expression_Should_FAIL_when_returns_results()
        {
            Assert.Throws<MigrationException>(() => PerformTest(TestScripts.LinqPadQueryExpression_Failing, null));
        }

    }
}