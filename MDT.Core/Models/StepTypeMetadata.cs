namespace MDT.Core.Models;

public class StepTypeMetadata
{
    public string Type { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<PropertyDefinition> Properties { get; set; } = new();
}

public class PropertyDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // string, int, bool, etc.
    public bool Required { get; set; }
    public string DefaultValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
