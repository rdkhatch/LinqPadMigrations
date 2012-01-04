using LinqPadMigrations.ScriptCompiler;
using NUnit.Framework;

namespace LinqPadMigrations.Tests
{
    [TestFixture]
    public class Linq_Context_Tests : TestBase
    {

        #region Context Generation

        [Test]
        public void should_generate_linq_datacontext_with_connection_string_as_default_constructor()
        {
            var generator = new LinqToSQLDataContextGenerator();
            var connectionString = CopyNorthwindDatabaseAndGetConnectionString();

            // Pull .SDF filename out of the connection string  (required for SQL Compact)
            var extractedSQLCompactFilename = connectionString.Replace("Data Source", "").Replace("=", "");
            var sqlMetalConnectionString = string.Format("\"{0}\"", extractedSQLCompactFilename);

            // Generate DataContext
            var generatedDataContext = generator.GenerateDataContext_AndGetCSharpCode(connectionString, sqlMetalConnectionString, null);

            // Connection String should now be embedded into DataContext default constructor
            var defaultConstructorSignature = "public TypedDataContext() : this(@\"" + connectionString + "\")";
            Assert.IsTrue(generatedDataContext.Contains(defaultConstructorSignature));
        }

        #endregion

        #region Context Replacement Tests

        [Test]
        public void Linq_ContextReplacement_Should_ignore_anonymous_LET_and_SELECT_NEW_TABLENAME()
        {
            PerformTest(TestAssets.LinqPadQueryExpression_ContextReplace_UsingSelectNewTableName);
        }

        [Test]
        public void Linq_ContextReplacement_should_be_case_sensitive()
        {
            PerformTest(TestAssets.LinqPadQueryExpression_ContextReplace_UsingCaseSensitiveTableName);
        }

        [Test]
        public void Linq_ContextReplacement_should_ignore_strings_containing_table_names()
        {
            PerformTest(TestAssets.LinqPadQueryExpression_ContextReplace_TableNameWithinString);
        }

        #endregion


    }
}