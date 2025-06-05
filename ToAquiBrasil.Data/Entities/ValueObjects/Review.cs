namespace ToAquiBrasil.Data.Entities.ValueObjects;

public class Review : ValueObject<Review>
{
    public string Title { get; init; }

    public string Content { get; init; }

    public DateTime Date { get; init; }

    public string Avatar { get; init; }

    public int Stars { get; init; }

    protected override bool EqualsCore(Review other)
    {
        return Title == other.Title && Content == other.Content && Date == other.Date && Avatar == other.Avatar &&
               Stars == other.Stars;
    }

    protected override int GetHashCodeCore()
    {
        return HashCode.Combine(Title, Content, Date, Avatar, Stars);
    }
}