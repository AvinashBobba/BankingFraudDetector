using System.ComponentModel;
using System.Text.Json;
using BankingFraudDetector.Application.FraudDetection;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace BankingFraudDetector.McpServer.Tools;

[McpServerToolType]
public sealed class AssessmentTools(IServiceScopeFactory scopeFactory)
{
    [McpServerTool]
    [Description(
        "Get a saved fraud assessment, including its score, risk band, " +
        "rule findings and evidence event IDs.")]
    public async Task<string> GetAssessmentEvidence(
        [Description("The saved assessment ID.")]
        Guid assessmentId,
        CancellationToken cancellationToken)
    {
        if (assessmentId == Guid.Empty)
        {
            throw new ArgumentException(
                "An assessment ID is required.",
                nameof(assessmentId));
        }

        await using var scope = scopeFactory.CreateAsyncScope();

        var getAssessment = scope.ServiceProvider
            .GetRequiredService<GetAssessment>();

        var assessment = await getAssessment.ExecuteAsync(
            assessmentId,
            cancellationToken);

        if (assessment is null)
        {
            return JsonSerializer.Serialize(new
            {
                found = false,
                assessmentId
            });
        }

        return JsonSerializer.Serialize(new
        {
            found = true,
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
}