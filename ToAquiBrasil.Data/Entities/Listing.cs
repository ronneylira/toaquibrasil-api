using NetTopologySuite.Geometries;
using ToAquiBrasil.Data.Entities.ValueObjects;

namespace ToAquiBrasil.Data.Entities;

public class Listing : BaseEntity
{
    public string Name { get; set; }
    
    public string Category { get; set; }
    
    public string Person { get; set; }
    
    public string Email { get; set; }
    
    public int Stars { get; set; }
    
    public string Phone { get; set; }
    
    public string Address { get; set; }
    
    public string About { get; set; }
    
    public string Summary { get; set; }
    
    public int Index { get; set; }
    
    public string Logo { get; set; }
    
    public string Image { get; set; }
    
    public string Link { get; set; }
    
    public string[] Tags { get; set; }
    
    public string[] Services { get; set; }

    public Point Location { get; set; }

    public IEnumerable<ListingImage> Images { get; set; } = new List<ListingImage>();

    public IEnumerable<Review> Reviews { get; set; } = new List<Review>();

    public IEnumerable<OpeningHours> OpeningHours { get; set; } = new List<OpeningHours>();

    public IEnumerable<Contact> Contacts { get; set; } = new List<Contact>();
}