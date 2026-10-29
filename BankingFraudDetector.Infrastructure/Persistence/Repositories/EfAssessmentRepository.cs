using BankingFraudDetector.Application.Abstractions;
using BankingFraudDetector.Domain.FraudDetection;
using Microsoft.EntityFrameworkCore;

namespace BankingFraudDetector.Infrastructure.Persistence.Repositories;

public sealed class EfAssessmentRepository(FraudDbContext dbContext)
    : IAssessmentRepository
{
    public async Task AddAsync(
        AssessmentRecord assessment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assessment);

        dbContext.AssessmentRecords.Add(assessment);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AssessmentRecord?> GetByIdAsync(
        Guid assessmentId,
        CancellationToken cancellationToken = default)
    {
        if (assessmentId == Guid.Empty)
        {
            throw new ArgumentException(
                "An assessment ID is required.",
                nameof(assessmentId));
        }

        return await dbContext.AssessmentRecords
            .AsNoTracking()
            .Include(x => x.Findings)
            .SingleOrDefaultAsync(
                x => x.Id == assessmentId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AssessmentRecord>> GetRecentByAccountAsync(
    Guid accountId,
    CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "An account ID is required.",
                nameof(accountId));
        }

        return await dbContext.AssessmentRecords
            .AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .OrderByDescending(x => x.AssessedAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(50)
            .ToListAsync(cancellationToken);
    }
}