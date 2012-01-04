using System;
using LinqPadMigrations.Support;

namespace LinqPadMigrations.Migrators
{
    public class SQLServerLinqMigrator : LinqMigratorBase
    {
        public SQLServerLinqMigrator(IDbmlManipulator manipulator)
            : base(manipulator)
        {
        }

        protected override string GetSqlMetalConnectionStringArg(string connectionString)
        {
            var command = String.Format("/conn:\"{0}\"", connectionString);
            return command;
        }
    }
}
