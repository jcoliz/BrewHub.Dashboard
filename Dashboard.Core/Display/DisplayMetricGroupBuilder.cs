using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Models;

namespace BrewHub.Dashboard.Core.Display;

/// <summary>
/// Builds displaymetric groups out of groupings of datapoints, using DTMI models
/// for organization and readability
/// </summary>
public class DisplayMetricGroupBuilder
{
    public readonly DeviceModelDetails _models;

    public DisplayMetricGroupBuilder(DeviceModelDetails models)
    {
        _models = models;
    }

    public DisplayMetricGroup FromDeviceComponentTelemetry(IGrouping<string,Datapoint> group)
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

    public DisplayMetricGroup FromComponent(IGrouping<string,Datapoint> c)
    {
        var schema = new DisplayMetric() { Name = "Schema", Id = "schema", Value = c.First().__Model };

        return new DisplayMetricGroup() 
        { 
            Title = _models.MapComponentName(c.First()) ?? "Device Details",
            Kind = DisplayMetricGroupKind.Component,
            Id = c.Key,
            Telemetry = c.Where(x=>_models.IsMetricTelemetry(x)).Select(FromDatapoint).ToArray(), 
            ReadOnlyProperties = c.Where(x=>!_models.IsMetricWritable(x) && !_models.IsMetricTelemetry(x)).Select(FromDatapoint).Concat(new[] { schema }).ToArray(), 
            WritableProperties = c.Where(x=>_models.IsMetricWritable(x)).Select(FromDatapoint).ToArray(), 
            Commands = _models.GetCommands(c.First()).ToArray()
        };
    }

    public DisplayMetricGroup[] FromSingleComponent(IEnumerable<Datapoint> c)
    {
        var result = new List<DisplayMetricGroup>();

        var schema = new DisplayMetric() { Name = "Schema", Id = "schema", Value = c.First().__Model };

        var telemetry = c.Where(x => _models.IsMetricTelemetry(x));
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
        var ro = c.Where(x=>!_models.IsMetricWritable(x) && !_models.IsMetricTelemetry(x));
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
        var writable = c.Where(x=>_models.IsMetricWritable(x));
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
        var commands = _models.GetCommands(c.First());
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