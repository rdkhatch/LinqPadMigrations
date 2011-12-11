using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqPadMigrations
{
    public class BatchMigrationResult
    {
        public BatchMigrationResult(IEnumerable<MigrationResult> results)
        {
            Results = results;
            Success = !results.Any(r => r.Success == false);
        }

        public IEnumerable<MigrationResult> Results { get; private set; }
        public bool Success { get; private set; }
    }

    public class MigrationResult
    {
        public MigrationResult(string scriptFile, bool success, IEnumerable<string> errorMessages, Exception migrationException)
        {
            ErrorMessages = new List<string>();

            ScriptFile = scriptFile;
            Success = success;
            MigrationException = migrationException;

            if (errorMessages != null)
                ErrorMessages.AddRange(errorMessages);
        }

        public bool Success { get; private set; }
        public string ScriptFile { get; private set; }
        public List<string> ErrorMessages { get; private set; }
        public Exception MigrationException { get; private set; }
    }
}
