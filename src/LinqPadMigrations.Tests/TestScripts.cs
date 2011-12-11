
namespace LinqPadMigrations.Tests
{
    public static class TestScripts
    {
        public static readonly string SQL_RenameCustomerNamesToBrody = Wrap("RenameCustomerNamesToBrody.sql");
        public static readonly string SQL_BadSyntax = Wrap("BadSQL.sql");
        public static readonly string LINQ_UpdateCustomerBOLIDTitlefromOwnertoManager = Wrap("Update Customer BOLID Title from Owner to Manager.linq");
        public static readonly string LinqPadQueryExpression_Passing = Wrap("LinqPad Query Expression.Passing - Returns Zero Results.linq");
        public static readonly string LinqPadQueryExpression_Failing = Wrap("LinqPad Query Expression.Failing - Returns Results.linq");

        private static string Wrap(string filename)
        {
            return @"..\..\TestScripts\" + filename;
        }
    }
}
