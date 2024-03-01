<Query Kind="Program">
  <Connection>
    <ID>0d4f0f85-1297-42a1-8865-af30fea80505</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>.</Server>
    <Database>eBike_DMIT2018</Database>
    <DisplayName>eBikeEntity</DisplayName>
    <DriverData>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
      <TrustServerCertificate>True</TrustServerCertificate>
    </DriverData>
  </Connection>
</Query>

#load ".\ViewModels\*.cs"

using Receiving;

void Main()
{
	try
	{	        
		List<PurchaseOrderDetailsView> podList = new();
		List<UnorderedItemView> cart = new();
		int purchaseOrderId = 458, employeeID = 5;
		
		// Good Data
		//(podList, cart) = CreateGoodData(purchaseOrderId);
		(podList, cart) = CreateGoodData2(purchaseOrderId);
		
		// Bad Data
		//(podList, cart) = CreateBadData(purchaseOrderId);


		podList.Dump();
		cart.Dump();

		Console.WriteLine("Before Receiving");	
		ReceiveOrders.Dump();
		ReceiveOrderDetails.Dump();
		PurchaseOrders.Dump();
		UnorderedPurchaseItemCarts.Dump();
		ReturnedOrderDetails.Dump();
		Parts.Dump();

		Receive(purchaseOrderId, employeeID, podList, cart);

		Console.WriteLine("After receiving");
		ReceiveOrders.Dump();
		ReceiveOrderDetails.Dump();
		PurchaseOrders.Dump();
		UnorderedPurchaseItemCarts.Dump();
		ReturnedOrderDetails.Dump();
		Parts.Dump();
	}
	#region Exception handling
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
	#endregion
}

#region Exception Method

private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
	{
		ex = ex.InnerException;
	}
	return ex;
}
#endregion

public (List<PurchaseOrderDetailsView>, List<UnorderedItemView>) CreateGoodData(int purchaseOrderID)
{
	List<PurchaseOrderDetailsView> podList = GetOrderDetails(purchaseOrderID);
	List<UnorderedItemView> cart = new() {
			new UnorderedItemView{ItemDescription = "Wind Breaker",
								Quantity = 10,
								VendorPartNumber = "4587op23"}
	};

	foreach (var item in podList)
	{
		item.ReceivedQty = 5;

		if (item.PartID == 105)
		{
			item.ReturnQty = 2;
			item.ReturnReason = "Over shipped";
		}
		else
		{
			item.ReturnQty = 0;
		}
	}

	return (podList, cart);
}

public (List<PurchaseOrderDetailsView>, List<UnorderedItemView>) CreateGoodData2(int purchaseOrderID)
{
	List<PurchaseOrderDetailsView> podList = GetOrderDetails(purchaseOrderID);
	List<UnorderedItemView> cart = new() {
			new UnorderedItemView{ItemDescription = "Wind Breaker",
								Quantity = 10,
								VendorPartNumber = "4587op23"}
	};

	foreach (var item in podList)
	{
		if (item.PartID == 109)
		{
			item.ReceivedQty = 50;

		}
		else
		{
			item.ReceivedQty = 5;
		}
	}

	return (podList, cart);
}


public (List<PurchaseOrderDetailsView>, List<UnorderedItemView>) CreateBadData(int purchaseOrderID)
{
	List<PurchaseOrderDetailsView> podList = new(); //GetOrderDetails(purchaseOrderID);
	List<UnorderedItemView> cart = new() {
			new UnorderedItemView{ItemDescription = "Wind Breaker",
								Quantity = 1,
								VendorPartNumber = "4587op23"}
	};

	foreach (var item in podList)
	{
		item.ReceivedQty = 5;

		if (item.PartID == 105)
		{
			item.ReturnQty = 2;
		}
		else
		{
			item.ReturnQty = 0;
		}
	}

	return (podList, cart);
}

