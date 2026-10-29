using BankingFraudDetector.Application.Abstractions;
using BankingFraudDetector.Domain.FraudDetection;

namespace BankingFraudDetector.Application.FraudDetection;

public sealed class GetAssessment(IAssessmentRepository repository)
{
    public Task<AssessmentRecord?> ExecuteAsync(
        Guid assessmentId,
        CancellationToken cancellationToken = default)
    {
        return repository.GetByIdAsync(
            assessmentId,
            cancellationToken);
    }
}