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
		PurchaseOrderView purchaseOrder = null;
		//ResetPO(565);

		//Display order and order details

		DisplayOrder();


		// a) Good Data
		purchaseOrder = Update_Active_Order();


		//purchaseOrder = Add_New_Order();

		// b) Bad Data 
		//purchaseOrder = Create_Bad_Data();

		//purchaseOrder= Create_Empty_Order_Data();

		//purchaseOrder = Invalid_Order();

		
		PurchaseOrder_PlaceOrder(purchaseOrder);

		//Display order and order details
		DisplayOrder();

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

	catch (ArgumentException ex)
	{

		GetInnerException(ex).Message.Dump();
	}
	catch (Exception ex)
	{
		GetInnerException(ex).Message.Dump();
	}
}

//Method 1: Display order
public void DisplayOrder()
{
	PurchaseOrders
		.OrderByDescending(x => x.PurchaseOrderID)
			.Take(10)
			.Select(x => new
			{
				PurchaseOrderID = x.PurchaseOrderID,
				PurchaseOrderNum = x.PurchaseOrderNumber,
				OrderDate = x.OrderDate,
				TaxAmount = x.TaxAmount,
				SubTotal = x.SubTotal,
				VendorID = x.VendorID,
				EmployeeID = x.EmployeeID,
				PurchaseOrderDetails = x.PurchaseOrderDetails
							.Select(p => new
							{
								PartID = p.PartID,
								Description = p.Part.Description,
								QOH = p.Part.QuantityOnHand,
								QOO = p.Part.QuantityOnOrder,
								ROL = p.Part.ReorderLevel,
								QTO = p.Quantity,
								Price = p.PurchasePrice,
							})
			}).ToList().Dump();
}

