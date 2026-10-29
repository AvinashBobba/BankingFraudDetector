namespace BankingFraudDetector.Application.Graphs;

public sealed class FindConnectedAccounts(IAccountGraph graph)
{
    public Task<IReadOnlyList<ConnectedAccount>> ExecuteAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "An account ID is required.",
                nameof(accountId));
        }

        return graph.FindConnectedAccountsAsync(
            accountId,
            cancellationToken);
    }
}