namespace BrewHub.Dashboard.Core.Dtmi;

public enum DeviceModelMetricKind { Invalid = 0, Component, Telemetry, ReadOnlyProperty, WritableProperty, Command }

public enum DeviceModelMetricFormatter { None = 0, Float, KibiBits, kBytes, PercentInteger, Status }

// Note that "solution" visibility implies device and component visibility as well
public enum DeviceModelMetricVisualizationLevel { Never = 0, Component, Device, Solution }

public record DeviceModelMetric
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DeviceModelMetricKind Kind { get; init; }
    public DeviceModelMetricFormatter Formatter { get; init; }
    public DeviceModelMetricVisualizationLevel VisualizationLevel { get; init; }
    public string? Schema { get; init; }
    public string? Units { get; init; }
    public string? ValueLabel { get; init; }
}
