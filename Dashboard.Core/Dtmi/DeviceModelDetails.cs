using BrewHub.Dashboard.Core.Display;
using BrewHub.Dashboard.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("Dashboard.Core.Tests.Unit")]

namespace BrewHub.Dashboard.Core.Dtmi;

/// <summary>
/// Provides the information needed to display data from a single top-level device model properly
/// </summary>
/// <remarks>
/// Currently this is hard-coded. In the future, it should be more dynamic
/// </remarks>
public class DeviceModelDetails
{
    #region Schema-Specific Formatting

    private readonly Dictionary<string, DeviceModel> _models = new();

    // For testing!!
    internal Dictionary<string, DeviceModel> GetModels() => _models;

    public DeviceModelDetails()
    {
        // Next step would be to load these from storage

        _models["Thermostat;1"] = new("Thermostat")
        {
            Metrics = new()
            {
                { "temperature", new() { Name = "Temperature", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.Float, Units = "°C", VisualizationLevel = DeviceModelMetricVisualizationLevel.Solution } },
                { "maxTempSinceLastReboot", new() { Name = "Max Temperature Since Reboot", Kind = DeviceModelMetricKind.ReadOnlyProperty, Formatter = DeviceModelMetricFormatter.Float, Units = "°C" } },
                { "targetTemperature", new() { Name = "Target Temperature", Kind = DeviceModelMetricKind.WritableProperty, Formatter = DeviceModelMetricFormatter.Float, Units = "°C" } },
                { "getMinMax", new() { Name = "Get Max-Min report", Kind = DeviceModelMetricKind.Command, Units = "D/T", ValueLabel = "Since" } }
            }
        };

        _models["TemperatureController;2"] = new("Temperature Controller")
        {
            Metrics = new()
            {
                { "workingSet", new() { Name = "Working Set", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.KibiBits, VisualizationLevel = DeviceModelMetricVisualizationLevel.Component } },
                { "telemetryPeriod", new() { Name = "Telemetry Period", Kind = DeviceModelMetricKind.WritableProperty } },
                { "reboot", new() { Name = "Reboot", Kind = DeviceModelMetricKind.Command, Units = "s", ValueLabel = "Delay" } },
                { "thermostat1", new() { Name = "Thermostat One", Kind = DeviceModelMetricKind.Component, Schema = "Thermostat;1", VisualizationLevel = DeviceModelMetricVisualizationLevel.Solution }},
                { "thermostat2", new() { Name = "Thermostat Two", Kind = DeviceModelMetricKind.Component, Schema = "Thermostat;1", VisualizationLevel = DeviceModelMetricVisualizationLevel.Solution }},
                { "deviceInformation", new() { Name = "Device Information", Kind = DeviceModelMetricKind.Component }},
            }
        };

        _models["DeviceInformation;1"] = new("Device Information")
        {
            Metrics = new()
            {
                { "serialNumber", new() { Name = "Serial Number", Kind = DeviceModelMetricKind.ReadOnlyProperty } },
                { "manufacturer", new() { Name = "Manufacturer", Kind = DeviceModelMetricKind.ReadOnlyProperty } },
                { "model", new() { Name = "Device Model", Kind = DeviceModelMetricKind.ReadOnlyProperty } },
                { "swVersion", new() { Name = "Software Version", Kind = DeviceModelMetricKind.ReadOnlyProperty } },
                { "osName", new() { Name = "Operating System", Kind = DeviceModelMetricKind.ReadOnlyProperty } },
                { "processorArchitecture", new() { Name = "Processor Architecture", Kind = DeviceModelMetricKind.ReadOnlyProperty } },
                { "totalStorage", new() { Name = "Total Storage", Kind = DeviceModelMetricKind.ReadOnlyProperty, Formatter = DeviceModelMetricFormatter.kBytes } },
                { "totalMemory", new() { Name = "Total Memory", Kind = DeviceModelMetricKind.ReadOnlyProperty, Formatter = DeviceModelMetricFormatter.kBytes } },
            }
        };
    }

