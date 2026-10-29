using BankingFraudDetector.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("FraudDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'FraudDatabase' was not configured.");

builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "BankingFraudDetector.Api"
}));

app.Run();