using BankingFraudDetector.Domain.Events;

namespace BankingFraudDetector.Api.Contracts;

public sealed record RecordAccountEventRequest(
    Guid AccountId,
    AccountEventType EventType,
    DateTimeOffset OccurredAt);