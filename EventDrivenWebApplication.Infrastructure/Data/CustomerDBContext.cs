using EventDrivenWebApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Data;

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.CustomerId)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Customer>()
            .Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Domain.Entities.Customer>()
            .Property(c => c.Email)
            .IsRequired();

        modelBuilder.Entity<Domain.Entities.Customer>()
            .HasIndex(c => c.Email)
            .IsUnique();
    }
}