// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

using DashboardIoT.InfluxDB;
using BrewHub.Dashboard.Core.MockData;
using BrewHub.Dashboard.Core.Providers;
using BrewHub.Protocol.Mqtt;
using System.Reflection;
using Dashboard.Services.DeviceMessaging;

var builder = WebApplication.CreateBuilder(args);

// https://stackoverflow.com/questions/41287648/how-do-i-write-logs-from-within-startup-cs
// https://github.com/dotnet/aspnetcore/issues/9337#issuecomment-539859667
using var loggerFactory = LoggerFactory.Create(logbuilder =>
{
    logbuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logbuilder.AddConsole();
    logbuilder.AddEventSourceLogger();
});
var logger = loggerFactory.CreateLogger("Startup");
logger.LogInformation("*** STARTING ***");

builder.Configuration.AddTomlFile("config.toml", optional: true, reloadOnChange: true);

// Add services to the container.

// This is PROBABLY only needed in development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin() /*WithOrigins
                    (
                        "http://localhost",
                        "https://localhost"
                    )*/
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    ;
        });
});

builder.Services.AddControllersWithViews();
builder.Services.AddOpenApiDocument( o => {
    o.Title = "BrewHub.Net Dashboard Backend API";
    o.Description = "Application boundary between .NET backend and Vue.JS frontend.";
});
builder.Services.AddRazorPages()
                .AddRazorRuntimeCompilation();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

// Set influxdb:Server to "-" to force use of mock data
var section = builder.Configuration.GetSection(InfluxDBDataSource.Options.Section);
if (section.Exists() && builder.Configuration["influxdb:server"] != "-")
{
    logger.LogInformation("InfluxDB Options OK");
    builder.Services.Configure<InfluxDBDataSource.Options>(section);
    builder.Services.AddSingleton<IDataSource, InfluxDBDataSource>();
}
else
{
    logger.LogWarning("InfluxDB Options not found. Using mock data");
    builder.Services.AddSingleton<IDataSource>(new MockDataSource());
}

// Bring up MQTT connection
section = builder.Configuration.GetSection(MqttOptions.Section);
if (section.Exists())
{
    logger.LogInformation("MqttDeviceMessaging Options OK");
    builder.Services.Configure<MqttOptions>(section);
    builder.Services.AddSingleton<IDeviceMessaging, MqttDeviceMessaging>();
}
else
{
    logger.LogWarning("MqttDeviceMessaging Options not found. DeviceMessaging will not be available");
}

// Get app version, store in configuration for later use
var assembly = Assembly.GetEntryAssembly();
var resource = assembly!.GetManifestResourceNames().Where(x => x.EndsWith(".version.txt")).SingleOrDefault();
if (resource is not null)
{
    using var stream = assembly.GetManifestResourceStream(resource);
    using var streamreader = new StreamReader(stream!);
    var version = streamreader.ReadLine();
    builder.Configuration["Codebase:Version"] = version;
    logger.LogInformation("Version: {version}", version);
}

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseExceptionHandler();
app.UseStatusCodePages();

var runningincontainer = Convert.ToBoolean(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? "false");
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // Note that in container we DON'T want dev server, but we also don't want https. So we can't use development mode
    // in container. Have to check the env var.
    
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    if (!runningincontainer)
        app.UseHsts();
}
if (!runningincontainer)
    app.UseHttpsRedirection();

 // Add OpenAPI/Swagger middlewares
app.UseOpenApi(); // Serves the registered OpenAPI/Swagger documents by default on `/swagger/{documentName}/swagger.json`
app.UseSwaggerUi3(); // Serves the Swagger UI 3 web ui to view the OpenAPI/Swagger documents by default on `/swagger`

app.UseStaticFiles();
app.UseRouting();
app.UseCors();

#pragma warning disable ASP0014
// https://alexpotter.dev/net-6-with-vue-3/
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action=Index}/{id?}");
    endpoints.MapHealthChecks("/health");
});
#pragma warning restore ASP0014

if (app.Environment.IsDevelopment())
{
    app.UseSpa(spa =>
    {
        // Set this to `https` if https is enabled in `ClientApp/vite.config.ts`
        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    });
}
else
{
    app.MapFallbackToFile("index.html");
}

app.Run();
