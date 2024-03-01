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
		// Good data
		int PurchaseOrderID = 458;
		string closeReason = "Vendor out of business";


		// Bad data
		//int PurchaseOrderID = 567;
		//string closeReason = "1";

		Console.WriteLine("Parts before");

		PurchaseOrderDetails
		.Where(pod => pod.PurchaseOrderID == PurchaseOrderID)
		.Select(pod => pod.Part)
		.Dump();

		Console.WriteLine("Purchase Order before");
		PurchaseOrders.Where(po => po.PurchaseOrderID == 458).Select(po => po).Dump();
		
		ForceClose(PurchaseOrderID, closeReason);
		
		Console.WriteLine("Parts updated");
		
		PurchaseOrderDetails
		.Where(pod => pod.PurchaseOrderID == PurchaseOrderID)
		.Select(pod => pod.Part)
		.Dump();
		
		Console.WriteLine("Purchase Order updated");
		PurchaseOrders.Where(po => po.PurchaseOrderID == 458).Select(po => po).Dump();
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

#region Method

private Exception GetInnerException(Exception ex)
{
	while (ex.InnerException != null)
	{
		ex = ex.InnerException;
	}
	return ex;
}
#endregion

public void ForceClose(int purchaseOrderId, string closeReason)
{
	List<Exception> errorList = new List<Exception>();


	if (purchaseOrderId <= 0 || string.IsNullOrWhiteSpace(closeReason))
	{
		throw new ArgumentNullException("Purchase Order ID and/or Reason for closing is missing");
	}

	var closingPO = PurchaseOrders
					.Where(po => po.PurchaseOrderID == purchaseOrderId)
					.FirstOrDefault();
	if (closingPO == null)
	{
		errorList.Add(new Exception("There is no purchase order for this ID."));
	}

	var poDetails = PurchaseOrderDetails
					.Where(pod => pod.PurchaseOrderID == purchaseOrderId)
					.Select(pod => pod)
					.ToList();


	foreach (var item in poDetails)
	{
		var part = Parts.Where(x => x.PartID == item.PartID).FirstOrDefault();
		
		if(part.QuantityOnOrder < (item.Quantity - item.ReceiveOrderDetails.Sum(x => x.QuantityReceived)))
		{
			errorList.Add(new Exception("Quantity on Order (Parts) can not be lower than the Outstanding Quantity"));
		}
		part.QuantityOnOrder = part.QuantityOnOrder - (item.Quantity - item.ReceiveOrderDetails.Sum(x => x.QuantityReceived));
	}


	closingPO.Closed = true;
	closingPO.Notes = closeReason;

	if (errorList.Count() > 0)
	{
		throw new AggregateException("Unable to add the Force Close, check concerns", errorList);
	}
	else
	{
		var cart = UnorderedPurchaseItemCarts.Select(x => x).ToList();
		UnorderedPurchaseItemCarts.RemoveRange(cart);
		SaveChanges();
	}
}

// You can define other methods, fields, classes and namespaces here