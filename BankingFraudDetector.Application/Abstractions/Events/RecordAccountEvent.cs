using BankingFraudDetector.Application.Abstractions;
using BankingFraudDetector.Domain.Events;

namespace BankingFraudDetector.Application.Events;

public sealed class RecordAccountEvent
{
    private readonly IAccountEventRepository _repository;

    public RecordAccountEvent(IAccountEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> ExecuteAsync(
        Guid accountId,
        AccountEventType eventType,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        var accountEvent = new AccountEvent(
            accountId,
            eventType,
            occurredAt);

        await _repository.AddAsync(accountEvent, cancellationToken);

        return accountEvent.Id;
    }
}