using BankingFraudDetector.Application.Graphs;
using Microsoft.AspNetCore.Mvc;

namespace BankingFraudDetector.Api.Controllers;

[ApiController]
[Route("api/accounts/{accountId:guid}/graph")]
public sealed class AccountGraphController(
    FindLinkedAccounts findLinkedAccounts,
    FindConnectedAccounts findConnectedAccounts,
    FindAccountConnectionPath findConnectionPath) : ControllerBase
{
    [HttpGet("paths/{targetAccountId:guid}")]
    public async Task<IActionResult> GetConnectionPath(
        Guid accountId, Guid targetAccountId, CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty || targetAccountId == Guid.Empty || accountId == targetAccountId)
        {
            ModelState.AddModelError("accounts", "Two distinct, nonempty account IDs are required.");
            return ValidationProblem(ModelState);
        }

        var path = await findConnectionPath.ExecuteAsync(accountId, targetAccountId, cancellationToken);
        return Ok(new
        {
            found = path is not null,
            maxHops = 6,
            path,
            limitation = "One shortest path in the current graph, within six hops. No path does not prove no connection. Connectivity does not prove fraud."
        });
    }

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
