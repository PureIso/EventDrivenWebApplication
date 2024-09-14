using EventDrivenWebApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Data;

/// <summary>
/// DbContext for managing product entities.
/// </summary>
public class ProductDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for the DbContext.</param>
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the DbSet of products.
    /// </summary>
    public DbSet<Product> Products { get; set; } = default!;

    /// <summary>
    /// Configures the entity models and relationships for the DbContext.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure the model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            // Set primary key and configure Id as auto-generated
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            // Configure unique constraint for CorrelationId
            entity.HasIndex(p => p.CorrelationId)
                .IsUnique();

            // Configure unique constraint for Name
            entity.HasIndex(p => p.Name)
                .IsUnique();

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Quantity)
                .HasDefaultValue(0)
                .HasAnnotation("Range", new { Min = 0, Max = int.MaxValue });

            entity.Property(p => p.Price)
                .HasPrecision(18, 2)
                .HasAnnotation("Range", new { Min = 0.0m, Max = double.MaxValue });

            entity.Property(p => p.DateTimeCreated)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(p => p.DateTimeLastModified)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}