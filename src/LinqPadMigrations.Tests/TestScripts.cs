
namespace LinqPadMigrations.Tests
{
    public static class TestScripts
    {
        public static readonly string SQL_RenameCustomerNamesToBrody = Wrap("RenameCustomerNamesToBrody.sql");
        public static readonly string SQL_BadSyntax = Wrap("BadSQL.sql");
        public static readonly string LINQ_UpdateCustomerBOLIDTitlefromOwnertoManager = Wrap("Update Customer BOLID Title from Owner to Manager.linq");

        private static string Wrap(string filename)
        {
            return @"..\..\TestScripts\" + filename;
        }
    }
}
