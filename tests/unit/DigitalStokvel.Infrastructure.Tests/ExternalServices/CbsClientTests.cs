using System.Net;
using System.Net.Http.Json;
using DigitalStokvel.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace DigitalStokvel.Infrastructure.Tests.ExternalServices;

public class CbsClientTests
{
    private readonly Mock<ILogger<CbsClient>> _loggerMock;
    private readonly CbsClientOptions _options;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;

    public CbsClientTests()
    {
        _loggerMock = new Mock<ILogger<CbsClient>>();
        _options = new CbsClientOptions
        {
            BaseUrl = "https://cbs.example.com",
            ApiKey = "test-api-key",
            TimeoutSeconds = 30,
            EnableRetry = true,
            EnableCircuitBreaker = true
        };
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(_options.BaseUrl)
        };
    }

    private CbsClient CreateClient()
    {
        return new CbsClient(
            _httpClient,
            Options.Create(_options),
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateGroupAccountAsync_Success_ReturnsAccountNumber()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var expectedAccountNumber = "SA1234567890";
        var response = new CreateAccountResponse(
            AccountNumber: expectedAccountNumber,
            Status: "ACTIVE",
            CreatedDate: DateTime.UtcNow);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains("/api/accounts")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        var client = CreateClient();

        // Act
        var result = await client.CreateGroupAccountAsync(groupId, "Test Group Savings");

        // Assert
        Assert.Equal(expectedAccountNumber, result);
    }

    [Fact]
    public async Task CreateGroupAccountAsync_InvalidResponse_ThrowsCbsClientException()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var response = new CreateAccountResponse(
            AccountNumber: string.Empty,
            Status: "ACTIVE",
            CreatedDate: DateTime.UtcNow);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        var client = CreateClient();

        // Act & Assert
        await Assert.ThrowsAsync<CbsClientException>(
            () => client.CreateGroupAccountAsync(groupId, "Test Group"));
    }

    [Fact]
    public async Task CreditAccountAsync_Success_ReturnsTransactionId()
    {
        // Arrange
        var accountNumber = "SA1234567890";
        var amount = 500.00m;
        var reference = "CONTRIB-2026-001";
        var expectedTransactionId = "TXN-2026-001";

        var response = new TransactionResponse(
            TransactionId: expectedTransactionId,
            Status: "COMPLETED",
            TransactionDate: DateTime.UtcNow,
            Balance: 1500.00m);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains("/api/transactions")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        var client = CreateClient();

        // Act
        var result = await client.CreditAccountAsync(accountNumber, amount, reference);

        // Assert
        Assert.Equal(expectedTransactionId, result);
    }

    [Fact]
    public async Task DebitAccountAsync_Success_ReturnsTransactionId()
    {
        // Arrange
        var accountNumber = "SA1234567890";
        var amount = 500.00m;
        var reference = "PAYOUT-2026-001";
        var expectedTransactionId = "TXN-2026-002";

        var response = new TransactionResponse(
            TransactionId: expectedTransactionId,
            Status: "COMPLETED",
            TransactionDate: DateTime.UtcNow,
            Balance: 500.00m);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains("/api/transactions")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        var client = CreateClient();

        // Act
        var result = await client.DebitAccountAsync(accountNumber, amount, reference);

        // Assert
        Assert.Equal(expectedTransactionId, result);
    }

    [Fact]
    public async Task GetAccountBalanceAsync_Success_ReturnsBalance()
    {
        // Arrange
        var accountNumber = "SA1234567890";
        var expectedBalance = 1234.56m;

        var response = new BalanceResponse(
            AccountNumber: accountNumber,
            Balance: expectedBalance,
            AvailableBalance: expectedBalance,
            AsOfDate: DateTime.UtcNow);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/accounts/{accountNumber}/balance")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        var client = CreateClient();

        // Act
        var result = await client.GetAccountBalanceAsync(accountNumber);

        // Assert
        Assert.Equal(expectedBalance, result);
    }

    [Fact]
    public async Task GetAccountStatementAsync_Success_ReturnsTransactions()
    {
        // Arrange
        var accountNumber = "SA1234567890";
        var fromDate = new DateTime(2026, 3, 1);
        var toDate = new DateTime(2026, 3, 24);

        var response = new StatementResponse(
            AccountNumber: accountNumber,
            FromDate: fromDate,
            ToDate: toDate,
            Transactions: new List<StatementTransaction>
            {
                new(
                    Date: new DateTime(2026, 3, 10),
                    Description: "Contribution",
                    Amount: 500.00m,
                    Type: "CREDIT",
                    Balance: 500.00m),
                new(
                    Date: new DateTime(2026, 3, 15),
                    Description: "Contribution",
                    Amount: 500.00m,
                    Type: "CREDIT",
                    Balance: 1000.00m)
            });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/accounts/{accountNumber}/statement")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        var client = CreateClient();

        // Act
        var result = await client.GetAccountStatementAsync(accountNumber, fromDate, toDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal(500.00m, result.First().Amount);
        Assert.Equal("CREDIT", result.First().TransactionType);
    }

    [Fact]
    public async Task GetAccountStatementAsync_NoTransactions_ReturnsEmpty()
    {
        // Arrange
        var accountNumber = "SA1234567890";
        var fromDate = new DateTime(2026, 3, 1);
        var toDate = new DateTime(2026, 3, 24);

        var response = new StatementResponse(
            AccountNumber: accountNumber,
            FromDate: fromDate,
            ToDate: toDate,
            Transactions: new List<StatementTransaction>());

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(response)
            });

        var client = CreateClient();

        // Act
        var result = await client.GetAccountStatementAsync(accountNumber, fromDate, toDate);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreditAccountAsync_HttpError_ThrowsCbsClientException()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var client = CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CbsClientException>(
            () => client.CreditAccountAsync("SA1234567890", 500.00m, "REF-001"));

        Assert.Contains("Failed to communicate with Core Banking System", exception.Message);
    }

    [Fact]
    public async Task CreditAccountAsync_InvalidStatusCode_ThrowsException()
    {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Internal Server Error")
            });

        var client = CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CbsClientException>(
            () => client.CreditAccountAsync("SA1234567890", 500.00m, "REF-001"));
        
        Assert.Contains("Failed to communicate", exception.Message);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }
}

// Make internal records accessible to tests
internal record CreateAccountResponse(
    string AccountNumber,
    string Status,
    DateTime CreatedDate);

internal record TransactionResponse(
    string TransactionId,
    string Status,
    DateTime TransactionDate,
    decimal Balance);

internal record BalanceResponse(
    string AccountNumber,
    decimal Balance,
    decimal AvailableBalance,
    DateTime AsOfDate);

internal record StatementResponse(
    string AccountNumber,
    DateTime FromDate,
    DateTime ToDate,
    List<StatementTransaction> Transactions);

internal record StatementTransaction(
    DateTime Date,
    string Description,
    decimal Amount,
    string Type,
    decimal Balance);
