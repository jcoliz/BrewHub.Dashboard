﻿using System;
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

        public IEnumerable<IReading> Metrics { get; private set; }

        public ComponentPageModel(ILogger<IndexModel> logger, IDataSource datasource)
        {
            _logger = logger;
            _datasource = datasource;
        }

        static readonly ChartColor _red = new("#B81d13");
        static readonly ChartColor _yellow = new("#EFB700");
        static readonly ChartColor _green = new("#008450");

        public enum TimeframeEnum { Hour = 0, Day, Week, Month };

        public async Task OnGetAsync(TimeframeEnum t, string d, string c)
        {
            Timeframe = t;

            if (!string.IsNullOrEmpty(d))
                DeviceId = d;

            if (!string.IsNullOrEmpty(c))
                ComponentId = c;

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

            Chart = ChartMaker.Engine.CreateMultiLineChart(data, new[] { "WorkingSet" }, labelformat);
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