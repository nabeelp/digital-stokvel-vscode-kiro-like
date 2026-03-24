using DigitalStokvel.GovernanceService.DTOs;
using DigitalStokvel.GovernanceService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalStokvel.GovernanceService.Controllers;

/// <summary>
/// REST API controller for governance operations (voting and disputes).
/// </summary>
[ApiController]
[Route("api/v1")]
[Authorize]
public class GovernanceController : ControllerBase
{
    private readonly IGovernanceService _governanceService;
    private readonly ILogger<GovernanceController> _logger;

    public GovernanceController(IGovernanceService governanceService, ILogger<GovernanceController> logger)
    {
        _governanceService = governanceService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new vote for a group.
    /// POST /api/v1/votes
    /// </summary>
    /// <param name="request">Vote creation request</param>
    /// <returns>Vote creation response with quorum information</returns>
    [HttpPost("votes")]
    public async Task<ActionResult<CreateVoteResponse>> CreateVote([FromBody] CreateVoteRequest request)
    {
        try
        {
            // Extract user ID from JWT claims
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized vote creation attempt - invalid user ID");
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            _logger.LogInformation("User {UserId} creating vote for group {GroupId}", userId, request.GroupId);

            var response = await _governanceService.CreateVoteAsync(request, userId);

            return CreatedAtAction(
                nameof(GetVoteStatus),
                new { voteId = response.Id },
                response
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for vote creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during vote creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vote");
            return StatusCode(500, new { error = "An error occurred while creating the vote" });
        }
    }

    /// <summary>
    /// Casts a vote for a member.
    /// POST /api/v1/votes/{voteId}/responses
    /// </summary>
    /// <param name="voteId">ID of the vote</param>
    /// <param name="request">Vote casting request with selected option</param>
    /// <returns>Vote casting response</returns>
    [HttpPost("votes/{voteId}/responses")]
    public async Task<ActionResult<CastVoteResponse>> CastVote(Guid voteId, [FromBody] CastVoteRequest request)
    {
        try
        {
            // Extract user ID from JWT claims
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized vote casting attempt - invalid user ID");
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            _logger.LogInformation("User {UserId} casting vote for vote {VoteId}", userId, voteId);

            var response = await _governanceService.CastVoteAsync(voteId, request, userId);

            return CreatedAtAction(
                nameof(GetVoteStatus),
                new { voteId = voteId },
                response
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for vote casting");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during vote casting: {Message}", ex.Message);
            
            // Check if vote not found
            if (ex.Message.Contains("not found"))
            {
                return NotFound(new { error = ex.Message });
            }
            
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error casting vote for vote {VoteId}", voteId);
            return StatusCode(500, new { error = "An error occurred while casting the vote" });
        }
    }

    /// <summary>
    /// Retrieves vote status including results and quorum information.
    /// GET /api/v1/votes/{voteId}
    /// </summary>
    /// <param name="voteId">ID of the vote</param>
    /// <returns>Vote status with results and quorum information</returns>
    [HttpGet("votes/{voteId}")]
    public async Task<ActionResult<VoteStatusResponse>> GetVoteStatus(Guid voteId)
    {
        try
        {
            _logger.LogInformation("Retrieving vote status for {VoteId}", voteId);

            var response = await _governanceService.GetVoteStatusAsync(voteId);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Vote {VoteId} not found", voteId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vote status for {VoteId}", voteId);
            return StatusCode(500, new { error = "An error occurred while retrieving the vote status" });
        }
    }

    /// <summary>
    /// Raises a dispute for a group.
    /// POST /api/v1/disputes
    /// </summary>
    /// <param name="request">Dispute creation request</param>
    /// <returns>Dispute creation response with resolution deadline</returns>
    [HttpPost("disputes")]
    public async Task<ActionResult<RaiseDisputeResponse>> RaiseDispute([FromBody] RaiseDisputeRequest request)
    {
        try
        {
            // Extract user ID from JWT claims
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized dispute creation attempt - invalid user ID");
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            _logger.LogInformation("User {UserId} raising dispute for group {GroupId}", userId, request.GroupId);

            var response = await _governanceService.RaiseDisputeAsync(request, userId);

            return CreatedAtAction(
                nameof(GetDisputeDetails),
                new { disputeId = response.Id },
                response
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for dispute creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during dispute creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error raising dispute");
            return StatusCode(500, new { error = "An error occurred while raising the dispute" });
        }
    }

    /// <summary>
    /// Retrieves dispute details.
    /// GET /api/v1/disputes/{disputeId}
    /// </summary>
    /// <param name="disputeId">ID of the dispute</param>
    /// <returns>Dispute details with evidence and status</returns>
    [HttpGet("disputes/{disputeId}")]
    public async Task<ActionResult<DisputeDetailsResponse>> GetDisputeDetails(Guid disputeId)
    {
        try
        {
            _logger.LogInformation("Retrieving dispute details for {DisputeId}", disputeId);

            var response = await _governanceService.GetDisputeDetailsAsync(disputeId);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Dispute {DisputeId} not found", disputeId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dispute details for {DisputeId}", disputeId);
            return StatusCode(500, new { error = "An error occurred while retrieving the dispute details" });
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
