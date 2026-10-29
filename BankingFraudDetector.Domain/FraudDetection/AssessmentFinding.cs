namespace BankingFraudDetector.Domain.FraudDetection;

public sealed class AssessmentFinding
{
    public Guid Id { get; private set; }

    public Guid AssessmentRecordId { get; private set; }

    public string RuleCode { get; private set; } = string.Empty;

    public int Score { get; private set; }

    public string Explanation { get; private set; } = string.Empty;

    public List<Guid> EvidenceEventIds { get; private set; } = [];

    private AssessmentFinding()
    {
    }

    internal AssessmentFinding(
        Guid assessmentRecordId,
        RuleMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);

        if (assessmentRecordId == Guid.Empty)
        {
            throw new ArgumentException(
                "An assessment ID is required.",
                nameof(assessmentRecordId));
        }

        if (string.IsNullOrWhiteSpace(match.RuleCode))
        {
            throw new ArgumentException(
                "A rule code is required.",
                nameof(match));
        }

        if (match.Score < 0)
        {
            throw new ArgumentException(
                "A rule score cannot be negative.",
                nameof(match));
        }

        if (string.IsNullOrWhiteSpace(match.Explanation))
        {
            throw new ArgumentException(
                "An explanation is required.",
                nameof(match));
        }

        if (match.EvidenceEventIds is null ||
            match.EvidenceEventIds.Count == 0 ||
            match.EvidenceEventIds.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException(
                "Valid evidence event IDs are required.",
                nameof(match));
        }

        Id = Guid.NewGuid();
        AssessmentRecordId = assessmentRecordId;
        RuleCode = match.RuleCode;
        Score = match.Score;
        Explanation = match.Explanation;
        EvidenceEventIds = match.EvidenceEventIds.ToList();
    }
}