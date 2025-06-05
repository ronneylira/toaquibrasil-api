namespace ToAquiBrasil.Core.Models;

public class CategoriesModel
{
    public CategoriesModel(CategoryModel[] categories)
    {
        Categories = categories;
    }

    public CategoryModel[] Categories { get; init; }
}