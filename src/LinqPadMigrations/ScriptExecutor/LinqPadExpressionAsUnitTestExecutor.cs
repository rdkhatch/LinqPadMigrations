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

            int? enumerableItemCount = GetEnumerableItemCount(result);

            // Expect Null or True or IEnumerable.Count == 0 results
            bool success = result == null || Equals(result, true) || (enumerableItemCount != null && enumerableItemCount == 0);

            string[] errorMessages = null;
            if (success)
            {
                return new MigrationResult(scriptFileName, true, errorMessages, null);
            }
            else
            {
                errorMessages = new[] { "Linq Unit Test returned a failing value. Expected Null, True, or a collection of Zero items. Instead, collection count is: " + enumerableItemCount + ". Actual value: " + result.ToString() };
                var migrationResult = new MigrationResult(scriptFileName, success, errorMessages, null);
                throw new MigrationException(migrationResult);
            }
        }

        private static int? GetEnumerableItemCount(object enumerable)
        {
            int? enumerableItemCount = null;

            if (enumerable != null && enumerable is IEnumerable)
                enumerableItemCount = (enumerable as IEnumerable).Cast<object>().Count();

            return enumerableItemCount;
        }
    }
}
