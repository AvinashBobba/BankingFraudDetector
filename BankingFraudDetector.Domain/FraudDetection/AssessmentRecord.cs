namespace BankingFraudDetector.Domain.FraudDetection;

public sealed class AssessmentRecord
{
    private readonly List<AssessmentFinding> _findings = [];
    public IReadOnlyCollection<AssessmentFinding> Findings =>
        _findings.AsReadOnly();
    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public DateTimeOffset AssessedAtUtc { get; private set; }

    public DateTimeOffset FromInclusiveUtc { get; private set; }

    public DateTimeOffset ToExclusiveUtc { get; private set; }

    public int TotalScore { get; private set; }

    public RiskBand RiskBand { get; private set; }

    public string RuleSetVersion { get; private set; } = string.Empty;

    private AssessmentRecord()
    {
    }

    public AssessmentRecord(
        FraudAssessment assessment,
        DateTimeOffset assessedAt,
        DateTimeOffset fromInclusive,
        DateTimeOffset toExclusive,
        string ruleSetVersion)
    {
        ArgumentNullException.ThrowIfNull(assessment);

        if (assessment.AccountId == Guid.Empty)
        {
            throw new ArgumentException(
                "An account ID is required.",
                nameof(assessment));
        }

        if (assessedAt == default)
        {
            throw new ArgumentException(
                "An assessment time is required.",
                nameof(assessedAt));
        }

        if (fromInclusive >= toExclusive)
        {
            throw new ArgumentException(
                "The start must precede the end.",
                nameof(toExclusive));
        }

        if (string.IsNullOrWhiteSpace(ruleSetVersion))
        {
            throw new ArgumentException(
                "A rule set version is required.",
                nameof(ruleSetVersion));
        }

        Id = Guid.NewGuid();
        AccountId = assessment.AccountId;
        AssessedAtUtc = assessedAt.ToUniversalTime();
        FromInclusiveUtc = fromInclusive.ToUniversalTime();
        ToExclusiveUtc = toExclusive.ToUniversalTime();
        TotalScore = assessment.TotalScore;
        RiskBand = assessment.RiskBand;
        RuleSetVersion = ruleSetVersion;
        foreach (var match in assessment.Matches)
        {
            _findings.Add(new AssessmentFinding(Id, match));
        }
    }

}