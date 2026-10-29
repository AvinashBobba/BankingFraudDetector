using BankingFraudDetector.Domain.Events;

namespace BankingFraudDetector.Application.Abstractions;

public interface IAccountEventRepository
{
    Task AddAsync(
        AccountEvent accountEvent,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AccountEvent>> GetByAccountAsync(
        Guid accountId,
        DateTimeOffset fromInclusive,
        DateTimeOffset toExclusive,
        CancellationToken cancellationToken = default);
}