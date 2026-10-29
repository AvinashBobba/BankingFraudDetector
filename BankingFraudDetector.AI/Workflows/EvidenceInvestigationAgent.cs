namespace BankingFraudDetector.AI.Workflows;

public sealed class EvidenceInvestigationAgent(StructuredInvestigationAgent runner)
{
    public Task<(AgentFindings Findings, long DurationMs)> RunAsync(InvestigationContext context,
        IReadOnlyList<InvestigationSource> sources, CancellationToken cancellationToken) =>
        runner.RunAsync("EvidenceAgent", """
            You review saved deterministic fraud decisions and account activity.
            Explain matched rules and chronological events. Distinguish saved findings
            from current activity, which can include late-arriving events.
            Do not infer graph connections or recalculate the score.
            """, context, sources, null, cancellationToken);
}
