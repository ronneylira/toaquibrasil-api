namespace ToAquiBrasil.Core.Models;

public class TagsModel(TagModel[] tags)
{
    public TagModel[] Tags { get; init; } = tags;
} 