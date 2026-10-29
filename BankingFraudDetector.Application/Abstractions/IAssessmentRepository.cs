using BankingFraudDetector.Domain.FraudDetection;

namespace BankingFraudDetector.Application.Abstractions;

public interface IAssessmentRepository
{
    Task AddAsync(
        AssessmentRecord assessment,
        CancellationToken cancellationToken = default);

    Task<AssessmentRecord?> GetByIdAsync(
        Guid assessmentId,
        CancellationToken cancellationToken = default);
}