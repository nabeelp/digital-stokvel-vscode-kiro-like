using DigitalStokvel.NotificationService.DTOs;
using DigitalStokvel.NotificationService.Entities;

namespace DigitalStokvel.NotificationService.Services;

/// <summary>
/// Interface for notification service operations
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification using the specified channel
    /// </summary>
    Task<SendNotificationResponse> SendNotificationAsync(SendNotificationRequest request);

    /// <summary>
    /// Gets notification details by ID
    /// </summary>
    Task<GetNotificationResponse?> GetNotificationAsync(Guid id);

    /// <summary>
    /// Gets notification delivery status
    /// </summary>
    Task<NotificationStatusResponse?> GetNotificationStatusAsync(Guid id);

    /// <summary>
    /// Gets all notifications for a recipient
    /// </summary>
    Task<List<GetNotificationResponse>> GetUserNotificationsAsync(Guid recipientId, int skip = 0, int take = 20);

    /// <summary>
    /// Processes pending notifications (called by background worker)
    /// </summary>
    Task ProcessPendingNotificationsAsync(int batchSize = 100);

    /// <summary>
    /// Retries failed notifications (called by background worker)
    /// </summary>
    Task RetryFailedNotificationsAsync(int batchSize = 50);
}
