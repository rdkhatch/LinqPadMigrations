
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CSharp;

namespace LinqPadMigrations.ScriptCompiler
{
    public class LinqPadCSharpCreator
    {
        public readonly static string GeneratedClass = "Generated.MyProgram";
        public readonly static string GeneratedMethod = "Main";
        public readonly static string QuotedStringReplacePrefix = "_____ReplacedString____";
        public readonly static string ContextName = "____context";

        public LinqPadQuery Parse(string scriptFilePath)
        {
            var scriptFileContents = File.ReadAllLines(scriptFilePath);
            var query = ParseXML(scriptFileContents);
            return query;
        }

        public ExecutableScript MakeScriptExecutable(LinqPadQuery script, string generatedDataContextCode)
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
            var scriptMethodCode = ConvertScriptIntoMethodByQueryKind(script, generatedDataContextCode);

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

            // Add Code and DataContext source to executable script
            program.SourceCodePieces.Add(code);
            program.SourceCodePieces.Add(generatedDataContextCode);

            return program;
        }

        private static string ConvertScriptIntoMethodByQueryKind(LinqPadQuery script, string generatedDataContextCode)
        {
            if (script.Kind == LinqPadQueryKind.Program)
            {
                // Make Programs Public
                return string.Format("public {0}", script.ScriptCode);
            }
            else if (script.Kind == LinqPadQueryKind.Expression)
            {
                // Add Context to Expression
                var expressionWithContext = AddContextToExpression(script.ScriptCode, ContextName, generatedDataContextCode);

                // Hookup Expressions to DataContext
                var code = String.Format(@"

                                public object Main()
                                {{

                                    // Create new DataContext
                                    var {0} = new {1}();

                                    object _q_ = {2};

                                    // Execute Query, if a deferred enumerable
                                    if (_q_ != null && _q_ is IEnumerable)
                                        _q_ = (_q_ as IEnumerable).Cast<object>().ToList();

                                    return _q_;
                                }}

                            ", ContextName, LinqToSQLDataContextGenerator.DataContextName, expressionWithContext);

                return code;
            }
            else
                return string.Format(@"public void Main() {{ {0} }}", script.ScriptCode);
        }

        private static List<string> GetTableNamesFromDataContextAssembly(Assembly assembly)
        {
            // Reflect over table names
            var tableAttributeType = typeof(System.Data.Linq.Mapping.TableAttribute);

            var tableTypes = from type in assembly.GetTypes()
                             where type.GetCustomAttributes(tableAttributeType, true).Any()
                             select type;

            // List of the table names
            var tableNames = tableTypes.Select(type => type.Name).ToList();
            return tableNames;
        }

        private static string ReplaceQuotedStringsWithTokens(string expression, out Dictionary<string, string> tokens)
        {
            // Match Quoted Strings (http://stackoverflow.com/questions/2148587/regex-quoted-string-with-escaped-quotes-in-c-sharp)
            Regex stringRegex = new Regex(@"""[^""\\]*(?:\\.[^""\\]*)*""");

            var myTokens = new Dictionary<string, string>();
            int stringReplacementCounter = 0;

            MatchEvaluator replaceStringWithToken = (match) =>
            {
                stringReplacementCounter++;

                var token = QuotedStringReplacePrefix + stringReplacementCounter;
                myTokens.Add(token, match.Value);

                return token;
            };

            // Replace quoted strings with Tokens
            var result = stringRegex.Replace(expression, replaceStringWithToken);
            tokens = myTokens;

            return result;
        }

        private static string AddContextToExpression(string originalExpression, string contextName, string generatedDataContextCode)
        {
            // Should Match
            //      from _var_ in Customers
            //      let _var_ = Customers.Where(.....)

            // Should NOT Match
            //      select new { Customers = c }
            //      select new { CustomersSuffix = c }
            //      where "Customers".Length == 9

            var expression = originalExpression;

            // Compile DataContext
            Assembly assembly = new CodeDomScriptCompiler(new CSharpCodeProvider()).Compile(StandardAssemblies, new[] { generatedDataContextCode });

            // Get Table Names
            var tableNames = GetTableNamesFromDataContextAssembly(assembly);

            // Replace quoted strings with Tokens
            Dictionary<string, string> tokens = null;
            expression = ReplaceQuotedStringsWithTokens(expression, out tokens);

            // Replace TableNames with ___context.[TableName]
            foreach (var tableName in tableNames)
            {
                // Find tableName - which is preceded by a whitespace, and either followed by a period (.) OR (followed by spaces AND NOT followed by an equal sign)
                // Avoiding equal sign avoids assignments like LET and SELECT NEW { tableName = .... }
                string findExpr = string.Format(@"(?<=\s)({0})(?=(\.|\s+[^\s=]))", tableName);
                // Replace with context prefix
                string replaceExpr = string.Format(" {0}.$1", contextName);
                expression = Regex.Replace(expression, findExpr, replaceExpr);
            }

            // Replace Tokens with their original string values
            Regex tokenRegex = new Regex(QuotedStringReplacePrefix + @"[0-9]*");
            expression = tokenRegex.Replace(expression, (match) => tokens[match.Value]);

            return expression;
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
