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
        public MigrationResult(string scriptFile, bool success, List<string> errorMessages, Exception migrationException)
        {
            ScriptFile = scriptFile;
            Success = success;
            ErrorMessages = errorMessages;
            MigrationException = migrationException;
        }

        public bool Success { get; private set; }
        public string ScriptFile { get; private set; }
        public List<string> ErrorMessages { get; private set; }
        public Exception MigrationException { get; private set; }
    }
}
