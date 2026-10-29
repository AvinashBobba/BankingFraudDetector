namespace BankingFraudDetector.AI.Workflows;

public sealed record InvestigationSource(string Id, string Kind, string Json);

public sealed record InvestigationContext(
    Guid AssessmentId, Guid AccountId, int RecordedScore, string RecordedRiskBand);

// Both specialist reports and the review must preserve the recorded decision.
public sealed class AgentFindings
{
    public Guid AssessmentId { get; set; }
    public Guid AccountId { get; set; }
    public int RecordedScore { get; set; }
    public string RecordedRiskBand { get; set; } = "";
    public string Summary { get; set; } = "";
    public string[] EvidenceIds { get; set; } = [];
    public string[] Limitations { get; set; } = [];
}

public sealed class InvestigationOutputException(string message, Exception? inner = null)
    : Exception(message, inner);

public static class InvestigationValidation
{
    public static void Validate(AgentFindings? report, InvestigationContext context,
        IReadOnlyList<InvestigationSource> sources)
    {
        if (report is null || report.AssessmentId != context.AssessmentId ||
            report.AccountId != context.AccountId || report.RecordedScore != context.RecordedScore ||
            !string.Equals(report.RecordedRiskBand, context.RecordedRiskBand, StringComparison.Ordinal))
            throw new InvestigationOutputException("An agent changed or omitted the recorded assessment identity or decision.");

        if (string.IsNullOrWhiteSpace(report.Summary) || report.Summary.Length > 600 ||
            report.EvidenceIds is null || report.EvidenceIds.Length is < 1 or > 8 ||
            report.Limitations is null || report.Limitations.Length is < 1 or > 3 ||
            report.Limitations.Any(s => string.IsNullOrWhiteSpace(s) || s.Length > 140))
            throw new InvestigationOutputException("An agent returned incomplete investigation findings.");

        var allowed = sources.Select(s => s.Id).ToHashSet(StringComparer.Ordinal);
        if (report.EvidenceIds.Any(id => !allowed.Contains(id)))
            throw new InvestigationOutputException("An agent cited evidence outside its supplied sources.");
    }
}
