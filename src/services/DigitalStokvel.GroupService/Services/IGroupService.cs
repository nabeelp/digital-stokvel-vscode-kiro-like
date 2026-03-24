using DigitalStokvel.GroupService.DTOs;

namespace DigitalStokvel.GroupService.Services;

/// <summary>
/// Service interface for group management operations
/// </summary>
public interface IGroupService
{
    /// <summary>
    /// Create a new group
    /// </summary>
    Task<CreateGroupResponse> CreateGroupAsync(CreateGroupRequest request, Guid creatorUserId);

    /// <summary>
    /// Get group details by ID
    /// </summary>
    Task<GroupDetailsResponse?> GetGroupByIdAsync(Guid groupId);

    /// <summary>
    /// Get all groups for a user
    /// </summary>
    Task<UserGroupsResponse> GetUserGroupsAsync(Guid userId);
}
