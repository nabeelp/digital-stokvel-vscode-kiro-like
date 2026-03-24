using Microsoft.AspNetCore.Mvc;

namespace DigitalStokvel.UssdGateway.Controllers;

/// <summary>
/// Health check controller for USSD Gateway Service
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check performed");
        return Ok(new
        {
            Service = "USSD Gateway",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}
