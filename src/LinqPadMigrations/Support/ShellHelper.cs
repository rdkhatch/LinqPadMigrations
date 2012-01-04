
using System.Diagnostics;
namespace LinqPadMigrations.Support
{
    internal static class ShellHelper
    {
        public static string RunShellCommand(string command, string args = null, string workingDirectory = null)
        {
            // http://stackoverflow.com/questions/206323/how-to-execute-command-line-in-c-get-std-out-results

            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Arguments = args;
            p.StartInfo.FileName = command;
            p.StartInfo.WorkingDirectory = workingDirectory;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output;
        }
    }
}
