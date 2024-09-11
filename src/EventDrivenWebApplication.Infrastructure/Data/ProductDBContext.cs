using EventDrivenWebApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
    : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.ProductId)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            // Specify precision and scale for the Price property
            // Precision 18, Scale 2 (for up to 999,999,999,999,999.99)
            .HasPrecision(18, 2); 
    }
}