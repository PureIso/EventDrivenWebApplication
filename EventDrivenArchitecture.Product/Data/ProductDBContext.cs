using Microsoft.EntityFrameworkCore;

namespace EventDrivenArchitecture.Inventory.Data;

public class ProductDBContext : DbContext
{
    public ProductDBContext() { }
    public ProductDBContext(DbContextOptions options) : base(options) { }
    public DbSet<Product> Product { get; set; }
}
