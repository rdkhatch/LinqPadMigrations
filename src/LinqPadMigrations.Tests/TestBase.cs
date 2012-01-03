using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using LinqPadMigrations.Tests.Northwind;
using NUnit.Framework;

namespace LinqPadMigrations.Tests
{
    public class TestBase
    {

        protected void PerformTest(IEnumerable<string> scriptFiles, Action<NorthwindContext> afterScript)
        {
            PerformTest(scriptFiles, null, afterScript);
        }

        protected void PerformTest(IEnumerable<string> scriptFiles, Action<NorthwindContext> beforeScript, Action<NorthwindContext> afterScript)
        {
            var migrator = GetMigrator();
            string connectionString = CopyNorthwindDatabaseAndGetConnectionString();

            // Make copy of database

            if (beforeScript != null)
            {
                var context = new NorthwindContext(connectionString);
                beforeScript.Invoke(context);
            }

            // Execute Migrations
            var batchResult = migrator.ExecuteMigrations(connectionString, scriptFiles);
            if (batchResult.Success == false)
            {
                var failingMigration = batchResult.Results.First(r => r.Success == false);
                Assert.Fail("Migration Failed, but should have thrown a MigrationException!", failingMigration.MigrationException);
            }

            if (afterScript != null)
            {
                var context = new NorthwindContext(connectionString);
                afterScript.Invoke(context);
            }

        }

        protected static string CopyNorthwindDatabaseAndGetConnectionString()
        {
            var goodDatabasePath = @"App_Data\Northwind.Backup.sdf";
            var tempDatabasePath = String.Format(@"TempDBs\{0}.sdf", Guid.NewGuid());

            File.Copy(goodDatabasePath, tempDatabasePath);

            string connectionString = String.Format("Data Source={0}", tempDatabasePath);
            return connectionString;
        }

        protected void PerformTest(string scriptFile)
        {
            PerformTest(scriptFile, null);
        }

        protected void PerformTest(string scriptFile, Action<NorthwindContext> afterScript)
        {
            PerformTest(scriptFile, null, afterScript);
        }

        protected void PerformTest(string scriptFile, Action<NorthwindContext> beforeScript, Action<NorthwindContext> afterScript)
        {
            PerformTest(new[] { scriptFile }, beforeScript, afterScript);
        }

        private ILinqPadMigrator GetMigrator()
        {
            Func<string, DbConnection> sqlCompactConnectionFactory = (connString) => new SqlCeConnection(connString);
            var migrator = new LinqPadMigrator(sqlCompactConnectionFactory);
            return migrator;
        }
    }
}
