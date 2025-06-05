namespace ToAquiBrasil.Core.Services.Dtos;

public class IpLocationResponseDto
{
    public string Ip { get; set; }
    
    public string LocalityLanguageRequested { get; set; }
    public IpLocationCountryDto Country { get; set; }
    public IpLocationLocationDto Location { get; set; }

    public class IpLocationCountryDto
    {
        public string Name { get; set; }
    }

    public class IpLocationLocationDto
    {
        public string Continent { get; set; }
        
        public string ContinentCode { get; set; }
        public string City { get; set; }
        
        public string LocalityName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}