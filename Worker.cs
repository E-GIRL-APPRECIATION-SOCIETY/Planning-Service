using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace ServiceWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    // POCO klasse format til beskeder
    private class PlanningMessage
    {
        public string CustomerName { get; set; }
        public DateTime PickupTime { get; set; }
        public string PickupLocation { get; set; }
        public string EndLocation { get; set; }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "Planning Service",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
        
        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" [x] Received {0}", message);

            // Deserialize beskeden
            var planningMessage = JsonConvert.DeserializeObject<PlanningMessage>(message);

            //Bruger data fra POCO klasse til håndterbar data
            Console.WriteLine(" [x] Received Planning Message: {0} - {1} (Due Date: {2})",
                planningMessage.CustomerName,
                planningMessage.PickupTime,
                planningMessage.PickupLocation,
                planningMessage.EndLocation);

        };

        channel.BasicConsume(queue: "Planning Service",
                             autoAck: true,
                             consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();


        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
