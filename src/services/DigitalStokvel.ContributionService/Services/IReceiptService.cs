namespace DigitalStokvel.ContributionService.Services;

/// <summary>
/// Interface for generating contribution receipt PDFs
/// </summary>
public interface IReceiptService
{
    /// <summary>
    /// Generates a PDF receipt for a contribution
    /// </summary>
    /// <param name="receiptNumber">Unique receipt number</param>
    /// <param name="groupName">Name of the group</param>
    /// <param name="memberName">Name of the contributing member</param>
    /// <param name="amount">Contribution amount</param>
    /// <param name="balanceAfter">Group account balance after contribution</param>
    /// <param name="date">Receipt date</param>
    /// <returns>URL to access the generated PDF</returns>
    Task<string> GenerateReceiptPdfAsync(
        string receiptNumber,
        string groupName,
        string memberName,
        decimal amount,
        decimal balanceAfter,
        DateTime date);
}
