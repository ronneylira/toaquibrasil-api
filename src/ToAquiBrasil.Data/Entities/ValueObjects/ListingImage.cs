namespace ToAquiBrasil.Data.Entities.ValueObjects;

public class ListingImage : ValueObject<ListingImage>
{
    public string Image { get; init; }

    public string Title { get; init; }

    protected override bool EqualsCore(ListingImage other)
    {
        return Image == other.Image && Title == other.Title;
    }

    protected override int GetHashCodeCore()
    {
        return HashCode.Combine(Image, Title);
    }
}
