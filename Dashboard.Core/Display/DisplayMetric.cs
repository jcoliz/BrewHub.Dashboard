namespace BrewHub.Dashboard.Core.Display;

/// <summary>
/// A metric with its value, formatted for human-readable display
/// </summary>
public record DisplayMetric
{
    /// <summary>
    /// Human-readable name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Machine-readable identifier
    /// </summary>
    /// <remarks>
    /// This is how the metric is known in the database
    /// </remarks>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Current value of the metric, formatted
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Units of the metric, if displayed separately
    /// </summary>
    /// <remarks>
    /// Only needed when the value is displayed separately from the units
    /// </remarks>
    public string? Units { get; init; } = null;
}
