using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.ChartJS;
using DashboardIoT.Core.Interfaces;
using DashboardIoT.Core.MockData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ChartMaker;
using DashboardIoT.Display;

namespace DashboardIoT.Pages
{
    public class DevicePageModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDataSource _datasource;

        public ChartConfig Chart { get; set; }

        public TimeframeEnum Timeframe { get; set; } = TimeframeEnum.Hour;

        public string DeviceId { get; set; } = "device-1";

        public IEnumerable<IReading> Metrics { get; private set; }

        public IEnumerable<Slab> Slabs { get; private set; }

        public DevicePageModel(ILogger<IndexModel> logger, IDataSource datasource)
        {
            _logger = logger;
            _datasource = datasource;
        }

        static readonly ChartColor _red = new("#B81d13");
        static readonly ChartColor _yellow = new("#EFB700");
        static readonly ChartColor _green = new("#008450");

        public enum TimeframeEnum { Hour = 0, Day, Week, Month };

        public async Task OnGetAsync(TimeframeEnum t, string id)
        {
            Timeframe = t;

            if (!string.IsNullOrEmpty(id))
                DeviceId = id;

            Metrics = Enumerable.Empty<IReading>();

            var lookback = Timeframe switch
            {
                TimeframeEnum.Hour => TimeSpan.FromHours(1),
                TimeframeEnum.Day => TimeSpan.FromHours(24),
                TimeframeEnum.Week => TimeSpan.FromDays(7),
                TimeframeEnum.Month => TimeSpan.FromDays(7*8),
                _ => throw new NotImplementedException()
            };

            var bininterval = Timeframe switch
            {
                TimeframeEnum.Hour => TimeSpan.FromMinutes(5),
                TimeframeEnum.Day => TimeSpan.FromHours(1),
                TimeframeEnum.Week => TimeSpan.FromDays(1),
                TimeframeEnum.Month => TimeSpan.FromDays(7),
                _ => throw new NotImplementedException()
            };

            var labelformat = Timeframe switch
            {
                TimeframeEnum.Hour or TimeframeEnum.Day => "H:mm",
                TimeframeEnum.Week or TimeframeEnum.Month => "M/dd",
                _ => throw new NotImplementedException()

            };

            var cosmos = new ChartMaker.CosmosQuery.MockEngine();
            var instream = await cosmos.DoQueryAsync(lookback,bininterval,new[]{"device-1"});
            var data = DatapointReader.ReadFromJson(instream);

            Chart = ChartMaker.Engine.CreateMultiLineChart(data, new[] { "Top/Temperature", "Condenser/Temperature" }, labelformat);

            // Query InfluxDB, compose into UI slabs

            var raw = await _datasource.GetLatestDevicePropertiesAsync(DeviceId);

            // Augment with DTMI information

            raw[string.Empty]["Schema"] = "Temperature Controller";

            // For starters, we will just directly translate results into slabs.
            // Next step will be breaking it apart, making it pretty

            Slab FromComponent(KeyValuePair<string,Dictionary<string,object>> c)
            {
                string ValueOrEmpty(string s, string alt) => string.IsNullOrEmpty(s) ? alt : s;

                var props = c.Value.Select(x => new KeyValueUnits() { Key = MapKey(x.Key), Value = MapValue(x), Writable = MapWritable(x.Key), Units = MapWritableUnits(x.Key) }).ToList();
                return new Slab() { Header = ValueOrEmpty(MapKey(c.Key),"Device Details"), ComponentId = c.Key, Properties = props };
            }

            Slabs = raw.Select(FromComponent).ToList();
        }

        // Translate into displayable properties
        string MapKey(string key)
        {
            return key switch
            {
                "serialNumber" => "Serial Number",
                "thermostat1" => "Thermostat One",
                "thermostat2" => "Thermostat Two",
                "deviceInformation" => "Device Information",
                "manufacturer" => "Manufacturer",
                "model" => "Device Model",
                "swVersion" => "Software Version",
                "osName" => "Operating System",
                "processorArchitecture" => "Processor Architecture",
                "totalStorage" => "Total Storage",
                "totalMemory" => "Total Memory",
                "temperature" => "Temperature",
                "maxTempSinceLastReboot" => "Max Temperature Since Reboot",
                "targetTemperature" => "Target Temperature",
                "workingSet" => $"Working Set",
                "telemetryPeriod" => "Telemetry Period",
                _ => key
            };
        }

        bool MapWritable(string key)
        {
            return key switch
            {
                "targetTemperature" or
                "telemetryPeriod" => true,
                _ => false
            };
        }

        string MapWritableUnits(string key)
        {
            return key switch
            {
                "targetTemperature" => "°C",
                _ => null
            };
        }

        string MapValue(KeyValuePair<string,object> kvp)
        {
            return kvp.Key switch
            {
                "maxTempSinceLastReboot" or
                "temperature" or
                "temperature"
                        => $"{kvp.Value:F1}°C",
                "targetTemperature" => $"{kvp.Value:F1}",
                "workingSet" => $"{(double)kvp.Value/7812.5:F1}MB",
                "totalStorage" or
                "totalMemory" 
                    => (double)kvp.Value switch
                    {
                        > 1000000 => $"{(double)kvp.Value/1000000:F1} GB",
                        > 1000 => $"{(double)kvp.Value/1000:F1} MB",
                        _ => $"{(double)kvp.Value:F1} kB"
                    },
                _ => kvp.Value.ToString()
            };
        }

        private static readonly ChartColor[] palette = new ChartColor[]
        {
            new ChartColor("540D6E"),
            new ChartColor("EE4266"),
            new ChartColor("FFD23F"),
            new ChartColor("875D5A"),
            new ChartColor("FFD3DA"),
            new ChartColor("8EE3EF"),
            new ChartColor("7A918D"),
        };

        private async Task SetTimespanChartAsync(TimeSpan timespan)
        {
            var now = DateTime.Now;
            var divisions = 48;

            var labels = Enumerable.Range(0, divisions).Select(x => timespan * ((double)x / (double)divisions)).Select(x => (now + x).ToString("hh:mm"));

            var alldata = await _datasource.GetSeriesReadingsAsync(DeviceId, timespan, divisions);
            var selector = alldata.First().Node;
            var subset = alldata.Where(x => x.Node == selector);
            var series = subset.Select(x => (x.Label, x.Values.Select(x=>Convert.ToInt32(x))));

            Chart = ChartConfig.CreateLineChart(labels, series, palette);
        }

        private void SetNowChart()
        {
            var points = Metrics.Select(x => (x.Label,(int)x.Last));

            var palette = points.Select(x =>
            {
                if (x.Item2 < 60)
                    return _green;
                if (x.Item2 < 65)
                    return _yellow;
                return _red;
            });

            Chart = ChartConfig.CreateBarChart(points, palette);
        }
    }
}
