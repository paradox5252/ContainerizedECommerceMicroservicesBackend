using RabbitMQ.Client;
using System.Text;
namespace OrderService.Services
{
public class RabbitMQPublisher
{
    private readonly string _hostname;
    private readonly string _queueName;

    public RabbitMQPublisher(string hostname, string queueName)
    {
        _hostname = hostname; // e.g., "localhost" if running locally
        _queueName = queueName; // e.g., "orders-queue"
    }

    public void Publish(string message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostname,
            UserName = "guest",
            Password = "guest"
        };

        // Create connection & channel
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // THIS DECLARES THE QUEUE
        channel.QueueDeclare(
            queue: _queueName,
            durable: true,      // survives broker restart
            exclusive: false,   // can be used by multiple connections
            autoDelete: false,  // won’t auto-delete when no consumers
            arguments: null
        );

        // Convert message to bytes
        var body = Encoding.UTF8.GetBytes(message);

        // Publish the message
        channel.BasicPublish(
            exchange: "",          // default exchange
            routingKey: _queueName,
            basicProperties: null,
            body: body
        );

        Console.WriteLine($"Message published to queue: {_queueName}");
    }
}
}

