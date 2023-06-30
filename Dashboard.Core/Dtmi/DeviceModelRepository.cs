// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

using BrewHub.Dashboard.Core.Display;
using BrewHub.Dashboard.Core.Models;
using System.Runtime.CompilerServices;

namespace BrewHub.Dashboard.Core.Dtmi;

/// <summary>
/// Contains a collection of device models, and provides methods to format
/// and organize datapoints according to the known models
/// </summary>
/// <remarks>
/// Currently this is hard-coded. In the future, it should be more dynamic
/// </remarks>
public class DeviceModelRepository
{
    public Dictionary<string, DeviceModel> Models { get; } = new();

    public DeviceModelRepository()
    {
        // Next step would be to load these from storage

        Models["Thermostat;1"] = new("Thermostat")
        {
            Metrics = new()
            {
                { "temperature", new() { Name = "Temperature", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.Float, Units = "°C", DashboardChartLevel = DeviceModelMetricVisualizationLevel.Solution, DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Solution } },
                { "maxTempSinceLastReboot", new() { Name = "Max Temperature Since Reboot", Kind = DeviceModelMetricKind.ReadOnlyProperty, Formatter = DeviceModelMetricFormatter.Float, Units = "°C" } },
                { "targetTemperature", new() { Name = "Target Temperature", Kind = DeviceModelMetricKind.WritableProperty, Formatter = DeviceModelMetricFormatter.Float, Units = "°C" } },
                { "getMinMax", new() { Name = "Get Max-Min report", Kind = DeviceModelMetricKind.Command, Units = "D/T", ValueLabel = "Since" } }
            }
        };

        Models["TemperatureController;2"] = new("Temperature Controller")
        {
            Metrics = new()
            {
                { "workingSet", new() { Name = "Working Set", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.KibiBits, DashboardChartLevel = DeviceModelMetricVisualizationLevel.Component, DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Solution } },
                { "telemetryPeriod", new() { Name = "Telemetry Period", Kind = DeviceModelMetricKind.WritableProperty } },
                { "reboot", new() { Name = "Reboot", Kind = DeviceModelMetricKind.Command, Units = "s", ValueLabel = "Delay" } },
                { "thermostat1", new() { Name = "Thermostat One", Kind = DeviceModelMetricKind.Component, Schema = "Thermostat;1", DashboardChartLevel = DeviceModelMetricVisualizationLevel.Solution }},
                { "thermostat2", new() { Name = "Thermostat Two", Kind = DeviceModelMetricKind.Component, Schema = "Thermostat;1", DashboardChartLevel = DeviceModelMetricVisualizationLevel.Solution }},
                { "deviceInformation", new() { Name = "Device Information", Kind = DeviceModelMetricKind.Component }},
            }
        };

        Models["DeviceInformation;1"] = new("Device Information")
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

        Models["dtmi:brewhub:prototypes:still_6_unit;1"] = new("6-Unit Distillery Prototype v1")
        {
            Metrics = new()
            {
                { "WorkingSet", new() { Name = "Working Set", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.KibiBits, DashboardChartLevel = DeviceModelMetricVisualizationLevel.Component } },
                { "CpuLoad", new() { Name = "CPU Load", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.PercentDouble, DashboardChartLevel = DeviceModelMetricVisualizationLevel.Device } },
                { "Status", new() { Name = "Status", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.Status, DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Solution } },
                { "TelemetryInterval", new() { Name = "Telemetry Interval", Kind = DeviceModelMetricKind.WritableProperty, DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Component } },
                { "reboot", new() { Name = "Reboot", Kind = DeviceModelMetricKind.Command, Units = "s", ValueLabel = "Delay", DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Component } },
                { "serialNumber", new() { Name = "Serial Number", Kind = DeviceModelMetricKind.ReadOnlyProperty } },
                { "manufacturer", new() { Name = "Manufacturer", Kind = DeviceModelMetricKind.ReadOnlyProperty } },
                { "model", new() { Name = "Device Model", Kind = DeviceModelMetricKind.ReadOnlyProperty } },
                { "swVersion", new() { Name = "Software Version", Kind = DeviceModelMetricKind.ReadOnlyProperty } },
                { "osName", new() { Name = "Operating System", Kind = DeviceModelMetricKind.ReadOnlyProperty, DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Component } },
                { "processorArchitecture", new() { Name = "Processor Architecture", Kind = DeviceModelMetricKind.ReadOnlyProperty, DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Component } },
                { "totalStorage", new() { Name = "Total Storage", Kind = DeviceModelMetricKind.ReadOnlyProperty, Formatter = DeviceModelMetricFormatter.kBytes, DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Component } },
                { "totalMemory", new() { Name = "Total Memory", Kind = DeviceModelMetricKind.ReadOnlyProperty, Formatter = DeviceModelMetricFormatter.kBytes, DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Component } },
                { "rt", new() { Name = "Reflux Thermostat", Kind = DeviceModelMetricKind.Component, Schema = "dtmi:brewhub:controls:Thermostat;1", DashboardChartLevel = DeviceModelMetricVisualizationLevel.Solution }},
                { "ct", new() { Name = "Condenser Thermostat", Kind = DeviceModelMetricKind.Component, Schema = "dtmi:brewhub:controls:Thermostat;1", DashboardChartLevel = DeviceModelMetricVisualizationLevel.Solution }},
                { "amb", new() { Name = "Ambient Conditions", Kind = DeviceModelMetricKind.Component, Schema = "dtmi:brewhub:sensors:TH;1", DashboardChartLevel = DeviceModelMetricVisualizationLevel.Device }},
                { "rv", new() { Name = "Reflux Valve", Kind = DeviceModelMetricKind.Component, Schema = "dtmi:brewhub:controls:BinaryValve;1" } },
                { "cv", new() { Name = "Condenser Valve", Kind = DeviceModelMetricKind.Component, Schema = "dtmi:brewhub:controls:BinaryValve;1" } },
            }
        };

        Models["dtmi:brewhub:controls:Thermostat;1"] = new("Thermostat")
        {
            Metrics = new()
            {
                { "t", new() { Name = "Temperature", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.Float, Units = "°C", DashboardChartLevel = DeviceModelMetricVisualizationLevel.Solution, DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Solution } },
                { "Status", new() { Name = "Status", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.Status } },
                { "targetTemp", new() { Name = "Target Temperature", Kind = DeviceModelMetricKind.WritableProperty, Formatter = DeviceModelMetricFormatter.Float, Units = "°C" } },
                { "tcorr", new() { Name = "Temperature Correction", Kind = DeviceModelMetricKind.WritableProperty, Formatter = DeviceModelMetricFormatter.Float, Units = "°C", DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Component } },
                { "targetMetric", new() { Name = "Target Metric", Kind = DeviceModelMetricKind.WritableProperty } },
                { "overTemp", new() { Name = "Is Over Target Temp", Kind = DeviceModelMetricKind.ReadOnlyProperty, DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Never } }
            }
        };
        
        Models["dtmi:brewhub:sensors:TH;1"] = new("Temp+Humidity")
        {
            Metrics = new()
            {
                { "t", new() { Name = "Temperature", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.Float, Units = "°C", DashboardChartLevel = DeviceModelMetricVisualizationLevel.Solution, DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Solution } },
                { "h", new() { Name = "Relative Humidity", Kind = DeviceModelMetricKind.Telemetry, Formatter = DeviceModelMetricFormatter.Float, Units = "%RH", DashboardChartLevel = DeviceModelMetricVisualizationLevel.Component } },
                { "tcorr", new() { Name = "Temperature Correction", Kind = DeviceModelMetricKind.WritableProperty, Formatter = DeviceModelMetricFormatter.Float, Units = "°C", DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Component } },
                { "hcorr", new() { Name = "Humidity Correction", Kind = DeviceModelMetricKind.WritableProperty, Formatter = DeviceModelMetricFormatter.Float, Units = "%RH", DashboardMetricLevel = DeviceModelMetricVisualizationLevel.Component } },
            }
        };

        Models["dtmi:brewhub:controls:BinaryValve;1"] = new("Binary Valve")
        {
            Metrics = new()
            {
                { "sourceMetric", new() { Name = "Source Metric", Kind = DeviceModelMetricKind.WritableProperty } },
                { "open", new() { Name = "Is Open", Kind = DeviceModelMetricKind.WritableProperty } }
            }
        };
    }

