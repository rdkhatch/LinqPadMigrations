<Query Kind="Expression">
  <Connection>
    <ID>de95ffc9-8038-41d0-8c65-e48422436bb7</ID>
    <Persist>true</Persist>
    <Provider>System.Data.SqlServerCe.3.5</Provider>
    <AttachFileName>C:\Data\Personal\LinqPadMigrations-Github\src\LinqPadMigrations.Tests\App_Data\Northwind.Backup.sdf</AttachFileName>
  </Connection>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
</Query>

(from c in Customers
// Regex should IGNORE this - Strings need to be ignored!!
// If it matches correct - this will return zero results.
// If it does not, this will return a collection of Customers (because the string length won't match)
where

// TEST ESCAPED STRING #1
"Customers".Length != 9

||

// TEST ESCAPED STRING #2
"some\"text".Length != 9

||

// TEST ESCAPED STRING #3
@"some""text".Length != 9

select c).Take(50)