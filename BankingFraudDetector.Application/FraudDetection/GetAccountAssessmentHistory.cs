using BankingFraudDetector.Application.Abstractions;
using BankingFraudDetector.Domain.FraudDetection;

namespace BankingFraudDetector.Application.FraudDetection;

public sealed class GetAccountAssessmentHistory(
    IAssessmentRepository repository)
{
    public Task<IReadOnlyList<AssessmentRecord>> ExecuteAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return repository.GetRecentByAccountAsync(
            accountId,
            cancellationToken);
    }
}