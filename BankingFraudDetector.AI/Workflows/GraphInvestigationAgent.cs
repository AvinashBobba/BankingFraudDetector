namespace BankingFraudDetector.AI.Workflows;

public sealed class GraphInvestigationAgent(StructuredInvestigationAgent runner)
{
    public Task<(AgentFindings Findings, long DurationMs)> RunAsync(InvestigationContext context,
        IReadOnlyList<InvestigationSource> sources, AgentFindings evidenceReport,
        CancellationToken cancellationToken) =>
        runner.RunAsync("GraphAgent", """
            You review graph evidence for an account investigation.
            Distinguish direct shared devices, snapshot component membership, and
            actual ordered paths. Only describe intermediate links present in a path source.
            Current graph data can differ from saved assessment-time data.
            An unavailable query is not an empty graph. Do not treat connectivity as fraud.
            The previous evidence report provides context, not new graph facts.
            """, context, sources, evidenceReport, cancellationToken);
}
