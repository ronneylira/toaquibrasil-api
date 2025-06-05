using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToAquiBrasil.Data.Entities;
using ToAquiBrasil.Data.Mappings;

namespace ToAquiBrasil.Data;

public class ToAquiBrasilDbContext(DbContextOptions options) : DbContext(options)
{
    public virtual DbSet<Listing> Listings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ListingMapping());

        ConfigureDefaultValues(modelBuilder);
    }

    public override int SaveChanges() => throw new InvalidOperationException("please use the async version");

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        SetShadowProperties();

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetShadowProperties()
    {
        var utcNow = DateTime.UtcNow;
        foreach (var entityEntry in ChangeTracker.Entries())
        {
            switch (entityEntry.State)
            {
                case EntityState.Modified:
                {
                    SetShadowPropertiesForModified(entityEntry, utcNow);
                    break;
                }
                case EntityState.Added:
                {
                    SetShadowPropertiesForCreated(entityEntry);
                    break;
                }
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                    break;
            }
        }
    }

    private static void SetShadowPropertiesForModified(EntityEntry entityEntry, DateTime utcNow)
    {
        if (HasProperty(entityEntry, WellKnownShadowPropertyNames.ChangedAt))
        {
            entityEntry.Property(WellKnownShadowPropertyNames.ChangedAt).CurrentValue = utcNow;
        }
    }

    private static void SetShadowPropertiesForCreated(EntityEntry entityEntry)
    {
        if (HasProperty(entityEntry, WellKnownShadowPropertyNames.CreatedAt) &&
            entityEntry.Property(WellKnownShadowPropertyNames.CreatedAt)
                .CurrentValue is DateTime createdAt &&
            createdAt == default)
        {
            entityEntry.Property(WellKnownShadowPropertyNames.CreatedAt).CurrentValue = DateTime.UtcNow;
        }

        if (HasProperty(entityEntry, WellKnownShadowPropertyNames.ExternalId) &&
            entityEntry.Property(WellKnownShadowPropertyNames.ExternalId).CurrentValue is Guid externalId &&
            externalId == Guid.Empty)
        {
            entityEntry.Property(WellKnownShadowPropertyNames.ExternalId).CurrentValue = Guid.NewGuid();
        }
    }

    private static bool HasProperty(EntityEntry entityEntry, string propertyName) =>
        entityEntry.Metadata.FindProperty(propertyName) != null;

    private void ConfigureDefaultValues(ModelBuilder modelBuilder)
    {
        foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes())
        {
            // is not a value object
            if (mutableEntityType.BaseType == null && !mutableEntityType.IsOwned())
            {
                var entity = modelBuilder.Entity(mutableEntityType.ClrType);

                // add shadow properties
                AddCreatedAtProperty(entity);
                entity.Property<DateTime?>(WellKnownShadowPropertyNames.ChangedAt);
                entity.Property<Guid>(WellKnownShadowPropertyNames.ExternalId);
            }
        }
    }

    protected virtual void AddCreatedAtProperty(EntityTypeBuilder entity)
    {
        entity.Property<DateTime>(WellKnownShadowPropertyNames.CreatedAt).ValueGeneratedOnAdd()
            .HasDefaultValueSql("GETUTCDATE()");
    }
}