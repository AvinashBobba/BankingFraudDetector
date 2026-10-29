namespace BankingFraudDetector.Application.Investigations;

public sealed record InvestigationSummary(
    Guid AssessmentId,
    string Summary,
    string Model)
{
    public string Workflow { get; init; } = "single-agent";
    public IReadOnlyList<InvestigationStage> Stages { get; init; } = [];
}

public sealed record InvestigationStage(
    string Agent, long DurationMs, string Summary,
    IReadOnlyList<string> EvidenceIds, IReadOnlyList<string> Limitations);
