using DigitalStokvel.NotificationService.Data;
using DigitalStokvel.NotificationService.DTOs;
using DigitalStokvel.NotificationService.Entities;
using DigitalStokvel.NotificationService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace DigitalStokvel.NotificationService.Tests;

public class NotificationServiceTests
{
    private readonly Mock<ILogger<DigitalStokvel.NotificationService.Services.NotificationService>> _mockLogger;
    private readonly Mock<INotificationTemplateService> _mockTemplateService;
    private readonly Mock<INotificationProvider> _mockPushProvider;
    private readonly Mock<INotificationProvider> _mockSmsProvider;

    public NotificationServiceTests()
    {
        _mockLogger = new Mock<ILogger<DigitalStokvel.NotificationService.Services.NotificationService>>();
        _mockTemplateService = new Mock<INotificationTemplateService>();
        _mockPushProvider = new Mock<INotificationProvider>();
        _mockSmsProvider = new Mock<INotificationProvider>();

        // Configure provider channels
        _mockPushProvider.Setup(p => p.Channel).Returns("push");
        _mockSmsProvider.Setup(p => p.Channel).Returns("sms");
    }

    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task SendNotificationAsync_CreatesNotificationWithPendingStatus()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var providers = new List<INotificationProvider> { _mockPushProvider.Object, _mockSmsProvider.Object };
        
