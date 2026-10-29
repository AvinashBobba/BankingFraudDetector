using BankingFraudDetector.Application.Graphs;
using Neo4j.Driver;

namespace BankingFraudDetector.Infrastructure.Graphs;

public sealed class Neo4jAccountConnectionPaths(IDriver driver) : IAccountConnectionPaths
{
    public async Task<AccountConnectionPath?> FindShortestPathAsync(
        Guid sourceAccountId,
        Guid targetAccountId,
        CancellationToken cancellationToken = default)
    {
        if (sourceAccountId == Guid.Empty || targetAccountId == Guid.Empty ||
            sourceAccountId == targetAccountId)
            throw new ArgumentException("Two distinct, nonempty account IDs are required.");

        cancellationToken.ThrowIfCancellationRequested();
        using var session = driver.AsyncSession(c => c.WithDatabase("neo4j"));
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync("""
                MATCH (source:Account {accountId: $sourceId}),
                      (target:Account {accountId: $targetId})
                MATCH path = shortestPath((source)-[:USED_DEVICE*1..6]-(target))
                RETURN [node IN nodes(path) |
                    {type: CASE WHEN node:Account THEN 'Account' ELSE 'Device' END,
                     identifier: coalesce(node.accountId, node.deviceId)}] AS nodes
                """, new { sourceId = sourceAccountId.ToString(), targetId = targetAccountId.ToString() });
            var records = await cursor.ToListAsync();
            cancellationToken.ThrowIfCancellationRequested();
            if (records.Count == 0) return null;

            var nodes = records[0]["nodes"].As<List<Dictionary<string, object>>>()
                .Select(n => new ConnectionPathNode(n["type"].As<string>(), n["identifier"].As<string>()))
                .ToList();
            return new AccountConnectionPath(sourceAccountId, targetAccountId, nodes);
        }, c => c.WithTimeout(TimeSpan.FromSeconds(15)));
    }
}
