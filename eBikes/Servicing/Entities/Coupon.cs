﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Servicing.Entities
{
    [Index("CouponIDValue", Name = "UQ_Coupons_CouponIDValue", IsUnique = true)]
    public partial class Coupon
    {
        public Coupon()
        {
            JobDetails = new HashSet<JobDetail>();
        }

        [Key]
        public int CouponID { get; set; }
        [Required]
        [StringLength(10)]
        public string CouponIDValue { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime EndDate { get; set; }
        public int CouponDiscount { get; set; }
        public int SalesOrService { get; set; }

        [InverseProperty("Coupon")]
        public virtual ICollection<JobDetail> JobDetails { get; set; }
    }
}