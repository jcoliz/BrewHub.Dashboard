using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Models;
using BrewHub.Dashboard.Core.Providers;
using Common.ChartJS;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDataSource _datasource;

        public ChartConfig Chart { get; set; }

        public TimeframeEnum Timeframe { get; set; } = TimeframeEnum.Hour;

        public Dictionary<string,List<(string,string)>> Telemetry { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, IDataSource datasource)
        {
            _logger = logger;
            _datasource = datasource;
        }

        static readonly ChartColor _red = new("#B81d13");
        static readonly ChartColor _yellow = new("#EFB700");
        static readonly ChartColor _green = new("#008450");

        public enum TimeframeEnum { Hour = 0, Day, Week, Month };        

        public async Task OnGetAsync(TimeframeEnum t, string s)
        {
            try
            {
                Timeframe = t;

                // Pull raw telemetry from database
                // TODO: Need to get all telemetry in this call
                var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
                var dtmi = new DeviceModelDetails();

                string ExtractComponentAndMetricName(Datapoint d)
                {
                    var f = dtmi.MapMetricName(d.__Field);
                    return (d.__Component is null) ? f : $"{dtmi.MapMetricName(d.__Component)}/{f}";
                }

                Telemetry = data
                        .GroupBy(x => x.__Device)
                        .ToDictionary
                        (
                            x => x.Key,
                            x => x.Select(y => (ExtractComponentAndMetricName(y), dtmi.FormatMetricValue(y))).ToList()
                        );

                Chart = BrewHub.Dashboard.Core.Charting.ChartMaker.CreateMultiDeviceBarChart(data, dtmi.VisualizeTelemetryTop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get: Failed");
            }
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
