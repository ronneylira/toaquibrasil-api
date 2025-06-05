using ToAquiBrasil.Core.Models.Domain;
using ToAquiBrasil.Core.Services.Abstractions;

namespace ToAquiBrasil.Core.Services;

/// <summary>
/// Service for calculating opening hours status and related operations
/// </summary>
public class OpeningHoursService : IOpeningHoursService
{
    /// <summary>
    /// Calculate overall opening status based on current day and time
    /// </summary>
    public (bool isOpen, string status) CalculateOverallOpeningStatus(IList<OpeningHoursResult> openingHours)
    {
        var now = DateTime.Now;
        var currentDayOfWeek = (int)now.DayOfWeek;
        var todayName = GetDayName(currentDayOfWeek);
        
        // Find today's opening hours
        var todayHours = openingHours.FirstOrDefault(oh => 
            oh.Day.Equals(todayName, StringComparison.OrdinalIgnoreCase));
        
        if (todayHours == null || string.IsNullOrWhiteSpace(todayHours.Hours))
            return (false, "Closed");
        
        return CalculateOpenStatusAndText(currentDayOfWeek, todayHours.Hours);
    }

    /// <summary>
    /// Convert DayOfWeek integer to day name string
    /// </summary>
    public string GetDayName(int dayOfWeek)
    {
        return dayOfWeek switch
        {
            0 => "Sunday",
            1 => "Monday", 
            2 => "Tuesday",
            3 => "Wednesday",
            4 => "Thursday",
            5 => "Friday",
            6 => "Saturday",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Calculate smart opening status and status text based on current time
    /// </summary>
    private static (bool isOpen, string status) CalculateOpenStatusAndText(int dayOfWeek, string hours)
    {
        if (string.IsNullOrWhiteSpace(hours))
            return (false, "Closed");

        // Handle special cases
        if (hours.ToLower().Contains("closed"))
            return (false, "Closed");
            
        if (hours.ToLower().Contains("24") || hours.ToLower().Contains("24/7"))
            return (true, "Open 24/7");

        var now = DateTime.Now;
        var currentDayOfWeek = (int)now.DayOfWeek;
        
        // Only check if it's the current day
        if (dayOfWeek != currentDayOfWeek)
            return (false, "Closed");

        // Try to parse opening hours
        var openClose = ParseOpeningHours(hours);
        if (openClose == null)
            return (false, "Closed");

        var currentTime = now.TimeOfDay;
        var (openTime, closeTime) = openClose.Value;

        // Handle cases where close time is next day (e.g., "10:00 PM - 2:00 AM")
        bool isCurrentlyOpen;
        if (closeTime < openTime)
        {
            isCurrentlyOpen = currentTime >= openTime || currentTime <= closeTime;
        }
        else
        {
            isCurrentlyOpen = currentTime >= openTime && currentTime <= closeTime;
        }

        if (isCurrentlyOpen)
        {
            return (true, "Open now");
        }

        // Check if opening soon (within 1 hour)
        var timeUntilOpen = CalculateTimeUntilOpen(currentTime, openTime, closeTime);
        if (timeUntilOpen != null && timeUntilOpen.Value.TotalMinutes > 0 && timeUntilOpen.Value.TotalMinutes <= 60)
        {
            var minutes = (int)timeUntilOpen.Value.TotalMinutes;
            return (false, $"Opening in {minutes} minute{(minutes == 1 ? "" : "s")}");
        }

        return (false, "Closed");
    }

    /// <summary>
    /// Calculate time until opening
    /// </summary>
    private static TimeSpan? CalculateTimeUntilOpen(TimeSpan currentTime, TimeSpan openTime, TimeSpan closeTime)
    {
        // If closes next day (e.g., 10 PM - 2 AM), and we're before open time
        if (closeTime < openTime && currentTime < openTime)
        {
            return openTime - currentTime;
        }
        
        // If normal hours (e.g., 9 AM - 5 PM), and we're before open time
        if (closeTime >= openTime && currentTime < openTime)
        {
            return openTime - currentTime;
        }

        return null; // Not opening today or already past opening
    }

    /// <summary>
    /// Parse opening hours string to TimeSpan tuple
    /// </summary>
    private static (TimeSpan open, TimeSpan close)? ParseOpeningHours(string hours)
    {
        try
        {
            // Look for common separators
            string[] separators = { "-", "–", "to", "until", "till" };
            string separator = separators.FirstOrDefault(s => hours.ToLower().Contains(s)) ?? "-";
            
            var parts = hours.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return null;

            var openTime = ParseTime(parts[0].Trim());
            var closeTime = ParseTime(parts[1].Trim());

            if (openTime == null || closeTime == null)
                return null;

            return (openTime.Value, closeTime.Value);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse individual time string to TimeSpan
    /// </summary>
    private static TimeSpan? ParseTime(string timeStr)
    {
        try
        {
            timeStr = timeStr.Trim().ToLower();
            
            // Handle 24-hour format (e.g., "14:30", "09:00")
            if (TimeSpan.TryParse(timeStr, out var time24))
                return time24;

            // Handle 12-hour format with AM/PM
            if (DateTime.TryParse(timeStr, out var dateTime))
                return dateTime.TimeOfDay;

            // Handle simple hour formats (e.g., "9", "17")
            if (int.TryParse(timeStr, out var hour) && hour >= 0 && hour <= 23)
                return new TimeSpan(hour, 0, 0);

            return null;
        }
        catch
        {
            return null;
        }
    }
} 