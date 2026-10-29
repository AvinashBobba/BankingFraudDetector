using BankingFraudDetector.Domain.Events;

namespace BankingFraudDetector.Domain.FraudDetection;

public sealed class AccountTakeoverSequenceRule : IFraudRule
{
    public const string Code = "ATO-001";

    private static readonly TimeSpan Window = TimeSpan.FromMinutes(30);

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
            .OrderBy(x => x.OccurredAtUtc)
            .ToArray();

        AccountEvent? latestReset = null;
        AccountEvent? matchedReset = null;
        AccountEvent? matchedMfa = null;

        foreach (var accountEvent in accountEvents)
        {
            switch (accountEvent.EventType)
            {
                case AccountEventType.PasswordReset:
                    latestReset = accountEvent;
                    break;

                case AccountEventType.MfaEnrolled:
                    if (latestReset is not null &&
                        latestReset.OccurredAtUtc < accountEvent.OccurredAtUtc &&
                        accountEvent.OccurredAtUtc - latestReset.OccurredAtUtc <= Window)
                    {
                        matchedReset = latestReset;
                        matchedMfa = accountEvent;
                    }

                    break;

                case AccountEventType.ContactDetailsChanged:
                    if (matchedReset is not null &&
                        matchedMfa is not null &&
                        matchedMfa.OccurredAtUtc < accountEvent.OccurredAtUtc &&
                        accountEvent.OccurredAtUtc - matchedReset.OccurredAtUtc <= Window)
                    {
                        return new RuleMatch(
                            RuleCode: Code,
                            Score: 80,
                            Explanation:
                                "Password reset followed by MFA enrolment " +
                                "and contact details change within 30 minutes.",
                            EvidenceEventIds: new[]
                            {
                                matchedReset.Id,
                                matchedMfa.Id,
                                accountEvent.Id
                            });
                    }

                    break;
            }
        }

        return null;
    }
}