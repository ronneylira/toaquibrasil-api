namespace ToAquiBrasil.Api.Dtos;

public record LayoutDto
{
    public string Title { get; init; } = string.Empty;
    public List<SortByItemDto> SortBy { get; init; } = [];
    public List<RadiusDto> Radius { get; init; } = [];
    public List<CategoryDto> Categories { get; init; } = [];
    public required TagGroupDto Tags { get; init; }
    public required TagGroupDto Cuisine { get; init; }
}

public record SortByItemDto
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}

public record TagItemDto
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}

public record TagGroupDto
{
    public string Title { get; init; } = string.Empty;
    public List<TagItemDto> Items { get; init; } = [];
} 