    /// <summary>
    /// Telemetry values to be shown when looking at the solution overall
    /// </summary>
    public IEnumerable<string> VisualizeTelemetry(IEnumerable<string> models, DeviceModelMetricVisualizationLevel level)
    {
        // All the things at the top level which have solution-level visibility. Could be metrics, could be components
        var toplevelsviz = 
            models
                .SelectMany(x => 
                    _models[x]
                    .Metrics
                    .Where(y=>y.Value.VisualizationLevel >= level)
                );

        // Considering the top level those, find just the metrics on the DEVICE
        var devicemetrics = 
            toplevelsviz
                .Where(x => x.Value.Kind != DeviceModelMetricKind.Component)
                .Select(x => x.Key);

        // Find the metrics which have top-level visibility AND are on a component which ALSO has top-level visibility
        var componentmetrics = 
            toplevelsviz
                .Where(x => x.Value.Kind == DeviceModelMetricKind.Component)
                .SelectMany(x => 
                    _models[x.Value.Schema!]
                    .Metrics
                    .Where(m=>m.Value.VisualizationLevel >= level)
                    .Select(z=>$"{x.Key}/{z.Key}")
                );

        return devicemetrics.Concat(componentmetrics);
    }

    /// <summary>
    /// Given a metric id, return the human-readable name
    /// </summary>
    /// <param name="metricid">Schema-defined identifier for metric (could be separated by `/`)</param>
    /// <returns>Human readable name</returns>
    internal string MapMetricName(Datapoint d)
    {
        return (_models.TryGetValue(d.__Model, out var model) && model.Metrics.TryGetValue(d.__Field, out var metric))
            ? metric.Name
            : d.__Field;
    }

    internal string? MapComponentName(Datapoint d)
    {
        // PROBLEM: We actually need the PARENT model to do this operation, not the
        // component model. So, for now, we're going to search for it. This will
        // not produce ideal results if there are two parents with the same child component ID's.
        //
        // Fixing that will take some more upstream work.

        return _models
            .SelectMany(x => x.Value.Metrics.Select(y => (model: x.Key, id: y.Key, metric: y.Value)))
            .Where(x => x.id == d.__Component && x.metric.Kind == DeviceModelMetricKind.Component)
            .Select(x => x.metric.Name)
            .FirstOrDefault();
    }

    Func<object, string> degreesCelcius = x => $"{x:F1}°C";
    Func<object, string> floatNoUnits = x => $"{x:F1}";
    Func<object, string> kibiBits = x => $"{(double)x / 7812.5:F1} MB";
    Func<object, string> noFormatting = x => x.ToString() ?? string.Empty;
    Func<object, string> kBytes = x => (double)x switch
    {
        > 1000000 => $"{(double)x / 1000000:F1} GB",
        > 1000 => $"{(double)x / 1000:F1} MB",
        _ => $"{(double)x:F1} kB"
    };

    /// <summary>
    /// Format a metric to human-readable form
    /// </summary>
    /// <param name="metric"></param>
    /// <returns></returns>
    internal string FormatMetricValue(Datapoint d)
    {
        if (_models.TryGetValue(d.__Model,out var model) && model.Metrics.TryGetValue(d.__Field, out var metric) )
        {
            var format = metric.Formatter switch
            {
                DeviceModelMetricFormatter.Float => floatNoUnits,
                DeviceModelMetricFormatter.KibiBits => kibiBits,
                DeviceModelMetricFormatter.kBytes => kBytes,
                _ => noFormatting
            };

            var units = metric.Kind switch
            {
                DeviceModelMetricKind.Telemetry or
                DeviceModelMetricKind.ReadOnlyProperty => metric.Units ?? string.Empty,
                _ => string.Empty
            };

            return $"{format(d.__Value)}{units}";
        }
        else
            return d.__Value?.ToString() ?? "(null)";
    }

    /// <summary>
    /// Whether a specific metric is telemetry metric
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    public bool IsMetricTelemetry(Datapoint d)
    {
        return  (    
                    _models.TryGetValue(d.__Model, out var model) 
                    && 
                    model.Metrics.TryGetValue(d.__Field, out var metric)
                )
                    ? metric.Kind == DeviceModelMetricKind.Telemetry
                    : false;
    }

    /// <summary>
    /// Whether a specific metric is a writable property
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    internal bool IsMetricWritable(Datapoint d)
    {
        return  (
                    _models.TryGetValue(d.__Model,out var model) 
                    && 
                    model.Metrics.TryGetValue(d.__Field, out var metric)
                )
                    ? metric.Kind == DeviceModelMetricKind.WritableProperty
                    : false;
    }

    /// <summary>
    /// Returns the units to be displayed with a metric, or null if metric is not writable, or no units need to be displayed
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    internal string? GetWritableUnits(Datapoint d)
    {
        return  (
                    _models.TryGetValue(d.__Model,out var model) 
                    && 
                    model.Metrics.TryGetValue(d.__Field, out var metric)
                    && 
                    metric.Kind == DeviceModelMetricKind.WritableProperty
                )
                    ? metric.Units
                    : null;
    }

