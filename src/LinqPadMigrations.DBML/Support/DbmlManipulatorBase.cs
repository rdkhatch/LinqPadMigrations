
using System.Linq;
using DbmlSchema;
using LinqPadMigrations.Support;

namespace LinqPadMigrations.DBML
{
    public abstract class DbmlManipulatorBase : IDbmlManipulator
    {
        string IDbmlManipulator.ManipulateDBML(string dbmlXmlContents)
        {
            var databaseIn = DbmlHelper.FromXml(dbmlXmlContents);

            var databaseOut = Manipulate(databaseIn);

            return DbmlHelper.ToXml(databaseOut);
        }

        public Database Manipulate(Database db)
        {
            return OnManipulateDatabase(db);
        }

        protected virtual Database OnManipulateDatabase(Database db)
        {
            foreach (var table in db.Table)
                OnManipulateTable(table);

            return db;
        }

        protected virtual Table OnManipulateTable(Table table)
        {
            var type = table.Type;

            foreach (var column in type.Items.OfType<Column>())
                OnManipulateColumn(column);

            return table;
        }

        protected virtual Column OnManipulateColumn(Column column)
        {
            return column;
        }
    }
}
