using BankingFraudDetector.Api.Contracts;
using BankingFraudDetector.Application.Events;
using Microsoft.AspNetCore.Mvc;

namespace BankingFraudDetector.Api.Controllers;

[ApiController]
[Route("api/account-events")]
public sealed class AccountEventsController(RecordAccountEvent useCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Record(
        [FromBody] RecordAccountEventRequest request,
        CancellationToken cancellationToken)
    {
        if (request.AccountId == Guid.Empty)
            ModelState.AddModelError("accountId", "An account ID is required.");

        if (!Enum.IsDefined(request.EventType))
            ModelState.AddModelError("eventType", "The event type is not supported.");

        if (request.OccurredAt == default)
            ModelState.AddModelError("occurredAt", "An occurrence time is required.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var eventId = await useCase.ExecuteAsync(
            request.AccountId, request.EventType, request.OccurredAt, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new { eventId });
    }
}
