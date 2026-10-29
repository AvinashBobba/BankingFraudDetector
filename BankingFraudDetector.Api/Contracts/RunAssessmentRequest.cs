namespace BankingFraudDetector.Api.Contracts;

public sealed record RunAssessmentRequest(
    Guid AccountId,
    DateTimeOffset FromInclusive,
    DateTimeOffset ToExclusive);