namespace BankingFraudDetector.Application.Graphs;

public interface IAccountConnectionPaths
{
    Task<AccountConnectionPath?> FindShortestPathAsync(
        Guid sourceAccountId,
        Guid targetAccountId,
        CancellationToken cancellationToken = default);
}
