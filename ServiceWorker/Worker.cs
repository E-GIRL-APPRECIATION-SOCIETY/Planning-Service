using System;
using System.IO;

namespace ServiceWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    // Tom variabel til at holde stien til filen
    private readonly string? _docPath;

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        // Tager Miljø variablen (filstien) og giver den til den tomme variabel ovenfor
        _docPath = config["DocPath"];
        _logger = logger;
        // Bruges til at skrive miljø variablen i konsolen
        _logger.LogInformation($"Miljø variabel er sat til : {_docPath}");
    }

    private class PlanDTO
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

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

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
            // Bruges til at teste implementering af skriv til CSV fil. Fjern // hvis testes
            // writeCSVFile();
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }

     // Metode der tager stien til filen, og navnet på filen og skriver noget i bunden af den.
    public void writeCSVFile()
    {
        try
        {
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(_docPath, "plan.csv"), true))
            {
                outputFile.WriteLine(testPlan.CustomerName + ", " + 
                testPlan.StartTidspunkt + ", " + 
                testPlan.StartSted + ", " + 
                testPlan.SlutSted);
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("This program failed : ", ex);
        }
    }
}


