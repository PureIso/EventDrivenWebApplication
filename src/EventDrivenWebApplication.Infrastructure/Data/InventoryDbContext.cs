using EventDrivenWebApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Data;

/// <summary>
/// Database context for managing inventory items.
/// </summary>
public class InventoryDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the inventory items DbSet.
    /// </summary>
    public DbSet<InventoryItem> InventoryItems { get; set; } = default!;

    /// <summary>
    /// Configures the schema needed for the inventory items.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for the context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            // Primary Key
            entity.HasKey(i => i.Id);

            // Auto-increment for the Id
            entity.Property(i => i.Id)
                .ValueGeneratedOnAdd();

            // ProductID is required
            entity.Property(i => i.ProductId)
                .IsRequired();

            // Quality validation flag with default value
            entity.Property(i => i.QualityValidated)
                .IsRequired()
                .HasDefaultValue(false);

            // Optional field for last quality check time
            entity.Property(i => i.DateTimeLastQualityChecked)
                .IsRequired(false);
        });
    }
}