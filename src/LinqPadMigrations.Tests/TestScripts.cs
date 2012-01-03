
namespace LinqPadMigrations.Tests
{
    public static class TestScripts
    {
        public static readonly string SQL_RenameCustomerNamesToBrody = Wrap("RenameCustomerNamesToBrody.sql");
        public static readonly string SQL_BadSyntax = Wrap("BadSQL.sql");
        public static readonly string SQL_BatchCommandsUsingGO = Wrap("BatchCommandsUsingGO.sql");

        public static readonly string LINQ_UpdateCustomerBOLIDTitlefromOwnertoManager = Wrap("Update Customer BOLID Title from Owner to Manager.linq");
        public static readonly string LinqPadQueryExpression_Passing = Wrap("LinqPad Query Expression.Passing - Returns Zero Results.linq");
        public static readonly string LinqPadQueryExpression_Failing_Returns_Collection = Wrap("LinqPad Query Expression.Failing - Returns Collection.linq");
        public static readonly string LinqPadQueryExpression_ContextReplace_UsingSelectNewTableName = Wrap("LinqPad Query Expression.ContextReplace.Using Select New TableName.linq");
        public static readonly string LinqPadQueryExpression_ContextReplace_UsingCaseSensitiveTableName = Wrap("LinqPad Query Expression.ContextReplace.Using Case Sensitive TableName.linq");
        public static readonly string LinqPadQueryExpression_ContextReplace_TableNameWithinString = Wrap("LinqPad Query Expression.ContextReplace.TableName within String.linq");
        public static readonly string LinqPadQueryExpression_Failing_Returns_SingleValue = Wrap("LinqPad Query Expression.Failing - Returns Single Value.linq");

        private static string Wrap(string filename)
        {
            return @"..\..\TestScripts\" + filename;
        }
    }
}
