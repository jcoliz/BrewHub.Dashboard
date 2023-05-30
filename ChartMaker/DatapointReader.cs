using ChartMaker.Models;
using System.IO;
using System.Text.Json;

namespace ChartMaker;

public static class DatapointReader
{
    public static List<Datapoint> ReadFromJson(Stream stream)
    {
        var json = JsonDocument.Parse(stream);
        var data = new List<Datapoint>();
        var root = json.RootElement;
        foreach(var el in root.EnumerateArray())
        {
            var time = DateTimeOffset.Parse(el.GetProperty("__Time").GetString()!);
            var device = el.GetProperty("__Device").GetString()!;
            string? component = (el.TryGetProperty("__Component", out var componentprop)) ? componentprop.GetString() : null;

            foreach(var prop in el.EnumerateObject())
            {
                if (!prop.Name.StartsWith("__"))
                {
                    var field = prop.Name;
                    var value = prop.Value.GetDouble();

                    data.Add(new Datapoint() { __Device = device, __Component = component, __Time = time, __Field = field, __Value = value });
                }
            }
        }

        return data;
    }
}