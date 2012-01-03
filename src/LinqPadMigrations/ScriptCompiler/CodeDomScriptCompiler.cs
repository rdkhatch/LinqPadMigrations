using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LinqPadMigrations.Migrators;

namespace LinqPadMigrations.ScriptCompiler
{
    internal class CodeDomScriptCompiler : IScriptCompiler
    {
        CodeDomProvider compiler;

        public CodeDomScriptCompiler(CodeDomProvider codeCompiler)
        {
            compiler = codeCompiler;
        }

        public Assembly Compile(IEnumerable<string> assemblyReferences, IEnumerable<string> codePieces)
        {
            bool success = true;

            var parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.AddRange(assemblyReferences.ToArray());
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.OutputAssembly = "TempAssembly-" + Guid.NewGuid().ToString().Replace("-", "");
            parameters.TreatWarningsAsErrors = false;

            // Compile
            var compiledResults = compiler.CompileAssemblyFromSource(parameters, codePieces.ToArray());
            var errors = compiledResults.Errors.Cast<CompilerError>().Where(error => error.IsWarning == false);
            if (errors.Any())
            {
                success = false;
                var errorText = String.Join("; ", errors.Select(error => error.ErrorText + " Line: " + error.Line));
                throw new Exception("Failed to compile script. " + errorText);
            }

            return compiledResults.CompiledAssembly;
        }

    }
}
