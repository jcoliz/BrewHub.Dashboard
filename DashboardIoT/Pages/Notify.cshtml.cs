using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DashboardIoT.Core.Interfaces;
using DashboardIoT.Core.MockData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashboardIoT.Pages
{
    public class NotifyModel : PageModel
    {
        private readonly IDataSource _datasource;

        public IEnumerable<ILevel> Levels;

        public NotifyModel(IDataSource datasource)
        {
            _datasource = datasource;
        }

        public async Task OnGetAsync()
        {
            var readings = await _datasource.GetMomentaryReadingsAsync("Home");

            var levelstatuses = new List<(LevelSeverityEnum, LevelSideEnum)>()
            {
                (LevelSeverityEnum.OutOfRange,LevelSideEnum.High),
                (LevelSeverityEnum.Error,LevelSideEnum.High),
                (LevelSeverityEnum.Warning,LevelSideEnum.High),
                (LevelSeverityEnum.Warning,LevelSideEnum.Low),
                (LevelSeverityEnum.Error,LevelSideEnum.Low),
                (LevelSeverityEnum.OutOfRange,LevelSideEnum.Low),
            };

            Levels = readings.SelectMany((r,i) => levelstatuses.Select((s,j) => new MockLevel() { Metric = r, Severity = s.Item1, Side = s.Item2, Value = 10*i+j })).ToList();
        }
    }
}