public List<PurchaseOrderDetailsView> GetOrderDetails(int PurchaseOrderID)
{
	return PurchaseOrderDetails
	.Where(x => x.PurchaseOrderID == PurchaseOrderID)
	.Select(x => new PurchaseOrderDetailsView
	{
		PurchaseOrderDetailID = x.PurchaseOrderDetailID,
		PartID = x.PartID,
		PartDescription = x.Part.Description,
		OriginalQty = x.Quantity,
		OutstandingQty = x.Quantity - x.ReceiveOrderDetails.Where(r => r.PurchaseOrderDetailID == x.PurchaseOrderDetailID).Select(r => r.QuantityReceived).FirstOrDefault()
	}).ToList();
}

public void Receive(int purchaseOrderID, int employeeID, List<PurchaseOrderDetailsView> outstandingItems,
 List<UnorderedItemView> unorderedCart)
{
	AddReceiveOrder(purchaseOrderID, employeeID);
	AddReceiveOrderDetails(purchaseOrderID, outstandingItems);
	AddReturnedOrderDetails(purchaseOrderID, unorderedCart, outstandingItems);
	UpdatePartsQty(outstandingItems);
	CloseOrder(outstandingItems, purchaseOrderID);
	
	SaveChanges();
}

public void AddReceiveOrder(int purchaseOrderID, int employeeID)
{
	List<Exception> errorList = new();
	ReceiveOrders receive = new();

	if (purchaseOrderID <= 0)
	{
		throw new ArgumentNullException($"Purchase Order ID cannot be 0. ID: {purchaseOrderID}");
	}
	if (employeeID <= 0)
	{
		throw new ArgumentNullException($"Employee ID cannot be 0. ID: {employeeID}");
	}

	var poExists = PurchaseOrders.FirstOrDefault(x => x.PurchaseOrderID == purchaseOrderID);
	var employeeExists = Employees.FirstOrDefault(e => e.EmployeeID == employeeID);

	if (employeeExists == null)
	{
		errorList.Add(new Exception($"Employee does not exist in the database. Employee ID: {employeeID}"));
	}

	if (poExists != null)
	{
		receive.PurchaseOrderID = purchaseOrderID;
		receive.ReceiveDate = DateTime.Now;
		receive.EmployeeID = employeeID;
		
		ReceiveOrders.Add(receive);
	}
	else
	{
		errorList.Add(new Exception($"Purchase Order ID does not exist in the database. PO ID: {purchaseOrderID}"));
	}
	if (errorList.Count > 0)
	{
		throw new AggregateException("Unable to add the Receive Order, check concerns", errorList);
	}
	else 
	{
		SaveChanges();
	}

}

public void AddReceiveOrderDetails(int purchaseOrderID, List<PurchaseOrderDetailsView> outstandingItems)
{
	List<Exception> errorList = new();
	var receiveOrderID = ReceiveOrders.OrderByDescending(x => x.ReceiveOrderID).Where(x => x.PurchaseOrderID == purchaseOrderID).Select(x => x.ReceiveOrderID).FirstOrDefault();
	
	if (outstandingItems.Count() <= 0)
	{
		throw new ArgumentNullException($"List of items are empty.");
	}

	var roExists = ReceiveOrders.FirstOrDefault(x => x.ReceiveOrderID == receiveOrderID);


	if (roExists == null)
	{
		errorList.Add(new Exception($"Receive Order ID does not exist in the database. ReceiveOrder ID: {receiveOrderID}"));
	}

	foreach (var item in outstandingItems)
	{
		ReceiveOrderDetails roDetails = new();

		if (item.ReceivedQty > 0)
		{
			roDetails.ReceiveOrderID = receiveOrderID;
			roDetails.PurchaseOrderDetailID = item.PurchaseOrderDetailID;
			roDetails.QuantityReceived = item.ReceivedQty;


			ReceiveOrderDetails.Add(roDetails);
		}
	}
	if (outstandingItems.Sum(x => x.ReceivedQty) == 0)
	{
		errorList.Add(new Exception("No record was added to the Received Order Details"));
	}

	if (errorList.Count > 0)
	{
		throw new AggregateException("Unable to add the Receive Order Details, check concerns", errorList);
	}
}

