using System.Collections.Generic;
using Demo.Services.Catalog.API.Models;
using Microsoft.EntityFrameworkCore;


namespace Demo.Services.Catalog.API.Data
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

    }
}
