using DigitalStokvel.ContributionService.DTOs;
using DigitalStokvel.ContributionService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalStokvel.ContributionService.Controllers;

/// <summary>
/// REST API controller for contribution operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ContributionsController : ControllerBase
{
    private readonly IContributionService _contributionService;
    private readonly ILogger<ContributionsController> _logger;

    public ContributionsController(IContributionService contributionService, ILogger<ContributionsController> logger)
    {
        _contributionService = contributionService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new contribution
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateContributionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateContribution([FromBody] CreateContributionRequest request)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Invalid user ID in JWT token");
                return Unauthorized(new { error = "Invalid user ID in token" });
            }

            var response = await _contributionService.CreateContributionAsync(request, userId);

            return CreatedAtAction(
                nameof(GetContributionById),
                new { contributionId = response.Id },
                response
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for contribution creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contribution");
            return StatusCode(500, new { error = "An error occurred while creating the contribution" });
        }
    }

    /// <summary>
    /// Get contribution by ID (placeholder)
    /// </summary>
    [HttpGet("{contributionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContributionById(Guid contributionId)
    {
        // TODO: Implement get contribution by ID
        await Task.CompletedTask;
        return NotFound(new { error = "Contribution not found" });
    }

    /// <summary>
    /// Get contribution history for a group
    /// </summary>
    [HttpGet("~/api/v1/groups/{groupId}/contributions")]
    [ProducesResponseType(typeof(ContributionHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGroupContributions(
        Guid groupId,
        [FromQuery] Guid? memberId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50)
    {
        try
        {
            if (page < 1 || limit < 1 || limit > 100)
            {
                return BadRequest(new { error = "Invalid pagination parameters" });
            }

            var response = await _contributionService.GetGroupContributionsAsync(
                groupId,
                memberId,
                fromDate,
                toDate,
                page,
                limit
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contribution history for group {GroupId}", groupId);
            return StatusCode(500, new { error = "An error occurred while fetching contributions" });
        }
    }

    /// <summary>
    /// Set up a recurring payment
    /// </summary>
    [HttpPost("~/api/v1/recurring-payments")]
    [ProducesResponseType(typeof(CreateRecurringPaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRecurringPayment([FromBody] CreateRecurringPaymentRequest request)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Invalid user ID in JWT token");
                return Unauthorized(new { error = "Invalid user ID in token" });
            }

            var response = await _contributionService.CreateRecurringPaymentAsync(request, userId);

            return CreatedAtAction(
                nameof(GetUserRecurringPayments),
                new { userId = response.Id },
                response
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for recurring payment creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation for recurring payment");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recurring payment");
            return StatusCode(500, new { error = "An error occurred while creating the recurring payment" });
        }
    }

    /// <summary>
    /// Get user's recurring payments
    /// </summary>
    [HttpGet("~/api/v1/users/me/recurring-payments")]
    [ProducesResponseType(typeof(RecurringPaymentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserRecurringPayments()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Invalid user ID in JWT token");
                return Unauthorized(new { error = "Invalid user ID in token" });
            }

            var response = await _contributionService.GetUserRecurringPaymentsAsync(userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recurring payments for user");
            return StatusCode(500, new { error = "An error occurred while fetching recurring payments" });
        }
    }

    /// <summary>
    /// Extract user ID from JWT claims
    /// </summary>
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
