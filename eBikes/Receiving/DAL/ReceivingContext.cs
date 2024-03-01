﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Receiving.Entities;

namespace Receiving.DAL
{
    public partial class ReceivingContext : DbContext
    {
        public ReceivingContext()
        {
        }

        public ReceivingContext(DbContextOptions<ReceivingContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Part> Parts { get; set; }
        public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public virtual DbSet<ReceiveOrder> ReceiveOrders { get; set; }
        public virtual DbSet<ReceiveOrderDetail> ReceiveOrderDetails { get; set; }
        public virtual DbSet<ReturnedOrderDetail> ReturnedOrderDetails { get; set; }
        public virtual DbSet<UnorderedPurchaseItemCart> UnorderedPurchaseItemCarts { get; set; }
        public virtual DbSet<Vendor> Vendors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.PostalCode).IsFixedLength();

                entity.Property(e => e.Province).IsFixedLength();

                entity.Property(e => e.SocialInsuranceNumber).IsFixedLength();
            });

            modelBuilder.Entity<Part>(entity =>
            {
                entity.Property(e => e.Refundable)
                    .HasDefaultValueSql("('Y')")
                    .IsFixedLength();

                entity.HasOne(d => d.Vendor)
                    .WithMany(p => p.Parts)
                    .HasForeignKey(d => d.VendorID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartsVendors_VendorID");
            });

            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.PurchaseOrders)
                    .HasForeignKey(d => d.EmployeeID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PurchaseOrdersEmployees_EmployeeID");

                entity.HasOne(d => d.Vendor)
                    .WithMany(p => p.PurchaseOrders)
                    .HasForeignKey(d => d.VendorID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PurchaseOrdersVednors_VendorID");
            });

            modelBuilder.Entity<PurchaseOrderDetail>(entity =>
            {
                entity.HasOne(d => d.Part)
                    .WithMany(p => p.PurchaseOrderDetails)
                    .HasForeignKey(d => d.PartID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PurchaseOrderDetailsParts_PartID");

                entity.HasOne(d => d.PurchaseOrder)
                    .WithMany(p => p.PurchaseOrderDetails)
                    .HasForeignKey(d => d.PurchaseOrderID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PurchaseOrderDetailsPurchaseOrders_PurchaseOrderID");
            });

            modelBuilder.Entity<ReceiveOrder>(entity =>
            {
                entity.HasOne(d => d.PurchaseOrder)
                    .WithMany(p => p.ReceiveOrders)
                    .HasForeignKey(d => d.PurchaseOrderID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReceiveOrdersPurchaseOrders_PurchaseOrderID");
            });

            modelBuilder.Entity<ReceiveOrderDetail>(entity =>
            {
                entity.HasOne(d => d.PurchaseOrderDetail)
                    .WithMany(p => p.ReceiveOrderDetails)
                    .HasForeignKey(d => d.PurchaseOrderDetailID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReceiveOrderDetailsPurchaseOrderDetails_OrderDetailID");

                entity.HasOne(d => d.ReceiveOrder)
                    .WithMany(p => p.ReceiveOrderDetails)
                    .HasForeignKey(d => d.ReceiveOrderID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReceiveOrderDetailsReceiveOrders_ReceiveOrderID");
            });

            modelBuilder.Entity<ReturnedOrderDetail>(entity =>
            {
                entity.HasOne(d => d.PurchaseOrderDetail)
                    .WithMany(p => p.ReturnedOrderDetails)
                    .HasForeignKey(d => d.PurchaseOrderDetailID)
                    .HasConstraintName("FK_ReturnedOrderDetailsPurchaseOrderDetails_OrderDetailID");

                entity.HasOne(d => d.ReceiveOrder)
                    .WithMany(p => p.ReturnedOrderDetails)
                    .HasForeignKey(d => d.ReceiveOrderID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReturnedOrderDetailsReceiveOrders_ReceiveOrderID");
            });

            modelBuilder.Entity<UnorderedPurchaseItemCart>(entity =>
            {
                entity.HasKey(e => e.CartID)
                    .HasName("PK__Unordere__51BCD797D19EBC88");
            });

            modelBuilder.Entity<Vendor>(entity =>
            {
                entity.Property(e => e.PostalCode).IsFixedLength();

                entity.Property(e => e.ProvinceID)
                    .HasDefaultValueSql("('AB')")
                    .IsFixedLength();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}