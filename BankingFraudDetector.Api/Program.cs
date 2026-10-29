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

var neo4jUri = builder.Configuration["Neo4j:Uri"]
    ?? throw new InvalidOperationException(
        "Neo4j URI was not configured.");

var neo4jUsername = builder.Configuration["Neo4j:Username"]
    ?? throw new InvalidOperationException(
        "Neo4j username was not configured.");

var neo4jPassword = builder.Configuration["Neo4j:Password"]
    ?? throw new InvalidOperationException(
        "Neo4j password was not configured.");

builder.Services.AddGraphInfrastructure(
    neo4jUri,
    neo4jUsername,
    neo4jPassword);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
