using System;

namespace LinqPadMigrations.Migrators
{
    public class SQLCompactLinqMigrator : LinqMigratorBase
    {
        public override bool CanExecute(string connectionString, string scriptFilePath)
        {
            return connectionString.ToUpper().Contains(".SDF") && base.CanExecute(connectionString, scriptFilePath);
        }

        protected override string BuildSQLMetalCommand(string sqlmetalPath, string connectionString)
        {
            var extractedSQLCompacatFilePathFromConnectionString = connectionString.Replace("Data Source", string.Empty).Replace("=", string.Empty).Replace(";", string.Empty);
            var command = String.Format("\"{0}\" /code /context:TypedDataContext /namespace:MyDataContext \"{1}\"", sqlmetalPath, extractedSQLCompacatFilePathFromConnectionString);
            return command;
        }
    }
}
