using BankingFraudDetector.Api.Contracts;
using BankingFraudDetector.Application.FraudDetection;
using Microsoft.AspNetCore.Mvc;

namespace BankingFraudDetector.Api.Controllers;

[ApiController]
[Route("api/assessments")]
public sealed class AssessmentsController(
    RunAccountAssessment runAssessment,
    GetAssessment getAssessment) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Run(
        [FromBody] RunAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.AccountId == Guid.Empty)
            ModelState.AddModelError(
                "accountId", "An account ID is required.");

        if (request.FromInclusive == default ||
            request.ToExclusive == default)
            ModelState.AddModelError(
                "timeWindow", "Both time boundaries are required.");

        if (request.FromInclusive >= request.ToExclusive)
            ModelState.AddModelError(
                "timeWindow", "The start must precede the end.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var assessmentId = await runAssessment.ExecuteAsync(
            request.AccountId,
            request.FromInclusive,
            request.ToExclusive,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { assessmentId },
            new { assessmentId });
    }

    [HttpGet("{assessmentId:guid}")]
    public async Task<IActionResult> GetById(
        Guid assessmentId,
        CancellationToken cancellationToken)
    {
        if (assessmentId == Guid.Empty)
        {
            ModelState.AddModelError(
                "assessmentId", "An assessment ID is required.");

            return ValidationProblem(ModelState);
        }

        var assessment = await getAssessment.ExecuteAsync(
            assessmentId,
            cancellationToken);

        if (assessment is null)
            return NotFound();

        return Ok(new
        {
            assessment.Id,
            assessment.AccountId,
            assessment.AssessedAtUtc,
            assessment.FromInclusiveUtc,
            assessment.ToExclusiveUtc,
            assessment.TotalScore,
            riskBand = assessment.RiskBand.ToString(),
            assessment.RuleSetVersion,
            findings = assessment.Findings
                .OrderBy(x => x.RuleCode)
                .Select(x => new
                {
                    x.Id,
                    x.RuleCode,
                    x.Score,
                    x.Explanation,
                    x.EvidenceEventIds
                })
        });
    }
}