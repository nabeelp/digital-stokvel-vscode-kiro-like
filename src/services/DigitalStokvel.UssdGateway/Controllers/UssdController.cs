using DigitalStokvel.UssdGateway.DTOs;
using DigitalStokvel.UssdGateway.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DigitalStokvel.UssdGateway.Controllers;

/// <summary>
/// USSD session controller
/// Handles USSD requests from aggregator per design.md Section 5.6
/// Implements 120-second session timeout and 3-level menu navigation
/// </summary>
[ApiController]
[Route("ussd")]
public class UssdController : ControllerBase
{
    private readonly IUssdSessionService _sessionService;
    private readonly IUssdMenuService _menuService;
    private readonly ILogger<UssdController> _logger;

    public UssdController(
        IUssdSessionService sessionService,
        IUssdMenuService menuService,
        ILogger<UssdController> logger)
    {
        _sessionService = sessionService;
        _menuService = menuService;
        _logger = logger;
    }

    /// <summary>
    /// USSD session endpoint
    /// POST /ussd/session
    /// Handles incoming USSD requests from aggregator
    /// </summary>
    /// <param name="request">USSD session request from aggregator</param>
    /// <returns>USSD session response with menu or result</returns>
    [HttpPost("session")]
    [ProducesResponseType(typeof(UssdSessionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UssdSessionResponseDto>> HandleSessionAsync(
        [FromBody] UssdSessionRequestDto request)
    {
        try
        {
            _logger.LogInformation("Received USSD request for session {SessionId} from {PhoneNumber}",
                request.SessionId, request.PhoneNumber);

            // Validate request
            if (string.IsNullOrEmpty(request.SessionId))
            {
                _logger.LogWarning("Invalid USSD request: Missing session ID");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = "Session ID is required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                _logger.LogWarning("Invalid USSD request: Missing phone number");
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = "Phone number is required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            // Get or create session
            var session = await _sessionService.GetOrCreateSessionAsync(
                request.SessionId,
                request.PhoneNumber,
                request.Language ?? "en");

            // Check if session has expired
            if (session.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("USSD session {SessionId} has expired", session.SessionId);
                await _sessionService.TerminateSessionAsync(session.SessionId);

                return Ok(new UssdSessionResponseDto
                {
                    SessionId = session.SessionId,
                    ResponseType = "END",
                    Message = "Session has expired. Please dial *120*7878# to start a new session.",
                    SessionState = null
                });
            }

            // Process the request through menu service
            var response = await _menuService.ProcessRequestAsync(request, session);

            // Update session if continuing
            if (response.ResponseType == "CON")
            {
                await _sessionService.UpdateSessionAsync(session);
            }
            else
            {
                // Terminate session if ending
                await _sessionService.TerminateSessionAsync(session.SessionId);
            }

            _logger.LogInformation("USSD response generated for session {SessionId}: {ResponseType}",
                session.SessionId, response.ResponseType);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing USSD request for session {SessionId}",
                request.SessionId);

            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while processing your request",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Health check endpoint for USSD service
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            Service = "USSD Gateway",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}
