using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using LinqPadMigrations.Support;

namespace LinqPadMigrations.Migrators
{
    public abstract class LinqMigratorBase : IMigrator
    {
        public virtual bool CanExecute(string connectionString, string scriptFilePath)
        {
            return scriptFilePath.ToUpper().EndsWith(".LINQ");
        }

        public MigrationResult ExecuteMigration(string connectionString, string scriptFilePath)
        {
            bool success = true;
            Exception migrationException = null;
            var errorMessages = new List<string>();

            Action executeMe = null;

            // Compile
            try
            {
                executeMe = CompileScript(connectionString, scriptFilePath);
            }
            catch (Exception ex)
            {
                success = false;
                migrationException = ex;
                errorMessages.Add("Error while compiling script.");
            }

            // Execute
            if (success)
            {
                try
                {
                    executeMe.Invoke();

                    //      .TEST.LINQ
                    //          Make sure it returns zero results... or doesn't throw an exception
                }
                catch (Exception ex)
                {
                    success = false;
                    migrationException = ex;
                    errorMessages.Add("Error while executing compiled script.");
                }
            }

            var result = new MigrationResult(scriptFilePath, success, errorMessages, migrationException);

            if (success = false)
                throw new MigrationException(result);

            return result;
        }

        private Action CompileScript(string connectionString, string scriptFilePath)
        {
            var generatedCSharpDataContext = GenerateDataContext_AndGetCSharpCode(connectionString);

            // Call LINQPadRunner here

            return () =>
            {
                // put execute code here
            };
        }

        protected abstract string BuildSQLMetalCommand(string sqlmetalPath, string connectionString);

        public string GenerateDataContext_AndGetCSharpCode(string connectionString)
        {
            var sqlmetalPath = ConfigurationManager.AppSettings["sqlmetalPath"];

            var command = BuildSQLMetalCommand(sqlmetalPath, connectionString);

            var workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var generatedCSharpDataContext = RunShellCommand(command, null, workingDirectory);

            return generatedCSharpDataContext;
        }

        private static string RunShellCommand(string command, string args = null, string workingDirectory = null)
        {
            // http://stackoverflow.com/questions/206323/how-to-execute-command-line-in-c-get-std-out-results

            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Arguments = args;
            p.StartInfo.FileName = command;
            p.StartInfo.WorkingDirectory = workingDirectory;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output;
        }
    }
}
