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
		int poID = 458;

		podList = GetOrderDetails(poID);
		
		Console.WriteLine("Fetch purchase order details");
		podList.Dump();
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
