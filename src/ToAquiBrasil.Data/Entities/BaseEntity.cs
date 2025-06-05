namespace ToAquiBrasil.Data.Entities;

public class BaseEntity
{
    public int Id { get; private set; }

    public DateTime CreatedAt { get; private set; }
    
    public DateTime? ChangedAt { get; private set; }

    public Guid ExternalId { get; set; }
}