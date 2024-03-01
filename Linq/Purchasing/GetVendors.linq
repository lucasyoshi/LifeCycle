<Query Kind="Program">
  <Connection>
    <ID>53ebebe9-38cf-429f-9b5c-0ede69e02dab</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>.</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>eBike_DMIT2018</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
</Query>

#load ".\ViewModels\*.cs"
using Purchasing;
void Main()
{
	PurchaseOrder_GetVendors();
}

public List<VendorView>	PurchaseOrder_GetVendors() 
{	
	return Vendors
		.Select(x => new VendorView
		{
			VendorID = x.VendorID,
			VendorName = x.VendorName,
			HomePhone = x.Phone,
			City = x.City
			
		}).ToList().Dump();
}