// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Models;

namespace BrewHub.Dashboard.Core.Display;

/// <summary>
/// Builds displaymetric groups out of groupings of datapoints, using DTMI models
/// for organization and readability
/// </summary>
public class DisplayMetricGroupBuilder
{
    public readonly DeviceModelRepository _models;

    public DisplayMetricGroupBuilder(DeviceModelRepository models)
    {
        _models = models;
    }

    /// <summary>
    /// Build SOLUTION metrics cards. Return one slab (display metric group) per device, containing all metrics sent in
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    public DisplayMetricGroup FromDevice(IGrouping<string,Datapoint> group)
    {
        string ExtractComponentAndMetricName(Datapoint d)
        {
            var f = _models.MapMetricName(d);
            return (d.__Component is null) ? f : $"{_models.MapComponentName(d)}/{f}";
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
                    Value = _models.FormatMetricValue(d)
                })
                .ToArray()
        };
    }

    /// <summary>
    /// Create a single display metric out of a single datapoint.
    /// As formatted by DTMI
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    private DisplayMetric FromDatapoint(Datapoint d)
    {
        return new DisplayMetric()
        {
            Name = _models.MapMetricName(d),
            Id = d.__Field,
            Value = _models.FormatMetricValue(d),
            Units = _models.GetWritableUnits(d)
        };
    }

    /// <summary>
    /// Build DEVICE-level metric cards (aka slabs, aka display metric groups) out of a single component
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    public DisplayMetricGroup FromComponent(IGrouping<string,Datapoint> group)
    {
        return new DisplayMetricGroup() 
        { 
            Title = _models.MapComponentName(group.First()) ?? "Device Details",
            Kind = DisplayMetricGroupKind.Component,
            Id = group.Key,
            Telemetry = group.Where(d=>_models.IsMetricTelemetry(d)).Select(FromDatapoint).ToArray(), 
            ReadOnlyProperties = group.Where(d=>!_models.IsMetricWritable(d) && !_models.IsMetricTelemetry(d)).Select(FromDatapoint).ToArray(), 
            WritableProperties = group.Where(d=>_models.IsMetricWritable(d)).Select(FromDatapoint).ToArray(), 
            Commands = _models.GetCommands(group.First(), DeviceModelMetricVisualizationLevel.Device).ToArray()
        };
    }

    /// <summary>
    /// Build many slabs (display metric group) out of a single component.
    /// One slab per DeviceModelMetricKind
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public DisplayMetricGroup[] ManyFromComponent(IEnumerable<Datapoint> points)
    {
        var result = new List<DisplayMetricGroup>();

        var schema = new DisplayMetric() { Name = "Schema", Id = "schema", Value = points.First().__Model };

        var telemetry = points.Where(d => _models.IsMetricTelemetry(d));
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
        var ro = points.Where(d=>!_models.IsMetricWritable(d) && !_models.IsMetricTelemetry(d));
        // Always will have 'schema' property
        //if (ro.Any())
        {
            result.Add(new DisplayMetricGroup()
            {
                Title = "Properties",
                Id = "roprops",
                Kind = DisplayMetricGroupKind.Grouping,
                ReadOnlyProperties = ro.Select(FromDatapoint).Concat(new[] { schema }).ToArray()
            });
        }
        var writable = points.Where(d=>_models.IsMetricWritable(d));
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
        var commands = _models.GetCommands(points.First(), DeviceModelMetricVisualizationLevel.Component);
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