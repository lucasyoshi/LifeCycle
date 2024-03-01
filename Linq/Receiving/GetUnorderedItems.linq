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
		List<UnorderedItemView> cart = new();
		
		cart = GetUnorderedItems();
		
		if (cart.Count() <= 0)
		{
			Console.WriteLine("There are no Unordered Items in the database.");
		}
		else
		{
			Console.WriteLine("Fetch Unordered Items");
			cart.Dump();
		}
	
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

public List<UnorderedItemView> GetUnorderedItems() 
{
	return UnorderedPurchaseItemCarts
		.Select(x => new UnorderedItemView
		{
			CartId = x.CartID,
			ItemDescription = x.Description,
			VendorPartNumber = x.VendorPartNumber,
			Quantity = x.Quantity
		}).ToList();
}