//Method 2: Saving/Updating Order
public void PurchaseOrder_PlaceOrder(PurchaseOrderView purchaseOrder)
{
	List<Exception> errorlist = new List<Exception>();
	PurchaseOrders order = new PurchaseOrders();

	//VALIDATIONS
	//----Parameter has no value - throw error---//
	if (purchaseOrder == null || purchaseOrder.PurchaseOrderDetails.Count == 0)
	{
		throw new ArgumentNullException("There is no order details");
	}

	//------Check for order date---//
	PurchaseOrders purchaseOrderData = PurchaseOrders
					.Where(x => x.VendorID == purchaseOrder.VendorID
										&& x.PurchaseOrderID == purchaseOrder.PurchaseOrderID).FirstOrDefault();
	if (purchaseOrderData != null && purchaseOrderData.OrderDate != null)
	{
		throw new ArgumentNullException("Order is closed and cannot be edited or placed");
	}

	bool employeeExists = Employees.Where(x => x.EmployeeID == purchaseOrder.EmployeeID).Any();

	if (!employeeExists)
	{
		errorlist.Add(new Exception($"Employee with the id {purchaseOrder.EmployeeID} does not exist in the database."));
	}

	//Looking if an active order is available for the vendor
	PurchaseOrders activeOrder = PurchaseOrders
									.Where(x => x.VendorID == purchaseOrder.VendorID && x.OrderDate == null).FirstOrDefault();

	//Validate Purchase Order Details 
	foreach (var orderDetail in purchaseOrder.PurchaseOrderDetails)
	{
		//Part must exist 
		if (orderDetail.PartID == 0)
		{
			throw new ArgumentNullException($"Part of order Num {purchaseOrder.PurchaseOrderNum} cannot be 0");
		}
		Parts part = Parts
						.Where(x => x.PartID == orderDetail.PartID && x.VendorID == purchaseOrder.VendorID)
						.FirstOrDefault();
		if (part == null)
		{
			//throw new ArgumentNullException($"Item {orderDetail.PartDescription} does not exist in vendor { purchaseOrder.VendorID} database");

			errorlist.Add(new Exception($"Item {orderDetail.PartDescription} does not exist in vendor {purchaseOrder.VendorID} database"));
		}

		//Positive price
		if (orderDetail.Price <= 0)
		{
			errorlist.Add(new Exception($"Item {orderDetail.PartDescription} price must be greater than zero"));
		}

		//Postive Quantity
		if (orderDetail.QTO <= 0)
		{
			errorlist.Add(new Exception($"Item {orderDetail.PartDescription} quantity must be greater than zero"));
		}


		if (errorlist.Count == 0)
		{
			//we checking if the vendor already has an active order open, if yes then we change the data directly
			if (activeOrder != null)
			{
				PurchaseOrderDetails activeOrderDetail = activeOrder.PurchaseOrderDetails
														.Where(x => x.PartID == orderDetail.PartID)
														.FirstOrDefault();
				if (activeOrderDetail != null)
				{
					activeOrderDetail.Quantity = orderDetail.QTO;
					activeOrderDetail.PurchasePrice = orderDetail.Price;

				}
				else
				{
					Parts newPart = Parts
										.Where(x => x.PartID == orderDetail.PartID && x.VendorID == purchaseOrder.VendorID)
										.FirstOrDefault();
					PurchaseOrderDetails newOrderItem = new PurchaseOrderDetails();
					if (newPart != null)
					{
						newOrderItem.PartID = newPart.PartID;
						newOrderItem.Part = newPart;
						newOrderItem.Quantity = orderDetail.QTO;
						newOrderItem.PurchasePrice = orderDetail.Price;
						activeOrder.PurchaseOrderDetails.Add(newOrderItem);
					}
					else
					{
						throw new ArgumentNullException($"Part {orderDetail.PartDescription} does not exist");
					}

				}
			}
			else
			{
				PurchaseOrderDetails newOrderDetails = new PurchaseOrderDetails();
				Parts partToEdit = new Parts();
				newOrderDetails.Part = Parts.Where(x => x.PartID == orderDetail.PartID).FirstOrDefault();
				newOrderDetails.PartID = orderDetail.PartID;
				newOrderDetails.PurchasePrice = orderDetail.Price;
				newOrderDetails.Quantity = orderDetail.QTO;
				order.PurchaseOrderDetails.Add(newOrderDetails);
			}
			part.QuantityOnOrder = part.QuantityOnOrder + orderDetail.QTO;//updating the quantity on order for the part we ordering

		}
	}


	if (errorlist.Count == 0)
	{
		if (activeOrder == null)
		{
			order.PurchaseOrderNumber = SetNewOrderNum();
			order.VendorID = purchaseOrder.VendorID;
			order.SubTotal = order.PurchaseOrderDetails.Sum(x => x.Quantity * x.PurchasePrice);
			order.TaxAmount = order.SubTotal * (decimal)0.05;
			order.EmployeeID = purchaseOrder.EmployeeID;
			order.OrderDate = DateTime.Now; //the date needs to be null otherwise it will set automatially <---(its not working)
			PurchaseOrders.Add(order);
		}
		else
		{
			activeOrder.SubTotal = activeOrder.PurchaseOrderDetails.Sum(x => x.Quantity * x.PurchasePrice);
			activeOrder.TaxAmount = activeOrder.SubTotal * (decimal)0.05;
			activeOrder.OrderDate = DateTime.Now; //the date needs to be null otherwise it will set automatially <---(its not working)

		}
	}


	if (errorlist.Count > 0)
	{
		throw new AggregateException("Unable to save the order. Check concerns", errorlist.OrderBy(x => x.Message).ToList());
	}
	else
	{
		SaveChanges();
	}
}



public int SetNewOrderNum()
{
	int newOrderNum = PurchaseOrders
						.Select(x => x.PurchaseOrderNumber).Max();
	newOrderNum++;
	return newOrderNum;
}



#region Method
private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
		ex = ex.InnerException;
	return ex;
}

