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
		List<OutstandingOrderView> outstandingOrders = new ();
		
		outstandingOrders = GetOutstandingOrders();
		
		Console.WriteLine("Fetch Outstanding Orders");
		outstandingOrders.Dump();
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

public List<OutstandingOrderView> GetOutstandingOrders()
{
	return PurchaseOrders
	.Where(x => x.Closed == false)
	.Select(x => new OutstandingOrderView
	{
		PurchaseOrderID = x.PurchaseOrderID,
		OrderDate =  x.OrderDate,
		VendorName = x.Vendor.VendorName,
		VendorContact = x.Vendor.Phone
	}).ToList();
}
// You can define other methods, fields, classes and namespaces here