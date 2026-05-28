using System.Text;
using System.Text.Json;
using Demo.Services.Catalog.API.Data;
using Demo.Services.Catalog.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed; // Make sure this is added at the top!
using RabbitMQ.Client;

namespace Demo.Services.Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RMQProductsController : ControllerBase
    {
        #region varibales
        private readonly CatalogDbContext _context;
        private readonly IDistributedCache _cache; // Inject the Redis Cache interface //// 1. Redis -- Added this for Redis
        #endregion

        #region constructor
        // 2. Redis -- Update your constructor to accept BOTH the DB context and Redis Cache
        public RMQProductsController(CatalogDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        #endregion

        #region Methods

        #region Create
        [HttpPost]
        public async Task<IActionResult> CreateRMQProduct(string name, decimal price)
        {
            // 1. Save to local Catalog DB
            var product = new Product { Name = name, Price = price };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // 2. CACHE INVALIDATION: Remove the stale data from Redis RAM
            string cacheKey = "all_products";
            await _cache.RemoveAsync(cacheKey);

            // 3. Publish to RabbitMQ (Keep your existing event code)
            //  Publish "ProductCreated" event to RabbitMQ (Added await here!)
            await PublishProductCreatedEventAsync(product.Id, product.Price);

            return Ok(product);
        }

        #endregion

        #region CreateEvent_RabbitMQ
        private async Task PublishProductCreatedEventAsync(int id, decimal price)
        {
            // Establish connection to local Docker RabbitMQ instance asynchronously
            //var factory = new ConnectionFactory() { HostName = "localhost" };
            //change RabbitMQ password. not default guest and guest
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",             // Or your custom user if you changed the username too
                Password = "DKM82@rabbitmq" // <-- Put your exact new password here
            };
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
        #endregion

        #region update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, string newName, decimal newPrice)
        {
            // 1. Find product in Catalog SQL Database
            var product = await _context.Products.FindAsync(id);
            if (product == null) { return NotFound($"Product with ID {id} not found."); }

            // Update the values
            product.Name = newName;
            product.Price = newPrice;
            await _context.SaveChangesAsync();

            // 2. Clear Redis cache so GET list reflects changes immediately
            string cacheKey = "all_products";
            await _cache.RemoveAsync(cacheKey);

            // 3. Publish update event to RabbitMQ
            await PublishProductUpdatedEventAsync(product);

            return Ok(new { message = "Product updated and event published.", product });
        }
        #endregion

        #region UpdateEvent_RabbitMQ
        private async Task PublishProductUpdatedEventAsync(Product product)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "DKM82@rabbitmq" // <-- Keep your custom password here!
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Declare a dedicated queue for updates
            await channel.QueueDeclareAsync(
                queue: "product_updated_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Prepare payload using your new event class
            var updateEvent = new Demo.Services.Catalog.API.Events.ProductUpdatedEvent
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price
            };

            var messageJson = JsonSerializer.Serialize(updateEvent);
            var body = Encoding.UTF8.GetBytes(messageJson);

            // Publish to the update queue
            await channel.BasicPublishAsync(exchange: "", routingKey: "product_updated_queue", body: body);
        }
        #endregion

        #region GetAll
        // 3. Redis -- Just drop this brand new function right at the bottom!
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            string cacheKey = "all_products";

            // Check RAM (Redis) first
            var cachedProductsJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedProductsJson))
            {
                // Cache Hit! Read from RAM instantly
                var cachedProducts = JsonSerializer.Deserialize<List<Product>>(cachedProductsJson);
                return Ok(new { source = "Redis Cache (RAM)", data = cachedProducts });
            }

            // Cache Miss! Fallback to SQL Server Disk
            var databaseProducts = await _context.Products.ToListAsync();

            // Save into Redis so the next person gets it instantly
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
            var productsJsonString = JsonSerializer.Serialize(databaseProducts, jsonOptions);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) // Cache lasts for 60 seconds
            };

            await _cache.SetStringAsync(cacheKey, productsJsonString, cacheOptions);

            return Ok(new { source = "SQL Server Database (Disk)", data = databaseProducts });
        }
        #endregion

        #endregion
    }

}
