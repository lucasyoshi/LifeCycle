using System;

namespace Servicing
{
	public class ServiceView
	{
		public int ServiceID { get; set; }
		public string ServiceName { get; set; }
		public decimal ServiceHours { get; set; }
		public string ServiceComment { get; set; }
		public int CouponID { get; set; }
		public int EmployeeID { get; set; }
		public string VIN { get; set; }
	}
}