using BankingFraudDetector.Domain.Events;

namespace BankingFraudDetector.Domain.FraudDetection;

public sealed class BankChangeThenSaleRule : IFraudRule
{
    public const string Code = "PAY-001";

    private static readonly TimeSpan Window = TimeSpan.FromMinutes(60);

    public RuleMatch? Evaluate(
        Guid accountId,
        IEnumerable<AccountEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "An account ID is required.",
                nameof(accountId));
        }

        var accountEvents = events
            .Where(x => x.AccountId == accountId)
            .OrderBy(x => x.OccurredAtUtc);

        AccountEvent? latestBankChange = null;

        foreach (var accountEvent in accountEvents)
        {
            if (accountEvent.EventType == AccountEventType.BankDetailsChanged)
            {
                latestBankChange = accountEvent;
                continue;
            }

            if (accountEvent.EventType == AccountEventType.SaleRequested &&
                latestBankChange is not null &&
                latestBankChange.OccurredAtUtc < accountEvent.OccurredAtUtc &&
                accountEvent.OccurredAtUtc -
                    latestBankChange.OccurredAtUtc <= Window)
            {
                return new RuleMatch(
                    RuleCode: Code,
                    Score: 60,
                    Explanation:
                        "Bank details changed followed by a sale request " +
                        "within 60 minutes.",
                    EvidenceEventIds: new[]
                    {
                        latestBankChange.Id,
                        accountEvent.Id
                    });
            }
        }

        return null;
    }
}