using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace ServiceWorker;

[TestMethod]
public void TestPublishMessageToQueue()
{
    var factory = new ConnectionFactory() { HostName = "localhost" };
    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();

    channel.QueueDeclare(queue: "Planning Service",
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);

    var message = new PlanningMessage()
    {
        CustomerName = "TestCustomer",
        PickupTime = DateTime.Now,
        PickupLocation = "TestPickupLocation",
        EndLocation = "TestEndLocation"
    };

    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

    channel.BasicPublish(exchange: "",
                         routingKey: "Planning Service",
                         basicProperties: null,
                         body: body);
}
