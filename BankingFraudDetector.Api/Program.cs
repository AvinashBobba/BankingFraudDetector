using BankingFraudDetector.Infrastructure;
using BankingFraudDetector.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

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

app.MapControllers();

app.Run();
