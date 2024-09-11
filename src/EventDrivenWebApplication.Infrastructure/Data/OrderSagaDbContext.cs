using Microsoft.EntityFrameworkCore;
using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Infrastructure.Sagas;

namespace EventDrivenWebApplication.Infrastructure.Data;

public class OrderSagaDbContext : DbContext
{
    private readonly OrderProcessStateMachine _stateMachine;

    // Inject the state machine into the DbContext
    public OrderSagaDbContext(DbContextOptions<OrderSagaDbContext> options, OrderProcessStateMachine stateMachine)
        : base(options)
    {
        _stateMachine = stateMachine;
    }

    public DbSet<OrderProcessState> OrderProcessStates { get; set; }

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

            entity.Property(x => x.OrderId)
                .IsRequired();

            entity.Property(x => x.ProductId)
                .IsRequired();

            entity.Property(x => x.IsInventoryAvailable)
                .IsRequired();

            entity.Property(x => x.IsInventoryChecked)
                .IsRequired();

            entity.Property(x => x.InventoryCheckedAt)
                .HasColumnType("datetime");

            // Store MassTransit.State as a string (State.Name)
            entity.Property(x => x.CurrentState)
                .HasConversion(
                    state => state.Name,
                    name => _stateMachine.GetState(name)
                )
                .IsRequired();

            // Configure RowVersion for concurrency control
            entity.Property(x => x.RowVersion)
                .IsRowVersion();
        });
    }
}