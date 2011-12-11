using System;

namespace LinqPadMigrations.Migrators
{
    public class SQLServerLinqMigrator : LinqMigratorBase
    {
        protected override string GetSqlMetalConnectionStringArg(string connectionString)
        {
            var command = String.Format("/conn:\"{0}\"", connectionString);
            return command;
        }
    }
}
