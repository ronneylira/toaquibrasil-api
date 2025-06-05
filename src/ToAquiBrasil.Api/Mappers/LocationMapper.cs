using ToAquiBrasil.Api.Dtos;
using ToAquiBrasil.Core.Models;

namespace ToAquiBrasil.Api.Mappers;

public class LocationMapper
{
    public LocationDto MapToLocationDto(LocationModel model)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }
        
        return new LocationDto
        {
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            DisplayName = model.DisplayName
        };
    }
    
    public LocationSuggestionsDto MapToLocationSuggestionsDto(List<LocationSuggestionModel> models, string query)
    {
        if (models == null)
        {
            throw new ArgumentNullException(nameof(models));
        }
        
        var suggestions = models.Select(model => new LocationSuggestionDto
        {
            Id = model.Id,
            DisplayName = model.DisplayName,
            PlaceType = model.PlaceType,
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            CountryCode = model.CountryCode,
            AddressType = model.AddressType
        }).ToList();
        
        return new LocationSuggestionsDto
        {
            Suggestions = suggestions,
            Query = query
        };
    }
} 