using System.Collections.Generic;
using System.Reflection;

namespace LinqPadMigrations.Migrators
{
    public interface IScriptExecutor
    {
        MigrationResult Execute(Assembly generatedAssembly, string scriptFileName);
    }
}
