using DbmlSchema;

namespace LinqPadMigrations.DBML.Manipulators
{
    public class PreserveColumnNameCasing : DbmlManipulatorBase
    {
        protected override Column OnManipulateColumn(Column column)
        {
            column.Member = column.Name;
            return column;
        }
    }

}
