using System;

namespace Servicing
{
	public class VehicleView
	{
		public string VehicleName { get; set; }
		public string VIN { get; set; }
		public int CustomerID { get; set; }
		public List<ServiceView> Services { get; set; }
	}
}