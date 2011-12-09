<Query Kind="Program">
  <Connection>
    <ID>de95ffc9-8038-41d0-8c65-e48422436bb7</ID>
    <Persist>true</Persist>
    <Provider>System.Data.SqlServerCe.3.5</Provider>
    <AttachFileName>C:\Data\Personal\LinqPadMigrations\LinqPadMigrations\LinqPadMigrations.Tests\App_Data\Northwind.Backup.sdf</AttachFileName>
  </Connection>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
</Query>

void Main()
{
	var context = new TypedDataContext();
	
	var customer = context.Customers.Where (c => c.CustomerID == "BOLID").Single();
	// Rename Title from 'Owner' to 'Manager'
	customer.ContactTitle = "Manager";
	
	context.SubmitChanges();
}