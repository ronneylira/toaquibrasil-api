namespace ToAquiBrasil.Core.Utils;

/// <summary>
/// Helper class for date formatting operations
/// </summary>
public static class DateFormatHelper
{
    /// <summary>
    /// Format review date as "Mon YYYY" (e.g., "Dec 2018")
    /// </summary>
    public static string FormatReviewDate(DateTime date)
    {
        return date.ToString("MMM yyyy");
    }
} 