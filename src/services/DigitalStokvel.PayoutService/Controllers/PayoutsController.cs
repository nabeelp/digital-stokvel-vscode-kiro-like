using DigitalStokvel.PayoutService.DTOs;
using DigitalStokvel.PayoutService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalStokvel.PayoutService.Controllers;

/// <summary>
/// REST API controller for payout operations.
/// Handles payout initiation, approval, and status retrieval.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PayoutsController : ControllerBase
{
    private readonly IPayoutService _payoutService;
    private readonly ILogger<PayoutsController> _logger;

    public PayoutsController(IPayoutService payoutService, ILogger<PayoutsController> logger)
    {
        _payoutService = payoutService;
        _logger = logger;
    }

    /// <summary>
    /// Initiates a new payout for a group (typically by Chairperson).
    /// POST /api/v1/payouts
    /// </summary>
    /// <param name="request">Payout initiation request</param>
    /// <returns>Payout initiation response with status pending_approval</returns>
    [HttpPost]
    public async Task<ActionResult<InitiatePayoutResponse>> InitiatePayout([FromBody] InitiatePayoutRequest request)
    {
        try
        {
            // Extract user ID from JWT claims
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized payout initiation attempt - invalid user ID");
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            _logger.LogInformation("User {UserId} initiating payout for group {GroupId}", userId, request.GroupId);

            var response = await _payoutService.InitiatePayoutAsync(request, userId);

            return CreatedAtAction(
                nameof(GetPayoutStatus),
                new { payoutId = response.Id },
                response
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for payout initiation");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during payout initiation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payout");
            return StatusCode(500, new { error = "An error occurred while initiating the payout" });
        }
    }

    /// <summary>
    /// Approves a payout (typically by Treasurer).
    /// POST /api/v1/payouts/{payoutId}/approve
    /// </summary>
    /// <param name="payoutId">ID of the payout to approve</param>
    /// <param name="request">Approval request with PIN and optional comment</param>
    /// <returns>Approval response with execution status</returns>
    [HttpPost("{payoutId}/approve")]
    public async Task<ActionResult<ApprovePayoutResponse>> ApprovePayout(Guid payoutId, [FromBody] ApprovePayoutRequest request)
    {
        try
        {
            // Extract user ID from JWT claims
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized payout approval attempt - invalid user ID");
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            _logger.LogInformation("User {UserId} approving payout {PayoutId}", userId, payoutId);

            var response = await _payoutService.ApprovePayoutAsync(payoutId, request, userId);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for payout approval");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during payout approval: {Message}", ex.Message);
            
            // Check if payout not found
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { error = ex.Message });
            }
            
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving payout {PayoutId}", payoutId);
            return StatusCode(500, new { error = "An error occurred while approving the payout" });
        }
    }

    /// <summary>
    /// Retrieves the status of a payout including disbursements.
    /// GET /api/v1/payouts/{payoutId}
    /// </summary>
    /// <param name="payoutId">ID of the payout</param>
    /// <returns>Payout status with disbursements</returns>
    [HttpGet("{payoutId}")]
    public async Task<ActionResult<PayoutStatusResponse>> GetPayoutStatus(Guid payoutId)
    {
        try
        {
            _logger.LogInformation("Retrieving status for payout {PayoutId}", payoutId);

            var response = await _payoutService.GetPayoutStatusAsync(payoutId);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Payout {PayoutId} not found", payoutId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payout status for {PayoutId}", payoutId);
            return StatusCode(500, new { error = "An error occurred while retrieving the payout status" });
        }
    }

    /// <summary>
    /// Extracts the user ID from JWT claims.
    /// </summary>
    /// <returns>User ID as Guid, or Guid.Empty if not found or invalid</returns>
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Guid.Empty;
        }

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
