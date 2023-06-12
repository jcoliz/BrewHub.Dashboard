namespace BrewHub.Dashboard.Core.Display;

/// <summary>
/// A group of display metrics, organized in the way they will be displayed
/// </summary>
public record DisplayMetricGroup
{
    /// <summary>
    /// Human-readable name for this group
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// How we are known in the database
    /// </summary>
    public string? Id { get; init; } = null;

    public DisplayMetricGroupKind Kind { get; init; } = DisplayMetricGroupKind.Empty;

    public DisplayMetric[] Telemetry { get; init; } = Array.Empty<DisplayMetric>();

    public DisplayMetric[] ReadOnlyProperties { get; init; } = Array.Empty<DisplayMetric>();

    public DisplayMetric[] WritableProperties { get; init; } = Array.Empty<DisplayMetric>();

    public DisplayMetric[] Commands { get; init; } = Array.Empty<DisplayMetric>();
}

public enum DisplayMetricGroupKind { Empty = 0, Device, Component, Grouping };
