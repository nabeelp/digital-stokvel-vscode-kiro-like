using System.Text.Json;
using DigitalStokvel.NotificationService.Data;
using DigitalStokvel.NotificationService.DTOs;
using DigitalStokvel.NotificationService.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.NotificationService.Services;

/// <summary>
/// Core notification service with multi-channel delivery, templating, and retry logic
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationTemplateService _templateService;
    private readonly ILogger<NotificationService> _logger;
    private readonly Dictionary<NotificationChannel, INotificationProvider> _providers;

    public NotificationService(
        ApplicationDbContext context,
        INotificationTemplateService templateService,
        IEnumerable<INotificationProvider> providers,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _templateService = templateService;
        _logger = logger;
        
        // Map providers by channel
        _providers = providers.ToDictionary(
            p => Enum.Parse<NotificationChannel>(p.Channel, ignoreCase: true),
            p => p
        );
    }

    public async Task<SendNotificationResponse> SendNotificationAsync(SendNotificationRequest request)
    {
        try
        {
            // Create notification record
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = request.RecipientId,
                Channel = request.Channel,
                TemplateKey = request.TemplateKey,
                Payload = JsonSerializer.Serialize(request.Payload),
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Created notification {NotificationId} for user {RecipientId} via {Channel}",
                notification.Id,
                request.RecipientId,
                request.Channel);

            // Render template
            var languageCode = request.LanguageCode ?? "en";
            var message = _templateService.RenderTemplate(
                request.TemplateKey,
                languageCode,
                request.Payload);

            // Send immediately
            var sent = await SendNotificationInternalAsync(notification, message);

            return new SendNotificationResponse
            {
                Id = notification.Id,
                Status = notification.Status,
                Message = sent 
                    ? "Notification sent successfully" 
                    : "Notification queued for delivery",
                CreatedAt = notification.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            throw;
        }
    }

    public async Task<GetNotificationResponse?> GetNotificationAsync(Guid id)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notification == null)
        {
            return null;
        }

        return MapToGetNotificationResponse(notification);
    }

    public async Task<NotificationStatusResponse?> GetNotificationStatusAsync(Guid id)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notification == null)
        {
            return null;
        }

        return new NotificationStatusResponse
        {
            Id = notification.Id,
            Status = notification.Status,
            Channel = notification.Channel,
            SentAt = notification.SentAt,
            DeliveredAt = notification.DeliveredAt,
            RetryCount = notification.RetryCount,
            ErrorMessage = notification.ErrorMessage
        };
    }

    public async Task<List<GetNotificationResponse>> GetUserNotificationsAsync(
        Guid recipientId,
        int skip = 0,
        int take = 20)
    {
        var notifications = await _context.Notifications
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return notifications.Select(MapToGetNotificationResponse).ToList();
    }

    public async Task ProcessPendingNotificationsAsync(int batchSize = 100)
    {
        try
        {
            _logger.LogInformation("Processing pending notifications (batch size: {BatchSize})", batchSize);

            var pendingNotifications = await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Pending)
                .OrderBy(n => n.CreatedAt)
                .Take(batchSize)
                .ToListAsync();

            _logger.LogInformation("Found {Count} pending notifications", pendingNotifications.Count);

            foreach (var notification in pendingNotifications)
            {
                try
                {
                    // Deserialize payload
                    var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(notification.Payload)
                        ?? new Dictionary<string, string>();

                    // Render template (default to English)
                    var message = _templateService.RenderTemplate(
                        notification.TemplateKey,
                        "en",
                        payload);

                    // Send notification
                    await SendNotificationInternalAsync(notification, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing notification {NotificationId}",
                        notification.Id);

                    // Mark as failed
                    notification.Status = NotificationStatus.Failed;
                    notification.ErrorMessage = ex.Message;
                    notification.RetryCount++;
                    notification.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation("Completed processing pending notifications");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessPendingNotificationsAsync");
        }
    }

    public async Task RetryFailedNotificationsAsync(int batchSize = 50)
    {
        try
        {
            _logger.LogInformation("Retrying failed notifications (batch size: {BatchSize})", batchSize);

            var failedNotifications = await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Failed && n.RetryCount < 3)
                .OrderBy(n => n.CreatedAt)
                .Take(batchSize)
                .ToListAsync();

            _logger.LogInformation("Found {Count} failed notifications to retry", failedNotifications.Count);

            foreach (var notification in failedNotifications)
            {
                try
                {
                    // Reset to pending
                    notification.Status = NotificationStatus.Pending;
                    notification.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Deserialize payload
                    var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(notification.Payload)
                        ?? new Dictionary<string, string>();

                    // Render template
                    var message = _templateService.RenderTemplate(
                        notification.TemplateKey,
                        "en",
                        payload);

                    // Retry send with exponential backoff delay
                    var delaySeconds = Math.Pow(2, notification.RetryCount) * 10; // 10s, 20s, 40s
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

                    await SendNotificationInternalAsync(notification, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error retrying notification {NotificationId}",
                        notification.Id);

                    // Increment retry count
                    notification.Status = NotificationStatus.Failed;
                    notification.ErrorMessage = ex.Message;
                    notification.RetryCount++;
                    notification.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation("Completed retrying failed notifications");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RetryFailedNotificationsAsync");
        }
    }

    /// <summary>
    /// Internal method to send notification using appropriate provider
    /// </summary>
    private async Task<bool> SendNotificationInternalAsync(Notification notification, string message)
    {
        try
        {
            // Get provider for channel
            if (!_providers.TryGetValue(notification.Channel, out var provider))
            {
                throw new InvalidOperationException($"No provider found for channel: {notification.Channel}");
            }

            // Mark as sent
            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Send using provider
            var success = await provider.SendAsync(notification.RecipientId, message);

            if (success)
            {
                // Mark as delivered
                notification.Status = NotificationStatus.Delivered;
                notification.DeliveredAt = DateTime.UtcNow;
                notification.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Notification {NotificationId} delivered successfully via {Channel}",
                    notification.Id,
                    notification.Channel);
            }
            else
            {
                // Mark as failed
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = "Provider returned failure";
                notification.RetryCount++;
                notification.UpdatedAt = DateTime.UtcNow;

                _logger.LogWarning(
                    "Notification {NotificationId} delivery failed via {Channel}",
                    notification.Id,
                    notification.Channel);
            }

            await _context.SaveChangesAsync();
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending notification {NotificationId}",
                notification.Id);

            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = ex.Message;
            notification.RetryCount++;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return false;
        }
    }

    private GetNotificationResponse MapToGetNotificationResponse(Notification notification)
    {
        return new GetNotificationResponse
        {
            Id = notification.Id,
            RecipientId = notification.RecipientId,
            Channel = notification.Channel,
            TemplateKey = notification.TemplateKey,
            Payload = notification.Payload,
            Status = notification.Status,
            SentAt = notification.SentAt,
            DeliveredAt = notification.DeliveredAt,
            RetryCount = notification.RetryCount,
            ErrorMessage = notification.ErrorMessage,
            CreatedAt = notification.CreatedAt,
            UpdatedAt = notification.UpdatedAt
        };
    }
}
