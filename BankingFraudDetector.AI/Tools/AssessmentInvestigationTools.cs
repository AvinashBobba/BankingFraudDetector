using System.ComponentModel;
using System.Text.Json;
using BankingFraudDetector.Application.Events;
using BankingFraudDetector.Domain.FraudDetection;

namespace BankingFraudDetector.AI.Tools;

// One instance per investigation: tools cannot select a different account or window.
public sealed class AssessmentInvestigationTools(
    AssessmentRecord assessment,
    GetAccountEvents getAccountEvents,
    CancellationToken cancellationToken)
{
    public bool EvidenceWasRequested { get; private set; }

    [Description("Get the saved assessment evidence for the current investigation, " +
        "including its recorded score, risk band, rule findings and evidence event IDs.")]
    public string GetAssessmentEvidence()
    {
        EvidenceWasRequested = true;

        return JsonSerializer.Serialize(new
        {
            assessment.Id,
            assessment.AccountId,
            assessment.TotalScore,
            riskBand = assessment.RiskBand.ToString(),
            assessment.RuleSetVersion,
            assessment.FromInclusiveUtc,
            assessment.ToExclusiveUtc,
            findings = assessment.Findings.Select(x => new
            {
                x.RuleCode,
                x.Score,
                x.Explanation,
                x.EvidenceEventIds
            })
        });
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
