namespace BankingFraudDetector.Application.Graphs;

public sealed class FindLinkedAccounts(IAccountGraph graph)
{
    public Task<IReadOnlyList<LinkedAccount>> ExecuteAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return graph.FindAccountsSharingDevicesAsync(
            accountId,
            cancellationToken);
    }
}