using System;

namespace LinqPadMigrations.Support
{
    public class MigrationException : Exception
    {
        public MigrationResult Result { get; private set; }

        public MigrationException(MigrationResult result)
            : base(GetMessage(result), result.MigrationException)
        {
            Result = result;
        }

        private static string GetMessage(MigrationResult result)
        {
            var messages = String.Join(", ", result.ErrorMessages);
            return String.Format("Script '{0}' resulted in an exception. {1}", result.ScriptFile, messages);
        }
    }
}
