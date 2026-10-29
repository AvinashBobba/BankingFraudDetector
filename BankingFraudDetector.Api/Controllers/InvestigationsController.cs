using BankingFraudDetector.Application.Investigations;
using Microsoft.AspNetCore.Mvc;
using BankingFraudDetector.AI.Workflows;

namespace BankingFraudDetector.Api.Controllers;

[ApiController]
[Route("api/assessments/{assessmentId:guid}/investigations")]
public sealed class InvestigationsController(
    IAssessmentInvestigator investigator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Investigate(
        Guid assessmentId,
        CancellationToken cancellationToken)
    {
        if (assessmentId == Guid.Empty)
        {
            ModelState.AddModelError(
                "assessmentId", "An assessment ID is required.");

            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await investigator.InvestigateAsync(
                assessmentId,
                cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvestigationOutputException)
        {
            return Problem(statusCode: StatusCodes.Status502BadGateway,
                title: "Investigation output could not be validated.",
                detail: "A specialist returned malformed output, changed decision fields or cited unsupported sources. No investigation was saved; retry or inspect the API logs.");
        }
        catch (OperationCanceledException)
    when (!cancellationToken.IsCancellationRequested)
        {
            return Problem(
                statusCode: StatusCodes.Status504GatewayTimeout,
                title: "The local model timed out.",
                detail: "Ollama did not complete the investigation in time.");
        }
    }
}
