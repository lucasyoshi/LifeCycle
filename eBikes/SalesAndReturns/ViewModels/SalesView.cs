namespace SalesAndReturns.ViewModels
{
	public class SalesView
	{
		public int EmployeeID { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal SubTotal { get; set; }
		public int CouponId { get; set; }
		public int DiscountPercent { get; set; }
		public string PaymentType { get; set; }
	}
}