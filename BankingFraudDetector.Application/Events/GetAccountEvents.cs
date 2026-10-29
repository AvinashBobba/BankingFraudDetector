using BankingFraudDetector.Application.Abstractions;
using BankingFraudDetector.Domain.Events;

namespace BankingFraudDetector.Application.Events;

public sealed class GetAccountEvents(IAccountEventRepository repository)
{
    public Task<IReadOnlyList<AccountEvent>> ExecuteAsync(
        Guid accountId,
        DateTimeOffset fromInclusive,
        DateTimeOffset toExclusive,
        CancellationToken cancellationToken = default)
    {
        return repository.GetByAccountAsync(
            accountId,
            fromInclusive,
            toExclusive,
            cancellationToken);
    }
}