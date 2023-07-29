using System.Reflection;
using BrewHub.Dashboard.Core.Charting;
using BrewHub.Dashboard.Core.MockData;
using BrewHub.Dashboard.Core.Models;
using System.Text.Json;

namespace Dashboard.Core.Tests.Unit;

/// <summary>
/// This should test everything in BrewHub.Dashboard.Core.Charting
/// Currently only DeviceModelRepository has any actual features
/// </summary>
public class ChartingTests
{
    private readonly MockDataSource _datasource = new();

    [SetUp]
    public void Setup()
    {
    }

    private async Task<Datapoint[]> LoadChartData(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var names  = assembly!.GetManifestResourceNames();
        var resource = assembly!.GetManifestResourceNames().Where(x => x.EndsWith(filename)).SingleOrDefault();
        if (resource is null)
            throw new System.IO.FileNotFoundException("Manifest resource not found", filename);
        using var stream = assembly.GetManifestResourceStream(resource);
        if (stream is null)
            throw new System.IO.FileNotFoundException("Cannot open manifest resource", filename);
        var result = await JsonSerializer.DeserializeAsync<Datapoint[]>(stream);
        if (result is null)
            throw new FormatException("Cannot deserialize json data");

        // Need to convert this input. Because Datapoint.__Value is an object, it leaves the object as a JSonValue,
        // which is doing to cause us problems later.
        var converted =
            result
                .Select(x =>
                    new Datapoint()
                    {
                        __Component = x.__Component,
                        __Device = x.__Device,
                        __Model = x.__Model,
                        __Time = x.__Time,
                        __Field = x.__Field,
                        __Value = FromElement((JsonElement)x.__Value)
                    }
                )
                .ToArray();
        return converted;
    }

    private object FromElement(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.Number => el.GetDouble(),
            JsonValueKind.False => false,
            _ => el.GetString() ?? string.Empty
        };
    }

    [Test]
    public async Task CreateMultiDeviceBarChart()
    {
        // Given: The latest (mock) telemetry from all devices
        var raw = await _datasource.GetLatestDeviceTelemetryAllAsync();

        // ...Fixed to a list, else the randoms will recalculate every time
        var data = raw.ToList();

        // And: Selecting a single component/field combination 
        var component = data.Select(x => x.__Component).Distinct().OrderBy(x => x).Last();
        var field = data.Select(x => x.__Field).Distinct().OrderBy(x => x).Last();
        var label = $"{component}/{field}";

        // When: Creating a chart from the returned data set focused on the selected label
        var chart = ChartMaker.CreateMultiDeviceBarChart(data, new[] { label });

        // Then: The chart contains all the devices
        var devices = data.Select(x => x.__Device).Distinct().OrderBy(x => x);
        Assert.That(chart.Data.Labels,Is.EquivalentTo(devices));

        // And: The label on the chart matches the label we asked for
        Assert.That(chart.Data.Datasets.First().Label, Is.EqualTo(label));

        // And: The values on the chart match the device values for that label
        var values = data.Where(x => x.__Component == component && x.__Field == field).Select(x => Convert.ToInt32(x.__Value));
        Assert.That(chart.Data.Datasets.First().Data,Is.EquivalentTo(values));
    }

    [Test]
    public async Task CreateSolutionChart()
    {
        // The problem here is that the data in this case has MULTIPLE device models. Which we would like
        // to support. Some of the devices have the rt/t+ct/t data, others have the thermostat1/etc data.
        // What should happen is that NULL values are given for cases where one device doesn't HAVE
        // a certain kind of data

        var filename = "SampleData.TelemetryChart.json";
        var data = await LoadChartData(filename);
        var result = ChartMaker.CreateMultiDeviceBarChart(data, new[] { "rt/t", "ct/t", "thermostat1/temperature", "thermostat2/temperature" });

        // For each label (device) there should be a value for each series (metric). Obviously in this
        // case, some metrics won't have values for some devices. In this case, there should be a 'null'

        // There are 4 devices in the data set we supplied
        var devices = 4;

        // Make sure correct number of labels
        var numlabels = result.Data.Labels.Count();
        Assert.That(numlabels, Is.EqualTo(devices));

        // Now make sure each dataset has correct number of data points
        foreach(var dataset in result.Data.Datasets)
        {
            Assert.That(dataset.Data.Count(), Is.EqualTo(devices), $"Incorrect number of labels for series {dataset.Label}");
        }
    }
    [Test]
    public async Task CreateMultiLineChart()
    {
        // Note that this data set triggers Bug 1613, which we've now fixed
        // Bug 1613: Charting: Handle cases where series have different time series

        var filename = "DeviceChart-pizero-1c.json";
        var origdata = await LoadChartData(filename);

        // Just for testing, we're doing to reduce the data to just the sequence markers
        var data = origdata.Where(x => x.__Field == "Seq");
        var result = ChartMaker.CreateMultiLineChart(data, "mm:ss");

        // There are 16 time slices in the data set we supplied
        var timeslices = 16;

        // Make sure correct number of labels (one for each timeslice)
        var numlabels = result.Data.Labels.Count();
        Assert.That(numlabels, Is.EqualTo(timeslices));

        // Make sure correct number of data sets (one for each metric)
        // (These are the metrics we know are in our sample data!)
        var metrics = new[] { "Seq", "rt/Seq", "rv/Seq", "ct/Seq", "cv/Seq", "amb/Seq" };
        var numdatasets = result.Data.Datasets.Count();
        Assert.That(numdatasets, Is.EqualTo(metrics.Length));

        // Make sure correct number of data points in each data set (one for each timeslice)
        foreach(var dataset in result.Data.Datasets)
        {
            Assert.That(dataset.Data.Count(), Is.EqualTo(timeslices), $"Incorrect number of labels for series {dataset.Label}");
        }
    }
}