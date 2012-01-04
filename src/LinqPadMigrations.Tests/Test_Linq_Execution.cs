using System.Linq;
using LinqPadMigrations.Support;
using NUnit.Framework;

namespace LinqPadMigrations.Tests
{
    [TestFixture]
    public class Linq_Execute_Tests : TestBase
    {

        [Test]
        // Passes - This has wide code-coverage
        public void linq_QueryKind_Program_should_execute()
        {
            PerformTest(TestAssets.LINQ_UpdateCustomerBOLIDTitlefromOwnertoManager,
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
            PerformTest(TestAssets.LinqPadQueryExpression_Passing);
        }

        [Test]
        public void linq_QueryKind_Expression_Should_FAIL_when_returns_collection()
        {
            Assert.Throws<MigrationException>(() => PerformTest(TestAssets.LinqPadQueryExpression_Failing_Returns_Collection));
        }

        [Test]
        public void linq_QueryKind_Expression_Should_FAIL_when_returns_single_value()
        {
            Assert.Throws<MigrationException>(() => PerformTest(TestAssets.LinqPadQueryExpression_Failing_Returns_SingleValue));
        }

    }
}