using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Models;

namespace Dashboard.Core.Tests.Unit;

/// <summary>
/// This should test everything in BrewHub.Dashboard.Core.Dtmi
/// Currently only DeviceModelRepository has any actual features
/// </summary>
public class DtmiTests
{
    DeviceModelRepository details = new DeviceModelRepository();

    [SetUp]
    public void Setup()
    {
        details = new DeviceModelRepository();
    }

    [TestCase("Thermostat;1", "maxTempSinceLastReboot", "Max Temperature Since Reboot")]
    [TestCase("TemperatureController;2", "workingSet", "Working Set")]
    [TestCase("DeviceInformation;1", "swVersion", "Software Version")]
    [TestCase("UnknownModel;3", "randomStuff", "randomStuff")]
    [Test]
    public void TestNameRemap(string model, string field, string expected)
    {
        var datapoint = new Datapoint() { __Model = model, __Field = field };
        var result = details.MapMetricName(datapoint);

        Assert.That(result, Is.EqualTo(expected));
    }


    [TestCase("Thermostat;1", "maxTempSinceLastReboot", 123.4567d, "123.5°C")]
    [TestCase("TemperatureController;2", "workingSet", 567895d, "72.7 MB")]
    [TestCase("DeviceInformation;1", "totalStorage", 7890123d, "7.9 GB")]
    [TestCase("UnknownModel;3", "randomStuff", 123.45, "123.45")]
    [Test]
    public void TestFormatting(string model, string field, object value, string expected)
    {
        var datapoint = new Datapoint() { __Model = model, __Field = field, __Value = value };
        var result = details.FormatMetricValue(datapoint);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("Thermostat;1", "temperature", true)]
    [TestCase("Thermostat;1", "maxTempSinceLastReboot", false)]
    [TestCase("TemperatureController;2", "workingSet", true)]
    [TestCase("TemperatureController;2", "telemetryPeriod", false)]
    [TestCase("DeviceInformation;1", "totalStorage", false)]
    [TestCase("UnknownModel;3", "randomStuff", false)]
    [Test]
    public void TestIsTelemetry(string model, string field, bool expected)
    {
        var datapoint = new Datapoint() { __Model = model, __Field = field };
        var result = details.IsMetricTelemetry(datapoint);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("Thermostat;1", "targetTemperature", "°C")]
    [TestCase("Thermostat;1", "maxTempSinceLastReboot", null)]
    [TestCase("Thermostat;1", "getMinMax", null)]
    [TestCase("TemperatureController;2", "telemetryPeriod", null)]
    [TestCase("DeviceInformation;1", "totalStorage", null)]
    [TestCase("UnknownModel;1", "totalStorage", null)]
    [Test]
    public void TestGetWritableUnits(string model, string field, string? expected)
    {
        var datapoint = new Datapoint() { __Model = model, __Field = field };
        var result = details.GetWritableUnits(datapoint);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("Thermostat;1", 1, "getMinMax")]
    [TestCase("TemperatureController;2", 1, "reboot")]
    [TestCase("DeviceInformation;1", 0, "")]
    [TestCase("UnknownModel;1", 0, "")]
    public void TestGetCommands(string model, int numexpected, string example)
    {
        var datapoint = new Datapoint() { __Model = model };
        var result = details.GetCommands(datapoint).Select(x=>x.Id);

        Assert.That(result.Count(), Is.EqualTo(numexpected));
        if (numexpected > 0)
            Assert.That(result, Contains.Item(example));
    }

    [TestCase("TemperatureController;2", "deviceInformation", "Device Information")]
    [TestCase("TemperatureController;2", "thermostat1", "Thermostat One")]
    public void TestMapComponentName(string _, string component, string expected)
    {
        // This tests points out something problematic in the current implementation.
        // At this moment, we only know the model of the COMPONENT, here in this 
        // case "Thermostat;1". However, the readable name of component instance
        // is a property of the DEVICE, here "TemperatureController;2". So we will
        // need to look up the model of the device before we can truly perform
        // this operation in a model-driven way.

        var datapoint = new Datapoint() { __Component = component };
        var result = details.MapComponentName(datapoint);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void TestSolutionVisualization()
    {
        var result = details.VisualizeTelemetry(new[] { "TemperatureController;2"} , DeviceModelMetricVisualizationLevel.Solution);
        var expected = new[] { "thermostat1/temperature", "thermostat2/temperature" };

        Assert.That(result, Is.EquivalentTo(expected));
    }

    [Test]
    public void TestSolutionVisualizationWithUnknownModel()
    {
        var result = details.VisualizeTelemetry(new[] { "TemperatureController;2", "UnknownModel;3" } , DeviceModelMetricVisualizationLevel.Solution);
        var expected = new[] { "thermostat1/temperature", "thermostat2/temperature" };

        Assert.That(result, Is.EquivalentTo(expected));
    }

    [Test]
    public void TestSolutionVisualizationT2NotVisible()
    {
        // Add another thermostat, but set its visibility to device
        details.Models["TemperatureController;2"].Metrics["thermostat3"] = new() { Kind = DeviceModelMetricKind.Component, VisualizationLevel = DeviceModelMetricVisualizationLevel.Device };
        var result = details.VisualizeTelemetry(new[] { "TemperatureController;2"} , DeviceModelMetricVisualizationLevel.Solution);
        var expected = new[] { "thermostat1/temperature", "thermostat2/temperature" };

        Assert.That(result, Is.EquivalentTo(expected));
    }

    [Test]
    public void TestDeviceVisualization()
    {
        var result = details.VisualizeTelemetry(new[] { "TemperatureController;2"} , DeviceModelMetricVisualizationLevel.Device);
        var expected = new[] { "thermostat1/temperature", "thermostat2/temperature" };

        Assert.That(result, Is.EquivalentTo(expected));
    }

    [Test]
    public void TestDeviceVisualizationWithAddedComponent()
    {
        // Add another thermostat, but set its visibility to device
        details.Models["TemperatureController;2"].Metrics["thermostat3"] = new() { Kind = DeviceModelMetricKind.Component, Schema = "Thermostat;1", VisualizationLevel = DeviceModelMetricVisualizationLevel.Device };

        var result = details.VisualizeTelemetry(new[] { "TemperatureController;2"} , DeviceModelMetricVisualizationLevel.Device);
        var expected = new[] { "thermostat1/temperature", "thermostat2/temperature", "thermostat3/temperature" };

        Assert.That(result, Is.EquivalentTo(expected));
    }

}