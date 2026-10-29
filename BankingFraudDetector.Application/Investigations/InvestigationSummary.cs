namespace BankingFraudDetector.Application.Investigations;

public sealed record InvestigationSummary(
    Guid AssessmentId,
    string Summary,
    string Model);