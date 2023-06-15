using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Models;

namespace Dashboard.Core.Tests.Unit;

public class DtmiTests
{
    DeviceModelDetails details = new DeviceModelDetails();

    [SetUp]
    public void Setup()
    {
        details = new DeviceModelDetails();
    }

    [TestCase("Thermostat;1", "maxTempSinceLastReboot", "Max Temperature Since Reboot")]
    [TestCase("TemperatureController;2", "workingSet", "Working Set")]
    [TestCase("DeviceInformation;1", "swVersion", "Software Version")]
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
    [Test]
    public void TestIsTelemetry(string model, string field, bool expected)
    {
        var datapoint = new Datapoint() { __Model = model, __Field = field };
        var result = details.IsMetricTelemetry(datapoint);

        Assert.That(result, Is.EqualTo(expected));
    }

}