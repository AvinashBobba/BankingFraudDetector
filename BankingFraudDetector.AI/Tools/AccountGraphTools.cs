using System.ComponentModel;
using System.Text.Json;
using BankingFraudDetector.Application.Graphs;

namespace BankingFraudDetector.AI.Tools;

public sealed class AccountGraphTools(
    Guid accountId,
    FindLinkedAccounts findLinkedAccounts,
    FindConnectedAccounts findConnectedAccounts,
    FindAccountConnectionPath findConnectionPath,
    CancellationToken cancellationToken)
{
    [Description("Explain a connection to a target account by returning one shortest account/device path, up to six relationship hops. The source is fixed to the account under investigation. Use after discovering a linked account. This queries the current graph, not the GDS snapshot; a missing path is not proof of no connection.")]
    public async Task<string> GetAccountConnectionPath(
        [Description("The other account ID to explain a connection to.")] Guid targetAccountId)
    {
        if (targetAccountId == Guid.Empty || targetAccountId == accountId)
            return JsonSerializer.Serialize(new { available = false, reason = "A distinct, nonempty target account ID is required." });

        var path = await findConnectionPath.ExecuteAsync(accountId, targetAccountId, cancellationToken);
        return JsonSerializer.Serialize(new
        {
            found = path is not null,
            maxHops = 6,
            path,
            limitation = "Current graph evidence only; connectivity does not prove fraud. No path within six hops does not rule out longer connections."
        });
    }

    [Description(
        "Find other accounts that share devices with the account under " +
        "investigation. Use when investigating linked accounts or shared " +
        "devices. Shared devices are an investigation signal, not proof " +
        "of fraud. Results reflect the current graph, not necessarily " +
        "the graph at the time of the saved assessment.")]
    public async Task<string> FindAccountsSharingDevices()
    {
        var linkedAccounts = await findLinkedAccounts.ExecuteAsync(
            accountId,
            cancellationToken);

        return JsonSerializer.Serialize(new
        {
            accountId,
            linkedAccounts,
            limitation =
                "Shared-device links do not establish fraud. " +
                "The graph may be incomplete and is not time-filtered."
        });
    }

    [Description(
    "Find other accounts in the same connected component as the account " +
    "under investigation, including indirect connections through accounts " +
    "and devices. Use when investigating a wider connected network. " +
    "Results use a GDS snapshot and do not establish fraud or explain " +
    "the exact connection path.")]
public async Task<string> FindAccountsInConnectedComponent()
{
    try
    {
        var accounts = await findConnectedAccounts.ExecuteAsync(
            accountId,
            cancellationToken);

        return JsonSerializer.Serialize(new
        {
            available = true,
            accountId,
            connectedAccounts = accounts,
            limitation =
                "Component membership includes indirect connections. " +
                "The projection may be stale. Connectivity does not prove fraud."
        });
    }
    catch (GraphProjectionUnavailableException)
    {
        return JsonSerializer.Serialize(new
        {
            available = false,
            accountId,
            reason = "The GDS projection is unavailable."
        });
    }
}
}
