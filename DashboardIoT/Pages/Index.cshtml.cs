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
using DashboardIoT.Core.Dtmi;

namespace DashboardIoT.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDataSource _datasource;

        public ChartConfig Chart { get; set; }

        public TimeframeEnum Timeframe { get; set; } = TimeframeEnum.Hour;

        public Dictionary<string,Dictionary<string,string>> Telemetry { get; private set; }

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
            Timeframe = t;

                // Pull raw telemetry from database
                // TODO: Need to get all telemetry in this call
                var raw = await _datasource.GetLatestDeviceTelemetryAllAsync();

                var dtmi = new DeviceModelDetails();

                Telemetry = raw.ToDictionary(
                    x => x.Key,
                    x => x.Value.ToDictionary(x=>dtmi.MapMetricName(x.Key),dtmi.FormatMetricValue)
                );

#if false
                var cosmos = new ChartMaker.CosmosQuery.MockEngine();
                var instream = await cosmos.DoQueryAsync(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10),new [] {"device-1", "device-2", "device-3", "device-4", "device-5", "device-6"});
                var data2 = DatapointReader.ReadFromJson(instream);
#endif

                var data = DatapointReader.ReadFromInfluxDB(raw);
                Chart = ChartMaker.Engine.CreateMultiDeviceBarChart(data, dtmi.VisualizeTelemetryTop);
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
