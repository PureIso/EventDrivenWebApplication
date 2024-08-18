using Microsoft.EntityFrameworkCore;

namespace EventDrivenArchitecture.Customer.Data
{
    public class CustomerDBContext : DbContext
    {
        public CustomerDBContext() { }
        public CustomerDBContext(DbContextOptions options) : base(options) { }
        public DbSet<Customer> Customer { get; set; }
    }
}