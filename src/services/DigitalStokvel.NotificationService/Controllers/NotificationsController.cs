using DigitalStokvel.NotificationService.DTOs;
using DigitalStokvel.NotificationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalStokvel.NotificationService.Controllers;

/// <summary>
/// REST API controller for notification operations.
/// </summary>
[ApiController]
[Route("api/v1")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Sends a notification to a user.
    /// POST /api/v1/notifications/send
    /// </summary>
    /// <param name="request">Notification send request</param>
    /// <returns>Notification send response</returns>
    [HttpPost("notifications/send")]
    public async Task<ActionResult<SendNotificationResponse>> SendNotification(
        [FromBody] SendNotificationRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Sending notification to user {RecipientId} via {Channel}",
                request.RecipientId,
                request.Channel);

            var response = await _notificationService.SendNotificationAsync(request);

            return CreatedAtAction(
                nameof(GetNotification),
                new { id = response.Id },
                response
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for sending notification");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            return StatusCode(500, new { error = "An error occurred while sending the notification" });
        }
    }

    /// <summary>
    /// Gets notification details by ID.
    /// GET /api/v1/notifications/{id}
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Notification details</returns>
    [HttpGet("notifications/{id}")]
    public async Task<ActionResult<GetNotificationResponse>> GetNotification(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving notification {NotificationId}", id);

            var notification = await _notificationService.GetNotificationAsync(id);

            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found", id);
                return NotFound(new { error = $"Notification with ID {id} not found" });
            }

            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification {NotificationId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the notification" });
        }
    }

    /// <summary>
    /// Gets notification delivery status.
    /// GET /api/v1/notifications/{id}/status
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Notification delivery status</returns>
    [HttpGet("notifications/{id}/status")]
    public async Task<ActionResult<NotificationStatusResponse>> GetNotificationStatus(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving notification status for {NotificationId}", id);

            var status = await _notificationService.GetNotificationStatusAsync(id);

            if (status == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found", id);
                return NotFound(new { error = $"Notification with ID {id} not found" });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification status for {NotificationId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the notification status" });
        }
    }

    /// <summary>
    /// Gets all notifications for the authenticated user.
    /// GET /api/v1/notifications/me
    /// </summary>
    /// <param name="skip">Number of records to skip (pagination)</param>
    /// <param name="take">Number of records to take (pagination)</param>
    /// <returns>List of user notifications</returns>
    [HttpGet("notifications/me")]
    public async Task<ActionResult<List<GetNotificationResponse>>> GetMyNotifications(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        try
        {
            // Extract user ID from JWT claims
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Unauthorized notification retrieval attempt - invalid user ID");
                return Unauthorized(new { error = "Invalid or missing user ID in token" });
            }

            _logger.LogInformation(
                "Retrieving notifications for user {UserId} (skip: {Skip}, take: {Take})",
                userId,
                skip,
                take);

            var notifications = await _notificationService.GetUserNotificationsAsync(
                userId,
                skip,
                take);

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user notifications");
            return StatusCode(500, new { error = "An error occurred while retrieving notifications" });
        }
    }

    /// <summary>
    /// Gets all notifications for a specific user (admin endpoint).
    /// GET /api/v1/users/{userId}/notifications
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="skip">Number of records to skip (pagination)</param>
    /// <param name="take">Number of records to take (pagination)</param>
    /// <returns>List of user notifications</returns>
    [HttpGet("users/{userId}/notifications")]
    [Authorize(Roles = "Admin,Chairperson")]
    public async Task<ActionResult<List<GetNotificationResponse>>> GetUserNotifications(
        Guid userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        try
        {
            _logger.LogInformation(
                "Retrieving notifications for user {UserId} (skip: {Skip}, take: {Take})",
                userId,
                skip,
                take);

            var notifications = await _notificationService.GetUserNotificationsAsync(
                userId,
                skip,
                take);

            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user notifications for {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while retrieving notifications" });
        }
    }

    /// <summary>
    /// Helper method to extract user ID from JWT claims.
    /// </summary>
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("user_id")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
