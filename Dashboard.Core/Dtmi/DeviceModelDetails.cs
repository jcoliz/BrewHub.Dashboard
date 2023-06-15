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

    public DeviceModelDetails()
    {
        _models["Thermostat;1"] = new("Thermostat")
        {
            Metrics = new()
            {
                { "temperature", new() { Name = "Temperature", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.Float, Units = "°C" } },
                { "maxTempSinceLastReboot", new() { Name = "Max Temperature Since Reboot", Kind = DeviceModelMetricKind.ReadOnlyProperty, Formatter = DeviceModelMetricFormatter.Float, Units = "°C" } },
                { "targetTemperature", new() { Name = "Target Temperature", Kind = DeviceModelMetricKind.WritableProperty, Formatter = DeviceModelMetricFormatter.Float, Units = "°C" } },
                { "getMinMax", new() { Name = "Get Max-Min report", Kind = DeviceModelMetricKind.Command, Units = "D/T", ValueLabel = "Delay" } }
            }
        };
    }

    /// <summary>
    /// Telemetry values to be shown when looking at the solution overall
    /// </summary>
    public IEnumerable<string> VisualizeTelemetryTop => new[] { "thermostat1/temperature", "thermostat2/temperature" };

    /// <summary>
    /// Telemetry values to be shown when looking at a single device
    /// </summary>
    public IEnumerable<string> VisualizeTelemetryDevice => new[] { "thermostat1/temperature", "thermostat2/temperature" };

    /// <summary>
    /// Given a metric id, return the human-readable name
    /// </summary>
    /// <param name="metricid">Schema-defined identifier for metric (could be separated by `/`)</param>
    /// <returns>Human readable name</returns>
    internal string MapMetricName(Datapoint d)
    {
        if (_models.TryGetValue(d.__Model,out var model))
        {
            return model.Metrics.TryGetValue(d.__Field, out var metric)
                    ? metric.Name
                    : d.__Field;
        }

        return d.__Field switch
        {
            "serialNumber" => "Serial Number",
            "deviceInformation" => "Device Information",
            "manufacturer" => "Manufacturer",
            "model" => "Device Model",
            "swVersion" => "Software Version",
            "osName" => "Operating System",
            "processorArchitecture" => "Processor Architecture",
            "totalStorage" => "Total Storage",
            "totalMemory" => "Total Memory",
            "temperature" => "Temperature",
            "maxTempSinceLastReboot" => "Max Temperature Since Reboot",
            "targetTemperature" => "Target Temperature",
            "workingSet" => $"Working Set",
            "telemetryPeriod" => "Telemetry Period",
            _ => d.__Field
        };
    } 

    // TODO: At this moment, we only know the model of the COMPONENT, here in this 
    // case "Thermostat;1". However, the readable name of component instance
    // is a property of the DEVICE, here "TemperatureController;2". So we will
    // need to look up the model of the device before we can truly perform
    // this operation in a model-driven way.

    internal string? MapComponentName(Datapoint d) => d.__Component switch
    {
        "deviceInformation" => "Device Information",
        "thermostat1" => "Thermostat One",
        "thermostat2" => "Thermostat Two",
        _ => d.__Component
    };

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
        {
            var format = d.__Model switch
            {
                "DeviceInformation;1" => d.__Field switch 
                {
                    "totalStorage" or
                    "totalMemory" => kBytes,
                    _ => noFormatting
                },
                "Thermostat;1" => d.__Field switch
                {
                    "targetTemperature" => floatNoUnits,
                    "maxTempSinceLastReboot" or
                    "temperature" => degreesCelcius,
                    _ => noFormatting
                },
                "TemperatureController;2" => d.__Field switch
                {
                    "workingSet" => kibiBits,
                    _ => noFormatting
                },
                _ => noFormatting
            };

            return format(d.__Value);
        }

    }

    /// <summary>
    /// Whether a specific metric is telemetry metric
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    internal bool IsMetricTelemetry(Datapoint d)
    {
        if (_models.TryGetValue(d.__Model,out var model))
        {
            return model.Metrics.TryGetValue(d.__Field, out var metric)
                    ? metric.Kind == DeviceModelMetricKind.Telemetry
                    : false;
        }

        return d.__Model switch
        {
            "TemperatureController;2" => d.__Field == "workingSet",
            "Thermostat;1" => d.__Field == "temperature",
            _ => false
        };
    }

    /// <summary>
    /// Whether a specific metric is a writable property
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    internal bool IsMetricWritable(Datapoint d)
    {
        if (_models.TryGetValue(d.__Model,out var model))
        {
            return model.Metrics.TryGetValue(d.__Field, out var metric)
                    ? metric.Kind == DeviceModelMetricKind.WritableProperty
                    : false;
        }

        return d.__Model switch
        {
            "Thermostat;1" => d.__Field == "targetTemperature",
            "TemperatureController;2" => d.__Field == "telemetryPeriod",
            _ => false
        };
    }

    /// <summary>
    /// Returns the units to be displayed with a metric, or null if metric is not writable, or no units need to be displayed
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    internal string? GetWritableUnits(Datapoint d)
    {
        if (_models.TryGetValue(d.__Model,out var model))
        {
            return  model.Metrics.TryGetValue(d.__Field, out var metric) 
                    && 
                    metric.Kind == DeviceModelMetricKind.WritableProperty
                        ? metric.Units
                        : null;
        }

        return d.__Model switch
        {
            "Thermostat;1" => (d.__Field == "targetTemperature") ? "°C" : null,
            _ => null
        };
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
            return model.Metrics
                            .Where(x => x.Value.Kind == DeviceModelMetricKind.Command)
                            .Select(x => 
                                new DisplayMetric()
                                {
                                    Name = x.Value.Name,
                                    Id = x.Key,
                                    Value = x.Value.ValueLabel!, // TODO: Probably should allow null for this
                                    Units = x.Value.Units
                                });
        }

        return d.__Model switch
        {
            "TemperatureController;2" => new List<DisplayMetric>()
            {
                new() { Name = "Reboot", Id = "reboot", Value = "Delay", Units = "s" }
            }
            ,
            "Thermostat;1" => new List<DisplayMetric>()
            {
                new() { Name = "Get Max-Min report", Id = "getMinMax", Value = "Since", Units = "D/T" }
            },
            _ => Enumerable.Empty<DisplayMetric>()
        };
    }

    #endregion

    public DisplayMetricGroup FromDeviceComponentTelemetry(IGrouping<string,Datapoint> d)
    {
        string ExtractComponentAndMetricName(Datapoint d)
        {
            var f = MapMetricName(d);
            return (d.__Component is null) ? f : $"{MapComponentName(d)}/{f}";
        }

        return new DisplayMetricGroup()
        {
            Title = d.Key,
            Kind = DisplayMetricGroupKind.Device,
            Id = d.Key,
            Telemetry = d.Select(y => new DisplayMetric()
            {
                Name = ExtractComponentAndMetricName(y),
                Value = FormatMetricValue(y)
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
