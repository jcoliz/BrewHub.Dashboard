using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using DashboardIoT.Core.Interfaces;
using DashboardIoT.Core.MockData;
using Microsoft.Extensions.Logging;

namespace DashboardIoT
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // https://stackoverflow.com/questions/41287648/how-do-i-write-logs-from-within-startup-cs
            // https://github.com/dotnet/aspnetcore/issues/9337#issuecomment-539859667
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddConsole();
                builder.AddEventSourceLogger();
            });
            var logger = loggerFactory.CreateLogger("Startup");
            logger.LogInformation("Configure Services Started");

            services.AddRazorPages()
                .AddRazorRuntimeCompilation();
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            // Not working with InfluxDB right now
#if false
            var section = Configuration.GetSection(InfluxDB.InfluxDBDataSource.Options.Section);
            if (section.Exists())
            {
                logger.LogInformation("InfluxDB Options OK");
                services.Configure<InfluxDB.InfluxDBDataSource.Options>(section);
                services.AddSingleton<IDataSource, InfluxDB.InfluxDBDataSource>();
            }
            else
#endif
            {
                logger.LogWarning("InfluxDB Options not found. Using mock data");
                services.AddSingleton<IDataSource>(new MockDataSource());
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseForwardedHeaders();                
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseForwardedHeaders();                
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Don't have https redirection set up on nginx
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
