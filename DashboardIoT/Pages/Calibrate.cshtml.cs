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
    public class CalibrateModel : PageModel
    {
        private readonly IDataSource _datasource;

        public IEnumerable<IReading> Sensors { get; private set; }

        public CalibrateModel(IDataSource datasource)
        {
            _datasource = datasource;
        }

        public async Task OnGet()
        {
            Sensors = await _datasource.GetMomentaryReadingsAsync("Home");
        }
    }
}
