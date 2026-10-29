using BankingFraudDetector.Application.Abstractions;
using BankingFraudDetector.Domain.FraudDetection;

namespace BankingFraudDetector.Application.FraudDetection;

public sealed class AssessAccountRisk(
    IAccountEventRepository repository,
    AccountTakeoverSequenceRule rule)
{
    public async Task<FraudAssessment> ExecuteAsync(
        Guid accountId,
        DateTimeOffset fromInclusive,
        DateTimeOffset toExclusive,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "An account ID is required.",
                nameof(accountId));
        }

        if (fromInclusive >= toExclusive)
        {
            throw new ArgumentException(
                "The start of the time window must precede its end.",
                nameof(toExclusive));
        }

        var events = await repository.GetByAccountAsync(
            accountId,
            fromInclusive,
            toExclusive,
            cancellationToken);

        var match = rule.Evaluate(accountId, events);

        IReadOnlyList<RuleMatch> matches = match is null
            ? Array.Empty<RuleMatch>()
            : new[] { match };

        var totalScore = matches.Sum(x => x.Score);

        var riskBand = totalScore switch
        {
            >= 80 => RiskBand.High,
            >= 40 => RiskBand.Medium,
            _ => RiskBand.Low
        };

        return new FraudAssessment(
            accountId,
            totalScore,
            riskBand,
            matches);
    }
}