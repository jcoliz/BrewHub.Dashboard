using BrewHub.Dashboard.Core.Display;
using BrewHub.Dashboard.Core.Models;
using System.Collections.Generic;

namespace BrewHub.Dashboard.Core.Dtmi;

/// <summary>
/// Provides the information needed to display data from a single top-level device model properly
/// </summary>
public interface IDeviceModel
{
    /// <summary>
    /// Human-readable name of this schema
    /// </summary>
    string SchemaName { get; }

    /// <summary>
    /// Telemetry values to be shown when looking at the solution overall
    /// </summary>
    IEnumerable<string> VisualizeTelemetryTop { get; }

    /// <summary>
    /// Telemetry values to be shown when looking at a single device
    /// </summary>
    IEnumerable<string> VisualizeTelemetryDevice { get; }

    /// <summary>
    /// Given a metric id, return the human-readable name
    /// </summary>
    /// <param name="metricid">Schema-defined identifier for metric (could be separated by `/`)</param>
    /// <returns>Human readable name</returns>
    string MapMetricName(Datapoint metric);

    /// <summary>
    /// Format a metric to human-readable form
    /// </summary>
    /// <param name="metric"></param>
    /// <returns></returns>
    string FormatMetricValue(Datapoint metric);

    /// <summary>
    /// Whether a specific metric is a writable property
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    bool IsMetricWritable(Datapoint metric);

    /// <summary> 
    /// Returns the units to be displayed with a metric, or null if metric is not writable, or no units need to be displayed
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    string? GetWritableUnits(Datapoint metric);

    /// <summary>
    /// Return all the commands for a given component id
    /// </summary>
    /// <param name="componentid"></param>
    /// <returns>Displaymetrics for all the commands</returns>
    IEnumerable<DisplayMetric> GetCommands(Datapoint metric);
} 