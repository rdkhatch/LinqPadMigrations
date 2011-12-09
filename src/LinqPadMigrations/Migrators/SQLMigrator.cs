
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using LinqPadMigrations.Support;
namespace LinqPadMigrations.Migrators
{

    // Transaction Documentation: http://msdn.microsoft.com/en-us/library/86773566.aspx
    internal class SQLMigrator : IMigrator
    {
        internal SQLMigrator(Func<string, DbConnection> sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        Func<string, DbConnection> _sqlConnectionFactory;

        //private static readonly string TransactionName = "LinqPad SQLMigrator Transaction";

        public MigrationResult ExecuteMigration(string connectionString, string scriptFilePath)
        {
            // Read SQL from Script
            var scriptSQL = File.ReadAllText(scriptFilePath);

            bool success = true;
            Exception exception = null;
            var errorMessages = new List<string>();

            using (DbConnection connection = _sqlConnectionFactory.Invoke(connectionString))
            {
                connection.Open();

                var transaction = connection.BeginTransaction();

                try
                {
                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    var command = connection.CreateCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;

                    // Execute SQL
                    command.CommandText = scriptSQL;
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception commitException)
                {
                    // No longer successful
                    success = false;
                    errorMessages.Add("Failed to Commit Transaction:" + commitException);
                    exception = commitException;

                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception rollbackException)
                    {
                        // Rollback failed
                        errorMessages.Add("Failed to Rollback Transaction:" + rollbackException);
                    }
                }

                connection.Close();
            }

            var result = new MigrationResult(scriptFilePath, success, errorMessages, exception);

            if (!success)
                throw new MigrationException(result);

            return result;
        }

        public bool CanExecute(string connectionString, string scriptFilePath)
        {
            return scriptFilePath.ToUpper().EndsWith(".SQL");
        }
    }
}