    /// <summary>
    /// Telemetry values to be shown when looking at the given level of the solution
    /// </summary>
    public IEnumerable<string> VisualizeTelemetry(IEnumerable<string> models, DeviceModelMetricVisualizationLevel level)
    {
        // NOTE: Right now we expect this to be called for DEVICE models only.
        // Now we are trying to get it to work for COMPONENT models.
        // Not sure if this can support it, or whether it should be its own call.

        // All the things on the given models which have the requested level of visibility. Could be metrics, could be components
        var toplevelsviz = 
            models
                .Where(x=>Models.ContainsKey(x))
                .SelectMany(x => 
                    Models[x]
                    .Metrics
                    .Where(y=>y.Value.DashboardChartLevel >= level)
                );

        // Considering the top level those, find just the metrics which are not on a subcomponent
        var devicemetrics = 
            toplevelsviz
                .Where(x => x.Value.Kind != DeviceModelMetricKind.Component)
                .Select(x => x.Key);

        // Find the metrics which have requested level visibility AND are on a child component which ALSO has requested level visibility
        var componentmetrics = 
            toplevelsviz
                .Where(x => x.Value.Kind == DeviceModelMetricKind.Component)
                .Where(x => Models.ContainsKey(x.Value.Schema!))
                .SelectMany(x => 
                    Models[x.Value.Schema!]
                    .Metrics
                    .Where(m=>m.Value.DashboardChartLevel >= level)
                    .Select(z=>$"{x.Key}/{z.Key}")
                );

        return devicemetrics.Concat(componentmetrics);
    }

