using BrewHub.Dashboard.Core.Charting;
using BrewHub.Dashboard.Core.MockData;
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
}