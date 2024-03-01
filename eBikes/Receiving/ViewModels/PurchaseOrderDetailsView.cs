using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receiving.ViewModels
{
    public class PurchaseOrderDetailsView
    {
        public int PurchaseOrderDetailID { get; set; }
        public int PartID { get; set; }
        public string PartDescription { get; set; }
        public int OriginalQty { get; set; }
        public int OutstandingQty { get; set; }
        public int ReceivedQty { get; set; }
        public int ReturnQty { get; set; }
        public string ReturnReason { get; set; }
    }
}
