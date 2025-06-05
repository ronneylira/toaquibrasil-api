using ToAquiBrasil.Data.Entities;

namespace ToAquiBrasil.Data;

public static class WellKnownShadowPropertyNames
{
    public static string ChangedAt => nameof(BaseEntity.ChangedAt);

    public static string CreatedAt => nameof(BaseEntity.CreatedAt);
    
    public static string ExternalId => nameof(BaseEntity.ExternalId);
}