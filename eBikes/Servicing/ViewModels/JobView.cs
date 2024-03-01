using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicing.ViewModels
{
    public class JobView
    {
        public int EmployeeID { get; set; } = 1;
        public decimal ShopRate { get; set; } = (decimal)65.50;
        public string VIN { get; set; }
        public decimal TaxAmount { get; set; } = 5;
        public decimal SubTotal { get; set; } = 0;
        public string? CouponIDValue { get; set; }
    }
}
