using System.Threading.Tasks;
using BrewHub.Dashboard.Core.Providers;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashboardIoT.Pages
{
    public class CalibrateModel : PageModel
    {
        private readonly IDataSource _datasource;

        public CalibrateModel(IDataSource datasource)
        {
            _datasource = datasource;
        }

        public Task OnGet()
        {
            return Task.CompletedTask;
        }
    }
}
