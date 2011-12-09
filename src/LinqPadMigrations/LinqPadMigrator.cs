﻿using System;
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
            // Match Script with Migrator
            var q = from scriptFile in scriptFilePaths
                    let migrator = Migrators.FirstOrDefault(m => m.CanExecute(connectionString, scriptFile))
                    select new { scriptFile, migrator };

            var migrationResults = new List<MigrationResult>();

            // Execute Migration Scripts
            foreach (var tuple in q)
            {
                var migrationResult = tuple.migrator.ExecuteMigration(connectionString, tuple.scriptFile);
                migrationResults.Add(migrationResult);

                // If Failed - Stop executing migrations
                if (migrationResult.Success == false)
                    break;
            }

            return new BatchMigrationResult(migrationResults);
        }
    }
}