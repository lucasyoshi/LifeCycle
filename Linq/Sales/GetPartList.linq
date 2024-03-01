<Query Kind="Program">
  <Connection>
    <ID>c099ebb1-5153-4c9d-92a0-91a4f8a19845</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>localhost\SQLEXPRESS01</Server>
    <Database>eBike_DMIT2018</Database>
    <DisplayName>eBike-Entity</DisplayName>
    <DriverData>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
</Query>

#load ".\ViewModels\*.cs"
using SalesAndReturns;
void Main()
{

	//will populate second dropDown based on value chosen from first dropDown
	Console.WriteLine("Parts List based on Category chosen (2: Parts)");
	GetPartList(2).Dump();

}

public List<PartView> GetPartList(int CategoryID)
{
	return Parts
		.Where(p => p.CategoryID == CategoryID && p.Discontinued == false)
		.Select(p => new PartView
		{
			PartID = p.PartID,
			Description = p.Description,
			SellingPrice = p.SellingPrice
		}).ToList();
}

