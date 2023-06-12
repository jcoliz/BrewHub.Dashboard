﻿using BrewHub.Dashboard.Core.Display;
using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Models;
using BrewHub.Dashboard.Core.Providers;
using BrewHub.Dashboard.Core.Charting;
using Common.ChartJS;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrewHub.Dashboard.AspNet.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDataSource _datasource;

        public ChartConfig? Chart { get; set; }

        public TimeframeEnum Timeframe { get; set; } = TimeframeEnum.Hour;

        public DisplayMetricGroup[] Slabs { get; private set; } = Array.Empty<DisplayMetricGroup>();

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

                Slabs = data
                        .GroupBy(x => x.__Device)
                        .Select(dtmi!.FromDeviceComponentTelemetry)
                        .ToArray();

                Chart = ChartMaker.CreateMultiDeviceBarChart(data, dtmi.VisualizeTelemetryTop);
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
