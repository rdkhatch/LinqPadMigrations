
using System.IO;
namespace LinqPadMigrations.Support
{
    internal static class FileHelper
    {
        public static string WriteTextToFileAndGetFilePath(string fileName, string fileContents)
        {
            //TODO: Output to Temporary directory
            File.WriteAllText(fileName, fileContents);

            return fileName;
        }
    }
}
