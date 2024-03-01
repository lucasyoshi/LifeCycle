<Query Kind="Program">
  <Connection>
    <ID>049b8f2a-01cf-4087-9e7a-1af396b6c24e</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>.</Server>
    <Database>eBike_DMIT2018</Database>
    <DisplayName>eBikesEntity</DisplayName>
    <DriverData>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
    </DriverData>
  </Connection>
</Query>

#load ".\ViewModels\*.cs"
using Purchasing;
void Main()
{

		try
		{
		
		//Vendor with no active PO
		int vendorID = 2;

		//Vendor with active PO
		//int vendorID = 3;
		
		
		//Invalid Vendor
		//int vendorID = 6;
		

		PurchaseOrder_GetPurchaseOrder(vendorID);

		}
	
		//  catch all exceptions
		catch (AggregateException ex)
		{
			foreach (var error in ex.InnerExceptions)
			{
				error.Message.Dump();			
			}
		}
	
		catch (ArgumentNullException ex)
		{
			GetInnerException(ex).Message.Dump();
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


public List<PurchaseOrderView> PurchaseOrder_GetPurchaseOrder(int vendorID)
{
	bool activeOrderExist = false;
	bool vendorExsits = false;
	if (vendorID == 0)
	{
		throw new ArgumentNullException("Vendor ID provided is invalid");
	}
	
	vendorExsits = Vendors
					.Where(x => x.VendorID == vendorID).Any();

	//Business Rule 
	//Check if an active order exits where order date is null
	//if yes then return the order
	//if no then return an empty order list with vendorId

	activeOrderExist = PurchaseOrders
			.Where(x => x.VendorID == vendorID && x.OrderDate == null).Any();
	
	if (vendorExsits)
	{
		if (activeOrderExist)
		{
			Console.WriteLine($"Active order exists");
			return PurchaseOrders
			.Where(x => x.VendorID == vendorID && x.OrderDate == null)
			.Select(x => new PurchaseOrderView
			{
				PurchaseOrderID = x.PurchaseOrderID,
				PurchaseOrderNum = x.PurchaseOrderNumber,
				VendorID = x.VendorID,
				SubTotal = x.SubTotal,
				GST = x.TaxAmount,
				EmployeeID = x.EmployeeID,
				PurchaseOrderDetails = x.PurchaseOrderDetails
							.Select(p => new PurchaseOrderDetailView
							{
								PartID = p.PartID,
								PartDescription = p.Part.Description,
								QOH = p.Part.QuantityOnHand,
								ROL = p.Part.ReorderLevel,
								QOO = p.Part.QuantityOnOrder,
								QTO = p.Quantity,
								Price = p.Part.PurchasePrice

							}).ToList()

		}).ToList().Dump();
		}
		else
		{
		
			var suggestedParts = Parts
								.Where(x => x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder) > 0 && x.VendorID == vendorID)
							.Select(x => new PurchaseOrderDetailView
							{
								PartID = x.PartID,
								PartDescription = x.Description,
								QOH = x.QuantityOnHand,
								ROL = x.ReorderLevel,
								QOO = x.QuantityOnOrder,
								QTO = x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder), //Suggested quantity to purchase based on the buffer
								Price = x.PurchasePrice
							}).ToList();
			Console.WriteLine($"No active order exists. A suggested order of items based on the buffer level is provided");
			List<PurchaseOrderView> emptyPurchaseOrder = new List<PurchaseOrderView>
			{
				new PurchaseOrderView
				{
					VendorID = vendorID,

					PurchaseOrderDetails = suggestedParts,
					EmployeeID = 7,
					SubTotal = suggestedParts.Sum(x=>x.Price * x.QTO),
					GST = suggestedParts.Sum(x=>x.Price *x.QTO)*(decimal)0.05	
				}
			}.Dump();

			Console.WriteLine($"ALL the parts in vendor {vendorID}");
			Parts
				.Where(x => x.VendorID == vendorID)
				.Dump();
			return emptyPurchaseOrder;
		}
	} else {
		throw new Exception("Vendor does not exist!");
	}

}