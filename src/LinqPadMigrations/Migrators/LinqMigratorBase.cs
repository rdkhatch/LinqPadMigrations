using System;
using System.Linq;
using System.Reflection;
using LinqPadMigrations.ScriptCompiler;
using LinqPadMigrations.Support;
using Microsoft.CSharp;
using NLog;

namespace LinqPadMigrations.Migrators
{
    public abstract class LinqMigratorBase : IMigrator
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected readonly IDbmlManipulator dbmlManipulator;

        protected LinqMigratorBase(IDbmlManipulator manipulator)
        {
            dbmlManipulator = manipulator;
        }

        public virtual bool CanExecute(string connectionString, string scriptFilePath)
        {
            return scriptFilePath.ToUpper().EndsWith(".LINQ");
        }

        protected abstract string GetSqlMetalConnectionStringArg(string connectionString);

        public MigrationResult ExecuteMigration(string connectionString, string scriptFilePath)
        {
            // Generate C# DataContext using SqlMetal
            var sqlMetalConnectionStringArg = GetSqlMetalConnectionStringArg(connectionString);
            var generatedCSharpDataContext = new LinqToSQLDataContextGenerator().GenerateDataContext_AndGetCSharpCode(connectionString, sqlMetalConnectionStringArg, dbmlManipulator);

            // Get LinqPad Script
            var linqpadCreator = new LinqPadCSharpCreator();
            var script = linqpadCreator.Parse(scriptFilePath);

            // Add namespace for DataContext
            script.Namespaces.Add(LinqToSQLDataContextGenerator.DataContextNamespace);

            // Convert LinqPad Script to Executable Source Code
            var executableScript = linqpadCreator.MakeScriptExecutable(script, generatedCSharpDataContext);
            Action<Exception> logExceptionWithSourceCode = (ex) =>
                {
                    string primarySourceCode = executableScript.SourceCodePieces.First();
                    logger.ErrorException(scriptFilePath, ex);
                    logger.Error(primarySourceCode);
                };

            // Create Compiler
            var csharpProvider = new CSharpCodeProvider();
            IScriptCompiler compiler = new CodeDomScriptCompiler(csharpProvider);

            Assembly assembly = null;
            try
            {
                // Compile Script > .NET Assembly
                assembly = compiler.Compile(executableScript.AssemblyReferences, executableScript.SourceCodePieces);
            }
            catch (Exception ex)
            {
                var myException = new MigrationException(new MigrationResult(scriptFilePath, false, new[] { "Failed to compile script." }, ex));
                logExceptionWithSourceCode(myException);
                throw myException;
            }

            IScriptExecutor executor = GetScriptExecutor(script);

            try
            {
                // Execute!
                var result = executor.Execute(assembly, scriptFilePath);
                return result;
            }
            catch (MigrationException ex)
            {
                logExceptionWithSourceCode(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                var myException = new MigrationException(new MigrationResult(scriptFilePath, false, new[] { "Failed to execute compiled script." }, ex));
                logExceptionWithSourceCode(myException);
                throw myException;
            }
        }

        private static IScriptExecutor GetScriptExecutor(LinqPadQuery script)
        {
            if (script.Kind == LinqPadQueryKind.Program)
                return new ScriptExecutor.LinqPadProgramExecutor();
            else if (script.Kind == LinqPadQueryKind.Expression)
                return new ScriptExecutor.LinqPadExpressionAsUnitTestExecutor();
            else
                throw new NotSupportedException(string.Format("Query Kind '{0}' is not supported.", script.Kind));
        }

    }
}
