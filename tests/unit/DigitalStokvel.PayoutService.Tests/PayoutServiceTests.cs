using DigitalStokvel.PayoutService.Data;
using DigitalStokvel.PayoutService.DTOs;
using DigitalStokvel.PayoutService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DigitalStokvel.PayoutService.Tests;

public class PayoutServiceTests
{
    private readonly Mock<ILogger<DigitalStokvel.PayoutService.Services.PayoutService>> _mockLogger;

    public PayoutServiceTests()
    {
        _mockLogger = new Mock<ILogger<DigitalStokvel.PayoutService.Services.PayoutService>>();
    }

    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task InitiatePayoutAsync_CreatesPayoutWithPendingApprovalStatus()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.PayoutService.Services.PayoutService(context, _mockLogger.Object);

        var request = new InitiatePayoutRequest
        {
            GroupId = Guid.NewGuid(),
            PayoutType = "rotating",
            Amount = 5000.00m,
            RecipientMemberId = Guid.NewGuid()
        };
        var userId = Guid.NewGuid();

        // Act
        var response = await service.InitiatePayoutAsync(request, userId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(request.GroupId, response.GroupId);
        Assert.Equal(request.Amount, response.TotalAmount);
        Assert.Equal("PendingApproval", response.Status);
        Assert.Equal(userId, response.InitiatedBy.Id);

        // Verify payout was saved to database
        var payout = await context.Payouts.FirstOrDefaultAsync(p => p.Id == response.Id);
        Assert.NotNull(payout);
        Assert.Equal(PayoutStatus.PendingApproval, payout.Status);
    }

    [Fact]
    public async Task InitiatePayoutAsync_CreatesRotatingPayoutDisbursement()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.PayoutService.Services.PayoutService(context, _mockLogger.Object);

        var recipientId = Guid.NewGuid();
        var request = new InitiatePayoutRequest
        {
            GroupId = Guid.NewGuid(),
            PayoutType = "rotating",
            Amount = 5000.00m,
            RecipientMemberId = recipientId
        };
        var userId = Guid.NewGuid();

        // Act
        var response = await service.InitiatePayoutAsync(request, userId);

        // Assert
        var disbursements = await context.PayoutDisbursements
            .Where(d => d.PayoutId == response.Id)
            .ToListAsync();

