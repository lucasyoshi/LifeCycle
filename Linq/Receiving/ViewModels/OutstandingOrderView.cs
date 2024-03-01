using System;

namespace Receiving
{
	public class OutstandingOrderView
	{
		public int PurchaseOrderID { get; set; }
		public DateTime? OrderDate { get; set; }
		public string VendorName { get; set; }
		public string VendorContact { get; set; }
	}
}