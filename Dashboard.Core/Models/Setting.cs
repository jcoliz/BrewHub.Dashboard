namespace BrewHub.Dashboard.Core.Models;

public record Setting
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}