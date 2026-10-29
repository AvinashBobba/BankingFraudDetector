
using BankingFraudDetector.Application.Abstractions;
using BankingFraudDetector.Domain.FraudDetection;

namespace BankingFraudDetector.Application.FraudDetection;

public sealed class RunAccountAssessment(
    AssessAccountRisk assessAccountRisk,
    IAssessmentRepository assessmentRepository,
    TimeProvider timeProvider)
{
    private const string RuleSetVersion = "1.0";

    public async Task<Guid> ExecuteAsync(
        Guid accountId,
        DateTimeOffset fromInclusive,
        DateTimeOffset toExclusive,
        CancellationToken cancellationToken = default)
    {
        var assessedAt = timeProvider.GetUtcNow();

        var assessment = await assessAccountRisk.ExecuteAsync(
            accountId,
            fromInclusive,
            toExclusive,
            cancellationToken);

        var record = new AssessmentRecord(
            assessment,
            assessedAt,
            fromInclusive,
            toExclusive,
            RuleSetVersion);

        await assessmentRepository.AddAsync(
            record,
            cancellationToken);

        return record.Id;
    }
}