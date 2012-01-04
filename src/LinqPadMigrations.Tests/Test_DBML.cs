
using System;
using System.IO;
using System.Linq;
using DbmlSchema;
using LinqPadMigrations.DBML;
using LinqPadMigrations.DBML.Manipulators;
using NUnit.Framework;

namespace LinqPadMigrations.Tests
{
    [TestFixture]
    public class Test_DBML
    {
        [Test]
        public void should_deserialize_dbml()
        {
            var db = GetNorthwindDatabase();
        }

        [Test]
        public void should_serialize_dbml_into_UTF8()
        {
            var db = GetNorthwindDatabase();
            var xml = DbmlHelper.ToXml(db);

            Assert.IsTrue(xml.ToUpper().Contains("ENCODING=\"UTF-8\""));
        }

        [Test]
        public void should_serialize_dbml_which_can_be_deserialized()
        {
            var db = GetNorthwindDatabase();
            var xml = DbmlHelper.ToXml(db);
            var db2 = DbmlHelper.FromXml(xml);

            Assert.AreEqual(db.Table.Count(), db2.Table.Count());
        }

        [Test]
        public void should_support_preserving_column_name_casing_for_property_names()
        {
            var ColumnName = "categoryName";

            Column column = null;

            TestManipulator(TestAssets.DBML_LowerCaseColumnName, new PreserveColumnNameCasing(),
                db =>
                {
                    column = db.Table[0].Type.Items.Cast<Column>().Single();

                    // Verify our test has a lower-case column name
                    Assert.AreEqual(ColumnName, column.Name);

                    // Clear member to prepare test
                    column.Member = string.Empty;
                },

                db2 =>
                {
                    // Assert MemberName now uses the same capitalization
                    Assert.AreEqual(ColumnName, column.Name);
                    Assert.AreEqual(ColumnName, column.Member);
                });
        }

        [Test]
        public void should_support_capitalizing_first_letter_of_column_name()
        {
            var ColumnName = "categoryName";

            Column column = null;

            TestManipulator(TestAssets.DBML_LowerCaseColumnName, new CapitalizePropertyNames(),
                db =>
                {
                    column = db.Table[0].Type.Items.Cast<Column>().Single();

                    // Verify our test has a lower-case column name
                    Assert.AreEqual(ColumnName, column.Name);

                    // Clear member to prepare test
                    column.Member = string.Empty;
                },

                db2 =>
                {
                    // Assert MemberName now has a capital first letter
                    Assert.AreEqual(ColumnName, column.Name);
                    Assert.AreEqual("CategoryName", column.Member);
                });
        }

        private static void TestManipulator(string testAsset, DbmlManipulatorBase dbmlManipulator, Action<Database> testBefore, Action<Database> testAfter)
        {
            var xml = File.ReadAllText(testAsset);
            var db = DbmlHelper.FromXml(xml);

            testBefore.Invoke(db);

            var db2 = dbmlManipulator.Manipulate(db);

            testAfter.Invoke(db2);
        }

        private static Database GetNorthwindDatabase()
        {
            var xml = File.ReadAllText(TestAssets.DBML_Northwind);
            var db = DbmlHelper.FromXml(xml);
            return db;
        }
    }
}
