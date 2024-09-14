using Microsoft.EntityFrameworkCore;
using EventDrivenWebApplication.Domain.Entities;

namespace EventDrivenWebApplication.Infrastructure.Data;

public class OrderSagaDbContext : DbContext
{
    // Inject state machine and DbContext options
    public OrderSagaDbContext(DbContextOptions<OrderSagaDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderProcessState> OrderProcessStates { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderProcessState>(entity =>
        {
            entity.ToTable("OrderProcessStates");

            entity.HasKey(x => x.CorrelationId);

            // Map saga properties
            entity.Property(x => x.CorrelationId)
                .IsRequired();

            entity.Property(x => x.ProductId)
                .IsRequired();

            entity.Property(x => x.ProductName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.ProductQuantity)
                .IsRequired();

            entity.Property(x => x.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(x => x.IsQualityGood)
                .IsRequired();

            entity.Property(x => x.DateTimeProductCreated)
                .IsRequired()
                .HasColumnType("datetime");

            entity.Property(x => x.DateTimeInventoryCheckRequested)
                .IsRequired()
                .HasColumnType("datetime");

            entity.Property(x => x.DateTimeInventoryCheckCompleted)
                .IsRequired()
                .HasColumnType("datetime");

            // Store CurrentState as an integer
            entity.Property(x => x.CurrentState)
                .IsRequired();

            // Configure RowVersion for concurrency control
            entity.Property(x => x.RowVersion)
                .IsRowVersion();
        });
    }
}