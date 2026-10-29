namespace BankingFraudDetector.AI.Mcp;

public sealed class AssessmentMcpOptions
{
    public string ServerDllPath { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 60;
}