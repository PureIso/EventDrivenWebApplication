using Microsoft.EntityFrameworkCore;
using EventDrivenWebApplication.Domain.Entities;

/// <summary>
/// Represents the database context for managing order saga states and their history.
/// </summary>
public class OrderSagaDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderSagaDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the <see cref="DbContext"/>.</param>
    public OrderSagaDbContext(DbContextOptions<OrderSagaDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{OrderProcessState}"/> representing the order process states.
    /// </summary>
    public DbSet<OrderProcessState> OrderProcessStates { get; set; } = default!;

    /// <summary>
    /// Gets or sets the <see cref="DbSet{OrderProcessStateHistory}"/> representing the order process state history.
    /// </summary>
    public DbSet<OrderProcessStateHistory> OrderProcessStateHistories { get; set; } = default!;

    /// <summary>
    /// Configures the entity mappings and relationships for the context.
    /// </summary>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/> used to configure the model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderProcessState>(entity =>
        {
            entity.ToTable("OrderProcessStates");

            entity.HasKey(x => x.CorrelationId);

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

            entity.Property(x => x.CurrentState)
                .IsRequired();

            entity.Property(x => x.RowVersion)
                .IsRowVersion();
        });

        modelBuilder.Entity<OrderProcessStateHistory>(entity =>
        {
            entity.ToTable("OrderProcessStateHistories");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CorrelationId)
                .IsRequired();

            entity.Property(x => x.PreviousState)
                .IsRequired();

            entity.Property(x => x.CurrentState)
                .IsRequired();

            entity.Property(x => x.TransitionedAt)
                .IsRequired()
                .HasColumnType("datetime");

            entity.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);
        });
    }
}
