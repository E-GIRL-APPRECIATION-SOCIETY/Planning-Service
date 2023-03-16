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

    // instansieret C# objekt der ligner en PlanDTO
    planDTO testPlan = new planDTO{PlanID = 1, CustomerName = "Steve", StartTidspunkt = "16:30", StartSted = "København", SlutSted = "Tønder"};


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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Bruges til at teste implementering af skriv til CSV fil. Fjern // hvis testes
            // writeCSVFile();
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}

// Test Model til at undersøge om StreamWriter kan skrive C# objekter
public class planDTO
{
    public int PlanID { get; set; }
    public string? CustomerName { get; set; }
    public string? StartTidspunkt { get; set; }
    public string? StartSted { get; set; }
    public string? SlutSted { get; set; }
}

