using System;

namespace LinqPadMigrations.Migrators
{
    public class SQLServerLinqMigrator : LinqMigratorBase
    {
        protected override string BuildSQLMetalCommand(string sqlmetalPath, string connectionString)
        {
            var command = String.Format("\"{0}\" /conn:\"{1}\" /code /context:TypedDataContext /namespace:MyDataContext", sqlmetalPath, connectionString);
            return command;
        }
    }
}
