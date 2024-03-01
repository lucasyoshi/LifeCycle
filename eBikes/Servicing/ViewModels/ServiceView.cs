using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicing.ViewModels
{
	public class ServiceView
	{

		public string ServiceName { get; set; }
		public decimal ServiceHours { get; set; }
		public string? ServiceComment { get; set; }
	}
}