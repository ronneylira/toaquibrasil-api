namespace ToAquiBrasil.Core.Models;

public class CategoryModel
{
    public CategoryModel(string name)
    {
        Name = name;
    }

    public string Name { get; init; }
}