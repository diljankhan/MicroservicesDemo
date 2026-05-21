using Demo.Services.Orders.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services.Orders.API.Data
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
    }
}
