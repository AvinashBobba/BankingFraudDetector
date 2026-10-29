using BankingFraudDetector.Application.Graphs;
using Microsoft.AspNetCore.Mvc;

namespace BankingFraudDetector.Api.Controllers;

[ApiController]
[Route("api/accounts/{accountId:guid}/graph")]
public sealed class AccountGraphController(
    FindLinkedAccounts findLinkedAccounts,
    FindConnectedAccounts findConnectedAccounts) : ControllerBase
{
    [HttpGet("shared-devices")]
    public async Task<IActionResult> GetSharedDevices(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty)
        {
            ModelState.AddModelError(
                "accountId", "An account ID is required.");

            return ValidationProblem(ModelState);
        }

        var results = await findLinkedAccounts.ExecuteAsync(
            accountId,
            cancellationToken);

        return Ok(results);
    }

    [HttpGet("connected-accounts")]
    public async Task<IActionResult> GetConnectedAccounts(
    Guid accountId,
    CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty)
        {
            ModelState.AddModelError(
                "accountId", "An account ID is required.");

            return ValidationProblem(ModelState);
        }

        try
        {
            var accounts = await findConnectedAccounts.ExecuteAsync(
                accountId,
                cancellationToken);

            return Ok(new
            {
                accountId,
                connectedAccounts = accounts,
                limitation =
                    "Connectivity is based on the current GDS projection " +
                    "and does not establish fraud."
            });
        }
        catch (GraphProjectionUnavailableException)
        {
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Graph analysis is not ready.",
                detail:
                    "The graph projection is unavailable. " +
                    "It must be prepared before connected-account analysis.");
        }
    }
}