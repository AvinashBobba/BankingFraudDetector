using BankingFraudDetector.Application.Graphs;
using Neo4j.Driver;

namespace BankingFraudDetector.Infrastructure.Graphs;

public sealed class Neo4jAccountGraph(IDriver driver) : IAccountGraph
{
    public async Task<IReadOnlyList<LinkedAccount>>
        FindAccountsSharingDevicesAsync(
            Guid accountId,
            CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "An account ID is required.",
                nameof(accountId));
        }

        cancellationToken.ThrowIfCancellationRequested();

        const string cypher = """
            MATCH (source:Account {accountId: $accountId})
                  -[:USED_DEVICE]->(device:Device)
                  <-[:USED_DEVICE]-(linked:Account)
            WHERE linked.accountId <> source.accountId
            RETURN DISTINCT
                linked.accountId AS accountId,
                device.deviceId AS sharedDeviceId
            ORDER BY accountId, sharedDeviceId
            """;

        using var session = driver.AsyncSession(
            config => config.WithDatabase("neo4j"));

        var results = await session.ExecuteReadAsync(
            async transaction =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var cursor = await transaction.RunAsync(
                    cypher,
                    new { accountId = accountId.ToString() });

                var records = await cursor.ToListAsync();

                cancellationToken.ThrowIfCancellationRequested();

                return records.Select(record => new LinkedAccount(
                    Guid.Parse(record["accountId"].As<string>()),
                    record["sharedDeviceId"].As<string>()
                )).ToList();
            },
            config => config.WithTimeout(TimeSpan.FromSeconds(15)));

        return results;
    }

    public async Task<IReadOnlyList<ConnectedAccount>>
    FindConnectedAccountsAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "An account ID is required.",
                nameof(accountId));
        }

        cancellationToken.ThrowIfCancellationRequested();

        const string graphName = "fraud-device-graph";

        using var session = driver.AsyncSession(
            config => config.WithDatabase("neo4j"));

        return await session.ExecuteReadAsync(
            async transaction =>
            {
                var existenceCursor = await transaction.RunAsync(
                    """
                CALL gds.graph.exists($graphName)
                YIELD exists
                RETURN exists
                """,
                    new { graphName });

                var existence = await existenceCursor.SingleAsync();

                if (!existence["exists"].As<bool>())
                {
                    throw new GraphProjectionUnavailableException();
                }

                cancellationToken.ThrowIfCancellationRequested();

                var cursor = await transaction.RunAsync(
                    """
                CALL gds.wcc.stream($graphName)
                YIELD nodeId, componentId
                WITH componentId, gds.util.asNode(nodeId) AS node
                WHERE node:Account
                WITH componentId, collect(node.accountId) AS accountIds
                WHERE $accountId IN accountIds
                UNWIND accountIds AS connectedAccountId
                WITH connectedAccountId
                WHERE connectedAccountId <> $accountId
                RETURN DISTINCT connectedAccountId AS accountId
                ORDER BY accountId
                """,
                    new
                    {
                        graphName,
                        accountId = accountId.ToString()
                    });

                var records = await cursor.ToListAsync();

                cancellationToken.ThrowIfCancellationRequested();

                return (IReadOnlyList<ConnectedAccount>)records
                    .Select(record => new ConnectedAccount(
                        Guid.Parse(record["accountId"].As<string>())))
                    .ToList();
            },
            config => config.WithTimeout(TimeSpan.FromSeconds(15)));
    }
}