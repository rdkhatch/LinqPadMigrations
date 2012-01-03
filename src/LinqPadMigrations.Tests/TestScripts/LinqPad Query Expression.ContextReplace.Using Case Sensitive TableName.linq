<Query Kind="Expression">
  <Connection>
    <ID>de95ffc9-8038-41d0-8c65-e48422436bb7</ID>
    <Persist>true</Persist>
    <Provider>System.Data.SqlServerCe.3.5</Provider>
    <AttachFileName>C:\Data\Personal\LinqPadMigrations-Github\src\LinqPadMigrations.Tests\App_Data\Northwind.Backup.sdf</AttachFileName>
  </Connection>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
</Query>

from customers in Customers
// Regex should avoid this - [Case-Sensitive].  If it doesn't - we will get a compile exception, because it will think 'customers' = 'context.Customers' which is a collection, not a single object - and thus the .ContactTitle property will not be available on the collection.
where customers.ContactTitle == "Nobody should match this"
// Select New - using [Case-Sensitive] the Context table name. Regex should be smart enough to avoid this
select new { customers }