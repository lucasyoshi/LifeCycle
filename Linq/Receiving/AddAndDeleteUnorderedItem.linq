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
		//Good data
		var item = new UnorderedItemView
		{
			ItemDescription = "Wind Breaker",
			Quantity = 10,
			VendorPartNumber = "4587op23"
		};

		//Bad data
		//var item = new UnorderedItemView
		//{
		//	ItemDescription = "Wind Breaker",
		//	Quantity = 0,
		//	VendorPartNumber = ""
		//};

		Console.WriteLine("Unordered Table - before adding");
		UnorderedPurchaseItemCarts.Dump();
		

		Console.WriteLine("Unordered Table - After adding");

		AddUnorderedItem(item);
		UnorderedPurchaseItemCarts.Dump();


		Console.WriteLine("Unordered Table - After removing");
		DeleteUnorderedItem(2);
		UnorderedPurchaseItemCarts.Dump();
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

public void AddUnorderedItem(UnorderedItemView unorderedItem)
{
	List<Exception> errorList = new List<Exception>();

	UnorderedPurchaseItemCart newItems = new();

	if (string.IsNullOrWhiteSpace(unorderedItem.ItemDescription))
	{
		throw new ArgumentNullException("Item Description is missing");
	}

	if (string.IsNullOrWhiteSpace(unorderedItem.VendorPartNumber))
	{
		throw new ArgumentNullException("Vendor Part Number is missing");
	}

	if (unorderedItem.Quantity <= 0)
	{
		throw new ArgumentNullException("Quantity is missing");
	}

	newItems.Description = unorderedItem.ItemDescription;
	newItems.VendorPartNumber = unorderedItem.VendorPartNumber;
	newItems.Quantity = unorderedItem.Quantity;

	UnorderedPurchaseItemCarts.Add(newItems);

	SaveChanges();
}

public void DeleteUnorderedItem(int cartId)
{
	var deleteItem = UnorderedPurchaseItemCarts.Where(x => x.CartID == cartId).FirstOrDefault();

	UnorderedPurchaseItemCarts.Remove(deleteItem);

	SaveChanges();
}

// You can define other methods, fields, classes and namespaces here