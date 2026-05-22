using Demo.Services.Orders.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services.Orders.API.Data
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }

        public DbSet<ProductReference> ProductReferences { get; set; } // Local cache copy
        public DbSet<CustomerReference> CustomerReferences { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tell EF Core that Id is not an Identity column here
            modelBuilder.Entity<ProductReference>().Property(p => p.Id).ValueGeneratedNever();             
          
            modelBuilder.Entity<CustomerReference>().Property(c => c.Id).ValueGeneratedNever();
        }

        

       
    }
}
