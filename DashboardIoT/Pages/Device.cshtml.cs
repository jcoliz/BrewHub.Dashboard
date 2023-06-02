using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.ChartJS;
using DashboardIoT.Core.Interfaces;
using DashboardIoT.Core.Dtmi;
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

        public enum TimeframeEnum { Minutes = 0, Hour, Hours, Day, Week, Month };

        public async Task OnGetAsync(TimeframeEnum t, string id)
        {
            Timeframe = t;

            if (!string.IsNullOrEmpty(id))
                DeviceId = id;

            Metrics = Enumerable.Empty<IReading>();

            var lookback = Timeframe switch
            {
                TimeframeEnum.Minutes => "5m",
                TimeframeEnum.Hour => "1h",
                TimeframeEnum.Hours => "4h",
                TimeframeEnum.Day => "24h",
                TimeframeEnum.Week => "7d",
                TimeframeEnum.Month => "28d",
                _ => throw new NotImplementedException()
            };

            var bininterval = Timeframe switch
            {
                TimeframeEnum.Minutes => "20s",
                TimeframeEnum.Hour => "2m",
                TimeframeEnum.Hours => "15m",
                TimeframeEnum.Day => "1h",
                TimeframeEnum.Week => "1d",
                TimeframeEnum.Month => "7d",
                _ => throw new NotImplementedException()
            };

            var labelformat = Timeframe switch
            {
                TimeframeEnum.Minutes => "mm:ss",
                TimeframeEnum.Hour or 
                TimeframeEnum.Hours or 
                TimeframeEnum.Day => "H:mm",
                TimeframeEnum.Week or 
                TimeframeEnum.Month => "M/dd",
                _ => throw new NotImplementedException()

            };

            // Get historical telemetry data for all components on this device
            var influx = await _datasource.GetSingleDeviceTelemetryAsync(DeviceId, lookback, bininterval);
            var data = DatapointReader.ReadFromInfluxDB(influx);

#if false
            var cosmos = new ChartMaker.CosmosQuery.MockEngine();
            var instream = await cosmos.DoQueryAsync(lookback,bininterval,new[]{"device-1"});
            var data = DatapointReader.ReadFromJson(instream);
#endif
            var dtmi = new DeviceModelDetails();
            Chart = ChartMaker.Engine.CreateMultiLineChart(data, dtmi.VisualizeTelemetryDevice, labelformat);

            // Query InfluxDB, compose into UI slabs

            var raw = await _datasource.GetLatestDevicePropertiesAsync(DeviceId);

            // Augment with DTMI information
            raw[string.Empty]["Schema"] = dtmi.SchemaName;

            // For starters, we will just directly translate results into slabs.
            // Next step will be breaking it apart, making it pretty

            Slab FromComponent(KeyValuePair<string,Dictionary<string,object>> c)
            {
                string ValueOrEmpty(string s, string alt) => string.IsNullOrEmpty(s) ? alt : s;

                var props = c.Value.Select(x => new KeyValueUnits() { Key = dtmi.MapMetricName(x.Key), Value = dtmi.FormatMetricValue(x), Writable = dtmi.IsMetricWritable(x.Key), Units = dtmi.GetWritableUnits(x.Key) }).ToList();
                return new Slab() { Header = ValueOrEmpty(dtmi.MapMetricName(c.Key),"Device Details"), ComponentId = c.Key, Properties = props, Commands = dtmi.GetCommands(c.Key) };
            }

            Slabs = raw.Select(FromComponent).ToList();
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
