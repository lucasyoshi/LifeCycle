using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicing.ViewModels
{
	public class StandardServiceView
	{
		public int StandardServiceID { get; set; }
		public string Description { get; set; }
		public decimal StandardHours { get; set; }
	}
}