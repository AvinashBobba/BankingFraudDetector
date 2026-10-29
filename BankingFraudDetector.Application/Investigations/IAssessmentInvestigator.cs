namespace BankingFraudDetector.Application.Investigations;

public interface IAssessmentInvestigator
{
    Task<InvestigationSummary> InvestigateAsync(
        Guid assessmentId,
        CancellationToken cancellationToken = default);
}