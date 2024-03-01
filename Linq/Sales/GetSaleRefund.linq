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
	//get a return
	Console.WriteLine("Get a return");
	GetSaleRefund(1203).Dump();

}

public SaleRefundView GetSaleRefund(int saleID)
{
	return SaleRefunds.Where(sr => sr.SaleID == saleID)
				.Select(sr => new SaleRefundView
				{
					SaleId = sr.SaleID,
					EmployeeID = sr.EmployeeID,
					TaxAmount = sr.TaxAmount,
					SubTotal = sr.SubTotal,
					DiscountPercent = sr.Sale.Coupon.CouponDiscount
				}).FirstOrDefault();
}

