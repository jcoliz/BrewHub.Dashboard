using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashboardIoT.Pages
{
    public class About : PageModel
    {
        public About()
        {
        }

        public Task OnGet()
        {
            return Task.CompletedTask;
        }
    }
}
