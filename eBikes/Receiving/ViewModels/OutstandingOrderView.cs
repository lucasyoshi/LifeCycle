﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receiving.ViewModels
{
    public class OutstandingOrderView
    {
        public int PurchaseOrderID { get; set; }
        public DateTime? OrderDate { get; set; }
        public string VendorName { get; set; }
        public string VendorContact { get; set; }
    }
}
