namespace DigitalStokvel.Infrastructure.ExternalServices;

/// <summary>
/// Core Banking System integration interface
/// </summary>
public interface ICbsClient
{
    /// <summary>
    /// Create a new group savings account
    /// </summary>
    Task<string> CreateGroupAccountAsync(Guid groupId, string accountName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Credit account with contribution
    /// </summary>
    Task<string> CreditAccountAsync(string accountNumber, decimal amount, string reference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Debit account for payout
    /// </summary>
    Task<string> DebitAccountAsync(string accountNumber, decimal amount, string reference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get account balance
    /// </summary>
    Task<decimal> GetAccountBalanceAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get account statement
    /// </summary>
    Task<IEnumerable<AccountTransaction>> GetAccountStatementAsync(string accountNumber, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Account transaction details
/// </summary>
public record AccountTransaction(
    DateTime TransactionDate,
    string Description,
    decimal Amount,
    string TransactionType,
    decimal Balance);

/// <summary>
/// Payment Gateway integration interface
/// </summary>
public interface IPaymentGatewayClient
{
    /// <summary>
    /// Process a payment
    /// </summary>
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a payout via EFT
    /// </summary>
    Task<PaymentResult> ProcessPayoutAsync(PayoutRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check payment status
    /// </summary>
    Task<PaymentStatus> GetPaymentStatusAsync(string transactionReference, CancellationToken cancellationToken = default);
}

/// <summary>
/// Payment request details
/// </summary>
public record PaymentRequest(
    Guid ContributionId,
    decimal Amount,
    string AccountNumber,
    string Description);

/// <summary>
/// Payout request details
/// </summary>
public record PayoutRequest(
    Guid PayoutId,
    decimal Amount,
    string BeneficiaryAccountNumber,
    string BeneficiaryName,
    string Description);

/// <summary>
/// Payment result
/// </summary>
public record PaymentResult(
    bool IsSuccess,
    string TransactionReference,
    string? ErrorMessage = null);

/// <summary>
/// Payment status
/// </summary>
public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// SMS Gateway integration interface
/// </summary>
public interface ISmsClient
{
    /// <summary>
    /// Send SMS message
    /// </summary>
    Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send bulk SMS messages
    /// </summary>
    Task SendBulkSmsAsync(IEnumerable<(string PhoneNumber, string Message)> messages, CancellationToken cancellationToken = default);
}