    /// <summary>
    /// Given a metric id, return the human-readable name
    /// </summary>
    /// <param name="metricid">Schema-defined identifier for metric (could be separated by `/`)</param>
    /// <returns>Human readable name</returns>
    public string MapMetricName(Datapoint d)
    {
        return (Models.TryGetValue(d.__Model, out var model) && model.Metrics.TryGetValue(d.__Field, out var metric))
            ? metric.Name
            : d.__Field;
    }

    public string? MapComponentName(Datapoint d)
    {
        // PROBLEM: We actually need the PARENT model to do this operation, not the
        // component model. So, for now, we're going to search for it. This will
        // not produce ideal results if there are two parents with the same child component ID's.
        //
        // Fixing that will take some more upstream work.

        return Models
            .SelectMany(x => x.Value.Metrics.Select(y => (model: x.Key, id: y.Key, metric: y.Value)))
            .Where(x => x.id == d.__Component && x.metric.Kind == DeviceModelMetricKind.Component)
            .Select(x => x.metric.Name)
            .FirstOrDefault();
    }

    Func<object, string> degreesCelcius = x => $"{x:F1}°C";
    Func<object, string> floatNoUnits = x => $"{x:F1}";
    Func<object, string> kibiBits = x => $"{(double)x / 7812.5:F1} MB";
    Func<object, string> noFormatting = x => x.ToString() ?? string.Empty;
    Func<object, string> percentInt = x => $"{x}%";
    Func<object, string> percentDouble = x => $"{x:F1}%";
    Func<object, string> status = x => (double)x == 0 ? "OK" : $"ERROR {x:F0}";
    Func<object, string> kBytes = x => (double)x switch
    {
        > 1000000 => $"{(double)x / 1000000:F1} GB",
        > 1000 => $"{(double)x / 1000:F1} MB",
        _ => $"{(double)x:F1} kB"
    };

    // Just a little visual indication that we don't actually know how to 
    // format this. This just happens if we add a new value to the formatters
    // enum, but don't add a case for it in the switch expression below.
    Func<object, string> notFound = x => $"{x} ???";

