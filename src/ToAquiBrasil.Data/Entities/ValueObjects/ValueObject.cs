namespace ToAquiBrasil.Data.Entities.ValueObjects;

/// <summary>
///     Base ValueObject class, used to implement value objects, courtesy of https://enterprisecraftsmanship.com/2017/06/15/value-objects-when-to-create-one/
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ValueObject<T>
    where T : ValueObject<T>
{
    public override bool Equals(object obj)
    {
        if (!(obj is T valueObject))
            return false;

        return EqualsCore(valueObject);
    }

    protected abstract bool EqualsCore(T other);

    public override int GetHashCode() => GetHashCodeCore();

    protected abstract int GetHashCodeCore();

    public static bool operator ==(ValueObject<T> a, ValueObject<T> b)
    {
        if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
            return true;

        if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject<T> a, ValueObject<T> b)
    {
        return !(a == b);
    }
}