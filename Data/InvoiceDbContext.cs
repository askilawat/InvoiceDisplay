using Microsoft.EntityFrameworkCore;
using BuggyApp.Models;

namespace BuggyApp.Data;

public class InvoiceDbContext : DbContext
{
    public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(e => e.CustomerID);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Contact).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(200);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(e => e.InvoiceID);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Total).HasColumnType("decimal(10,2)");
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Invoices)
                  .HasForeignKey(e => e.CustomerID)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Items");
            entity.HasKey(e => e.ItemID);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("InvoiceItems");
            entity.HasKey(e => e.InvoiceItemID);
            entity.Property(e => e.Quantity).IsRequired().HasDefaultValue(1);
            entity.HasOne(e => e.Invoice)
                  .WithMany(i => i.Items)
                  .HasForeignKey(e => e.InvoiceID)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Item)
                  .WithMany(i => i.InvoiceItems)
                  .HasForeignKey(e => e.ItemID)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

