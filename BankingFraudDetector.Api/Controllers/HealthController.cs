using Microsoft.AspNetCore.Mvc;

namespace BankingFraudDetector.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "Healthy",
        service = "BankingFraudDetector.Api"
    });
}
