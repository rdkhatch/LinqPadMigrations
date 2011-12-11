
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
namespace LinqPadMigrations.ScriptCompiler
{
    public class LinqToSQLDataContextGenerator
    {

        public readonly static string DataContextName = "TypedDataContext";
        public readonly static string DataContextNamespace = "MyDataContext";

        public string GenerateDataContext_AndGetCSharpCode(string connectionString, string sqlMetalConnectionStringArg)
        {
            // Build SQL Metal Command
            var sqlmetalPath = ConfigurationManager.AppSettings["sqlmetalPath"];
            var sqlmetalCommand = String.Format("\"{0}\" /code /context:{1} /namespace:{2} {3}", sqlmetalPath, DataContextName, DataContextNamespace, sqlMetalConnectionStringArg);

            // Generate Linq-To-SQL Data Context
            var workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var generatedCSharpDataContext = RunShellCommand(sqlmetalCommand, null, workingDirectory);

            // Strip Microsoft Header out of code
            generatedCSharpDataContext = StripHeader(generatedCSharpDataContext);

            // Embed Connection String into Data Context source code
            generatedCSharpDataContext = EmbedConnectionStringIntoDataContextCode(generatedCSharpDataContext, connectionString);

            return generatedCSharpDataContext;
        }

        private static string StripHeader(string code)
        {
            // This header must be commented out... because it causes compile errors

            //Microsoft (R) Database Mapping Generator 2008 version 1.00.30729
            //for Microsoft (R) .NET Framework version 3.5
            //Copyright (C) Microsoft Corporation. All rights reserved.

            //#pragma warning disable 1591

            var lines = code.Split("\r\n".ToCharArray());

            bool completed = false;
            int lineNumber = 0;
            while (completed == false)
            {
                if (lines[lineNumber].Contains("#pragma"))
                    completed = true;
                else
                    // Add comment to beginning of header
                    lines[lineNumber] = "//" + lines[lineNumber];
                lineNumber++;
            }

            var modifiedCode = string.Join("\r\n", lines);
            return modifiedCode;
        }

        private static string EmbedConnectionStringIntoDataContextCode(string code, string connectionString)
        {
            // Embed connection string into LinqToSQL DataContext (otherwise, we can't use it!)
            var findConstructor = "public TypedDataContext(string connection)";
            var replaceConstructor = string.Format(@"

                // Default Constructor - Injected with our Connection String
		        public TypedDataContext() : this(@""{0}"")
		        {{
		        }}
	
                // Keep constructor
		        {1}
            ", connectionString, findConstructor);

            var modifiedCode = code.Replace(findConstructor, replaceConstructor);

            return modifiedCode;
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
