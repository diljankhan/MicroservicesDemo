using Demo.Services.Orders.API.Data;
using Demo.Services.Orders.API.Models;
using Demo.Services.Orders.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Services.Orders.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersDbContext _context;
        private readonly CustomerHttpService _customerService;
        private readonly CatalogHttpService _catalogService;
        
        public OrdersController(OrdersDbContext context,CustomerHttpService customerService,CatalogHttpService catalogService)

        {
            _context = context;
            _customerService = customerService;
            _catalogService = catalogService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(int customerId, int productId)
        {
            // 1. Cross-network validation: Check if customer exists in Customer API
            bool customerExists = await _customerService.CheckCustomerExistsAsync(customerId);
            if (!customerExists)
            {
                return BadRequest($"Validation Failed: Customer with ID {customerId} does not exist inside Customer DB.");
            }

            // 2. Cross-network validation: Fetch pricing information from Catalog API
            decimal? productPrice = await _catalogService.GetProductPriceAsync(productId);
            if (productPrice == null)
            {
                return BadRequest($"Validation Failed: Product with ID {productId} does not exist inside Catalog DB.");
            }            

            // 3. Execution: If both evaluations pass, save the data to Orders DB
            var order = new Order
            {
                CustomerId = customerId,
                ProductId = productId,
                TotalAmount = productPrice.Value,                
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}
