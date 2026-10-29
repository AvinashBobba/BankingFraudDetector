using BankingFraudDetector.Domain.Events;
using BankingFraudDetector.Domain.FraudDetection;

namespace BankingFraudDetector.Domain.Tests;

public sealed class AccountTakeoverSequenceRuleTests
{
    private readonly AccountTakeoverSequenceRule _rule = new();

    private static readonly DateTimeOffset Start =
        new(2026, 10, 29, 10, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(20, true)]
    [InlineData(30, true)]
    [InlineData(31, false)]
    public void Evaluate_RespectsWholeSequenceWindow(
        int contactChangeMinute,
        bool shouldMatch)
    {
        var accountId = Guid.NewGuid();
        var events = CreateSequence(accountId, contactChangeMinute);

        var result = _rule.Evaluate(accountId, events);

        Assert.Equal(shouldMatch, result is not null);

        if (shouldMatch)
        {
            Assert.NotNull(result);
            Assert.Equal("ATO-001", result.RuleCode);
            Assert.Equal(80, result.Score);
            Assert.False(string.IsNullOrWhiteSpace(result.Explanation));
            Assert.Equal(
                events.Select(x => x.Id).ToArray(),
                result.EvidenceEventIds.ToArray());
        }
    }

    [Fact]
    public void Evaluate_HandlesUnsortedInput()
    {
        var accountId = Guid.NewGuid();
        var events = CreateSequence(accountId, 20);

        var result = _rule.Evaluate(
            accountId,
            new[] { events[2], events[0], events[1] });

        Assert.NotNull(result);
        Assert.Equal(
            events.Select(x => x.Id).ToArray(),
            result.EvidenceEventIds.ToArray());
    }

    [Fact]
    public void Evaluate_RejectsWrongEventOrder()
    {
        var accountId = Guid.NewGuid();

        var events = new[]
        {
            Event(accountId, AccountEventType.MfaEnrolled, 0),
            Event(accountId, AccountEventType.PasswordReset, 5),
            Event(accountId, AccountEventType.ContactDetailsChanged, 10)
        };

        Assert.Null(_rule.Evaluate(accountId, events));
    }

    [Fact]
    public void Evaluate_DoesNotCombineDifferentAccounts()
    {
        var accountId = Guid.NewGuid();

        var events = new[]
        {
            Event(accountId, AccountEventType.PasswordReset, 0),
            Event(Guid.NewGuid(), AccountEventType.MfaEnrolled, 5),
            Event(accountId, AccountEventType.ContactDetailsChanged, 10)
        };

        Assert.Null(_rule.Evaluate(accountId, events));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(5, 5)]
    public void Evaluate_RequiresStrictlyIncreasingTimestamps(
        int mfaMinute,
        int contactMinute)
    {
        var accountId = Guid.NewGuid();

        var events = new[]
        {
            Event(accountId, AccountEventType.PasswordReset, 0),
            Event(accountId, AccountEventType.MfaEnrolled, mfaMinute),
            Event(accountId, AccountEventType.ContactDetailsChanged, contactMinute)
        };

        Assert.Null(_rule.Evaluate(accountId, events));
    }

    [Fact]
    public void Evaluate_ReturnsNoMatchForEmptyHistory()
    {
        Assert.Null(_rule.Evaluate(
            Guid.NewGuid(),
            Array.Empty<AccountEvent>()));
    }

    private static AccountEvent[] CreateSequence(
        Guid accountId,
        int contactChangeMinute) =>
        new[]
        {
            Event(accountId, AccountEventType.PasswordReset, 0),
            Event(accountId, AccountEventType.MfaEnrolled, 5),
            Event(
                accountId,
                AccountEventType.ContactDetailsChanged,
                contactChangeMinute)
        };

    private static AccountEvent Event(
        Guid accountId,
        AccountEventType eventType,
        int minute) =>
        new(accountId, eventType, Start.AddMinutes(minute));
}