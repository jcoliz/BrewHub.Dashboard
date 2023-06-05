namespace BrewHub.Dashboard.Core.Dtmi;

public record KeyValueUnits
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Units { get; init; } = null;
    public bool Writable { get; init; } = false;
}
