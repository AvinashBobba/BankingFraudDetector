namespace BankingFraudDetector.Domain.FraudDetection;

public sealed record FraudAssessment(
    Guid AccountId,
    int TotalScore,
    RiskBand RiskBand,
    IReadOnlyList<RuleMatch> Matches);