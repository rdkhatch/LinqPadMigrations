using System;
using LinqPadMigrations.Migrators;
using LinqPadMigrations.ScriptCompiler;

namespace LinqPadMigrations.ScriptExecutor
{
    internal class LinqPadProgramExecutor : IScriptExecutor
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
}
