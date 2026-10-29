namespace BankingFraudDetector.Application.Graphs;

public sealed record LinkedAccount(
    Guid AccountId,
    string SharedDeviceId);