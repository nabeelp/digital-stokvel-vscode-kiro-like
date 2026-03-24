namespace DigitalStokvel.UssdGateway.Services;

/// <summary>
/// Service for sending SMS notifications
/// Simulated implementation for MVP - integrates with Notification Service in production
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly IConfiguration _configuration;

    public SmsService(
        ILogger<SmsService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Sends an SMS receipt to the user after a successful contribution
    /// </summary>
    public async Task<bool> SendReceiptSmsAsync(
        string phoneNumber,
        string groupName,
        decimal amount,
        string receiptNumber,
        string languageCode)
    {
        try
        {
            // Check if SMS fallback is enabled
            var smsEnabled = _configuration.GetValue("Sms:Enabled", true);
            if (!smsEnabled)
            {
                _logger.LogDebug(
                    "SMS fallback is disabled, skipping SMS send to {PhoneNumber}",
                    phoneNumber);
                return true; // Return true as it's intentionally disabled
            }

            // Get SMS template based on language
            var smsMessage = GetSmsTemplate(languageCode, groupName, amount, receiptNumber);

            _logger.LogInformation(
                "Sending receipt SMS to {PhoneNumber} in language {Language}: {Message}",
                phoneNumber,
                languageCode,
                smsMessage);

            // Simulate network delay
            await Task.Delay(300);

            // Simulate success (95% success rate for SMS delivery)
            var success = Random.Shared.Next(100) < 95;

            if (success)
            {
                _logger.LogInformation(
                    "Receipt SMS sent successfully to {PhoneNumber} for receipt {ReceiptNumber}",
                    phoneNumber,
                    receiptNumber);
            }
            else
            {
                _logger.LogWarning(
                    "SMS delivery failed for {PhoneNumber}, receipt {ReceiptNumber}. User can view receipt in app.",
                    phoneNumber,
                    receiptNumber);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending receipt SMS to {PhoneNumber} for receipt {ReceiptNumber}",
                phoneNumber,
                receiptNumber);
            return false;
        }
    }

    /// <summary>
    /// Gets SMS message template based on language
    /// </summary>
    private string GetSmsTemplate(
        string languageCode,
        string groupName,
        decimal amount,
        string receiptNumber)
    {
        return languageCode.ToLowerInvariant() switch
        {
            "zu" => $"Ukukhokha kuphumelele! {groupName}: R{amount:F2}. Irisidi: {receiptNumber}. Siyabonga ngokusebenzisa i-Digital Stokvel.",
            "xh" => $"Intlawulo iphumelele! {groupName}: R{amount:F2}. Irisithi: {receiptNumber}. Enkosi ngokusebenzisa i-Digital Stokvel.",
            "st" => $"Tefo e atlehile! {groupName}: R{amount:F2}. Resiti: {receiptNumber}. Rea leboha ho sebedisa Digital Stokvel.",
            "af" => $"Betaling suksesvol! {groupName}: R{amount:F2}. Kwitansie: {receiptNumber}. Dankie vir die gebruik van Digital Stokvel.",
            _ => $"Payment successful! {groupName}: R{amount:F2}. Receipt: {receiptNumber}. Thank you for using Digital Stokvel."
        };
    }
}
