using System.ComponentModel;
using System.Text.Json;
using BankingFraudDetector.Application.Events;
using BankingFraudDetector.Domain.FraudDetection;
using BankingFraudDetector.AI.Mcp;

namespace BankingFraudDetector.AI.Tools;

// One instance per investigation: tools cannot select a different account or window.
public sealed class AssessmentInvestigationTools(
    AssessmentRecord assessment,
    GetAccountEvents getAccountEvents,
    AssessmentMcpGateway mcpGateway,
    CancellationToken cancellationToken)
{
    public bool EvidenceWasRequested { get; private set; }

    [Description(
     "Get the saved assessment evidence for the current investigation, " +
     "including its recorded score, risk band, rule findings and evidence event IDs.")]
    public async Task<string> GetAssessmentEvidence()
    {
        var evidence = await mcpGateway.GetEvidenceAsync(
            assessment.Id,
            cancellationToken);

        using var document = JsonDocument.Parse(evidence);

        if (!document.RootElement.TryGetProperty("found", out var found) ||
            found.ValueKind != JsonValueKind.True)
        {
            throw new InvalidOperationException(
                "The MCP server did not return the requested assessment.");
        }

        EvidenceWasRequested = true;

        return evidence;
    }

    [Description("Get the actual account activity events within the current assessment " +
        "window, ordered chronologically. Use this to inspect event types and timestamps. " +
        "This does not return events outside the assessment window. Current activity " +
        "may include events received after the assessment was saved.")]
    public async Task<string> GetAccountActivityEvents()
    {
        var events = await getAccountEvents.ExecuteAsync(
            assessment.AccountId,
            assessment.FromInclusiveUtc,
            assessment.ToExclusiveUtc,
            cancellationToken);

        return JsonSerializer.Serialize(events.Select(x => new
        {
            x.Id,
            eventType = x.EventType.ToString(),
            x.OccurredAtUtc
        }));
    }
}
