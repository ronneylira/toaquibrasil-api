using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ToAquiBrasil.Data.Entities;

namespace ToAquiBrasil.Data.Mappings;

public class ListingMapping : IEntityTypeConfiguration<Listing>
{
    private readonly ValueConverter _splitStringConverter =
        new ValueConverter<string[], string>(v => string.Join(",", v), v => v.Split(new[] {','}, StringSplitOptions.None));

    private readonly ValueComparer _stringComparer =
        new ValueComparer<string[]>(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToArray());

    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Category).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Person).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Email).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Stars).IsRequired();
        builder.Property(t => t.Phone).IsRequired();
        builder.Property(t => t.Address).IsRequired();
        builder.Property(t => t.About).IsRequired();
        builder.Property(t => t.Summary).IsRequired();
        builder.Property(t => t.Index).IsRequired();
        builder.Property(t => t.Logo).IsRequired();
        builder.Property(t => t.Image).IsRequired();
        builder.Property(t => t.Link).IsRequired();
        builder.Property(t => t.Link).IsRequired();
        builder.Property(t => t.Tags).HasConversion(_splitStringConverter, _stringComparer);
        builder.Property(t=> t.Services).HasConversion(_splitStringConverter, _stringComparer);
        builder.Property(t => t.Location).IsRequired();
        builder.OwnsMany(t => t.Images);
        builder.OwnsMany(t => t.Reviews);
        builder.OwnsMany(t => t.OpeningHours);
        builder.OwnsMany(t => t.Contacts);
        
        // Add regular index to Location
        builder.HasIndex(t => t.Location);
        // Note: For better performance, create a spatial index in SQL Server manually:
        // CREATE SPATIAL INDEX IX_Listings_Location ON Listings(Location) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 16);
    }
}