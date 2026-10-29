namespace BankingFraudDetector.AI;

public sealed class OllamaOptions
{
    public string Endpoint { get; init; } = "http://localhost:11434";

    public string Model { get; init; } = "qwen3:4b-instruct";

    public int TimeoutSeconds { get; init; } = 300;
    public int StageTimeoutSeconds { get; init; } = 180;
    public int WorkflowTimeoutSeconds { get; init; } = 540;
}
