namespace DigitalStokvel.UssdGateway.Services;

/// <summary>
/// Service interface for sending SMS notifications
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends an SMS receipt to the user after a successful contribution
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="groupName">Name of the stokvel group</param>
    /// <param name="amount">Contribution amount</param>
    /// <param name="receiptNumber">Payment receipt number</param>
    /// <param name="languageCode">Language code for SMS (en, zu, xh, st, af)</param>
    /// <returns>True if SMS was sent successfully, false otherwise</returns>
    Task<bool> SendReceiptSmsAsync(
        string phoneNumber,
        string groupName,
        decimal amount,
        string receiptNumber,
        string languageCode);
}
