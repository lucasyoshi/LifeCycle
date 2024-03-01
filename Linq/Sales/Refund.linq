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
//		// ********************** BAD DATA *****************************
//		// populating SalesRefundView with bad data (data from an existing sale)
//		Sales saleToReturn = Sales.Where(s => s.SaleID == 1155).FirstOrDefault();
//		SaleRefundView testRefundView = new SaleRefundView
//		{
//			SaleId = saleToReturn.SaleID,
//			EmployeeID = saleToReturn.EmployeeID,
//		};
//
//		// populating list of SalesRefundDetailView with good data from SaleDetails from same existing sale.
//		List<SaleRefundDetailView> testRefundDetails = new List<SaleRefundDetailView>();
//		bool isSecondItem = false;
//		foreach (var item in saleToReturn.SaleDetails)
//		{
//			var returnDetail = new SaleRefundDetailView
//			{
//				PartID = item.PartID,
//				Description = item.Part.Description,
//				OriginalQuantity = item.Quantity,
//				SellingPrice = item.SellingPrice,
//				Refundable = item.Part.Refundable == "Y" ? true : false,
//				Quantity = 999,
//				Reason = isSecondItem?"Test": "",
//				ReturnQuantity = isSecondItem? 0: 1
//			};
//			testRefundDetails.Add(returnDetail);
//			isSecondItem = true;
//		}

		// ********************** GOOD DATA *****************************
		// populating SalesRefundView with good data (data from an existing sale)
		Sales saleToReturn = Sales.Where(s => s.SaleID == 2000).FirstOrDefault();
		SaleRefundView testRefundView = new SaleRefundView
		{
			SaleId = saleToReturn.SaleID,
			EmployeeID = saleToReturn.EmployeeID,
			DiscountPercent = saleToReturn.Coupon.CouponDiscount
		};
		
		// populating list of SalesRefundDetailView with good data from SaleDetails from same existing sale.
		List<SaleRefundDetailView> testRefundDetails = new List<SaleRefundDetailView>();
		foreach (var item in saleToReturn.SaleDetails)
		{
			var returnDetail = new SaleRefundDetailView
			{
				PartID = item.PartID,
				Description = item.Part.Description,
				OriginalQuantity = item.Quantity,
				SellingPrice = item.SellingPrice,
				Refundable = item.Part.Refundable == "Y"? true: false,
				Quantity = 1,
				Reason = "Test",
				ReturnQuantity = 0
			};
			testRefundDetails.Add(returnDetail);
		}
		
		
		
		// ************** creating refund *********************
		Console.WriteLine("SaleRefundID:");
		Refund(testRefundView, testRefundDetails).Dump();
		
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

public int Refund( SaleRefundView saleRefund, List<SaleRefundDetailView> saleRefundDetails)
{
	//a container to hold x number of Exception messages
	List<Exception> errorList = new List<Exception>();

	#region Validation
	if (saleRefund.SaleId < 0) 
	{
		throw new ArgumentNullException("Missing SaleID");
	}
	foreach (var item in saleRefundDetails)
	{
		if(item.Quantity > item.OriginalQuantity)
		{
			errorList.Add(new Exception($"Cannot return greater quantity ({item.Quantity}) than the original "
					+ $"quantity ({item.OriginalQuantity}) for item: {item.Description}"));
		}
		if (item.ReturnQuantity != 0 && item.Quantity > (item.OriginalQuantity - item.ReturnQuantity))
		{
			errorList.Add(new Exception($"Cannot return quantity {item.Quantity} for item: {item.Description}. " 
					+ $"Previously returned quantity of {item.ReturnQuantity} means that maximum quantity that "
					+ $"can be returned is: {item.OriginalQuantity - item.ReturnQuantity}."));
		}
		if(String.IsNullOrWhiteSpace(item.Reason) && item.Quantity > 0)
		{
			errorList.Add(new Exception($"{item.Description} must have a reason for return"));
		}
	}
	#endregion
	
	SaleRefunds thisSaleRefund = new SaleRefunds
	{
		SaleRefundDate = DateTime.Now,
		SaleID = saleRefund.SaleId,
		EmployeeID = saleRefund.EmployeeID,
		TaxAmount = saleRefund.TaxAmount,
		SubTotal = saleRefund.SubTotal
	};
	

	foreach (var detail in saleRefundDetails)
	{
		SaleRefundDetails thisSaleRefundDetails = new SaleRefundDetails
		{
			PartID = detail.PartID,
			Quantity = detail.Quantity,
			SellingPrice = detail.SellingPrice,
			Reason = detail.Reason,
			Part = Parts.Where(p => p.PartID == detail.PartID).FirstOrDefault()
		};

		//var discount = Sales.Where(s => s.SaleID == saleRefund.SaleId ).FirstOrDefault().Coupon.CouponDiscount;
		var itemPrice = thisSaleRefundDetails.SellingPrice;
		if (saleRefund.DiscountPercent > 0)
		{
			itemPrice -= itemPrice * ((decimal)saleRefund.DiscountPercent / 100);
		}
		thisSaleRefund.TaxAmount += itemPrice * (decimal)0.05;
		thisSaleRefund.SubTotal += itemPrice;

		thisSaleRefund.SaleRefundDetails.Add(thisSaleRefundDetails);
		Parts.Where(p => p.PartID == thisSaleRefundDetails.PartID).FirstOrDefault().QuantityOnHand += thisSaleRefundDetails.Quantity;

	}
	Employees employee = Employees.Where(e => e.EmployeeID == thisSaleRefund.EmployeeID).FirstOrDefault();
	employee.SaleRefunds.Add(thisSaleRefund);

	if (errorList.Count > 0)
	{
		//  throw the list of business processing error(s)
		throw new AggregateException("Unable to Refund sale. Check concerns", errorList.OrderBy(x => x.Message).ToList());
	}
	else
	{
		//  consider data valid
		//	has passed business processing rules
		SaveChanges();
		return thisSaleRefund.SaleRefundID;
	};
	

}
