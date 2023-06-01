using System.Collections.Generic;
using System.Linq;

namespace DashboardIoT.Display;

/// <summary>
/// A slab containing a subset of device information for display
/// </summary>
public record Slab
{
    public string Header { get; init; } = string.Empty;
    public IEnumerable<KeyValueUnits> Properties { get; init; } = Enumerable.Empty<KeyValueUnits>();
    public IEnumerable<KeyValueUnits> WritableProperties { get; init; } = Enumerable.Empty<KeyValueUnits>();
    public IEnumerable<KeyValueUnits> Commands { get; init; } = Enumerable.Empty<KeyValueUnits>();
    public string ComponentId { get; init; } = null;
}

public record KeyValueUnits
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Units { get; init; } = null;
}