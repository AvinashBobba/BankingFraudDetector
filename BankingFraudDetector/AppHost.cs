var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BankingFraudDetector_Api>(
    "fraud-api",
    launchProfileName: "https");

builder.Build().Run();