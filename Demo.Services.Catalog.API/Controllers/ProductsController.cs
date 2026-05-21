using Demo.Services.Catalog.API.Data;
using Demo.Services.Catalog.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Services.Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    { 
        private readonly CatalogDbContext _context;

        public ProductsController(CatalogDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        }

        // This endpoint will be requested by the Orders Service over HTTP later!
        [HttpGet("{id}/price")]
        public async Task<IActionResult> GetProductPrice(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product.Price);
        }
    }
}
