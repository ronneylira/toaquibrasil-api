using ToAquiBrasil.Core.Models.Domain;

namespace ToAquiBrasil.Core.Services.Abstractions;

/// <summary>
/// Service for calculating opening hours status and related operations
/// </summary>
public interface IOpeningHoursService
{
    /// <summary>
    /// Calculate overall opening status based on current day and time
    /// </summary>
    /// <param name="openingHours">Collection of opening hours for all days</param>
    /// <returns>Tuple containing whether the business is open and the status text</returns>
    (bool isOpen, string status) CalculateOverallOpeningStatus(IList<OpeningHoursResult> openingHours);
    
    /// <summary>
    /// Convert DayOfWeek integer to day name string
    /// </summary>
    /// <param name="dayOfWeek">Integer representation of day (0 = Sunday, 1 = Monday, etc.)</param>
    /// <returns>Day name as string</returns>
    string GetDayName(int dayOfWeek);
} 