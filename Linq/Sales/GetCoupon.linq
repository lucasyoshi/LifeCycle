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

	//will fetch discount percentage (int) based on a user inputted string. if there is any
	//coupon with the same string it will return the discount amount, if not it will return 0
	Console.WriteLine("Coupon percentage for real coupon code");
	GetCoupon("TopHat").Dump();
	Console.WriteLine("Coupon percentage for non-existing coupon code");
	GetCoupon("fakeCoupon").Dump();

}

public int GetCoupon(string coupons)
{
	if (Coupons.Where(c => c.CouponIDValue == coupons).Any())
	{
		return Coupons.Where(c => c.CouponIDValue == coupons)
					.Select(c => c.CouponDiscount).FirstOrDefault();
	}
	else
	{
		return 0;
	}
}

