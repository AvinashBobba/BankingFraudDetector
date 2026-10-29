namespace BankingFraudDetector.Domain.FraudDetection;

public sealed record RuleMatch(
    string RuleCode,
    int Score,
    string Explanation,
    IReadOnlyList<Guid> EvidenceEventIds);