    /// <summary>
    /// Format a metric to human-readable form
    /// </summary>
    /// <param name="metric"></param>
    /// <returns></returns>
    public string FormatMetricValue(Datapoint d)
    {
        if (Models.TryGetValue(d.__Model,out var model) && model.Metrics.TryGetValue(d.__Field, out var metric) )
        {
            var format = metric.Formatter switch
            {
                DeviceModelMetricFormatter.Float => floatNoUnits,
                DeviceModelMetricFormatter.KibiBits => kibiBits,
                DeviceModelMetricFormatter.kBytes => kBytes,
                DeviceModelMetricFormatter.PercentInteger => percentInt,
                DeviceModelMetricFormatter.PercentDouble => percentDouble,
                DeviceModelMetricFormatter.Status => status,
                DeviceModelMetricFormatter.None => noFormatting,
                _ => notFound
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
    /// Given a formatted value, turn that into the correct type of object 
    /// </summary>
    /// <param name="d"></param>
    /// <param name="formatted"></param>
    /// <returns></returns>
    public object UnformatMetricValue(Datapoint d, string formatted)
    {
        if (Models.TryGetValue(d.__Model,out var model) && model.Metrics.TryGetValue(d.__Field, out var metric) )
        {
            return metric.Formatter switch
            {
                DeviceModelMetricFormatter.Float or
                DeviceModelMetricFormatter.PercentDouble => Convert.ToDouble(formatted),
                DeviceModelMetricFormatter.KibiBits or
                DeviceModelMetricFormatter.kBytes or
                DeviceModelMetricFormatter.PercentInteger or
                DeviceModelMetricFormatter.Status => Convert.ToInt32(formatted),
                DeviceModelMetricFormatter.None or
                _ => formatted
            };
        }
        else
            return formatted;
    }

    /// <summary>
    /// Whether a specific metric is telemetry metric
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    public bool IsMetricTelemetry(Datapoint d)
    {
        return  (    
                    Models.TryGetValue(d.__Model, out var model) 
                    && 
                    model.Metrics.TryGetValue(d.__Field, out var metric)
                )
                    ? metric.Kind == DeviceModelMetricKind.Telemetry
                    : false;
    }

    /// <summary>
    /// Whether a specific metric is shown at a given level
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    public bool IsMetricShownAtLevel(Datapoint d, DeviceModelMetricVisualizationLevel level)
    {
        return  (    
                    Models.TryGetValue(d.__Model, out var model) 
                    && 
                    model.Metrics.TryGetValue(d.__Field, out var metric)
                )
                    ? metric.DashboardMetricLevel >= level
                    : false;
    }

    /// <summary>
    /// Whether a specific metric is a writable property
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    public bool IsMetricWritable(Datapoint d)
    {
        return  (
                    Models.TryGetValue(d.__Model,out var model) 
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
    public string? GetWritableUnits(Datapoint d)
    {
        return  (
                    Models.TryGetValue(d.__Model,out var model) 
                    && 
                    model.Metrics.TryGetValue(d.__Field, out var metric)
                    && 
                    metric.Kind == DeviceModelMetricKind.WritableProperty
                )
                    ? metric.Units
                    : null;
    }

    /// <summary>
    /// Return all the commands for a given component id, at a given metric level
    /// </summary>
    /// <param name="componentid"></param>
    /// <returns></returns>
    public IEnumerable<DisplayMetric> GetCommands(Datapoint d, DeviceModelMetricVisualizationLevel level)
    {
        if (Models.TryGetValue(d.__Model,out var model))
        {
            return model
                    .Metrics
                    .Where(x => x.Value.Kind == DeviceModelMetricKind.Command)
                    .Where(x => x.Value.DashboardMetricLevel >= level)
                    .Select(x => 
                        new DisplayMetric()
                        {
                            Name = x.Value.Name,
                            Id = x.Key,
                            // TODO: User Story 1615: Allow DisplayMetric to have null value
                            Value = x.Value.ValueLabel ?? string.Empty,
                            Units = x.Value.Units
                        });
                        // TODO: User Story 1616: Consider merging Display Metric and DeviceModelMetric
        }
        else
        {
            return Enumerable.Empty<DisplayMetric>();
        }
    }
}