        Assert.Single(disbursements);
        Assert.Equal(recipientId, disbursements[0].MemberId);
        Assert.Equal(request.Amount, disbursements[0].Amount);
        Assert.Equal(PayoutStatus.PendingApproval, disbursements[0].Status);
    }

    [Fact]
    public async Task ApprovePayoutAsync_UpdatesStatusToApproved ()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.PayoutService.Services.PayoutService(context, _mockLogger.Object);

        // Create a pending payout
        var payout = new Payout
        {
            GroupId = Guid.NewGuid(),
            PayoutType = PayoutType.Rotating,
            TotalAmount = 5000.00m,
            Status = PayoutStatus.PendingApproval,
            InitiatedBy = Guid.NewGuid(),
            InitiatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Payouts.Add(payout);

        var disbursement = new PayoutDisbursement
        {
            PayoutId = payout.Id,
            MemberId = Guid.NewGuid(),
            Amount = 5000.00m,
            Status = PayoutStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.PayoutDisbursements.Add(disbursement);
        await context.SaveChangesAsync();

        var approveRequest = new ApprovePayoutRequest
        {
            Pin = "1234"
        };
        var approverId = Guid.NewGuid();

        // Act
        var response = await service.ApprovePayoutAsync(payout.Id, approveRequest, approverId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Executed", response.Status);
        Assert.Equal(approverId, response.ApprovedBy.Id);

        // Verify payout status updated in database
        var updatedPayout = await context.Payouts.FirstOrDefaultAsync(p => p.Id == payout.Id);
        Assert.NotNull(updatedPayout);
        Assert.Equal(PayoutStatus.Executed, updatedPayout.Status);
        Assert.Equal(approverId, updatedPayout.ApprovedBy);
    }

    [Fact]
    public async Task ApprovePayoutAsync_ExecutesDisbursements()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.PayoutService.Services.PayoutService(context, _mockLogger.Object);

        // Create a pending payout with disbursement
        var payout = new Payout
        {
            GroupId = Guid.NewGuid(),
            PayoutType = PayoutType.Rotating,
            TotalAmount = 5000.00m,
            Status = PayoutStatus.PendingApproval,
            InitiatedBy = Guid.NewGuid(),
            InitiatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Payouts.Add(payout);

        var disbursement = new PayoutDisbursement
        {
            PayoutId = payout.Id,
            MemberId = Guid.NewGuid(),
            Amount = 5000.00m,
            Status = PayoutStatus.PendingApproval,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.PayoutDisbursements.Add(disbursement);
        await context.SaveChangesAsync();

        var approveRequest = new ApprovePayoutRequest
        {
            Pin = "1234"
        };
        var approverId = Guid.NewGuid();

        // Act
        await service.ApprovePayoutAsync(payout.Id, approveRequest, approverId);

        // Assert - verify disbursement has transaction reference (indicating execution)
        var executedDisbursement = await context.PayoutDisbursements
            .FirstOrDefaultAsync(d => d.Id == disbursement.Id);

        Assert.NotNull(executedDisbursement);
        Assert.Equal(PayoutStatus.Executed, executedDisbursement.Status);
        Assert.NotNull(executedDisbursement.TransactionRef);
        Assert.NotNull(executedDisbursement.ExecutedAt);
    }

    [Fact]
    public async Task GetPayoutStatusAsync_ReturnsPayoutWithDisbursements()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.PayoutService.Services.PayoutService(context, _mockLogger.Object);

        // Create a payout with disbursements
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var payout = new Payout
        {
            GroupId = groupId,
            PayoutType = PayoutType.Rotating,
            TotalAmount = 5000.00m,
            Status = PayoutStatus.Executed,
            InitiatedBy = Guid.NewGuid(),
            InitiatedAt = DateTime.UtcNow,
            ApprovedBy = Guid.NewGuid(),
            ApprovedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Payouts.Add(payout);

        var disbursement = new PayoutDisbursement
        {
            PayoutId = payout.Id,
            MemberId = memberId,
            Amount = 5000.00m,
            Status = PayoutStatus.Executed,
            TransactionRef = "TXN-12345",
            ExecutedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.PayoutDisbursements.Add(disbursement);
        await context.SaveChangesAsync();

        // Act
        var response = await service.GetPayoutStatusAsync(payout.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(payout.Id, response.Id);
        Assert.Equal(groupId, response.GroupId);
        Assert.Equal(5000.00m, response.TotalAmount);
        Assert.Equal("Executed", response.Status);
        Assert.Single(response.Disbursements);
        Assert.Equal(memberId, response.Disbursements[0].Member.Id);
        Assert.Equal("TXN-12345", response.Disbursements[0].TransactionRef);
    }

    [Fact]
    public async Task GetGroupPayoutHistoryAsync_ReturnsPayoutsForGroup()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var service = new DigitalStokvel.PayoutService.Services.PayoutService(context, _mockLogger.Object);

        var groupId = Guid.NewGuid();

        // Create multiple payouts for the group
        for (int i = 0; i < 3; i++)
        {
            var payout = new Payout
            {
                GroupId = groupId,
                PayoutType = PayoutType.Rotating,
                TotalAmount = 5000.00m + (i * 1000),
                Status = PayoutStatus.Executed,
                InitiatedBy = Guid.NewGuid(),
                InitiatedAt = DateTime.UtcNow.AddDays(-i),
                ApprovedBy = Guid.NewGuid(),
                ApprovedAt = DateTime.UtcNow.AddDays(-i),
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                UpdatedAt = DateTime.UtcNow.AddDays(-i)
            };
            context.Payouts.Add(payout);
        }
        await context.SaveChangesAsync();

        // Act
        var response = await service.GetGroupPayoutHistoryAsync(groupId, skip: 0, take: 10);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Payouts.Count);
        Assert.Equal(groupId, response.GroupId);
        Assert.Equal(3, response.TotalCount);
    }
}
