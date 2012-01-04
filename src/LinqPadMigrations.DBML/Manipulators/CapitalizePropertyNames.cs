using System;
using DbmlSchema;

namespace LinqPadMigrations.DBML.Manipulators
{
    public class CapitalizePropertyNames : DbmlManipulatorBase
    {
        protected override Column OnManipulateColumn(Column column)
        {
            var columnName = column.Name;
            column.Member = Char.ToUpper(columnName[0]) + columnName.Substring(1);

            return column;
        }
    }

}
