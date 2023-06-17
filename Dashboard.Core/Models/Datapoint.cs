namespace BrewHub.Dashboard.Core.Models;

public record Datapoint
{
    public string __Device { get; init; } = string.Empty;
    public string? __Component { get; init; }
    public string __Model { get; init; } = string.Empty;
    public DateTimeOffset __Time { get; init; } = DateTimeOffset.MinValue;
    public string __Field { get; init; } = string.Empty;
    public object __Value { get; init; } = 0.0;
}