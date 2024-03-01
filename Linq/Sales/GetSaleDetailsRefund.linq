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
	//get the return details
	Console.WriteLine("Get the return details");
	GetSaleDetailsRefund(1203).Dump();

}

public List<SaleRefundDetailView> GetSaleDetailsRefund(int saleID)
{
	return SaleRefundDetails
				.Where(srd => srd.SaleRefund.SaleID == saleID)
				.Select(srd => new SaleRefundDetailView
				{
					PartID = srd.PartID,
					Description = srd.Part.Description,
					OriginalQuantity = srd.SaleRefund.Sale.SaleDetails
											.Where(sd => sd.SaleID == saleID && sd.PartID == srd.PartID)
											.Select(sd => sd.Quantity).FirstOrDefault(),
					SellingPrice = srd.SellingPrice,
					ReturnQuantity = srd.Quantity,
					Refundable = srd.Part.Refundable == "Y" ? true : false,
					Reason = srd.Reason
				}).ToList();

}
