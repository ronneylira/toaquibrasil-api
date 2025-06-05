namespace ToAquiBrasil.Data.Entities.ValueObjects;

public class Contact : ValueObject<Contact>
{
    public string Icon { get; init; }
    public string Content { get; init; }
    public string Link { get; init; }

    protected override bool EqualsCore(Contact other)
    {
        return Icon == other.Icon && Content == other.Content && Link == other.Link;
    }

    protected override int GetHashCodeCore()
    {
        return HashCode.Combine(Icon, Content, Link);
    }
}