using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Inventory.API.Data;

public class ProductDBContext : DbContext
{
    public ProductDBContext() { }
    public ProductDBContext(DbContextOptions options) : base(options) { }
    public DbSet<Product> Product { get; set; }
}
