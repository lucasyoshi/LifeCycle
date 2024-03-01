﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Servicing.Entities
{
    public partial class StandardJob
    {
        [Key]
        public int StandardJobID { get; set; }
        [Required]
        [StringLength(100)]
        [Unicode(false)]
        public string Description { get; set; }
        [Column(TypeName = "decimal(5, 2)")]
        public decimal StandardHours { get; set; }
    }
}