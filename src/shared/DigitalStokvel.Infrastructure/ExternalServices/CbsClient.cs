using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace DigitalStokvel.Infrastructure.ExternalServices;

/// <summary>
/// Core Banking System REST API client implementation with resilience patterns
/// </summary>
public class CbsClient : ICbsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CbsClient> _logger;
    private readonly CbsClientOptions _options;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

    public CbsClient(
        HttpClient httpClient,
        IOptions<CbsClientOptions> options,
        ILogger<CbsClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure retry policy: Exponential backoff (3 attempts)
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning(
                        "CBS API call failed. Retry attempt {RetryAttempt} after {Delay}ms. Status: {StatusCode}",
                        retryAttempt,
                        timespan.TotalMilliseconds,
                        outcome.Result?.StatusCode);
                });

        // Configure circuit breaker: Open after 5 consecutive failures
        _circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (outcome, duration) =>
                {
                    _logger.LogError(
                        "CBS Circuit breaker opened for {Duration}s. Status: {StatusCode}",
                        duration.TotalSeconds,
                        outcome.Result?.StatusCode);
                },
                onReset: () =>
                {
                    _logger.LogInformation("CBS Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("CBS Circuit breaker half-open, testing...");
                });
    }

    /// <inheritdoc />
    public async Task<string> CreateGroupAccountAsync(
        Guid groupId,
        string accountName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating group savings account for group {GroupId}", groupId);

        var request = new CreateAccountRequest(
            GroupId: groupId.ToString(),
            AccountName: accountName,
            AccountType: "GROUP_SAVINGS",
            Currency: "ZAR");

        try
        {
            var response = await ExecuteWithResilienceAsync(
                async () => await _httpClient.PostAsJsonAsync("/api/accounts", request, cancellationToken),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CreateAccountResponse>(cancellationToken);

            if (result == null || string.IsNullOrEmpty(result.AccountNumber))
            {
                throw new CbsClientException("Invalid response from CBS: Account number not returned");
            }

            _logger.LogInformation(
                "Successfully created account {AccountNumber} for group {GroupId}",
                result.AccountNumber,
                groupId);

            return result.AccountNumber;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "CBS Circuit breaker is open. Cannot create account for group {GroupId}", groupId);
            throw new CbsClientException("Core Banking System is currently unavailable. Please try again later.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error creating account for group {GroupId}", groupId);
            throw new CbsClientException($"Failed to communicate with Core Banking System: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error parsing CBS response for group {GroupId}", groupId);
            throw new CbsClientException("Invalid response format from Core Banking System", ex);
        }
    }

    /// <inheritdoc />
    public async Task<string> CreditAccountAsync(
        string accountNumber,
        decimal amount,
        string reference,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Crediting account {AccountNumber} with {Amount:C}. Reference: {Reference}",
            accountNumber,
            amount,
            reference);

        var request = new TransactionRequest(
            AccountNumber: accountNumber,
            Amount: amount,
            TransactionType: "CREDIT",
            Reference: reference,
            Description: $"Contribution: {reference}");

        try
        {
            var response = await ExecuteWithResilienceAsync(
                async () => await _httpClient.PostAsJsonAsync("/api/transactions", request, cancellationToken),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TransactionResponse>(cancellationToken);

            if (result == null || string.IsNullOrEmpty(result.TransactionId))
            {
                throw new CbsClientException("Invalid response from CBS: Transaction ID not returned");
            }

            _logger.LogInformation(
                "Successfully credited account {AccountNumber}. Transaction: {TransactionId}",
                accountNumber,
                result.TransactionId);

            return result.TransactionId;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "CBS Circuit breaker is open. Cannot credit account {AccountNumber}", accountNumber);
            throw new CbsClientException("Core Banking System is currently unavailable. Please try again later.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error crediting account {AccountNumber}", accountNumber);
            throw new CbsClientException($"Failed to communicate with Core Banking System: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error parsing CBS response for account {AccountNumber}", accountNumber);
            throw new CbsClientException("Invalid response format from Core Banking System", ex);
        }
    }

    /// <inheritdoc />
    public async Task<string> DebitAccountAsync(
        string accountNumber,
        decimal amount,
        string reference,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Debiting account {AccountNumber} with {Amount:C}. Reference: {Reference}",
            accountNumber,
            amount,
            reference);

        var request = new TransactionRequest(
            AccountNumber: accountNumber,
            Amount: amount,
            TransactionType: "DEBIT",
            Reference: reference,
            Description: $"Payout: {reference}");

        try
        {
            var response = await ExecuteWithResilienceAsync(
                async () => await _httpClient.PostAsJsonAsync("/api/transactions", request, cancellationToken),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TransactionResponse>(cancellationToken);

            if (result == null || string.IsNullOrEmpty(result.TransactionId))
            {
                throw new CbsClientException("Invalid response from CBS: Transaction ID not returned");
            }

            _logger.LogInformation(
                "Successfully debited account {AccountNumber}. Transaction: {TransactionId}",
                accountNumber,
                result.TransactionId);

            return result.TransactionId;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "CBS Circuit breaker is open. Cannot debit account {AccountNumber}", accountNumber);
            throw new CbsClientException("Core Banking System is currently unavailable. Please try again later.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error debiting account {AccountNumber}", accountNumber);
            throw new CbsClientException($"Failed to communicate with Core Banking System: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error parsing CBS response for account {AccountNumber}", accountNumber);
            throw new CbsClientException("Invalid response format from Core Banking System", ex);
        }
    }

    /// <inheritdoc />
    public async Task<decimal> GetAccountBalanceAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting balance for account {AccountNumber}", accountNumber);

        try
        {
            var response = await ExecuteWithResilienceAsync(
                async () => await _httpClient.GetAsync($"/api/accounts/{accountNumber}/balance", cancellationToken),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<BalanceResponse>(cancellationToken);

            if (result == null)
            {
                throw new CbsClientException("Invalid response from CBS: Balance not returned");
            }

            _logger.LogInformation(
                "Retrieved balance for account {AccountNumber}: {Balance:C}",
                accountNumber,
                result.Balance);

            return result.Balance;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "CBS Circuit breaker is open. Cannot get balance for account {AccountNumber}", accountNumber);
            throw new CbsClientException("Core Banking System is currently unavailable. Please try again later.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error getting balance for account {AccountNumber}", accountNumber);
            throw new CbsClientException($"Failed to communicate with Core Banking System: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error parsing CBS balance response for account {AccountNumber}", accountNumber);
            throw new CbsClientException("Invalid response format from Core Banking System", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AccountTransaction>> GetAccountStatementAsync(
        string accountNumber,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting statement for account {AccountNumber} from {FromDate} to {ToDate}",
            accountNumber,
            fromDate,
            toDate);

        try
        {
            var url = $"/api/accounts/{accountNumber}/statement?from={fromDate:yyyy-MM-dd}&to={toDate:yyyy-MM-dd}";
            var response = await ExecuteWithResilienceAsync(
                async () => await _httpClient.GetAsync(url, cancellationToken),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<StatementResponse>(cancellationToken);

            if (result == null || result.Transactions == null)
            {
                _logger.LogWarning("No transactions found for account {AccountNumber}", accountNumber);
                return Enumerable.Empty<AccountTransaction>();
            }

            var transactions = result.Transactions.Select(t => new AccountTransaction(
                TransactionDate: t.Date,
                Description: t.Description,
                Amount: t.Amount,
                TransactionType: t.Type,
                Balance: t.Balance));

            _logger.LogInformation(
                "Retrieved {Count} transactions for account {AccountNumber}",
                transactions.Count(),
                accountNumber);

            return transactions;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "CBS Circuit breaker is open. Cannot get statement for account {AccountNumber}", accountNumber);
            throw new CbsClientException("Core Banking System is currently unavailable. Please try again later.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error getting statement for account {AccountNumber}", accountNumber);
            throw new CbsClientException($"Failed to communicate with Core Banking System: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error parsing CBS statement response for account {AccountNumber}", accountNumber);
            throw new CbsClientException("Invalid response format from Core Banking System", ex);
        }
    }

    /// <summary>
    /// Execute HTTP request with retry and circuit breaker policies
    /// </summary>
    private async Task<HttpResponseMessage> ExecuteWithResilienceAsync(
        Func<Task<HttpResponseMessage>> action,
        CancellationToken cancellationToken)
    {
        // Wrap with circuit breaker, then retry
        var combinedPolicy = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);
        return await combinedPolicy.ExecuteAsync(action);
    }
}

/// <summary>
/// CBS client configuration options
/// </summary>
public class CbsClientOptions
{
    public const string SectionName = "CbsClient";

    /// <summary>
    /// CBS API base URL
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API key for CBS authentication
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable retry logic
    /// </summary>
    public bool EnableRetry { get; set; } = true;

    /// <summary>
    /// Enable circuit breaker
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;
}

/// <summary>
/// CBS client exception
/// </summary>
public class CbsClientException : Exception
{
    public CbsClientException(string message) : base(message)
    {
    }

    public CbsClientException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

// DTOs for CBS REST API

internal record CreateAccountRequest(
    string GroupId,
    string AccountName,
    string AccountType,
    string Currency);

internal record CreateAccountResponse(
    string AccountNumber,
    string Status,
    DateTime CreatedDate);

internal record TransactionRequest(
    string AccountNumber,
    decimal Amount,
    string TransactionType,
    string Reference,
    string Description);

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