    /// <summary>
    /// Return all the commands for a given component id
    /// </summary>
    /// <param name="componentid"></param>
    /// <returns></returns>
    internal IEnumerable<DisplayMetric> GetCommands(Datapoint d)
    {
        if (_models.TryGetValue(d.__Model,out var model))
        {
            return model
                    .Metrics
                    .Where(x => x.Value.Kind == DeviceModelMetricKind.Command)
                    .Select(x => 
                        new DisplayMetric()
                        {
                            Name = x.Value.Name,
                            Id = x.Key,
                            Value = x.Value.ValueLabel!, // TODO: Probably should allow null for this
                            Units = x.Value.Units
                        });
                        // TODO: Consider if we may be able to merge Display Metric 
                        // and DeviceModelMetric
        }
        else
        {
            return Enumerable.Empty<DisplayMetric>();
        }
    }

    #endregion

    public DisplayMetricGroup FromDeviceComponentTelemetry(IGrouping<string,Datapoint> group)
    {
        string ExtractComponentAndMetricName(Datapoint d)
        {
            var f = MapMetricName(d);
            return (d.__Component is null) ? f : $"{MapComponentName(d)}/{f}";
        }

        return new DisplayMetricGroup()
        {
            Title = group.Key,
            Kind = DisplayMetricGroupKind.Device,
            Id = group.Key,
            Telemetry = group
                .Select(d => new DisplayMetric()
                {
                    Name = ExtractComponentAndMetricName(d),
                    Value = FormatMetricValue(d)
                })
                .ToArray()
        };
    }

    private DisplayMetric FromDatapoint(Datapoint d)
    {
        return new DisplayMetric()
        {
            Name = MapMetricName(d),
            Id = d.__Field,
            Value = FormatMetricValue(d),
            Units = GetWritableUnits(d)
        };
    }

    public DisplayMetricGroup FromComponent(IGrouping<string,Datapoint> c)
    {
        var schema = new DisplayMetric() { Name = "Schema", Id = "schema", Value = c.First().__Model };

        return new DisplayMetricGroup() 
        { 
            Title = MapComponentName(c.First()) ?? "Device Details",
            Kind = DisplayMetricGroupKind.Component,
            Id = c.Key,
            Telemetry = c.Where(x=>IsMetricTelemetry(x)).Select(FromDatapoint).ToArray(), 
            ReadOnlyProperties = c.Where(x=>!IsMetricWritable(x) && !IsMetricTelemetry(x)).Select(FromDatapoint).Concat(new[] { schema }).ToArray(), 
            WritableProperties = c.Where(x=>IsMetricWritable(x)).Select(FromDatapoint).ToArray(), 
            Commands = GetCommands(c.First()).ToArray()
        };
    }

    public DisplayMetricGroup[] FromSingleComponent(IEnumerable<Datapoint> c)
    {
        var result = new List<DisplayMetricGroup>();

        var schema = new DisplayMetric() { Name = "Schema", Id = "schema", Value = c.First().__Model };

        var telemetry = c.Where(x => IsMetricTelemetry(x));
        if (telemetry.Any())
        {
            result.Add(new DisplayMetricGroup()
            {
                Title = "Telemetry",
                Id = "telemetry",
                Kind = DisplayMetricGroupKind.Grouping,
                Telemetry = telemetry.Select(FromDatapoint).ToArray()
            });
        }
        var ro = c.Where(x=>!IsMetricWritable(x) && !IsMetricTelemetry(x));
        if (ro.Any())
        {
            result.Add(new DisplayMetricGroup()
            {
                Title = "Properties",
                Id = "roprops",
                Kind = DisplayMetricGroupKind.Grouping,
                ReadOnlyProperties = ro.Select(FromDatapoint).Concat(new[] { schema }).ToArray()
            });
        }
        var writable = c.Where(x=>IsMetricWritable(x));
        if (writable.Any())
        {
            result.Add(new DisplayMetricGroup()
            {
                Title = "Writable Properties",
                Id = "wprops",
                Kind = DisplayMetricGroupKind.Grouping,
                WritableProperties = writable.Select(FromDatapoint).ToArray()
            });
        }
        var commands = GetCommands(c.First());
        if (commands.Any())
        {
            result.Add(new DisplayMetricGroup()
            {
                Title = "Commands",
                Id = "commands",
                Kind = DisplayMetricGroupKind.Grouping,
                Commands = commands.ToArray()
            });
        }

        return result.ToArray();
    }
}
