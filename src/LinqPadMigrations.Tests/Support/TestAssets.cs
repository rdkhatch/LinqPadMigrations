
namespace LinqPadMigrations.Tests
{
    public static class TestAssets
    {
        public static readonly string SQL_RenameCustomerNamesToBrody = Asset("RenameCustomerNamesToBrody.sql");
        public static readonly string SQL_BadSyntax = Asset("BadSQL.sql");
        public static readonly string SQL_BatchCommandsUsingGO = Asset("BatchCommandsUsingGO.sql");

        public static readonly string LINQ_UpdateCustomerBOLIDTitlefromOwnertoManager = Asset("Update Customer BOLID Title from Owner to Manager.linq");

        public static readonly string LinqPadQueryExpression_Passing = Asset("LinqPad Query Expression.Passing - Returns Zero Results.linq");
        public static readonly string LinqPadQueryExpression_Failing_Returns_Collection = Asset("LinqPad Query Expression.Failing - Returns Collection.linq");
        public static readonly string LinqPadQueryExpression_ContextReplace_UsingSelectNewTableName = Asset("LinqPad Query Expression.ContextReplace.Using Select New TableName.linq");
        public static readonly string LinqPadQueryExpression_ContextReplace_UsingCaseSensitiveTableName = Asset("LinqPad Query Expression.ContextReplace.Using Case Sensitive TableName.linq");
        public static readonly string LinqPadQueryExpression_ContextReplace_TableNameWithinString = Asset("LinqPad Query Expression.ContextReplace.TableName within String.linq");
        public static readonly string LinqPadQueryExpression_Failing_Returns_SingleValue = Asset("LinqPad Query Expression.Failing - Returns Single Value.linq");

        public static readonly string DBML_Northwind = AppData("Northwind.dbml");
        public static readonly string DBML_LowerCaseColumnName = Asset("Dbml.LowerCaseColumnName.dbml");


        private static string Asset(string filename)
        {
            return @"..\..\TestAssets\" + filename;
        }

        private static string AppData(string filename)
        {
            return @"..\..\App_Data\" + filename;
        }
    }
}
