using DigitalStokvel.GroupService.DTOs;
using DigitalStokvel.GroupService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalStokvel.GroupService.Controllers;

/// <summary>
/// API controller for group management
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(IGroupService groupService, ILogger<GroupsController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new group
    /// </summary>
    /// <param name="request">Group creation request</param>
    /// <returns>Created group details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateGroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateGroupResponse>> CreateGroup([FromBody] CreateGroupRequest request)
    {
        try
        {
            // Get user ID from JWT token claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Invalid or missing user ID in token");
                return Unauthorized("Invalid user ID");
            }

            var response = await _groupService.CreateGroupAsync(request, userId);
            
            _logger.LogInformation("Group created successfully: {GroupId} by user {UserId}", 
                response.Id, userId);

            return CreatedAtAction(
                nameof(GetGroupById),
                new { groupId = response.Id },
                response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request while creating group");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return StatusCode(500, new { error = "An error occurred while creating the group" });
        }
    }

    /// <summary>
    /// Get group details by ID
    /// </summary>
    /// <param name="groupId">Group ID</param>
    /// <returns>Group details</returns>
    [HttpGet("{groupId}")]
    [ProducesResponseType(typeof(GroupDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GroupDetailsResponse>> GetGroupById(Guid groupId)
    {
        try
        {
            _logger.LogInformation("Retrieving group details for ID: {GroupId}", groupId);

            var group = await _groupService.GetGroupByIdAsync(groupId);

            if (group == null)
            {
                _logger.LogWarning("Group not found: {GroupId}", groupId);
                return NotFound(new { error = $"Group with ID {groupId} not found" });
            }

            return Ok(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving group {GroupId}", groupId);
            return StatusCode(500, new { error = "An error occurred while retrieving the group" });
        }
    }

    /// <summary>
    /// Get the current user's groups
    /// </summary>
    /// <returns>List of user's groups</returns>
    [HttpGet("~/api/v1/users/me/groups")]
    [ProducesResponseType(typeof(UserGroupsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserGroupsResponse>> GetMyGroups()
    {
        try
        {
            // Get user ID from JWT token claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Invalid or missing user ID in token");
                return Unauthorized("Invalid user ID");
            }

            _logger.LogInformation("Retrieving groups for user: {UserId}", userId);

            var response = await _groupService.GetUserGroupsAsync(userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user groups");
            return StatusCode(500, new { error = "An error occurred while retrieving your groups" });
        }
    }
}
