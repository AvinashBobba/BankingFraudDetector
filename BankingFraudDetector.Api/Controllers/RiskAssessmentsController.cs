using BankingFraudDetector.Application.FraudDetection;
using Microsoft.AspNetCore.Mvc;

namespace BankingFraudDetector.Api.Controllers;

[ApiController]
[Route("api/risk-assessments")]
public sealed class RiskAssessmentsController(AssessAccountRisk assessAccountRisk)
    : ControllerBase
{
    [HttpGet("{accountId:guid}")]
    public async Task<IActionResult> Get(
        Guid accountId,
        [FromQuery] DateTimeOffset fromInclusive,
        [FromQuery] DateTimeOffset toExclusive,
        CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty)
            ModelState.AddModelError("accountId", "An account ID is required.");

        if (fromInclusive == default || toExclusive == default)
            ModelState.AddModelError("timeWindow", "Both time boundaries are required.");

        if (fromInclusive >= toExclusive)
            ModelState.AddModelError("timeWindow", "The start must precede the end.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var assessment = await assessAccountRisk.ExecuteAsync(
            accountId, fromInclusive, toExclusive, cancellationToken);

        return Ok(new
        {
            assessment.AccountId,
            assessment.TotalScore,
            riskBand = assessment.RiskBand.ToString(),
            assessment.Matches
        });
    }
}
