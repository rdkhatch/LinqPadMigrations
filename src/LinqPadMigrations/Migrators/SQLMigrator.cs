
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using LinqPadMigrations.Support;
namespace LinqPadMigrations.Migrators
{

    // Transaction Documentation: http://msdn.microsoft.com/en-us/library/86773566.aspx
    // GO Command Documentation: http://social.msdn.microsoft.com/Forums/hr/sqldataaccess/thread/f8e3fa51-55af-4a03-a248-425a4e2aa82a
    //                           http://msdn.microsoft.com/en-us/library/ms188037.aspx

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
            string[] scriptSQLLines = File.ReadAllLines(scriptFilePath);

            bool success = true;
            Exception exception = null;
            var errorMessages = new List<string>();

            using (DbConnection connection = _sqlConnectionFactory.Invoke(connectionString))
            {
                connection.Open();

                var transaction = connection.BeginTransaction();

                Action<string> sendCommand = (commandText) =>
                {
                    // Avoid empty commands
                    if (string.IsNullOrWhiteSpace(commandText))
                        return;

                    // Must assign both transaction object and connection
                    // to Command object for a pending local transaction
                    DbCommand command = connection.CreateCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;

                    // Execute SQL
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                };

                try
                {
                    string currentBatchText = string.Empty;

                    foreach (var line in scriptSQLLines)
                    {
                        bool isGoCommand = System.Text.RegularExpressions.Regex.IsMatch(line, @"^\s*GO\s*$");

                        if (isGoCommand)
                        {
                            // Send Batch
                            sendCommand.Invoke(currentBatchText);
                            // Start new Batch
                            currentBatchText = string.Empty;
                        }
                        else
                            // Append to current Batch
                            currentBatchText += "\n" + line;
                    }

                    // Send whatever is left over in our batch text since last GO command
                    sendCommand(currentBatchText);

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
                finally
                {
                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
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
