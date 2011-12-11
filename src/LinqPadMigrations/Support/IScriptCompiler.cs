using System.Collections.Generic;
using System.Reflection;

namespace LinqPadMigrations.Migrators
{
    public interface IScriptCompiler
    {
        Assembly Compile(IEnumerable<string> assemblyReferences, IEnumerable<string> codePieces);
    }
}