using Microsoft.AspNetCore.Mvc;
using ToAquiBrasil.Api.Dtos;
using ToAquiBrasil.Core.Models;
using ToAquiBrasil.Core.Queries.Abstractions;

namespace ToAquiBrasil.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class LayoutController(ILayoutQueries layoutQueries) : ApiControllerBase
{
    private readonly ILayoutQueries _layoutQueries =
        layoutQueries ?? throw new ArgumentNullException(nameof(layoutQueries));

    [HttpGet("lat/{latitude}/long/{longitude}/radius/{radius}/{unit}")]
    public async Task<ActionResult<ApiResponse<LayoutDto>>> GetByLocationWithUnit(
        double latitude,
        double longitude,
        string unit,
        int radius, 
        CancellationToken cancellationToken)
    {
        // Fetch layout data based on location in a single database operation for better performance
        var locationBasedLayout = await _layoutQueries.GetLocationBasedLayoutAsync(
            latitude, longitude, radius, unit, cancellationToken);

        var layoutData = CreateLayoutDto(
            locationBasedLayout.Categories,
            locationBasedLayout.Radii,
            locationBasedLayout.Tags,
            "Places near you"); // Pass custom title

        return Success(layoutData);
    }

    private static LayoutDto CreateLayoutDto(CategoriesModel categoryResult, RadiiModel radiusResult,
        TagsModel? tagsResult = null, string title = "Brazilian places")
    {
        var sortByItems = new List<SortByItemDto>
        {
            new() {Value = "popular", Label = "Most popular"},
            new() {Value = "recommended", Label = "Recommended"},
            new() {Value = "newest", Label = "Newest"},
            new() {Value = "oldest", Label = "Oldest"},
            new() {Value = "closest", Label = "Closest"}
        };

        List<TagItemDto> tagItems;

        // Use dynamic tags if available, otherwise use default tags
        if (tagsResult is {Tags.Length: > 0})
        {
            tagItems = tagsResult.Tags
                .Select(t => new TagItemDto {Value = $"{t.Name}", Label = t.Name})
                .ToList();
        }
        else
        {
            tagItems =
            [
                new() {Value = "type_0", Label = "Hipster"},
                new() {Value = "type_1", Label = "Music club"},
                new() {Value = "type_2", Label = "Bar"},
                new() {Value = "type_3", Label = "Pub"},
                new() {Value = "type_4", Label = "Deli"},
                new() {Value = "type_5", Label = "Bistro"}
            ];
        }

        var cuisineItems = new List<TagItemDto>
        {
            new() {Value = "cuisine_0", Label = "Fusion"},
            new() {Value = "cuisine_1", Label = "Indian"},
            new() {Value = "cuisine_2", Label = "French"},
            new() {Value = "cuisine_3", Label = "American"},
            new() {Value = "cuisine_4", Label = "Mexican"},
            new() {Value = "cuisine_5", Label = "Other"}
        };

        return new LayoutDto
        {
            Title = title,
            SortBy = sortByItems,
            Radius = radiusResult.Radii.Select(c => new RadiusDto {Value = c.Value, Label = c.Label}).ToList(),
            Categories = categoryResult.Categories.Select(c => new CategoryDto {Value = c.Name, Label = c.Name})
                .ToList(),
            Tags = new TagGroupDto {Title = "Tag", Items = tagItems},
            Cuisine = new TagGroupDto {Title = "Cuisine", Items = cuisineItems}
        };
    }
}
