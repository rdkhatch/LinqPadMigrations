
using System;
using System.IO;
using System.Linq;

namespace LinqPadMigrations.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new GeniusCode.Components.Console.Support.RequiredValuesOptionSet();
            var connectionString = options.AddRequiredVariable<string>("conn", "Connection string to use for Linq To SQL.");
            var directory = options.AddRequiredVariable<string>("folder", "Folder that contains .SQL and .LINQ files for database migration.");

            var console = new GeniusCode.Components.Console.ConsoleManager(options, "LinqPadMigrations");
            bool success = console.PerformCanProceed(System.Console.Out, args);

            if (success)
            {
                var migrator = new LinqPadMigrator();

                if (!Directory.Exists(directory.Value))
                {
                    System.Console.Out.WriteLine("Directory for migration files does not exist.");
                }
                else
                {
                    var filePaths = from filePath in Directory.GetFiles(directory.Value)
                                    where filePath.ToUpper().EndsWith("*.SQL") || filePath.ToUpper().EndsWith("*.LINQ")
                                    select filePath;

                    if (filePaths.Count() > 0)
                    {
                        try
                        {
                            var result = migrator.ExecuteMigrations(connectionString.Value, filePaths);
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
                    {
                        System.Console.Out.WriteLine("Migration Failed - No .SQL or .LINQ files were found in specified directory.");
                    }
                }
            }

        }
    }
}
