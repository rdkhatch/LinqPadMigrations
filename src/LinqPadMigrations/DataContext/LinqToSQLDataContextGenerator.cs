
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using LinqPadMigrations.Support;

namespace LinqPadMigrations.ScriptCompiler
{
    // SQL Metal Documentation: http://msdn.microsoft.com/en-us/library/bb386987.aspx
    public class LinqToSQLDataContextGenerator
    {
        // 1.) Connection String > SQLMetal > DBML XML File
        // 2.) Manipulate DBML XML File
        // 3.) DBML XML File > SQLMetal > DataContext SourceCode

        public readonly static string DataContextName = "TypedDataContext";
        public readonly static string DataContextNamespace = "MyDataContext";
        private readonly static Func<string> getSQLMetalExe = () => ConfigurationManager.AppSettings["sqlmetalPath"];

        public string GenerateDataContext_AndGetCSharpCode(string connectionString, string sqlMetalConnectionStringArg, IDbmlManipulator dbmlManipulator)
        {
            // Generate DBML File
            var originalDBMLContents = GenerateDBMLFileContents(sqlMetalConnectionStringArg);

            // Manipulate DBML File (Capitalization, CamelCase, etc.)
            var dbmlContents = originalDBMLContents;
            if (dbmlManipulator != null)
                dbmlContents = dbmlManipulator.ManipulateDBML(originalDBMLContents);

            // Store Temporary DBML File
            var dbmlFilePath = FileHelper.WriteTextToFileAndGetFilePath(Guid.NewGuid() + ".xml", dbmlContents);

            // Generate DataContext Source Code from DBML File
            var dataContextSource = GenerateCSharpDataContextFromDBMLFile(dbmlFilePath, connectionString);

            return dataContextSource;
        }

        #region Generate DBML File

        private static string GenerateDBMLFileContents(string sqlMetalConnectionStringArg)
        {
            // Build SQL Metal Command
            var sqlmetalCommand = String.Format("\"{0}\" /dbml /context:{1} /namespace:{2} {3}", getSQLMetalExe(), DataContextName, DataContextNamespace, sqlMetalConnectionStringArg);

            // Run SqlMetal: Generate Linq-To-SQL Data Context DBML XML File
            var workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var generatedDBML = ShellHelper.RunShellCommand(sqlmetalCommand, null, workingDirectory);

            var cleanDBML = StripXmlHeader(generatedDBML);

            return cleanDBML;
        }

        private static string StripXmlHeader(string xml)
        {
            // This header must be removed... because it causes XML serialization errors

            //Microsoft (R) Database Mapping Generator 2008 version 1.00.30729
            //for Microsoft (R) .NET Framework version 3.5
            //Copyright (C) Microsoft Corporation. All rights reserved.

            //<?xml ...

            var lines = xml.Split("\r\n".ToCharArray());

            bool completed = false;
            int lineNumber = -1;
            while (completed == false)
            {
                lineNumber++;
                if (lines[lineNumber].Contains("<?"))
                    completed = true;
                else
                    // Add comment to beginning of header
                    lines[lineNumber] = "//" + lines[lineNumber];
            }

            var fixedLines = lines.ToList();
            fixedLines.RemoveRange(0, lineNumber);

            var modifiedCode = string.Join("\r\n", fixedLines);
            return modifiedCode;
        }


        #endregion

        #region Generate Data Context Source Code

        private static string GenerateCSharpDataContextFromDBMLFile(string dbmlFilePath, string connectionString)
        {
            // Build SQL Metal Command
            var sqlmetalCommand = String.Format("\"{0}\" /code /language:csharp \"{1}\"", getSQLMetalExe(), dbmlFilePath);

            // Run SqlMetal: Generate Linq-To-SQL Data Context
            var workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var originalDataContextSource = ShellHelper.RunShellCommand(sqlmetalCommand, null, workingDirectory);

            // Strip Microsoft Header out of code
            var dataContextSource = StripCodeHeader(originalDataContextSource);

            // Embed Connection String into Data Context source code
            dataContextSource = EmbedConnectionStringIntoDataContextCode(dataContextSource, connectionString);

            return dataContextSource;
        }

        private static string StripCodeHeader(string code)
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

        #endregion

    }
}
