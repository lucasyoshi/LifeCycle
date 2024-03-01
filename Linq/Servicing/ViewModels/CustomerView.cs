using System;

namespace Servicing
{
	public class CustomerView
	{
		public int CustomerID { get; set; }
		public string CustomerName { get; set; }
		public int PhoneNumber { get; set; }
		public string Address { get; set; }
		public List<VehicleView> Vehicles { get; set; }
	}
}