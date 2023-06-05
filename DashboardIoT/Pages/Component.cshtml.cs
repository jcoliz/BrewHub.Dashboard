using BrewHub.Dashboard.Core.Providers;
using BrewHub.Dashboard.Core.Charting;
using BrewHub.Dashboard.Core.Dtmi;
using Common.ChartJS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DashboardIoT.Pages
{
    public class ComponentPageModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDataSource _datasource;

        public ChartConfig Chart { get; set; }

        public TimeframeEnum Timeframe { get; set; } = TimeframeEnum.Hour;

        public string DeviceId { get; set; } = "device-1";
        public string ComponentId { get; set; } = string.Empty;

        public ComponentPageModel(ILogger<IndexModel> logger, IDataSource datasource)
        {
            _logger = logger;
            _datasource = datasource;
        }

        public enum TimeframeEnum { Minutes = 0, Hour, Hours, Day, Week, Month };

        public async Task OnGetAsync(TimeframeEnum t, string d, string c)
        {
            Timeframe = t;

            if (!string.IsNullOrEmpty(d))
                DeviceId = d;

            if (!string.IsNullOrEmpty(c))
                ComponentId = c;

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
                TimeframeEnum.Minutes => TimeSpan.FromSeconds(20), //"20s",
                TimeframeEnum.Hour => TimeSpan.FromMinutes(2), //"2m",
                TimeframeEnum.Hours => TimeSpan.FromMinutes(15), //"15m",
                TimeframeEnum.Day => TimeSpan.FromHours(1), //"1h",
                TimeframeEnum.Week => TimeSpan.FromDays(1), //"1d",
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

            var data = await _datasource.GetSingleDeviceTelemetryAsync(DeviceId, lookback, bininterval);
            var subset = data.Where(x => (x.__Component is null) ? (c == string.Empty) : x.__Component == c);
            var dtmi = new DeviceModelDetails();

            // TODO: Pick out the right telemetry for THIS component.
            Chart = ChartMaker.CreateMultiLineChart(subset, new[]{ "temperature" }, labelformat);
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
