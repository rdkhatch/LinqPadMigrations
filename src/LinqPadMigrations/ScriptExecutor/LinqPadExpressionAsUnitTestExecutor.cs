using System;
using System.Collections;
using System.Linq;
using LinqPadMigrations.Migrators;
using LinqPadMigrations.ScriptCompiler;
using LinqPadMigrations.Support;

namespace LinqPadMigrations.ScriptExecutor
{
    /// <summary>
    /// 
    /// </summary>
    internal class LinqPadExpressionAsUnitTestExecutor : IScriptExecutor
    {
        public MigrationResult Execute(System.Reflection.Assembly generatedAssembly, string scriptFileName)
        {
            var programType = generatedAssembly.GetType(LinqPadCSharpCreator.GeneratedClass);
            var programInstance = Activator.CreateInstance(programType);
            var method = programInstance.GetType().GetMethod(LinqPadCSharpCreator.GeneratedMethod);

            // Execute Script!
            object result = method.Invoke(programInstance, null);

            // Expect Null or True or IEnumerable.Count == 0 results
            bool success = result == null || Equals(result, true) || (result is IEnumerable && (result as IEnumerable).Cast<object>().Count() == 0);

            string[] errorMessages = null;
            if (!success)
            {
                errorMessages = new[] { "Linq Unit Test returned a failing value. Expected Null, True, or a collection of Zero items. Instead, values was: " + result.ToString() };
                var migrationResult = new MigrationResult(scriptFileName, success, errorMessages, null);
                throw new MigrationException(migrationResult);
            }

            return new MigrationResult(scriptFileName, true, errorMessages, null);
        }
    }
}
