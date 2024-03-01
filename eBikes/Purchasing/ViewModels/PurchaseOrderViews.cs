#nullable disable
using System.Collections.Generic;
namespace Purchasing.ViewModels
{
	public class PurchaseOrderView
	{
		public int PurchaseOrderID { get; set; }
		public int PurchaseOrderNum { get; set; }
		public int VendorID { get; set; } = 0;
		public decimal SubTotal { get; set; }
		public decimal GST { get; set; }
        public decimal Total { get; set; }

        public int EmployeeID { get; set; }
		public List<PurchaseOrderDetailView> PurchaseOrderDetails { get; set; } = new();
	}
	
		public class PurchaseOrderDetailView
	{
		public int PartID {get; set;}
		public string PartDescription { get; set; }
		public int QOH { get; set; }
		public int ROL { get; set; }
		public int QOO { get; set; }
		public int QTO { get; set; }
		public decimal Price { get; set; }
	}
}