// RESET DATA 
public void ResetPO(int orderID)	 
{
	PurchaseOrders placedPO = PurchaseOrders .Where(x=>x.PurchaseOrderID == orderID).FirstOrDefault();
	placedPO.OrderDate = null;
	SaveChanges();
}
//TESTS
//GOOD DATA Methods
public PurchaseOrderView Update_Active_Order()
{
	PurchaseOrderView purchaseOrder = new PurchaseOrderView();
	int item_One_ID = 113;
	int item_Two_ID = 117;
	int item_Three_ID = 119;

	purchaseOrder.VendorID = 1;
	purchaseOrder.PurchaseOrderID = 565;
	purchaseOrder.PurchaseOrderNum = 132;

	purchaseOrder.GST = PurchaseOrders
						.Where(x => x.PurchaseOrderID == purchaseOrder.PurchaseOrderID)
						.Select(x => x.TaxAmount).FirstOrDefault();
	purchaseOrder.SubTotal = PurchaseOrders
								.Where(x => x.PurchaseOrderID == purchaseOrder.PurchaseOrderID)
								.Select(x => x.SubTotal).FirstOrDefault();
	purchaseOrder.EmployeeID = 5;
	purchaseOrder.PurchaseOrderDetails = new List<PurchaseOrderDetailView>();

	//	Updating the quantity of an item already on the list

	purchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = item_One_ID,
		Price = 55,
		QTO = 35,
		PartDescription = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.Description)
					.FirstOrDefault(),
		QOH = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.QuantityOnHand)
					.FirstOrDefault(),
		QOO = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.QuantityOnOrder)
					.FirstOrDefault(),
		ROL = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.ReorderLevel)
					.FirstOrDefault(),
	});
	//	adding a new part to the list

	purchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = item_Two_ID,
		Price = 30,
		QTO = 12,
		PartDescription = Parts.Where(x => x.PartID == item_Two_ID)
					.Select(x => x.Description)
					.FirstOrDefault(),
		QOH = Parts.Where(x => x.PartID == item_Two_ID)
					.Select(x => x.QuantityOnHand)
					.FirstOrDefault(),
		QOO = Parts.Where(x => x.PartID == item_Two_ID)
					.Select(x => x.QuantityOnOrder)
					.FirstOrDefault(),
		ROL = Parts.Where(x => x.PartID == item_Two_ID)
					.Select(x => x.ReorderLevel)
					.FirstOrDefault(),
	});
	//updating that new item just added
	purchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = item_Three_ID,
		Price = 30,
		QTO = 24,
		PartDescription = Parts.Where(x => x.PartID == item_Three_ID)
					.Select(x => x.Description)
					.FirstOrDefault(),
		QOH = Parts.Where(x => x.PartID == item_Three_ID)
					.Select(x => x.QuantityOnHand)
					.FirstOrDefault(),
		QOO = Parts.Where(x => x.PartID == item_Three_ID)
					.Select(x => x.QuantityOnOrder)
					.FirstOrDefault(),
		ROL = Parts.Where(x => x.PartID == item_Three_ID)
					.Select(x => x.ReorderLevel)
					.FirstOrDefault(),
	});
	Console.WriteLine("Good Test Data");
	purchaseOrder.Dump();
	return purchaseOrder;
}

public PurchaseOrderView Add_New_Order()
{
	PurchaseOrderView purchaseOrder = new PurchaseOrderView();
	int item_One_ID = 112;
	//int item_Two_ID = 104;
	//int item_Three_ID = 105;

	purchaseOrder.VendorID = 4;
	purchaseOrder.PurchaseOrderID = 0;
	purchaseOrder.PurchaseOrderNum = 0;
	purchaseOrder.EmployeeID = 5;

	purchaseOrder.PurchaseOrderDetails = new List<PurchaseOrderDetailView>();

	purchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = item_One_ID,
		Price = 20,
		QTO = 18,
		PartDescription = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.Description)
					.FirstOrDefault(),
		QOH = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.QuantityOnHand)
					.FirstOrDefault(),
		QOO = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.QuantityOnOrder)
					.FirstOrDefault(),
		ROL = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.ReorderLevel)
					.FirstOrDefault(),
	});

	Console.WriteLine("Good Test Data");
	purchaseOrder.Dump();
	return purchaseOrder;
}

