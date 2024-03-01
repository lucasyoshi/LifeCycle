<Query Kind="Program">
  <Connection>
    <ID>21c6e002-be0f-42e0-af88-546e1eb05072</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>localhost\SQLEXPRESS01</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <Database>eBike_DMIT2018</Database>
    <DriverData>
      <LegacyMFA>false</LegacyMFA>
    </DriverData>
  </Connection>
</Query>

#load ".\ViewModels\*.cs"
using SalesAndReturns;
void Main()
{
	// Query methods
	try
	{
		// will populate the first dropDown:
		Console.WriteLine("Category list");
		GetCategoryList().Dump();
		//will populate second dropDown based on value chosen from first dropDown
		Console.WriteLine("Parts List based on Category chosen (2: Parts)");
		GetPartList(2).Dump();
		
		//will fetch discount percentage (int) based on a user inputted string. if there is any
		//coupon with the same string it will return the discount amount, if not it will return 0
		Console.WriteLine("Coupon percentage for real coupon code");
		GetCoupon("TopHat").Dump();
		Console.WriteLine("Coupon percentage for non-existing coupon code");
		GetCoupon("fakeCoupon").Dump();
		
		// return queries
		//get a return
		Console.WriteLine("Get a return");
		GetSaleRefund(1203).Dump();
		//get the return details
		Console.WriteLine("Get the return details");
		GetSaleDetailsRefund(1203).Dump();
	}
	
	catch (ArgumentNullException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	catch (ArgumentException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	catch (AggregateException ex)
	{
	//having collected a number of errors
	//    each error should be dumped to a separate line
		foreach (var error in ex.InnerExceptions)
	    {
	    	error.Message.Dump();
		}
	}
	catch (Exception ex)
	{
		GetInnerException(ex).Message.Dump();
    }

}

#region Method
private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}
#endregion

public List<CategoryView> GetCategoryList()
{
	return Categories.Select(x => new CategoryView {
		CategoryID = x.CategoryID,
		Description = x.Description
	}).ToList();	
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

public List<SaleRefundDetailView> GetSaleDetailsRefund(int saleID)
{
	return SaleRefundDetails.Where(srd => srd.SaleRefund.SaleID == saleID)
							.Select(srd => new SaleRefundDetailView
							{
								PartID = srd.PartID,
								Description = srd.Part.Description, 
								OriginalQuantity = srd.SaleRefund.Sale.SaleDetails
													.Where(sd => sd.SaleID == saleID && sd.PartID == srd.PartID)
													.Select(sd => sd.Quantity).FirstOrDefault(),
								SellingPrice = srd.SellingPrice,
								ReturnQuantity = srd.Quantity,
								Refundable = srd.Part.Refundable == "Y"? true: false,
								
								// not sure if this is the right value to put in there yet:
								Quantity = srd.Quantity,
								Reason = srd.Reason
							}).ToList();

}