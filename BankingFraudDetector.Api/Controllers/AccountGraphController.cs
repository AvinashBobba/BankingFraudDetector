using BankingFraudDetector.Application.Graphs;
using Microsoft.AspNetCore.Mvc;

namespace BankingFraudDetector.Api.Controllers;

[ApiController]
[Route("api/accounts/{accountId:guid}/graph")]
public sealed class AccountGraphController(
    FindLinkedAccounts findLinkedAccounts) : ControllerBase
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
}