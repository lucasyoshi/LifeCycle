﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SalesAndReturns.Entities
{
    internal partial class SaleRefund
    {
        public SaleRefund()
        {
            SaleRefundDetails = new HashSet<SaleRefundDetail>();
        }

        public int SaleRefundID { get; set; }
        public DateTime SaleRefundDate { get; set; }
        public int SaleID { get; set; }
        public int EmployeeID { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal SubTotal { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Sale Sale { get; set; }
        public virtual ICollection<SaleRefundDetail> SaleRefundDetails { get; set; }
    }
}