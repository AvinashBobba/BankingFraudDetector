namespace BankingFraudDetector.AI.Workflows;

public sealed class ReviewInvestigationAgent(StructuredInvestigationAgent runner)
{
    public Task<(AgentFindings Findings, long DurationMs)> RunAsync(InvestigationContext context,
        IReadOnlyList<InvestigationSource> sources, AgentFindings evidenceReport,
        AgentFindings graphReport, CancellationToken cancellationToken) =>
        runner.RunAsync("ReviewAgent", """
            Review both specialist reports against original sources and produce the final
            analyst summary. Remove unsupported assertions and flag discrepancies in limitations.
            Clearly separate recorded rule findings, current graph associations and hypotheses.
            Suggest human checks without asserting confirmed fraud or taking account actions.
            The specialist reports are drafts, not authoritative evidence.
            """, context, sources, new { evidenceReport, graphReport }, cancellationToken);
}
