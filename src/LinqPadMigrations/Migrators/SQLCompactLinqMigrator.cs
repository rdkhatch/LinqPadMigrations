using System;

namespace LinqPadMigrations.Migrators
{
    public class SQLCompactLinqMigrator : LinqMigratorBase
    {
        public override bool CanExecute(string connectionString, string scriptFilePath)
        {
            // SQL Compact connection strings contain:  "Data Source = MyDatabase.sdf"
            return connectionString.ToUpper().Contains(".SDF") && base.CanExecute(connectionString, scriptFilePath);
        }

        protected override string GetSqlMetalConnectionStringArg(string connectionString)
        {
            // SQL Compact connection strings contain:  "Data Source = MyDatabase.sdf"            
            var extractedSQLCompacatFilePathFromConnectionString = connectionString.Replace("Data Source", string.Empty).Replace("=", string.Empty).Replace(";", string.Empty);

            // SqlMetal simply needs the path to the .SDF file (with quotes around it)
            var connectionStringArg = String.Format("\"{0}\"", extractedSQLCompacatFilePathFromConnectionString);
            return connectionStringArg;
        }
    }
}
