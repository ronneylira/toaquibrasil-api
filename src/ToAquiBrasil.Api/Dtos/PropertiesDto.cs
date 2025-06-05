namespace ToAquiBrasil.Api.Dtos;

public record PropertiesDto
{
    public Guid Id { get; init; }
    public int Index { get; init; }
    public bool IsActive { get; init; }
    public string Logo { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
    public string Link { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Person { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int Stars { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string About { get; init; } = string.Empty;
    public string[] Tags { get; init; } = [];
}