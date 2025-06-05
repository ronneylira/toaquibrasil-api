namespace ToAquiBrasil.Core.Models;

public class RadiusModel
{
    public RadiusModel(string label, string value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; init; }
    
    public string Value { get; init; }
}