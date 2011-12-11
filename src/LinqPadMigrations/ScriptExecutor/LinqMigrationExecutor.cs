using System;
using System.Collections;
using System.Linq;
using LinqPadMigrations.Migrators;
using LinqPadMigrations.ScriptCompiler;

namespace LinqPadMigrations.ScriptExecutor
{
    internal class LinqMigrationExecutor : IScriptExecutor
    {
        public MigrationResult Execute(System.Reflection.Assembly generatedAssembly, string scriptFileName)
        {
            var programType = generatedAssembly.GetType(LinqPadCSharpCreator.GeneratedClass);
            var programInstance = Activator.CreateInstance(programType);
            var method = programInstance.GetType().GetMethod(LinqPadCSharpCreator.GeneratedMethod);

            // Execute Script!
            object result = method.Invoke(programInstance, null);

            bool success = true;
            return new MigrationResult(scriptFileName, success, null, null);
        }
    }

    internal class LinqUnitTestExecutor : IScriptExecutor
    {
        public MigrationResult Execute(System.Reflection.Assembly generatedAssembly, string scriptFileName)
        {
            var programType = generatedAssembly.GetType(LinqPadCSharpCreator.GeneratedClass);
            var programInstance = Activator.CreateInstance(programType);
            var method = programInstance.GetType().GetMethod(LinqPadCSharpCreator.GeneratedMethod);

            // Execute Script!
            object result = method.Invoke(programInstance, null);

            // Expect Null or True or IEnumerable.Count == 0 results
            bool success = result == null || Equals(result, true) || ((result as IEnumerable) != null && (result as IEnumerable).Cast<object>().Count() == 0);

            string[] errorMessages = null;
            if (!success)
                errorMessages = new[] { "Linq Unit Test returned a failing value. Expected Null, True, or a collection of Zero items. Instead, values was: " + result.ToString() };

            return new MigrationResult(scriptFileName, success, errorMessages, null);
        }
    }
}





//// Execute
//try
//{
//    object result = compiledResults.CompiledAssembly.EntryPoint.Invoke(null, args);

//    // TODO: Test for results for Unit Test scripts
//}
//catch (Exception ex)
//{
//    success = false;
//    throw new Exception("Failed to execute compiled script.", ex);
//}

//return success;

//if (success == false)
//    errorMessages.Add("Failed to compile or execute .LINQ program.");

//var result = new MigrationResult(scriptFilePath, success, errorMessages, migrationException);

//if (success == false)
//    throw new MigrationException(result);

//return result;