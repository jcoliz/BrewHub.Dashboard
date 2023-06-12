using DashboardIoT.InfluxDB;
using BrewHub.Dashboard.Core.MockData;
using BrewHub.Dashboard.Core.Providers;
using System.Reflection;

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
builder.Services.AddOpenApiDocument(); // registers a OpenAPI v3.0 document with the name "v1" (default)
builder.Services.AddRazorPages()
                .AddRazorRuntimeCompilation();
builder.Services.AddHealthChecks();

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
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapHealthChecks("/health");

app.Run();
