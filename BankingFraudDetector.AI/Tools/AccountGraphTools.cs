using System.ComponentModel;
using System.Text.Json;
using BankingFraudDetector.Application.Graphs;

namespace BankingFraudDetector.AI.Tools;

public sealed class AccountGraphTools(
    Guid accountId,
    FindLinkedAccounts findLinkedAccounts,
    CancellationToken cancellationToken)
{
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
}