//BAD DATA Methods
public PurchaseOrderView Create_Bad_Data()
{
	PurchaseOrderView purchaseOrder = new PurchaseOrderView();
	int item_One_ID = 120;
	int item_Two_ID = 113;
	int item_Three_ID = 114;
	purchaseOrder.VendorID = 4;
	purchaseOrder.PurchaseOrderID = 570;
	purchaseOrder.EmployeeID = 5;

	purchaseOrder.PurchaseOrderDetails = new List<PurchaseOrderDetailView>();

	purchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = item_One_ID,
		Price = 0,
		QTO = 35,
		PartDescription = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.Description)
					.FirstOrDefault(),
		QOH = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.QuantityOnHand)
					.FirstOrDefault(),
		QOO = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.QuantityOnOrder)
					.FirstOrDefault(),
		ROL = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.ReorderLevel)
					.FirstOrDefault(),
	});
	purchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = item_Two_ID,
		Price = -1,
		QTO = 0,
		PartDescription = Parts.Where(x => x.PartID == item_Two_ID)
					.Select(x => x.Description)
					.FirstOrDefault(),
		QOH = Parts.Where(x => x.PartID == item_Two_ID)
					.Select(x => x.QuantityOnHand)
					.FirstOrDefault(),
		QOO = Parts.Where(x => x.PartID == item_Two_ID)
					.Select(x => x.QuantityOnOrder)
					.FirstOrDefault(),
		ROL = Parts.Where(x => x.PartID == item_Two_ID)
					.Select(x => x.ReorderLevel)
					.FirstOrDefault(),
	});
	purchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = item_Three_ID,
		Price = 25,
		QTO = -5,
		PartDescription = Parts.Where(x => x.PartID == item_Three_ID)
					.Select(x => x.Description)
					.FirstOrDefault(),
		QOH = Parts.Where(x => x.PartID == item_Three_ID)
					.Select(x => x.QuantityOnHand)
					.FirstOrDefault(),
		QOO = Parts.Where(x => x.PartID == item_Three_ID)
					.Select(x => x.QuantityOnOrder)
					.FirstOrDefault(),
		ROL = Parts.Where(x => x.PartID == item_Three_ID)
					.Select(x => x.ReorderLevel)
					.FirstOrDefault(),
	});

	Console.WriteLine("Bad Test Data");
	purchaseOrder.Dump();
	return purchaseOrder;

}

public PurchaseOrderView Create_Empty_Order_Data()
{
	PurchaseOrderView purchaseOrder = new PurchaseOrderView();

	purchaseOrder.VendorID = 1;
	purchaseOrder.PurchaseOrderID = 565;
	purchaseOrder.PurchaseOrderNum = 132;
	purchaseOrder.EmployeeID = 5;

	purchaseOrder.PurchaseOrderDetails = new List<PurchaseOrderDetailView>();

	Console.WriteLine("Bad Test Data");
	purchaseOrder.Dump();
	return purchaseOrder;
}

//To test for order thats already been placed
public PurchaseOrderView Invalid_Order()
{
	PurchaseOrderView purchaseOrder = new PurchaseOrderView();
	int item_One_ID = 110;
	//    int item_Two_ID = 113;
	//    int item_Three_ID = 114;

	purchaseOrder.VendorID = 3;
	purchaseOrder.PurchaseOrderID = 454;
	purchaseOrder.PurchaseOrderNum = 123;
	purchaseOrder.GST = PurchaseOrders
					.Where(x => x.PurchaseOrderID == purchaseOrder.PurchaseOrderID)
					.Select(x => x.TaxAmount).FirstOrDefault();
	purchaseOrder.SubTotal = PurchaseOrders
								.Where(x => x.PurchaseOrderID == purchaseOrder.PurchaseOrderID)
								.Select(x => x.SubTotal).FirstOrDefault();
	purchaseOrder.EmployeeID = 5;

	purchaseOrder.PurchaseOrderDetails = new List<PurchaseOrderDetailView>();

	//Updating the quantity of an item already on the list
	purchaseOrder.PurchaseOrderDetails.Add(new PurchaseOrderDetailView
	{
		PartID = item_One_ID,
		Price = 55,
		QTO = 35,
		PartDescription = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.Description)
					.FirstOrDefault(),
		QOH = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.QuantityOnHand)
					.FirstOrDefault(),
		QOO = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.QuantityOnOrder)
					.FirstOrDefault(),
		ROL = Parts.Where(x => x.PartID == item_One_ID)
					.Select(x => x.ReorderLevel)
					.FirstOrDefault(),
	});

	Console.WriteLine("Bad Test Data");
	purchaseOrder.Dump();
	return purchaseOrder;
}

#endregion