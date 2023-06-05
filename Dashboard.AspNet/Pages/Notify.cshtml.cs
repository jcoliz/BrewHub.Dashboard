using System.Threading.Tasks;
using BrewHub.Dashboard.Core.Providers;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BrewHub.Dashboard.AspNet.Pages
{
    public class NotifyModel : PageModel
    {
        private readonly IDataSource _datasource;

        public NotifyModel(IDataSource datasource)
        {
            _datasource = datasource;
        }

        public Task OnGetAsync()
        {
            return Task.CompletedTask;
        }
    }
}
