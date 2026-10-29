namespace BankingFraudDetector.Application.Graphs;

public interface IAccountGraph
{
    Task<IReadOnlyList<LinkedAccount>> FindAccountsSharingDevicesAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);
}