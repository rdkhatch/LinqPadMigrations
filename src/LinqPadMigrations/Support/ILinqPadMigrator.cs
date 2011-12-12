using System;
using System.Collections.Generic;

namespace LinqPadMigrations
{
    public interface ILinqPadMigrator
    {
        BatchMigrationResult ExecuteMigrations(string connectionString, IEnumerable<string> scriptFilePaths);
        BatchMigrationResult ExecuteMigrations(string connectionString, IEnumerable<string> scriptFilePaths, Action<string> beforeScript, Action<MigrationResult> afterScript);
    }
}
