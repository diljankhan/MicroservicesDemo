using System.Text;
using System.Text.Json;
using Demo.Services.Catalog.API.Data;
using Demo.Services.Catalog.API.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

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

        //without RabbitMQ
        //[HttpPost]
        //public async Task<IActionResult> CreateProduct(Product product)
        //{
        //    _context.Products.Add(product);
        //    await _context.SaveChangesAsync();
        //    return Ok(product);
        //}


        //update code for RabbitMQ Implementation
        [HttpPost]
        public async Task<IActionResult> CreateProduct(string name, decimal price)
        {
            // 1. Save to local Catalog DB
            var product = new Product { Name = name, Price = price };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // 2. Publish "ProductCreated" event to RabbitMQ (Added await here!)
            await PublishProductCreatedEventAsync(product.Id, product.Price);

            return Ok(product);
        }

        private async Task PublishProductCreatedEventAsync(int id, decimal price)
        {
            // Establish connection to local Docker RabbitMQ instance asynchronously
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Declare a queue named "product_created_queue"
            await channel.QueueDeclareAsync(queue: "product_created_queue",
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            // Create the message payload
            var messageData = new { ProductId = id, Price = price };
            var messageJson = JsonSerializer.Serialize(messageData);
            var body = Encoding.UTF8.GetBytes(messageJson);

            // Send message to the queue asynchronously
            await channel.BasicPublishAsync(exchange: "",
                                            routingKey: "product_created_queue",
                                            body: body);
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
