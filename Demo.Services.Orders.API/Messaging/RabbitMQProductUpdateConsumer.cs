using System.Text;
using System.Text.Json;
using Demo.Services.Orders.API.Data;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Demo.Services.Orders.API.Messaging
{
    public class RabbitMQProductUpdateConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public RabbitMQProductUpdateConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "DKM82@rabbitmq" // <-- Matching your custom password!
            };

            using var connection = await factory.CreateConnectionAsync(stoppingToken);
            using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            // 1. Declare the exact same queue name used by the publisher
            await channel.QueueDeclareAsync(
                queue: "product_updated_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                try
                {
                    // 2. Parse the incoming json update data
                    using var doc = JsonDocument.Parse(messageJson);
                    var root = doc.RootElement;

                    int productId = root.GetProperty("ProductId").GetInt32();
                    decimal price = root.GetProperty("Price").GetDecimal();

                    // 3. Open a database scope to update the local Orders DB reference table
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                    var localProductRef = await dbContext.ProductReferences.FindAsync(productId);
                    if (localProductRef != null)
                    {
                        // Update the cached price to match the new Catalog pricing
                        localProductRef.Price = price;
                        await dbContext.SaveChangesAsync();
                        Console.WriteLine($"[Orders API Cache Sync] Product {productId} updated successfully to price: {price}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error Syncing Product Update]: {ex.Message}");
                }
            };

            await channel.BasicConsumeAsync(
                queue: "product_updated_queue",
                autoAck: true,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            // Keep the background task alive listening to RabbitMQ
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
