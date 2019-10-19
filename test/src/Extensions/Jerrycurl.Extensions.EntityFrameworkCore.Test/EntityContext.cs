using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Extensions.EntityFrameworkCore.Test.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jerrycurl.Extensions.EntityFrameworkCore.Test
{
    public class EntityContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlite("whatever");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Address>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Street)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.BillingAddress)
                    .WithMany(p => p.OrderBillingAddress)
                    .HasForeignKey(d => d.BillingAddressId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Order_Address__Billing");

                entity.HasOne(d => d.ShippingAddress)
                    .WithMany(p => p.OrderShippingAddress)
                    .HasForeignKey(d => d.ShippingAddressId)
                    .HasConstraintName("FK_Order_Address__Shipping");
            });

            modelBuilder.Entity<OrderLine>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Product)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderLine)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderLine_Order");
            });
        }
    }
}
