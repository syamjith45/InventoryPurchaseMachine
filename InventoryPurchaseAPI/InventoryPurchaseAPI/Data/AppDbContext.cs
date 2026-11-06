using InventoryPurchaseAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryPurchaseAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
          : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Tax> Taxes { get; set; }

        public DbSet<ProductTax> ProductTaxes { get; set; }

        public DbSet<Purchase> Purchases { get; set; }

        public DbSet<PurchaseItem> PurchaseItems { get; set; }

        public DbSet<Inventory> Inventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ProductTax junction table with composite primary key
            modelBuilder.Entity<ProductTax>()
                .HasKey(pt => new { pt.ProductId, pt.TaxId });

            // Configure foreign key relationship to Product
            modelBuilder.Entity<ProductTax>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTaxes)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure foreign key relationship to Tax
            modelBuilder.Entity<ProductTax>()
                .HasOne(pt => pt.Tax)
                .WithMany(t => t.ProductTaxes)
                .HasForeignKey(pt => pt.TaxId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Purchase entity with decimal precision
            modelBuilder.Entity<Purchase>()
                .Property(p => p.SubTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Purchase>()
                .Property(p => p.TotalDiscount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Purchase>()
                .Property(p => p.TotalTax)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Purchase>()
                .Property(p => p.GrandTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Purchase>()
                .HasIndex(p => p.InvoiceNumber)
                .IsUnique();

            // Configure Purchase foreign key relationship to Supplier
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Purchases)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure PurchaseItem entity with decimal precision
            modelBuilder.Entity<PurchaseItem>()
                .Property(pi => pi.Quantity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseItem>()
                .Property(pi => pi.PurchaseRate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseItem>()
                .Property(pi => pi.DiscountPercentage)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseItem>()
                .Property(pi => pi.TaxAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseItem>()
                .Property(pi => pi.LineTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseItem>()
                .HasIndex(pi => pi.LotNumber)
                .IsUnique();

            // Configure PurchaseItem foreign key relationship to Purchase
            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Purchase)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PurchaseItem foreign key relationship to Product
            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Inventory entity with decimal precision
            modelBuilder.Entity<Inventory>()
                .Property(i => i.Quantity)
                .HasPrecision(18, 2);

            // Configure Inventory foreign key relationship to Product
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Inventories)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Create unique constraint for ProductId + LotNumber in Inventory
            modelBuilder.Entity<Inventory>()
                .HasIndex(i => new { i.ProductId, i.LotNumber })
                .IsUnique();
        }
    }

}
