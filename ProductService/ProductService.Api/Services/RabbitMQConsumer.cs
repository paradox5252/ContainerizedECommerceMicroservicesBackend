using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var orderCreatedConsumer = new EventingBasicConsumer(_channel);
                orderCreatedConsumer.Received += (_, ea) =>
                {
                    try
                    {
                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                        ProcessOrderCreatedMessage(message);
                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error processing order_created: {ex.Message}");
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                var orderCancelledConsumer = new EventingBasicConsumer(_channel);
                orderCancelledConsumer.Received += (_, ea) =>
                {
                    try
                    {
                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                        ProcessOrderCancelledMessage(message);
                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error processing order_cancelled: {ex.Message}");
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                _channel.BasicConsume(queue: "order_created", autoAck: false, consumer: orderCreatedConsumer);
                _channel.BasicConsume(queue: "order_cancelled", autoAck: false, consumer: orderCancelledConsumer);

                Console.WriteLine("✅ BasicConsume started for queues: order_created, order_cancelled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fatal error in Consumer.Start(): {ex}");
                throw;
            }
        }

        private void ProcessOrderCreatedMessage(string message)
        {
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
        }

        private void ProcessOrderCancelledMessage(string message)
        {
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
        }
    }
}