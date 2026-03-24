using DigitalStokvel.ContributionService.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DigitalStokvel.ContributionService.Tests;

public class ReceiptServiceTests
{
    private readonly Mock<ILogger<ReceiptService>> _mockLogger;

    public ReceiptServiceTests()
    {
        _mockLogger = new Mock<ILogger<ReceiptService>>();
    }

    [Fact]
    public async Task GenerateReceiptPdfAsync_GeneratesPdfUrl()
    {
        // Arrange
        var service = new ReceiptService(_mockLogger.Object);
        var receiptNumber = "RCP-2026-03-24-1234";
        var groupName = "Test Group";
        var memberName = "Test Member";
        var amount = 500.00m;
        var balanceAfter = 5000.00m;
        var date = DateTime.UtcNow;

        // Act
        var pdfUrl = await service.GenerateReceiptPdfAsync(
            receiptNumber,
            groupName,
            memberName,
            amount,
            balanceAfter,
            date);

        // Assert
        Assert.NotNull(pdfUrl);
        Assert.Contains(receiptNumber, pdfUrl);
        Assert.StartsWith("https://cdn.stokvel.bank.co.za/receipts/", pdfUrl);
    }
}
