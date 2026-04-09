using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using ProductService.Api.Data;
using ProductService.Api.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace ProductService.Services
{
    public class RabbitMQConsumer
    {
        private readonly IServiceProvider _serviceProvider;

        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMQConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Start()
        {
            try
            {
                Console.WriteLine("Consumer starting...");

                var factory = new ConnectionFactory()
                {
                    HostName = "rabbitmq",
                    UserName = "guest",
                    Password = "guest"
                };

                // 🔥 retry
                while (_connection == null)
                {
                    try
                    {
                        _connection = factory.CreateConnection();
                        Console.WriteLine("✅ RabbitMQ connection established");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"RabbitMQ not ready, retrying... ({ex.Message})");
                        Thread.Sleep(3000);
                    }
                }

                _channel = _connection.CreateModel();
                Console.WriteLine("✅ Channel created");

                _channel.QueueDeclare("order_created", true, false, false, null);
                Console.WriteLine("✅ Queue declared");

                _channel.QueueDeclare("order_cancelled", true, false, false, null);
                Console.WriteLine("✅ Cancelled queue declared");

                Console.WriteLine("Waiting for orders (polling mode)...");

                // Poll messages every 2 seconds so queue ready count is observable in RabbitMQ UI.
                _ = Task.Run(() =>
                {
                    while (true)
                    {
                        ConsumeOrderCreated();
                        ConsumeOrderCancelled();
                        Thread.Sleep(2000);
                    }
                });

                Console.WriteLine("✅ Polling consumer started for queues: order_created, order_cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fatal error in Consumer.Start(): {ex}");
                throw;
            }
        }

        private void ConsumeOrderCreated()
        {
            if (_channel == null)
                return;

            var result = _channel.BasicGet("order_created", autoAck: false);
            if (result == null)
                return;

            try
            {
                var message = Encoding.UTF8.GetString(result.Body.ToArray());
                Console.WriteLine($"🔥 Order received: {message}");

                var order = JsonSerializer.Deserialize<OrderCreatedEventDto>(message);
                if (order != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

                    var product = db.Products.FirstOrDefault(p => p.Id == order.ProductId);
                    if (product != null)
                    {
                        product.Stock -= order.Quantity;
                        if (product.Stock < 0)
                            product.Stock = 0;

                        db.SaveChanges();
                        Console.WriteLine($"✅ Stock updated: {product.Stock}");
                    }
                }

                _channel.BasicAck(result.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing order_created: {ex.Message}");
                _channel.BasicNack(result.DeliveryTag, multiple: false, requeue: true);
            }
        }

        private void ConsumeOrderCancelled()
        {
            if (_channel == null)
                return;

            var result = _channel.BasicGet("order_cancelled", autoAck: false);
            if (result == null)
                return;

            try
            {
                var message = Encoding.UTF8.GetString(result.Body.ToArray());
                Console.WriteLine($"🔥🔥🔥 Order cancelled event received: {message}");

                var order = JsonSerializer.Deserialize<OrderCancelledEventDto>(message);
                if (order != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

                    var product = db.Products.FirstOrDefault(p => p.Id == order.ProductId);
                    if (product != null)
                    {
                        product.Stock += order.Quantity;
                        db.SaveChanges();
                        Console.WriteLine($"✅ Stock restored: {product.Stock} (returned {order.Quantity} items)");
                    }
                }

                _channel.BasicAck(result.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing order_cancelled: {ex.Message}");
                _channel.BasicNack(result.DeliveryTag, multiple: false, requeue: true);
            }
        }
    }
}