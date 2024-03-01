using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receiving.ViewModels
{
    public class UnorderedItemView
    {
        public int CartId { get; set; }
        public string ItemDescription { get; set; }
        public string VendorPartNumber { get; set; }
        public int Quantity { get; set; }
    }
}