        _mockTemplateService
            .Setup(t => t.RenderTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .Returns("Rendered message");

        _mockPushProvider
            .Setup(p => p.SendAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = new DigitalStokvel.NotificationService.Services.NotificationService(
            context,
            _mockTemplateService.Object,
            providers,
            _mockLogger.Object);

        var request = new SendNotificationRequest
        {
            RecipientId = Guid.NewGuid(),
            Channel = NotificationChannel.Push,
            TemplateKey = "contribution_received",
            Payload = new Dictionary<string, string>
            {
                { "amount", "500.00" },
                { "group_name", "Test Group" }
            },
            LanguageCode = "en"
        };

        // Act
        var response = await service.SendNotificationAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Contains("successfully", response.Message.ToLower());

        // Verify notification was saved
        var notification = await context.Notifications.FirstOrDefaultAsync(n => n.Id == response.Id);
        Assert.NotNull(notification);
        Assert.Equal(request.RecipientId, notification.RecipientId);
        Assert.Equal(NotificationChannel.Push, notification.Channel);
    }

    [Fact]
    public async Task SendNotificationAsync_RendersTemplateWithCorrectLanguage()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var providers = new List<INotificationProvider> { _mockPushProvider.Object };
        
        _mockTemplateService
            .Setup(t => t.RenderTemplate("payout_approved", "zu", It.IsAny<Dictionary<string, string>>()))
            .Returns("Inkokhelo igunyazwe");

        _mockPushProvider
            .Setup(p => p.SendAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = new DigitalStokvel.NotificationService.Services.NotificationService(
            context,
            _mockTemplateService.Object,
            providers,
            _mockLogger.Object);

        var request = new SendNotificationRequest
        {
            RecipientId = Guid.NewGuid(),
            Channel = NotificationChannel.Push,
            TemplateKey = "payout_approved",
            Payload = new Dictionary<string, string> { { "amount", "5000.00" } },
            LanguageCode = "zu"
        };

        // Act
        await service.SendNotificationAsync(request);

        // Assert
        _mockTemplateService.Verify(
            t => t.RenderTemplate("payout_approved", "zu", It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetNotificationAsync_ReturnsNotificationDetails()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var providers = new List<INotificationProvider> { _mockPushProvider.Object };

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Channel = NotificationChannel.Push,
            TemplateKey = "vote_created",
            Payload = "{\"subject\":\"Should we increase contributions?\"}",
            Status = NotificationStatus.Sent,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        var service = new DigitalStokvel.NotificationService.Services.NotificationService(
            context,
            _mockTemplateService.Object,
            providers,
            _mockLogger.Object);

        // Act
        var result = await service.GetNotificationAsync(notification.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(notification.Id, result.Id);
        Assert.Equal(notification.RecipientId, result.RecipientId);
        Assert.Equal(NotificationChannel.Push, result.Channel);
        Assert.Equal("vote_created", result.TemplateKey);
    }

    [Fact]
    public async Task GetNotificationStatusAsync_ReturnsDeliveryStatus()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var providers = new List<INotificationProvider> { _mockSmsProvider.Object };

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Channel = NotificationChannel.Sms,
            TemplateKey = "payment_reminder",
            Payload = "{}",
            Status = NotificationStatus.Delivered,
            SentAt = DateTime.UtcNow.AddMinutes(-5),
            DeliveredAt = DateTime.UtcNow.AddMinutes(-3),
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        var service = new DigitalStokvel.NotificationService.Services.NotificationService(
            context,
            _mockTemplateService.Object,
            providers,
            _mockLogger.Object);

        // Act
        var result = await service.GetNotificationStatusAsync(notification.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(notification.Id, result.Id);
        Assert.Equal(NotificationStatus.Delivered, result.Status);
        Assert.Equal(NotificationChannel.Sms, result.Channel);
        Assert.NotNull(result.SentAt);
        Assert.NotNull(result.DeliveredAt);
        Assert.Equal(0, result.RetryCount);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ReturnsNotificationsWithPagination()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var providers = new List<INotificationProvider> { _mockPushProvider.Object };
        var recipientId = Guid.NewGuid();

        // Create multiple notifications for the user
        for (int i = 0; i < 5; i++)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = recipientId,
                Channel = NotificationChannel.Push,
                TemplateKey = $"template_{i}",
                Payload = "{}",
                Status = NotificationStatus.Sent,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-i)
            };
            context.Notifications.Add(notification);
        }
        await context.SaveChangesAsync();

        var service = new DigitalStokvel.NotificationService.Services.NotificationService(
            context,
            _mockTemplateService.Object,
            providers,
            _mockLogger.Object);

        // Act
        var result = await service.GetUserNotificationsAsync(recipientId, skip: 0, take: 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, n => Assert.Equal(recipientId, n.RecipientId));
        
        // Verify ordering (newest first)
        Assert.True(result[0].CreatedAt >= result[1].CreatedAt);
        Assert.True(result[1].CreatedAt >= result[2].CreatedAt);
    }

    [Fact]
    public async Task ProcessPendingNotificationsAsync_ProcessesPendingNotifications()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var providers = new List<INotificationProvider> { _mockPushProvider.Object };

        // Create pending notifications
        for (int i = 0; i < 3; i++)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = Guid.NewGuid(),
                Channel = NotificationChannel.Push,
                TemplateKey = "test_template",
                Payload = JsonSerializer.Serialize(new Dictionary<string, string> { { "test", "value" } }),
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-i)
            };
            context.Notifications.Add(notification);
        }
        await context.SaveChangesAsync();

        _mockTemplateService
            .Setup(t => t.RenderTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .Returns("Test message");

        _mockPushProvider
            .Setup(p => p.SendAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = new DigitalStokvel.NotificationService.Services.NotificationService(
            context,
            _mockTemplateService.Object,
            providers,
            _mockLogger.Object);

        // Act
        await service.ProcessPendingNotificationsAsync(batchSize: 10);

        // Assert
        var pendingCount = await context.Notifications
            .CountAsync(n => n.Status == NotificationStatus.Pending);
        Assert.Equal(0, pendingCount);

        var deliveredCount = await context.Notifications
            .CountAsync(n => n.Status == NotificationStatus.Delivered);
        Assert.Equal(3, deliveredCount);
    }

    [Fact]
    public async Task RetryFailedNotificationsAsync_RetriesFailedNotificationsWithinRetryLimit()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var providers = new List<INotificationProvider> { _mockSmsProvider.Object };

        // Create failed notification with retry count < 3
        var retryableNotification = new Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Channel = NotificationChannel.Sms,
            TemplateKey = "test_template",
            Payload = JsonSerializer.Serialize(new Dictionary<string, string> { { "test", "value" } }),
            Status = NotificationStatus.Failed,
            RetryCount = 1,
            ErrorMessage = "Network timeout",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        // Create failed notification that has exceeded retry limit
        var exhaustedNotification = new Notification
        {
            Id = Guid.NewGuid(),
            RecipientId = Guid.NewGuid(),
            Channel = NotificationChannel.Sms,
            TemplateKey = "test_template",
            Payload = "{}",
            Status = NotificationStatus.Failed,
            RetryCount = 3,
            ErrorMessage = "Maximum retries exceeded",
            CreatedAt = DateTime.UtcNow.AddMinutes(-20),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-20)
        };

        context.Notifications.AddRange(retryableNotification, exhaustedNotification);
        await context.SaveChangesAsync();

        _mockTemplateService
            .Setup(t => t.RenderTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .Returns("Test message");

        _mockSmsProvider
            .Setup(p => p.SendAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = new DigitalStokvel.NotificationService.Services.NotificationService(
            context,
            _mockTemplateService.Object,
            providers,
            _mockLogger.Object);

        // Act
        await service.RetryFailedNotificationsAsync(batchSize: 10);

        // Assert
        var retriedNotification = await context.Notifications.FindAsync(retryableNotification.Id);
        Assert.NotNull(retriedNotification);
        Assert.Equal(NotificationStatus.Delivered, retriedNotification.Status);

        var exhaustedStillFailed = await context.Notifications.FindAsync(exhaustedNotification.Id);
        Assert.NotNull(exhaustedStillFailed);
        Assert.Equal(NotificationStatus.Failed, exhaustedStillFailed.Status);
        Assert.Equal(3, exhaustedStillFailed.RetryCount);
    }
}
