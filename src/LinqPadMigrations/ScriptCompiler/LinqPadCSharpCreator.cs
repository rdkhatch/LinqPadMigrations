
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace LinqPadMigrations.ScriptCompiler
{
    public class LinqPadCSharpCreator
    {
        public readonly static string GeneratedClass = "Generated.MyProgram";
        public readonly static string GeneratedMethod = "Main";

        public LinqPadQuery Parse(string scriptFilePath)
        {
            var scriptFileContents = File.ReadAllLines(scriptFilePath);
            var query = ParseXML(scriptFileContents);
            return query;
        }

        public ExecutableScript MakeScriptExecutable(LinqPadQuery script)
        {
            var program = new ExecutableScript();

            // Set Assemblies
            // GAC Reference parsing from: https://github.com/mcintyre321/LINQPadRunner/blob/master/LPRun/Program.cs
            var assemblyGroups = new[]
                                 {
                                     script.GACReferences.Select(s => s.Substring(0, s.IndexOf(",")) + ".dll"),
                                     script.RelativeReferences,
                                     script.OtherReferences,
                                     StandardAssemblies,
                                 };
            program.AssemblyReferences.AddRange(assemblyGroups.SelectMany(group => group));

            // Prepare to Include Namespaces
            var namespaceGroups = new[]
                {
                    script.Namespaces,
                    StandardNamespaces
                };
            var namespaces = String.Join("\n", from namespaceGroup in namespaceGroups
                                               from nspace in namespaceGroup
                                               select string.Format("using {0};", nspace));

            // Wrap Code in Main() method (if needed)
            var scriptMethodCode = ConvertScriptIntoMethodByQueryKind(script);

            var code = string.Format(@"

                // Namespaces
                {0}
            
                namespace Generated
                {{
                    public class MyProgram
                    {{
                        {1}
                    }}
                }}
            
            ", namespaces, scriptMethodCode);

            // Add Code to Program
            program.SourceCodePieces.Add(code);

            return program;
        }

        private static string ConvertScriptIntoMethodByQueryKind(LinqPadQuery script)
        {
            if (script.Kind == LinqPadQueryKind.Program)
            {
                // Make Programs Public
                return string.Format("public {0}", script.ScriptCode);
            }
            else if (script.Kind == LinqPadQueryKind.Expression)
            {
                string contextName = "____context";

                // Add Context to Expression
                var expressionWithContext = AddContextToExpression(script.ScriptCode, contextName);

                // Hookup Expressions to DataContext
                var code = String.Format(@"

                                public object Main()
                                {{

                                    // Create new DataContext
                                    var {0} = new {1}();

                                    var q = {2};

                                    // Execute Query
                                    return q.ToList();
                                }}

                            ", contextName, LinqToSQLDataContextGenerator.DataContextName, expressionWithContext);

                return code;
            }
            else
                return string.Format(@"public void Main() {{ {0} }}", script.ScriptCode);
        }

        private static string AddContextToExpression(string expression, string contextName)
        {
            // Use Cases:
            //      from _var_ in TableName
            //      let _var_ = TableName.Where(.....)

            // NOT SUPPORTED: [would require a C# parser]
            //      from c in Customers
            //      select new { Customers = c }

            // Must have a list of the data tables!
            // Compile DataContext and Reflect over table names

            throw new NotImplementedException();
        }

        /// <summary>
        /// Credit for this parsing routine goes to mcintyre321
        /// https://github.com/mcintyre321/LINQPadRunner/blob/master/LPRun/Program.cs
        /// </summary>
        private static LinqPadQuery ParseXML(string[] scriptFileContents)
        {
            // XML Lines start with <
            // Code lines do not

            var xml = string.Join("\r\n", scriptFileContents.TakeWhile(l => l.Trim().StartsWith("<")));

            var queryElement = XDocument.Parse(xml).Element("Query");
            var query = new LinqPadQuery
            {
                Kind = queryElement.Attribute("Kind").Value,
                Namespaces = queryElement.Elements("Namespace").Select(n => n.Value).ToList(),
                GACReferences = queryElement.Elements("GACReference").Select(n => n.Value).ToList(),
                RelativeReferences =
                    queryElement.Elements("Reference").Where(e => e.Attribute("Relative") != null).
                    Select(n => n.Attribute("Relative").Value).ToList(),
                OtherReferences =
                    queryElement.Elements("Reference").Where(e => e.Attribute("Relative") == null).
                    Select(
                        n =>
                        n.Value.Replace("<RuntimeDirectory>",
                                        RuntimeEnvironment.GetRuntimeDirectory())).ToList(),

            };

            var code = string.Join("\r\n", scriptFileContents.SkipWhile(l => l.Trim().StartsWith("<")));
            query.ScriptCode = code;

            return query;
        }


        private static List<string> StandardNamespaces
        {
            get
            {
                var strings = new List<string>
                {
                    "System",
                    "System.IO",
                    "System.Text",
                    "System.Text.RegularExpressions",
                    "System.Diagnostics",
                    "System.Threading",
                    "System.Reflection",
                    "System.Collections",
                    "System.Collections.Generic",
                    "System.Linq",
                    "System.Linq.Expressions",
                    "System.Data",
                    "System.Data.SqlClient",
                    "System.Data.Linq",
                    "System.Data.Linq.SqlClient",
                    "System.Xml",
                    "System.Xml.Linq",
                    "System.Xml.XPath"
                };
                return strings;
            }
        }

        private static List<string> StandardAssemblies
        {
            get
            {
                var strings = new List<string>
                {
                    "System.dll",
                    "System.Core.dll",
                    "System.Data.dll",
                    "System.Xml.dll",
                    "System.Xml.Linq.dll",
                    "System.Data.Linq.dll",
                    "System.Drawing.dll",
                    "System.Data.DataSetExtensions.dll"
                };

                return strings;
            }
        }

    }


    public class ExecutableScript
    {
        public readonly List<string> AssemblyReferences = new List<string>();
        public readonly List<string> SourceCodePieces = new List<string>();
    }

    public class LinqPadQuery
    {
        public string ScriptCode { get; set; }
        public string Kind { get; set; }
        public List<string> Namespaces { get; set; }
        public List<string> GACReferences { get; set; }
        public List<string> RelativeReferences { get; set; }
        public List<string> OtherReferences { get; set; }
    }

    public static class LinqPadQueryKind
    {
        public readonly static string Program = "Program";
        public readonly static string Expression = "Expression";
    }
}
