using Demo.Services.Orders.API.Data;
using Demo.Services.Orders.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Demo.Services.Orders.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RMQOrdersController : ControllerBase
    {

        private readonly OrdersDbContext _context;
        
        public RMQOrdersController(OrdersDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRMQOrder(int customerId, int productId)
        {
            // 1. Validate customer existence from local synced table
            var localCustomerRef = await _context.CustomerReferences.FindAsync(customerId);
            if (localCustomerRef == null)
            {
                return BadRequest($"Validation Failed: Customer with ID {customerId} does not exist in local Orders cache.");
            }

            // 2. Validate product existence and price from local synced table
            var localProductRef = await _context.ProductReferences.FindAsync(productId);
            if (localProductRef == null)
            {
                return BadRequest($"Validation Failed: Product with ID {productId} does not exist in local Orders cache.");
            }

            // 3. Create and save the order completely offline
            var order = new Order
            {
                CustomerId = customerId,
                ProductId = productId,
                TotalAmount = localProductRef.Price,
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(order);
        }

    }
}
