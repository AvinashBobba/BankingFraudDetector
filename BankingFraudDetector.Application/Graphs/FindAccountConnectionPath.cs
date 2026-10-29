namespace BankingFraudDetector.Application.Graphs;

public sealed class FindAccountConnectionPath(IAccountConnectionPaths paths)
{
    public Task<AccountConnectionPath?> ExecuteAsync(
        Guid sourceAccountId,
        Guid targetAccountId,
        CancellationToken cancellationToken = default)
    {
        if (sourceAccountId == Guid.Empty || targetAccountId == Guid.Empty ||
            sourceAccountId == targetAccountId)
            throw new ArgumentException("Two distinct, nonempty account IDs are required.");

        return paths.FindShortestPathAsync(sourceAccountId, targetAccountId, cancellationToken);
    }
}
