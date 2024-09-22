using EventDrivenWebApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Data;

/// <summary>
/// The DbContext responsible for managing customer-related data.
/// </summary>
public class CustomerDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerDbContext"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options for configuring the context.</param>
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> of customers.
    /// </summary>
    public DbSet<Customer> Customers { get; set; } = default!;

    /// <summary>
    /// Configures the model properties and relationships when creating the database schema.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.HasIndex(c => c.CustomerId)
                  .IsUnique();

            entity.Property(c => c.FirstName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(c => c.LastName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(c => c.Email)
                  .IsRequired();

            entity.HasIndex(c => c.Email)
                  .IsUnique();
        });
    }
}
