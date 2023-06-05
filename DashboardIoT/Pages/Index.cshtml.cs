using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.ChartJS;
using BrewHub.Core.Providers;
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

        public List<ChartMaker.Models.Datapoint> Telemetry { get; private set; }

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
                Telemetry = data.ToList();

                var dtmi = new DeviceModelDetails();

                Chart = ChartMaker.Engine.CreateMultiDeviceBarChart(Telemetry, dtmi.VisualizeTelemetryTop);
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
