using Demo.Services.Customers.API.Data;
using Demo.Services.Customers.API.Models;
using Microsoft.AspNetCore.Mvc;



using Microsoft.EntityFrameworkCore;

namespace Demo.Services.Customers.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomersDbContext _context;

        public CustomersController(CustomersDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return Ok(customer);
        }

        // This validation endpoint will be hit by the Orders Service over HTTP later!
        [HttpGet("{id}/exists")]
        public async Task<IActionResult> CheckCustomerExists(int id)
        {
            var exists = await _context.Customers.AnyAsync(c => c.Id == id);
            return Ok(exists); // Returns true or false
        }
    }
}
