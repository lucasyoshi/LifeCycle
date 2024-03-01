namespace Purchasing
{
	public class ItemView
	{
		public int PurchaseOrderID { get; set; }
		public int PurchaseOrderNum { get; set; }
		public int PartID { get; set; }
		public string PartDescription { get; set; }
		public int QOH { get; set; }
		public int ROL { get; set; }
		public int QOO { get; set; }
		public int Buffer { get; set; }
		public decimal Price { get; set; }
	}
}
