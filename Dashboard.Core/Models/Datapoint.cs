namespace BrewHub.Dashboard.Core.Models;

public record Datapoint
{
    public string __Device = string.Empty;
    public string? __Component;
    public string __Model = string.Empty;
    public DateTimeOffset __Time = DateTimeOffset.MinValue;
    public string __Field = string.Empty;
    public object __Value = 0.0;
}