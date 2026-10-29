namespace BankingFraudDetector.Domain.Events;

public sealed class AccountEvent
{
    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public AccountEventType EventType { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    private AccountEvent()
    {
    }

    public AccountEvent(
        Guid accountId,
        AccountEventType eventType,
        DateTimeOffset occurredAt)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "An account ID is required.",
                nameof(accountId));
        }

        if (!Enum.IsDefined(eventType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(eventType),
                eventType,
                "The account event type is not supported.");
        }

        if (occurredAt == default)
        {
            throw new ArgumentException(
                "An occurrence time is required.",
                nameof(occurredAt));
        }

        Id = Guid.NewGuid();
        AccountId = accountId;
        EventType = eventType;
        OccurredAtUtc = occurredAt.ToUniversalTime();
    }
}