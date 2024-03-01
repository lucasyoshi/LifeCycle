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
	try
	{
		//// ********************* BAD DATA ****************************
		//// populating List of salesDetailView with bad data
		//List<Parts> parts = Parts.Where(p => p.QuantityOnHand == 0).Take(2).ToList();
		//List<SaleDetailView> testSaleDetails = new List<SaleDetailView>();
		//bool isSecondPart = false;
		//foreach (var part in parts)
		//{
		//	var saleDetail = new SaleDetailView
		//	{
		//		Description = part.Description,
		//		PartID = part.PartID,
		//		Quantity = isSecondPart? 1: -5,
		//		SellingPrice = part.SellingPrice,
		//		Total = 1 * part.SellingPrice
		//	};
		//	testSaleDetails.Add(saleDetail);
		//	isSecondPart = true;
		//}
		//
		////testSaleDetails.Add();
		//// populating SalesView with neccessary bad data
		//SalesView testSalesView = new SalesView
		//{
		//	EmployeeID = 1,
		//	PaymentType = "",
		//	CouponId = 66
		//};

		// ****************** GOOD DATA *****************
		// populating List of salesDetailView with good data (3 items with quantity greater than zero)
		List<Parts> parts = Parts.Where(p => p.QuantityOnHand > 0).Take(3).ToList();
		List<SaleDetailView> testSaleDetails = new List<SaleDetailView>();
		foreach (var part in parts)
		{
			var saleDetail =  new SaleDetailView 
			{
				Description = part.Description,
				PartID = part.PartID,
				Quantity = 1,
				SellingPrice = part.SellingPrice,
				Total = 1 * part.SellingPrice
			};
			testSaleDetails.Add(saleDetail);
		}
		// populating SalesView with neccessary good data
		SalesView testSalesView = new SalesView
		{
			EmployeeID = 1,
			PaymentType = "D",
			CouponId = 66
		};
		
		
		// ************** creating Sale **************
		Console.WriteLine("SaleID:");
		Checkout(testSalesView, testSaleDetails).Dump();
		
	}

	catch (ArgumentNullException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	catch (ArgumentException ex)
	{
		GetInnerException(ex).Message.Dump();
	}
	catch (AggregateException ex)
	{
		//having collected a number of errors
		//    each error should be dumped to a separate line
		foreach (var error in ex.InnerExceptions)
		{
			error.Message.Dump();
		}
	}
	catch (Exception ex)
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

public int Checkout(SalesView sale, List<SaleDetailView> saleDetails)
{
	//a container to hold x number of Exception messages
	List<Exception> errorList = new List<Exception>();

	#region Validation
	if (saleDetails.Count() < 0)
	{
		throw new ArgumentNullException("sale details (cart) is empty. No parts to checkout.");
	}
	foreach (var item in saleDetails)
	{
		if (item.Quantity <= 0)
		{
			errorList.Add(new Exception($"Quantity for {item.Description} must be a positive non zero number."));
		}
		if (item.Quantity > Parts.Where(p => p.PartID == item.PartID).FirstOrDefault().QuantityOnHand)
		{
			errorList.Add(new Exception($"Quantity requested for {item.Description} ({item.Quantity}) can't be greater than "
				+ $"quantity on hand ({Parts.Where(p => p.PartID == item.PartID).FirstOrDefault().QuantityOnHand})."));
		}
	}
	if (String.IsNullOrWhiteSpace(sale.PaymentType))
	{
		errorList.Add(new Exception("Payment type must be selected."));
	}
	#endregion


	Sales thisSale = new Sales
	{
		SaleDate = DateTime.Now,
		EmployeeID = sale.EmployeeID,
		PaymentType = sale.PaymentType,
		Coupon = Coupons.Where(c => c.CouponID == sale.CouponId).FirstOrDefault(),
		Employee = Employees.Where(e => e.EmployeeID == sale.EmployeeID).FirstOrDefault()
	};

	foreach (var detail in saleDetails)
	{
		SaleDetails thisSaleDetails = new SaleDetails
		{
			SaleID = thisSale.SaleID,
			PartID = detail.PartID,
			Quantity = detail.Quantity,
			SellingPrice = detail.SellingPrice,
			Part = Parts.Where(p => p.PartID == detail.PartID).FirstOrDefault()
		};
		
		var discount = thisSale.Coupon.CouponDiscount;
		var itemPrice = thisSaleDetails.SellingPrice;
		if (discount > 0)
		{
			itemPrice -= itemPrice * ((decimal)discount/100);
		}
		thisSale.TaxAmount += itemPrice * (decimal)0.05;
		thisSale.SubTotal += itemPrice;
		
		thisSale.SaleDetails.Add(thisSaleDetails);
		Parts.Where(p => p.PartID == thisSaleDetails.PartID).FirstOrDefault().QuantityOnHand -= thisSaleDetails.Quantity;
		
	}
	Employees employee = Employees.Where(e => e.EmployeeID == sale.EmployeeID).FirstOrDefault();
	employee.Sales.Add(thisSale);

	if (errorList.Count > 0)
	{
		//  throw the list of business processing error(s)
		
		throw new AggregateException("Unable checkout sale. Check concerns", errorList.OrderBy(x => x.Message).ToList());
	}
	else
	{
		//  consider data valid
		//	has passed business processing rules
		
		SaveChanges();
		return thisSale.SaleID;
	}
}
