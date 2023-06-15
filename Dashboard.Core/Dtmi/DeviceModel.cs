namespace BrewHub.Dashboard.Core.Dtmi;

public class DeviceModel
{
    public string Name { get; set; }

    public Dictionary<string, DeviceModelMetric> Metrics { get; set; } = new();

    public DeviceModel(string name)
    {
        Name = name;
    }
}