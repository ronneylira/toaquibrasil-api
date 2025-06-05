namespace ToAquiBrasil.Api.Dtos;

public record LocationSuggestionsDto
{
    public List<LocationSuggestionDto> Suggestions { get; init; } = [];
    public string Query { get; init; } = string.Empty;
} 