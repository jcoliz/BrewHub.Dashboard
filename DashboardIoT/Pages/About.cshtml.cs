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
