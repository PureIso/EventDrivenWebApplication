using EventDrivenWebApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<InventoryCheckLog> InventoryCheckLogs { get; set; }
}