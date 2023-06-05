using BrewHub.Dashboard.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BrewHub.Dashboard.Services.CosmosDB;

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

    public static List<Datapoint> ReadFromInfluxDB(Dictionary<string,Dictionary<string,object>> input)
    {
        Datapoint FromKvp(string device, KeyValuePair<string,object> kvp)
        {
            var split = kvp.Key.Split('/');

            return new Datapoint()
            {
                __Device = device,
                __Field = split.Last(),
                __Value = (double)kvp.Value,
                __Component = (split.Length > 1) ? split.First() : null
            };
        }

        var result = input.SelectMany(x => x.Value.Select(y => FromKvp(x.Key, y))).ToList();

        return result;
    }

    public static List<Datapoint> ReadFromInfluxDB(Dictionary<string, List<(DateTimeOffset,double)>> input)
    {
        // TODO: This data pipeline is fairly nuts. It goes through a LOT of steps and permutations to get
        // where we need it. Must rethink!!

        IEnumerable<Datapoint> FromKvp(KeyValuePair<string,List<(DateTimeOffset,double)>> kvp)
        {
            var split = kvp.Key.Split('/');

            return kvp.Value.Select(
                x =>
                new Datapoint()
                {
                    __Field = split.Last(),
                    __Component = (split.Length > 1) ? split.First() : null,
                    __Value = x.Item2,
                    __Time = x.Item1
                }
            );
        }

        var result = input.SelectMany(FromKvp).ToList();

        return result;
    }

}