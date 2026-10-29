using System.Text.Json;
using BankingFraudDetector.Application.Events;
using BankingFraudDetector.Application.FraudDetection;
using BankingFraudDetector.Application.Graphs;
using BankingFraudDetector.Application.Investigations;
using Microsoft.Extensions.Logging;

namespace BankingFraudDetector.AI.Workflows;

public sealed class SequentialAssessmentInvestigator(
    GetAssessment getAssessment, GetAccountEvents getEvents,
    FindLinkedAccounts findDirect, FindConnectedAccounts findConnected,
    FindAccountConnectionPath findPath, EvidenceInvestigationAgent evidenceAgent,
    GraphInvestigationAgent graphAgent, ReviewInvestigationAgent reviewAgent,
    OllamaOptions options, ILogger<SequentialAssessmentInvestigator> logger)
    : IAssessmentInvestigator
{
    public async Task<InvestigationSummary> InvestigateAsync(Guid assessmentId,
        CancellationToken cancellationToken = default)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(options.WorkflowTimeoutSeconds));
        var ct = deadline.Token;
        var assessment = await getAssessment.ExecuteAsync(assessmentId, ct)
            ?? throw new KeyNotFoundException("The saved assessment was not found.");
        var context = new InvestigationContext(assessment.Id, assessment.AccountId,
            assessment.TotalScore, assessment.RiskBand.ToString());

        // Mandatory reads are ordinary C# operations, not dependent on model tool selection.
        var activity = await getEvents.ExecuteAsync(assessment.AccountId,
            assessment.FromInclusiveUtc, assessment.ToExclusiveUtc, ct);
        var evidenceSources = new List<InvestigationSource>
        {
            Source("assessment", "saved-decision", new
            {
                context, assessment.RuleSetVersion, assessment.FromInclusiveUtc, assessment.ToExclusiveUtc,
                findings = assessment.Findings.Select(f => new
                {
                    f.RuleCode, f.Score, f.Explanation, f.EvidenceEventIds
                })
            }),
            Source("activity", "current-activity", new
            {
                totalCount = activity.Count, truncated = activity.Count > 200,
                events = activity.Take(200).Select(e => new { e.Id, type = e.EventType.ToString(), e.OccurredAtUtc }),
                limitation = "Current activity may include late-arriving events. Ingestion timestamps are not supplied, so arrival order relative to the saved assessment cannot be established."
            })
        };
        var evidence = await evidenceAgent.RunAsync(context, evidenceSources, ct);

        var graphSources = new List<InvestigationSource>();
        async Task<T?> Capture<T>(string id, string kind, Func<Task<T>> operation) where T : class
        {
            try
            {
                var result = await operation();
                graphSources.Add(Source(id, kind, new { available = true, result }));
                return result;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception e)
            {
                // Partial graph failure is explicit evidence of unavailability, never of no links.
                logger.LogWarning("Investigation {AssessmentId}: graph source {Source} unavailable ({ErrorType})",
                    assessmentId, id, e.GetType().Name);
                graphSources.Add(Source(id, kind, new
                {
                    available = false,
                    reason = e is GraphProjectionUnavailableException ? "GDS projection missing" : "Graph query failed"
                }));
                return null;
            }
        }

        var direct = await Capture("direct-links", "current-shared-devices", async () =>
        {
            var links = await findDirect.ExecuteAsync(assessment.AccountId, ct);
            return new { totalCount = links.Count, truncated = links.Count > 50, links = links.Take(50).ToArray() };
        });
        var component = await Capture("component", "gds-snapshot-membership", async () =>
        {
            var accounts = await findConnected.ExecuteAsync(assessment.AccountId, ct);
            return new { totalCount = accounts.Count, truncated = accounts.Count > 50, accounts = accounts.Take(50).ToArray() };
        });
        var targets = (component?.accounts.Select(a => a.AccountId) ?? [])
            .Concat(direct?.links.Select(a => a.AccountId) ?? []).Distinct().Order().Take(3);
        foreach (var target in targets)
        {
            await Capture($"path:{target}", "current-path-max-six-hops", async () =>
                new { target, path = await findPath.ExecuteAsync(assessment.AccountId, target, ct) });
        }
        graphSources.Add(Source("graph-limitations", "coverage", new
        {
            maxPaths = 3, maxHops = 6,
            limitation = "Graph is current; GDS is a snapshot. Missing or truncated results do not rule out other links. Connectivity does not prove fraud."
        }));
        var graph = await graphAgent.RunAsync(context, graphSources, evidence.Findings, ct);
        var allSources = evidenceSources.Concat(graphSources).ToArray();
        var review = await reviewAgent.RunAsync(context, allSources,
            evidence.Findings, graph.Findings, ct);

        // Canonical score/band are composed in C#, not taken from generated prose.
        var summary = $"Recorded decision: {context.RecordedRiskBand} / {context.RecordedScore} points.\n\n" +
            review.Findings.Summary + "\n\nLimitations: " +
            string.Join("; ", review.Findings.Limitations.Append(
                "Event arrival time relative to the assessment is unknown. AI analysis requires analyst verification. Connectivity does not prove fraud."));
        return new InvestigationSummary(assessmentId, summary, options.Model)
        {
            Workflow = "evidence-graph-review-v1",
            Stages =
            [
                Stage("EvidenceAgent", evidence), Stage("GraphAgent", graph), Stage("ReviewAgent", review)
            ]
        };
    }

    private static InvestigationSource Source(string id, string kind, object value) =>
        new(id, kind, JsonSerializer.Serialize(value));

    private static InvestigationStage Stage(string name, (AgentFindings Findings, long DurationMs) stage) =>
        new(name, stage.DurationMs, stage.Findings.Summary, stage.Findings.EvidenceIds, stage.Findings.Limitations);
}
