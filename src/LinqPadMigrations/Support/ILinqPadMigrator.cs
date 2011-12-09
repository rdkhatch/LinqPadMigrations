using System.Collections.Generic;

namespace LinqPadMigrations
{
    public interface ILinqPadMigrator
    {
        BatchMigrationResult ExecuteMigrations(string connectionString, IEnumerable<string> scriptFilePaths);
    }
}
