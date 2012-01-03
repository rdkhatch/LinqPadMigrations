using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using LinqPadMigrations.Migrators;

namespace LinqPadMigrations
{
    public class LinqPadMigrator : ILinqPadMigrator
    {
        private List<IMigrator> Migrators = new List<IMigrator>();

        public LinqPadMigrator(Func<string, DbConnection> sqlConnectionFactory)
        {
            // Add Migrators, in order of priority
            // SQLCompact must be added before SQLServer (because it has a narrower CanExecute() match - used to generate correct SQLMetal command-line)
            Migrators.Add(new SQLMigrator(sqlConnectionFactory));
            Migrators.Add(new SQLCompactLinqMigrator());
            Migrators.Add(new SQLServerLinqMigrator());
        }

        public LinqPadMigrator()
            : this((connectionString) => new SqlConnection(connectionString))
        {
        }

        public BatchMigrationResult ExecuteMigrations(string connectionString, IEnumerable<string> scriptFilePaths)
        {
            return ExecuteMigrations(connectionString, scriptFilePaths, null, null);
        }

        public BatchMigrationResult ExecuteMigrations(string connectionString, IEnumerable<string> scriptFilePaths, Action<string> beforeScript, Action<MigrationResult> afterScript)
        {
            // Match Script with Migrator
            var q = from scriptFile in scriptFilePaths
                    let migrator = Migrators.FirstOrDefault(m => m.CanExecute(connectionString, scriptFile))
                    select new { scriptFile, migrator };

            var migrationResults = new List<MigrationResult>();

            // Switch to temp folder so generated assemblies go there
            var previousCurrentDirectory = Environment.CurrentDirectory;
            //Environment.CurrentDirectory = Path.GetTempPath();

            // Execute Migration Scripts
            foreach (var tuple in q)
            {
                if (beforeScript != null)
                    beforeScript.Invoke(tuple.scriptFile);

                var migrationResult = tuple.migrator.ExecuteMigration(connectionString, tuple.scriptFile);
                migrationResults.Add(migrationResult);

                if (afterScript != null)
                    afterScript.Invoke(migrationResult);

                // If Failed - Stop executing migrations
                if (migrationResult.Success == false)
                    break;
            }

            // Resume back to previous current folder
            Environment.CurrentDirectory = previousCurrentDirectory;

            return new BatchMigrationResult(migrationResults);
        }
    }
}
