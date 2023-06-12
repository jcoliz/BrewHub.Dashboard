using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Models;
using BrewHub.Dashboard.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.ChartJS;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using BrewHub.Dashboard.Core.Display;

namespace BrewHub.Dashboard.AspNet.Pages
{
    public class DevicePageModel : PageModel
    {
        private readonly ILogger<DevicePageModel> _logger;
        private readonly IDataSource _datasource;

        public ChartConfig? Chart { get; set; }

        public TimeframeEnum Timeframe { get; set; } = TimeframeEnum.Hour;

        public string DeviceId { get; set; } = "device-1";

        public DisplayMetricGroup[] Slabs { get; private set; }

        public DevicePageModel(ILogger<DevicePageModel> logger, IDataSource datasource)
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

            var lookback = Timeframe switch
            {
                TimeframeEnum.Minutes => TimeSpan.FromMinutes(5), // "5m",
                TimeframeEnum.Hour => TimeSpan.FromHours(1), //"1h",
                TimeframeEnum.Hours => TimeSpan.FromHours(4), //"4h",
                TimeframeEnum.Day => TimeSpan.FromHours(24), //"24h",
                TimeframeEnum.Week => TimeSpan.FromDays(7), //"7d",
                TimeframeEnum.Month => TimeSpan.FromDays(28), //"28d",
                _ => throw new NotImplementedException()
            };

            var bininterval = Timeframe switch
            {
                TimeframeEnum.Minutes =>  TimeSpan.FromSeconds(20), //"20s",
                TimeframeEnum.Hour =>  TimeSpan.FromMinutes(2), //"2m",
                TimeframeEnum.Hours =>  TimeSpan.FromMinutes(15), //"15m",
                TimeframeEnum.Day =>  TimeSpan.FromHours(1), //"1h",
                TimeframeEnum.Week =>  TimeSpan.FromDays(1), //"1d",
                TimeframeEnum.Month => TimeSpan.FromDays(7), //"7d",
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
            var data = await _datasource.GetSingleDeviceTelemetryAsync(DeviceId, lookback, bininterval);
            var dtmi = new DeviceModelDetails();
            Chart = BrewHub.Dashboard.Core.Charting.ChartMaker.CreateMultiLineChart(data, dtmi.VisualizeTelemetryDevice, labelformat);

            // Query InfluxDB, compose into UI slabs

            data = await _datasource.GetLatestDevicePropertiesAsync(DeviceId);

            // Augment with DTMI information
            //raw[string.Empty]["Schema"] = dtmi.SchemaName;

            // For starters, we will just directly translate results into slabs.
            // Next step will be breaking it apart, making it pretty

            DisplayMetric FromDatapoint(Datapoint d)
            {
                return new DisplayMetric()
                {
                    Name = dtmi!.MapMetricName(d.__Field),
                    Id = d.__Field,
                    Value = dtmi.FormatMetricValue(d),
                    Units = dtmi.GetWritableUnits(d.__Field)
                };
            }

            DisplayMetricGroup FromComponent(IGrouping<string,Datapoint> c)
            {
                string ValueOrEmpty(string s, string alt) => string.IsNullOrEmpty(s) ? alt : s;

                return new DisplayMetricGroup() 
                { 
                    Title = ValueOrEmpty(dtmi.MapMetricName(c.Key),"Device Details"),
                    Id = c.Key,
                    Telemetry = c.Where(x=>dtmi.IsMetricTelemetry(x.__Field)).Select(FromDatapoint).ToArray(), 
                    ReadOnlyProperties = c.Where(x=>!dtmi.IsMetricWritable(x.__Field) && !dtmi.IsMetricTelemetry(x.__Field)).Select(FromDatapoint).ToArray(), 
                    WritableProperties = c.Where(x=>dtmi.IsMetricWritable(x.__Field)).Select(FromDatapoint).ToArray(), 
                    Commands = dtmi.GetCommands(c.Key).ToArray()
                };
            }

            Slabs = data.GroupBy(x => x.__Component ?? string.Empty).Select(FromComponent).ToArray();
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
    }
}
