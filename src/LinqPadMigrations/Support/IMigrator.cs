
namespace LinqPadMigrations
{
    internal interface IMigrator
    {
        bool CanExecute(string connectionString, string scriptFilePath);
        MigrationResult ExecuteMigration(string connectionString, string scriptFilePath);
    }
}
