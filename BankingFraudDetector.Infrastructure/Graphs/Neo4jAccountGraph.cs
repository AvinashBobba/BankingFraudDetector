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
}