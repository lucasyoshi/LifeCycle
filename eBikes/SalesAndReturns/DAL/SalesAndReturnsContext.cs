﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SalesAndReturns.Entities;

namespace SalesAndReturns.DAL
{
    internal partial class SalesAndReturnsContext : DbContext
    {
        public SalesAndReturnsContext()
        {
        }

        public SalesAndReturnsContext(DbContextOptions<SalesAndReturnsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Coupon> Coupons { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Part> Parts { get; set; }
        public virtual DbSet<Sale> Sales { get; set; }
        public virtual DbSet<SaleDetail> SaleDetails { get; set; }
        public virtual DbSet<SaleRefund> SaleRefunds { get; set; }
        public virtual DbSet<SaleRefundDetail> SaleRefundDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Latin1_General_CI_AS");

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.HasIndex(e => e.CouponIDValue, "UQ_Coupons_CouponIDValue")
                    .IsUnique();

                entity.Property(e => e.CouponIDValue)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Address)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.City)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPhone)
                    .IsRequired()
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.EmailAddress)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.PostalCode)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Province)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.SocialInsuranceNumber)
                    .IsRequired()
                    .HasMaxLength(9)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            modelBuilder.Entity<Part>(entity =>
            {
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.PurchasePrice).HasColumnType("smallmoney");

                entity.Property(e => e.Refundable)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('Y')")
                    .IsFixedLength();

                entity.Property(e => e.SellingPrice).HasColumnType("smallmoney");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Parts)
                    .HasForeignKey(d => d.CategoryID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartsCategories_CategoryID");
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.Property(e => e.PaymentType)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsFixedLength();

                entity.Property(e => e.SaleDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SubTotal).HasColumnType("money");

                entity.Property(e => e.TaxAmount).HasColumnType("money");

                entity.HasOne(d => d.Coupon)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.CouponID)
                    .HasConstraintName("FK_SalesCoupons_CouponID");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.EmployeeID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SalesEmployees_EmployeeID");
            });

            modelBuilder.Entity<SaleDetail>(entity =>
            {
                entity.HasIndex(e => new { e.SaleID, e.PartID }, "UQ_SaleDetails_SaleIDPartID")
                    .IsUnique();

                entity.Property(e => e.SellingPrice).HasColumnType("money");

                entity.HasOne(d => d.Part)
                    .WithMany(p => p.SaleDetails)
                    .HasForeignKey(d => d.PartID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SaleDetailsParts_PartID");

                entity.HasOne(d => d.Sale)
                    .WithMany(p => p.SaleDetails)
                    .HasForeignKey(d => d.SaleID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SaleDetailsSalesSaleID");
            });

            modelBuilder.Entity<SaleRefund>(entity =>
            {
                entity.Property(e => e.SaleRefundDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.SubTotal).HasColumnType("money");

                entity.Property(e => e.TaxAmount).HasColumnType("money");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.SaleRefunds)
                    .HasForeignKey(d => d.EmployeeID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SaleRefundsEmployees_EmployeeID");

                entity.HasOne(d => d.Sale)
                    .WithMany(p => p.SaleRefunds)
                    .HasForeignKey(d => d.SaleID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("CK_SaleRefundsSales_SaleID");
            });

            modelBuilder.Entity<SaleRefundDetail>(entity =>
            {
                entity.HasIndex(e => new { e.SaleRefundID, e.PartID }, "UQ_SaleRefundDetails_SaleRefundIDPartID")
                    .IsUnique();

                entity.Property(e => e.Reason).HasMaxLength(150);

                entity.Property(e => e.SellingPrice).HasColumnType("money");

                entity.HasOne(d => d.Part)
                    .WithMany(p => p.SaleRefundDetails)
                    .HasForeignKey(d => d.PartID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SaleRefundDetailsParts_PartId");

                entity.HasOne(d => d.SaleRefund)
                    .WithMany(p => p.SaleRefundDetails)
                    .HasForeignKey(d => d.SaleRefundID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SaleRefundDetailsSaleRefunds_SaleRefundID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}