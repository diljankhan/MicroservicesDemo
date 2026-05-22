using System.Text;
using System.Text.Json;
using Demo.Services.Orders.API.Data;
using Demo.Services.Orders.API.Models;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Demo.Services.Orders.API.Messaging
{
    public class RabbitMQCustomerConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQCustomerConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(queue: "customer_created_queue",
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null,
                                             cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var data = JsonSerializer.Deserialize<CustomerCreatedMessage>(messageJson);

                if (data != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                    var existingCustomer = await dbContext.CustomerReferences.FindAsync(data.CustomerId);
                    if (existingCustomer == null)
                    {
                        var newCustomerRef = new CustomerReference { Id = data.CustomerId };
                        dbContext.CustomerReferences.Add(newCustomerRef);
                        await dbContext.SaveChangesAsync();
                    }
                }
            };

            await _channel.BasicConsumeAsync(queue: "customer_created_queue",
                                             autoAck: true,
                                             consumer: consumer,
                                             cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }

    public class CustomerCreatedMessage
    {
        public int CustomerId { get; set; }
    }
}
