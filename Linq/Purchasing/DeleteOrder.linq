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
        //Display order and order details
        DisplayOrders();

        // a) Good Data
				purchaseOrder = Create_Valid_Data();
			
				
        // b) Bad Data 
				//purchaseOrder = Deleting_Placed_Order(); 
            
				//purchaseOrder= Create_Invalid_Data();
        
        
        PurchaseOrder_DeleteOrder(purchaseOrder);

        //Display order and order details
        DisplayOrders();

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
public void DisplayOrders()
{
    PurchaseOrders
        .OrderByDescending(x => x.PurchaseOrderID)
            .Take(20)
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
public void PurchaseOrder_DeleteOrder(PurchaseOrderView purchaseOrder)
{
    List<Exception> errorlist = new List<Exception>();
	PurchaseOrders order = new PurchaseOrders();
	bool employeeExists = Employees.Where(x => x.EmployeeID == purchaseOrder.EmployeeID).Any();
	bool orderExists = PurchaseOrders.Where(x => x.VendorID == purchaseOrder.VendorID && x.PurchaseOrderID==purchaseOrder.PurchaseOrderID).Any();
	bool vendorExists = Vendors.Where(x=>x.VendorID == purchaseOrder.VendorID).Any();
	
	//VALIDATIONS
    //----Parameter has no value - throw error---//
    if (purchaseOrder == null )
    {
        throw new ArgumentNullException("Order is null");
    }
    
    //------Check for order date---//
    PurchaseOrders	purchaseOrderData = PurchaseOrders
                    .Where(x => x.VendorID == purchaseOrder.VendorID 
                                        && x.PurchaseOrderID == purchaseOrder.PurchaseOrderID).FirstOrDefault();
    if (purchaseOrderData != null && purchaseOrderData.OrderDate != null)
    {
        throw new ArgumentNullException("Order is closed and cannot be deleted");
	}
	
	if (!orderExists)
	{
		errorlist.Add(new Exception($"PO with the id {purchaseOrder.PurchaseOrderID} does not exist in the database."));
	}
	if (!employeeExists)
	{
		errorlist.Add(new Exception($"Employee with the id {purchaseOrder.EmployeeID} does not exist in the database."));
	}
	if (!vendorExists)
	{
		errorlist.Add(new Exception($"Vendor with the id {purchaseOrder.VendorID} does not exist in the database."));
	}


	if (errorlist.Count == 0)
	{
		List<PurchaseOrderDetails> orderDetailsToRemove = PurchaseOrderDetails
													.Where(x => x.PurchaseOrderID == purchaseOrder.PurchaseOrderID).ToList();
        var orderToRemove = PurchaseOrders.Where(x=>x.PurchaseOrderID == purchaseOrder.PurchaseOrderID).FirstOrDefault();
	
		foreach (var item in orderDetailsToRemove)
		{
			PurchaseOrderDetails.Remove(item);
		}
		PurchaseOrders.Remove(orderToRemove);
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


#region Method
private Exception GetInnerException(Exception ex)
{
    while (ex.InnerException != null)
        ex = ex.InnerException;
    return ex;
}


//TESTS
//public class PurchaseOrderView
//{
//	public int VendorID { get; set; }
//	public int PurchaseOrderID { get; set; }
//	public int EmployeeID { get; set; }
//}
//GOOD DATA Methods
public PurchaseOrderView Create_Valid_Data()
{
	
	PurchaseOrderView PurchaseOrderView = new PurchaseOrderView();

	PurchaseOrderView.VendorID = 3;
	PurchaseOrderView.PurchaseOrderID = 567;
	PurchaseOrderView.EmployeeID = 5;

	Console.WriteLine("Good Test Data");
	PurchaseOrderView.Dump();
    return PurchaseOrderView;
}


//BAD DATA Methods
public PurchaseOrderView Deleting_Placed_Order()
{
    PurchaseOrderView PurchaseOrderView = new PurchaseOrderView();

    PurchaseOrderView.VendorID = 2;
	PurchaseOrderView.PurchaseOrderID = 571;
	PurchaseOrderView.EmployeeID = 5;


	Console.WriteLine("Bad Test Data");
	PurchaseOrderView.Dump();
    return PurchaseOrderView;

}

public PurchaseOrderView Create_Invalid_Data()
{
	PurchaseOrderView PurchaseOrderView = new PurchaseOrderView();
	PurchaseOrderView.VendorID = 123;
	PurchaseOrderView.PurchaseOrderID = 123;
	PurchaseOrderView.EmployeeID = 123;
	Console.WriteLine("Bad Test Data");
	PurchaseOrderView.Dump();
    return PurchaseOrderView;
}




#endregion