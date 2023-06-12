namespace BrewHub.Dashboard.Core.Models;

public record Script
{
    public string Name { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public DateTimeOffset Updated { get; init; } = DateTimeOffset.MinValue;
}