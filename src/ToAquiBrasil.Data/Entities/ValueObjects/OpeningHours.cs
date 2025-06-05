namespace ToAquiBrasil.Data.Entities.ValueObjects;

public class OpeningHours : ValueObject<OpeningHours>
{
    public int DayOfWeek { get; set; }
    public string Hours { get; set; }

    protected override bool EqualsCore(OpeningHours other)
    {
        return Hours == other.Hours && DayOfWeek == other.DayOfWeek;
    }

    protected override int GetHashCodeCore()
    {
        return HashCode.Combine(DayOfWeek, Hours);
    }
}