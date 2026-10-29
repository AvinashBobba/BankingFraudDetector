
using BankingFraudDetector.Application.Abstractions;
using BankingFraudDetector.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace BankingFraudDetector.Infrastructure.Persistence.Repositories;

public sealed class EfAccountEventRepository : IAccountEventRepository
{
    private readonly FraudDbContext _dbContext;

    public EfAccountEventRepository(FraudDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        AccountEvent accountEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accountEvent);

        _dbContext.AccountEvents.Add(accountEvent);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AccountEvent>> GetByAccountAsync(
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

        return await _dbContext.AccountEvents
            .AsNoTracking()
            .Where(x =>
                x.AccountId == accountId &&
                x.OccurredAtUtc >= fromInclusive &&
                x.OccurredAtUtc < toExclusive)
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}