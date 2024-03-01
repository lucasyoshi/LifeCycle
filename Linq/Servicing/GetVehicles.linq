<Query Kind="Program">
  <Connection>
    <ID>dc9c00b2-68b5-4d2c-9c48-c9717df3aeb0</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>KIERINLAPTOP\SQLSERVER</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>eBike_DMIT2018</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
  <IncludeUncapsulator>false</IncludeUncapsulator>
</Query>

#load ".\ViewModels\*.cs"

using Servicing;

void Main()
{
	try
	{

	}
	catch (Exception ex)
	{

		throw;
	}
}

#region Method
private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
	{
		ex = ex.InnerException;
	}
	return ex;
}
#endregion

public List<VehicleView> GetVehicles(int customerID)
{
	return CustomerVehicles
		.Where(cv => cv.CustomerID == customerID)
		.Select(cv => new
		{
			VehicleName = cv.Make + ' ' + cv.Model,
			VIN = cv.VehicleIdentification,
			CustomerID = cv.CustomerID,
			Services = cv.Jobs
		}).Dump();
}