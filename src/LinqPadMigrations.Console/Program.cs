
using System;
using System.IO;
using System.Linq;
using LinqPadMigrations.DBML.Manipulators;
using LinqPadMigrations.Support;

namespace LinqPadMigrations.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new GeniusCode.Components.Console.Support.RequiredValuesOptionSet();
            var connectionString = options.AddRequiredVariable<string>("conn", "Connection string to use for Linq To SQL.");
            var relativeDirectory = options.AddRequiredVariable<string>("folder", "Folder that contains .SQL and .LINQ files for database migration.");
            var capitalizePropertyNames = options.AddRequiredVariable<bool>("capitalize", "Capitalize Property Names, otherwise leave property same as column name.");

            var console = new GeniusCode.Components.Console.ConsoleManager(options, "LinqPadMigrations");
            bool success = console.PerformCanProceed(System.Console.Out, args);

            if (success)
            {
                IDbmlManipulator dbmlManipulator = null;

                // DBML Manipulator to use.
                // TODO: Add command line option to not need any manipulator (by passing null to LinqPadMigrator)
                if (capitalizePropertyNames.Value)
                    dbmlManipulator = new CapitalizePropertyNames();
                else
                    dbmlManipulator = new PreserveColumnNameCasing();

                var migrator = new LinqPadMigrator(dbmlManipulator);

                var directory = relativeDirectory.Value;
                if (Path.IsPathRooted(directory) == false)
                {
                    // Make absolute using command prompt's working directory
                    var workingDirectory = Directory.GetCurrentDirectory(); //Environment.CurrentDirectory
                    directory = Path.Combine(workingDirectory, relativeDirectory.Value);
                }

                System.Console.Out.WriteLine("-----------------------------");
                System.Console.Out.WriteLine("Folder: " + directory);
                System.Console.Out.WriteLine("Connn: " + connectionString.Value);
                System.Console.Out.WriteLine("-----------------------------");

                if (!Directory.Exists(directory))
                {
                    System.Console.Out.WriteLine("Directory for migration files does not exist.");
                }
                else
                {
                    var filePaths = (from filePath in Directory.GetFiles(directory)
                                     where filePath.ToUpper().EndsWith(".SQL") || filePath.ToUpper().EndsWith(".LINQ")
                                     orderby filePath ascending
                                     select filePath).ToList();

                    if (filePaths.Count() > 0)
                    {
                        System.Console.Out.WriteLine("Executing Migrations...");

                        Action<string> beforeScript = (scriptFilePath) =>
                            {
                                var scriptFileName = Path.GetFileName(scriptFilePath);
                                System.Console.Out.Write(scriptFileName + " ... ");
                            };

                        Action<MigrationResult> afterScript = (result) =>
                            {
                                if (result.Success)
                                    System.Console.Out.WriteLine("Completed Successfully.");
                                else
                                    System.Console.Out.WriteLine("Completed with Errors.");
                            };

                        try
                        {
                            var result = migrator.ExecuteMigrations(connectionString.Value, filePaths, beforeScript, afterScript);
                            var fileCount = result.Results.Count();

                            if (filePaths.Count() == fileCount)
                                System.Console.Out.WriteLine(string.Format("Migration of {0} files Succeeded.", fileCount));
                            else
                                System.Console.Out.WriteLine(string.Format("Migration completed, but the number of files processed ({0}) did not match the number of .SQL / .LINQ files in the specified directory ({1})", fileCount, filePaths.Count()));

                        }
                        catch (Exception ex)
                        {
                            System.Console.Out.WriteLine("Migration Failed - An exception occurred. {0}; {1}", ex.Message, ex.ToString());
                        }
                    }
                    else
                    {
                        System.Console.Out.WriteLine("Migration Failed - No .SQL or .LINQ files were found in specified directory.");
                    }
                }
            }

        }
    }
}
