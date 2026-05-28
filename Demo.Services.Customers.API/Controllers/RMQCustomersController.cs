using System.Text;
using System.Text.Json;
using Demo.Services.Customers.API.Data;
using Demo.Services.Customers.API.Models;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace Demo.Services.Customers.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RMQCustomersController : ControllerBase
    {
        private readonly CustomersDbContext _context;

        public RMQCustomersController(CustomersDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRMQCustomer(string fullName, string email)
        {
            // 1. Save to local Customers DB
            var customer = new Customer
            {
                FullName = fullName,
                Email = email
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // 2. Publish "CustomerCreated" event to RabbitMQ
            await PublishCustomerCreatedEventAsync(customer.Id);

            return Ok(customer);
        }

        private async Task PublishCustomerCreatedEventAsync(int id)
        {
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

            // Declare a brand new queue for customers
            await channel.QueueDeclareAsync(queue: "customer_created_queue",
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            var messageData = new { CustomerId = id };
            var messageJson = JsonSerializer.Serialize(messageData);
            var body = Encoding.UTF8.GetBytes(messageJson);

            await channel.BasicPublishAsync(exchange: "",
                                            routingKey: "customer_created_queue",
                                            body: body);
        }


    }
}
