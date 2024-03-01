<Query Kind="Program">
  <Connection>
    <ID>b8f3d72c-448f-4cdf-a5e3-419ce8aeb713</ID>
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
	try
	{


		//Good Data
		//Vendor that does not have an active ordder open
		int vendorID = 2;

		//Vendor that has an active order
		//int vendorID = 1;


		//Bad data
		//int vendorID = 0;

		//Vendor that does not exist
		//int vendorID = 6;

		PurchaseOrder_GetInventory(vendorID);

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

public List<ItemView> PurchaseOrder_GetInventory(int vendorID)
{
	bool vendorExists = false;

	if (vendorID == 0)
	{
		throw new ArgumentNullException("Vendor ID is invalid");
	}
	
	vendorExists = Vendors
					.Where(x => x.VendorID == vendorID).Any();
	var activeOrder = PurchaseOrders
					.Where(x => x.VendorID == vendorID && x.OrderDate == null).FirstOrDefault();
	List<ItemView> updatedInventory = new();

	if (vendorExists)
	{
		if (activeOrder != null)
		{
			foreach (var item in Parts)
			{
				bool partInPO = false;

				if (item.VendorID == vendorID)
				{
					foreach (var part in activeOrder.PurchaseOrderDetails)
					{
						if (item.PartID == part.PartID)
						{
							partInPO = true;
							break;
						}
					}
						if (!partInPO)
						{
							updatedInventory.Add(new ItemView()
							{
								PurchaseOrderID = activeOrder.PurchaseOrderID,
								PurchaseOrderNum = activeOrder.PurchaseOrderNumber,
								PartID = item.PartID,
								PartDescription = item.Description,
								QOH = item.QuantityOnHand,
								ROL = item.ReorderLevel,
								QOO = item.QuantityOnOrder,
								Buffer = item.ReorderLevel - (item.QuantityOnHand + item.QuantityOnOrder),
								Price = item.PurchasePrice
							});
						}
					
				}
			}
			Console.WriteLine($"ALL the parts in vendor {vendorID}");
			Parts.Where(x => x.VendorID == vendorID).Dump();
			Console.WriteLine("Active PO");
			activeOrder.PurchaseOrderDetails.Dump();
			Console.WriteLine("Inventory view of only the parts that are not in the active PO");
			return updatedInventory.ToList().Dump();
			}
		else
		{

			foreach (var item in Parts)
			{
				if (item.VendorID == vendorID && item.ReorderLevel - (item.QuantityOnHand + item.QuantityOnOrder) <= 0)
				{
					updatedInventory.Add(new ItemView()
					{
						PartID = item.PartID,
						PartDescription = item.Description,
						QOH = item.QuantityOnHand,
						ROL = item.ReorderLevel,
						QOO = item.QuantityOnOrder,
						Buffer = item.ReorderLevel - (item.QuantityOnHand + item.QuantityOnOrder),
						Price = item.PurchasePrice
					});
				}
			}

			var suggestedParts = Parts
					.Where(x => x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder) > 0 && x.VendorID == vendorID)
				.Select(x => new PurchaseOrderDetailView
				{
					PartID = x.PartID,
					PartDescription = x.Description,
					QOH = x.QuantityOnHand,
					ROL = x.ReorderLevel,
					QOO = x.QuantityOnOrder,
					QTO = x.ReorderLevel - (x.QuantityOnHand + x.QuantityOnOrder), 
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
			Console.WriteLine("Items View");
			updatedInventory.Dump();
			return updatedInventory;
		}
	}

	else {
		throw new Exception(" Vendor does not exist!");
	}
}
