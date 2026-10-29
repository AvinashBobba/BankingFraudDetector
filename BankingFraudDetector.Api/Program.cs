using BankingFraudDetector.Infrastructure;
using BankingFraudDetector.Application;
using BankingFraudDetector.AI;
using BankingFraudDetector.AI.Mcp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var aiOptions = builder.Configuration
    .GetRequiredSection("AI")
    .Get<OllamaOptions>()
    ?? throw new InvalidOperationException(
        "AI configuration was not provided.");

builder.Services.AddAI(aiOptions);

var connectionString =
    builder.Configuration.GetConnectionString("FraudDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'FraudDatabase' was not configured.");

builder.Services.AddInfrastructure(connectionString);

var mcpOptions = builder.Configuration
    .GetRequiredSection("Mcp")
    .Get<AssessmentMcpOptions>()
    ?? throw new InvalidOperationException(
        "MCP configuration was not provided.");

builder.Services.AddSingleton(mcpOptions);

builder.Services.AddScoped<AssessmentMcpGateway>(provider =>
    new AssessmentMcpGateway(
        provider.GetRequiredService<AssessmentMcpOptions>(),
        connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
