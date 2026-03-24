namespace DigitalStokvel.Common.Extensions;

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Convert DateTime to South African Standard Time (SAST)
    /// </summary>
    public static DateTime ToSouthAfricanTime(this DateTime utcDateTime)
    {
        var sastTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, sastTimeZone);
    }

    /// <summary>
    /// Check if date is within a given range
    /// </summary>
    public static bool IsBetween(this DateTime date, DateTime startDate, DateTime endDate)
    {
        return date >= startDate && date <= endDate;
    }

    /// <summary>
    /// Get the start of the day (midnight)
    /// </summary>
    public static DateTime StartOfDay(this DateTime date)
    {
        return date.Date;
    }

    /// <summary>
    /// Get the end of the day (23:59:59)
    /// </summary>
    public static DateTime EndOfDay(this DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Check if date is overdue
    /// </summary>
    public static bool IsOverdue(this DateTime dueDate)
    {
        return dueDate < DateTime.UtcNow;
    }
}

/// <summary>
/// Extension methods for string operations
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Mask sensitive information (e.g., ID numbers, phone numbers)
    /// </summary>
    public static string Mask(this string value, int visibleChars = 4)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= visibleChars)
            return value;

        var masked = new string('*', value.Length - visibleChars);
        return masked + value[^visibleChars..];
    }

    /// <summary>
    /// Validate South African phone number format
    /// </summary>
    public static bool IsValidSouthAfricanPhoneNumber(this string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove common formatting characters
        var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        // Check formats: +27XXXXXXXXX, 0XXXXXXXXX, 27XXXXXXXXX
        return System.Text.RegularExpressions.Regex.IsMatch(cleaned,
            @"^(\+27|0|27)[0-9]{9}$");
    }

    /// <summary>
    /// Validate South African ID number format
    /// </summary>
    public static bool IsValidSouthAfricanIdNumber(this string idNumber)
    {
        if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length != 13)
            return false;

        return idNumber.All(char.IsDigit);
    }

    /// <summary>
    /// Truncate string to specified length with ellipsis
    /// </summary>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value[..(maxLength - 3)] + "...";
    }
}

/// <summary>
/// Extension methods for decimal operations
/// </summary>
public static class DecimalExtensions
{
    /// <summary>
    /// Format as South African Rand currency
    /// </summary>
    public static string ToRandString(this decimal amount)
    {
        return $"R {amount:N2}";
    }

    /// <summary>
    /// Check if amount is positive
    /// </summary>
    public static bool IsPositive(this decimal amount)
    {
        return amount > 0;
    }

    /// <summary>
    /// Round to 2 decimal places (currency standard)
    /// </summary>
    public static decimal RoundToCurrency(this decimal amount)
    {
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}

/// <summary>
/// Extension methods for collections
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Check if collection is null or empty
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }

    /// <summary>
    /// Perform action on each item in collection
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
    }
}
