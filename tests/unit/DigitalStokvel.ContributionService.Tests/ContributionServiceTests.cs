using DigitalStokvel.ContributionService.Data;
using DigitalStokvel.ContributionService.DTOs;
using DigitalStokvel.ContributionService.Entities;
using DigitalStokvel.ContributionService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DigitalStokvel.ContributionService.Tests;

public class ContributionServiceTests
{
    private readonly Mock<ILogger<DigitalStokvel.ContributionService.Services.ContributionService>> _mockLogger;
    private readonly Mock<IReceiptService> _mockReceiptService;

    public ContributionServiceTests()
    {
        _mockLogger = new Mock<ILogger<DigitalStokvel.ContributionService.Services.ContributionService>>();
        _mockReceiptService = new Mock<IReceiptService>();
    }

    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateContributionAsync_CreatesContribution_Successfully()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        
        _mockReceiptService
            .Setup(x => x.GenerateReceiptPdfAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync("https://cdn.example.com/receipt.pdf");

        var service = new DigitalStokvel.ContributionService.Services.ContributionService(
            context,
            _mockReceiptService.Object,
            _mockLogger.Object);

        var request = new CreateContributionRequest
        {
            GroupId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentMethod = "linked_account",
            Pin = "1234"
        };
        var userId = Guid.NewGuid();

        // Act
        var response = await service.CreateContributionAsync(request, userId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(request.GroupId, response.GroupId);
        Assert.Equal(request.Amount, response.Amount);
        Assert.Equal("paid", response.Status);
        Assert.NotNull(response.Receipt);
        Assert.NotNull(response.Receipt.PdfUrl);
       Assert.Equal(request.Amount, response.Receipt.Amount);
    }

    [Fact]
    public async Task CreateContributionAsync_CreatesLedgerEntry()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        
        _mockReceiptService
            .Setup(x => x.GenerateReceiptPdfAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync("https://cdn.example.com/receipt.pdf");

        var service = new DigitalStokvel.ContributionService.Services.ContributionService(
            context,
            _mockReceiptService.Object,
            _mockLogger.Object);

        var request = new CreateContributionRequest
        {
            GroupId = Guid.NewGuid(),
            Amount = 500.00m,
            PaymentMethod = "linked_account",
            Pin = "1234"
        };
        var userId = Guid.NewGuid();

        // Act
        await service.CreateContributionAsync(request, userId);

        // Assert
        var ledgerEntries = await context.ContributionLedger.ToListAsync();
        Assert.Single(ledgerEntries);
        Assert.Equal(request.GroupId, ledgerEntries[0].GroupId);
        Assert.Equal(request.Amount, ledgerEntries[0].Amount);
    }

    [Fact]
    public async Task GetGroupContributionsAsync_ReturnsContributions()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var groupId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        // Add test data
        context.Contributions.Add(new Contribution
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            MemberId = memberId,
            Amount = 500.00m,
            Status = ContributionStatus.Paid,
            DueDate = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new DigitalStokvel.ContributionService.Services.ContributionService(
            context,
            _mockReceiptService.Object,
            _mockLogger.Object);

        // Act
        var response = await service.GetGroupContributionsAsync(groupId, null, null, null, 1, 50);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Contributions);
        Assert.Equal(memberId, response.Contributions[0].Member?.Id);
    }

    [Fact]
    public async Task CreateRecurringPaymentAsync_CreatesRecurringPayment()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        
        var service = new DigitalStokvel.ContributionService.Services.ContributionService(
            context,
            _mockReceiptService.Object,
            _mockLogger.Object);

        var request = new CreateRecurringPaymentRequest
        {
            GroupId = Guid.NewGuid(),
            Amount = 500.00m,
            Frequency = "monthly",
            StartDate = DateTime.UtcNow.AddDays(1),
            PaymentMethod = "debit_order"
        };
        var userId = Guid.NewGuid();

        // Act
        var response = await service.CreateRecurringPaymentAsync(request, userId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(request.GroupId, response.GroupId);
        Assert.Equal(request.Amount, response.Amount);
        Assert.Equal("active", response.Status);
    }
}
