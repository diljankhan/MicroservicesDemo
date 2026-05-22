using System.Collections.Generic;
using Demo.Services.Customers.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services.Customers.API.Data
{
    public class CustomersDbContext : DbContext
    {
        public CustomersDbContext(DbContextOptions<CustomersDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }



    }
}