public void AddReturnedOrderDetails(int purchaseOrderID, List<UnorderedItemView> unorderedCart, List<PurchaseOrderDetailsView> wrongQtyItems)
{
	List<Exception> errorList = new();
	var receiveOrderID = ReceiveOrders.OrderByDescending(x => x.ReceiveOrderID).Where(x => x.PurchaseOrderID == purchaseOrderID).Select(x => x.ReceiveOrderID).FirstOrDefault();
	if (unorderedCart.Any())
	{
		foreach (var item in unorderedCart)
		{
			ReturnedOrderDetails returnItem = new();

			if (string.IsNullOrEmpty(item.ItemDescription))
			{
				throw new ArgumentNullException($"Item Description must not be empty.");
			}
			if (item.Quantity <= 0)
			{
				throw new ArgumentNullException($"Quantity must non-zero positive value. Qty: {item.Quantity}");
			}
			if (string.IsNullOrEmpty(item.VendorPartNumber))
			{
				throw new ArgumentNullException("Vendor Part Number must not be empty.");
			}

			returnItem.ReceiveOrderID = receiveOrderID;
			returnItem.ItemDescription = item.ItemDescription;
			returnItem.Quantity = item.Quantity;
			returnItem.Reason = "Not requested";
			returnItem.VendorPartNumber = item.VendorPartNumber;

			ReturnedOrderDetails.Add(returnItem);
		}
	}
	if (wrongQtyItems.Any())
	{
		foreach (var item in wrongQtyItems)
		{
			ReturnedOrderDetails returnItem = new();
			if (item.ReturnQty > 0)
			{
				if (string.IsNullOrWhiteSpace(item.ReturnReason))
				{
					errorList.Add(new Exception($"Record must contain a commented reason on why it's being returned. {item.PartDescription}"));
				}
				else
				{
					returnItem.ReceiveOrderID = receiveOrderID;
					returnItem.ItemDescription = item.PartDescription;
					returnItem.PurchaseOrderDetailID = item.PurchaseOrderDetailID;
					returnItem.Quantity = item.ReturnQty;
					returnItem.Reason = item.ReturnReason;
				}
				ReturnedOrderDetails.Add(returnItem);
			}
		}
	}

	if (errorList.Count > 0)
	{
		throw new AggregateException("Unable to add the Returned Order Details, check concerns", errorList);
	}
}

public void UpdatePartsQty(List<PurchaseOrderDetailsView> parts)
{
	List<Exception> errorList = new();

	if (parts.Count() == 0)
	{
		throw new ArgumentNullException("There are no items to be updated in the Parts table.");
	}

	foreach (var item in parts)
	{
		if (item.ReceivedQty > Parts.Where(x => x.PartID == item.PartID).Select(x => x.QuantityOnOrder).FirstOrDefault())
		{
			errorList.Add(new Exception("Received Quantity cannot be greater than the Quantity on Order."));
		}
		if (item.ReceivedQty != 0)
		{
			var updatedPart = Parts.FirstOrDefault(x => x.PartID == item.PartID);

			updatedPart.QuantityOnHand = updatedPart.QuantityOnHand + item.ReceivedQty;
			updatedPart.QuantityOnOrder = updatedPart.QuantityOnOrder - item.ReceivedQty;
		}
	}

	if (errorList.Count > 0)
	{
		throw new AggregateException("Unable to add the Receive Order, check concerns", errorList);
	}
}

public void CloseOrder(List<PurchaseOrderDetailsView> outstandingItems, int purchaseOrderID)
{
	List<Exception> errorList = new();

	int count = 0;

	foreach (var item in outstandingItems)
	{
		int totalQty = item.OutstandingQty;
		int qtyReceived = item.ReceivedQty;
		if (qtyReceived > totalQty)
		{
			errorList.Add(new Exception("Received Quantity cannot be greater than the Total Quantity."));
		}

		if (totalQty == qtyReceived)
		{
			count++;
		}
	}

	if (count == outstandingItems.Count())
	{
		var closeOrder = PurchaseOrders
						.Where(x => x.PurchaseOrderID == purchaseOrderID)
						.FirstOrDefault();
		closeOrder.Closed = true;
		closeOrder.Notes = "All items received";
	}
	if (errorList.Count() > 0)
	{
		throw new AggregateException("Unable to add the Receive Order, check concerns", errorList);
	}
	else
	{
		var cart = UnorderedPurchaseItemCarts.Select(x => x).ToList();
		UnorderedPurchaseItemCarts.RemoveRange(cart);
	}
}
