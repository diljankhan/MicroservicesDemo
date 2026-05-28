using System.Text;
using System.Text.Json;
using Demo.Services.Orders.API.Data;
using Demo.Services.Orders.API.Models;
//using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Demo.Services.Orders.API.Messaging
{
    public class RabbitMQProductConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQProductConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //var factory = new ConnectionFactory() { HostName = "localhost" };

            //change RabbitMQ password. not default guest and guest
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",             // Or your custom user if you changed the username too
                Password = "DKM82@rabbitmq" // <-- Put your exact new password here
            };

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(queue: "product_created_queue",
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null,
                                             cancellationToken: stoppingToken);

            // Setup the event consumer
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                // Extract the data from the message payload
                var data = JsonSerializer.Deserialize<ProductCreatedMessage>(messageJson);

                if (data != null)
                {
                    // BackgroundServices are singletons, so we must manually create a scope
                    // to inject our scoped Database Context safely.
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                    // Check if we already have this product synced
                    var existingProduct = await dbContext.ProductReferences.FindAsync(data.ProductId);
                    if (existingProduct == null)
                    {
                        var newProductRef = new ProductReference
                        {
                            Id = data.ProductId,
                            Price = data.Price
                        };
                        dbContext.ProductReferences.Add(newProductRef);
                        await dbContext.SaveChangesAsync();
                    }
                }
            };

            // Tell RabbitMQ to start delivering messages to this background worker
            await _channel.BasicConsumeAsync(queue: "product_created_queue",
                                             autoAck: true,
                                             consumer: consumer,
                                             cancellationToken: stoppingToken);

            // Keep running silently in the background
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }

    }

    // Simple helper DTO matching the JSON structure sent by the Catalog Service
    public class ProductCreatedMessage
    {
        public int ProductId { get; set; }
        public decimal Price { get; set; }
    }
}
